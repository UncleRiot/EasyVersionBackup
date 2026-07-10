using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace EasyVersionBackup
{
    public enum BackupFileErrorAction
    {
        Retry,
        Skip,
        IgnoreAll,
        Abort
    }

    public sealed class BackupFileOperationResult
    {
        public List<string> SkippedPaths { get; } = new List<string>();
        public string DestinationFileName { get; internal set; } = string.Empty;
        public int SkippedCount => SkippedPaths.Count;
    }

    public static class BackupFileService
    {
        public static BackupFileOperationResult CreateBackup(
            string sourceDirectory,
            string destinationPath,
            bool createZip,
            bool overwriteExisting,
            List<string> excludedPaths,
            bool ignoreAllErrors,
            Func<string, Exception, BackupFileErrorAction>? errorHandler)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
            }

            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                throw new ArgumentException("Destination path is required.", nameof(destinationPath));
            }

            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");
            }

            string fullSourceDirectory = Path.GetFullPath(sourceDirectory);
            string fullDestinationPath = Path.GetFullPath(destinationPath);
            string? destinationParentDirectory = Path.GetDirectoryName(fullDestinationPath);

            if (string.IsNullOrWhiteSpace(destinationParentDirectory))
            {
                throw new IOException($"Destination parent directory could not be determined: {destinationPath}");
            }

            Directory.CreateDirectory(destinationParentDirectory);

            BackupFileOperationState state = new BackupFileOperationState(
                ignoreAllErrors,
                errorHandler);

            string temporaryPath = BuildTemporaryPath(fullDestinationPath);

            try
            {
                if (createZip)
                {
                    CreateZipFromDirectory(
                        fullSourceDirectory,
                        temporaryPath,
                        excludedPaths ?? new List<string>(),
                        state);
                }
                else
                {
                    CopyDirectory(
                        fullSourceDirectory,
                        temporaryPath,
                        excludedPaths ?? new List<string>(),
                        state);
                }

                PublishTemporaryBackup(
                    temporaryPath,
                    fullDestinationPath,
                    createZip,
                    overwriteExisting);

                BackupFileOperationResult result = new BackupFileOperationResult
                {
                    DestinationFileName = createZip
                        ? Path.GetFileName(fullDestinationPath)
                        : Path.GetFileName(fullDestinationPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                };

                result.SkippedPaths.AddRange(state.SkippedPaths);
                return result;
            }
            catch
            {
                TryDeletePathIfExists(temporaryPath);
                throw;
            }
        }

        private static void CopyDirectory(
            string sourceDirectory,
            string destinationDirectory,
            List<string> excludedPaths,
            BackupFileOperationState state)
        {
            SourceTree sourceTree = EnumerateSourceTree(sourceDirectory, excludedPaths, state);
            Directory.CreateDirectory(destinationDirectory);

            foreach (string directoryPath in sourceTree.Directories)
            {
                string relativeDirectoryPath = Path.GetRelativePath(sourceDirectory, directoryPath);
                string targetDirectoryPath = relativeDirectoryPath == "."
                    ? destinationDirectory
                    : Path.Combine(destinationDirectory, relativeDirectoryPath);

                Directory.CreateDirectory(targetDirectoryPath);
            }

            foreach (string filePath in sourceTree.Files)
            {
                string relativeFilePath = Path.GetRelativePath(sourceDirectory, filePath);
                string targetFilePath = Path.Combine(destinationDirectory, relativeFilePath);
                string targetParentDirectory = Path.GetDirectoryName(targetFilePath) ?? destinationDirectory;

                Directory.CreateDirectory(targetParentDirectory);

                if (!TryExecuteSourceOperation(
                    filePath,
                    () =>
                    {
                        File.Copy(filePath, targetFilePath, true);
                        return true;
                    },
                    state,
                    out _))
                {
                    continue;
                }
            }
        }

        private static void CreateZipFromDirectory(
            string sourceDirectory,
            string zipPath,
            List<string> excludedPaths,
            BackupFileOperationState state)
        {
            SourceTree sourceTree = EnumerateSourceTree(sourceDirectory, excludedPaths, state);
            string stagingDirectory = Path.Combine(
                Path.GetTempPath(),
                "EasyVersionBackup",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(stagingDirectory);

            try
            {
                using FileStream zipStream = new FileStream(
                    zipPath,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None);

                using ZipArchive zipArchive = new ZipArchive(
                    zipStream,
                    ZipArchiveMode.Create);

                foreach (string directoryPath in sourceTree.Directories)
                {
                    string relativeDirectoryPath = Path.GetRelativePath(sourceDirectory, directoryPath)
                        .Replace('\\', '/')
                        .Trim('/');

                    if (string.IsNullOrWhiteSpace(relativeDirectoryPath) || relativeDirectoryPath == ".")
                    {
                        continue;
                    }

                    zipArchive.CreateEntry(relativeDirectoryPath + "/");
                }

                foreach (string filePath in sourceTree.Files)
                {
                    string stagedFilePath = Path.Combine(
                        stagingDirectory,
                        Guid.NewGuid().ToString("N") + ".tmp");

                    bool staged = TryExecuteSourceOperation(
                        filePath,
                        () =>
                        {
                            File.Copy(filePath, stagedFilePath, true);
                            return true;
                        },
                        state,
                        out _);

                    if (!staged)
                    {
                        TryDeletePathIfExists(stagedFilePath);
                        continue;
                    }

                    try
                    {
                        string relativeFilePath = Path.GetRelativePath(sourceDirectory, filePath)
                            .Replace('\\', '/');

                        ZipArchiveEntry entry = zipArchive.CreateEntry(
                            relativeFilePath,
                            CompressionLevel.Optimal);

                        try
                        {
                            DateTime lastWriteTime = File.GetLastWriteTime(filePath);

                            if (lastWriteTime.Year >= 1980 && lastWriteTime.Year <= 2107)
                            {
                                entry.LastWriteTime = lastWriteTime;
                            }
                        }
                        catch (Exception exception)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "ZIP timestamp could not be preserved: " +
                                exception.Message);
                        }

                        using FileStream stagedStream = new FileStream(
                            stagedFilePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read);

                        using Stream entryStream = entry.Open();
                        stagedStream.CopyTo(entryStream);
                    }
                    finally
                    {
                        TryDeletePathIfExists(stagedFilePath);
                    }
                }
            }
            finally
            {
                TryDeletePathIfExists(stagingDirectory);
            }
        }

        private static SourceTree EnumerateSourceTree(
            string sourceDirectory,
            List<string> excludedPaths,
            BackupFileOperationState state)
        {
            SourceTree sourceTree = new SourceTree();
            Stack<string> pendingDirectories = new Stack<string>();
            HashSet<string> visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string fullSourceDirectory =
                BackupHelper.NormalizeDirectoryPath(sourceDirectory);

            FileAttributes sourceAttributes = File.GetAttributes(fullSourceDirectory);

            if ((sourceAttributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException($"Source directory is a reparse point and cannot be backed up safely: {fullSourceDirectory}");
            }

            pendingDirectories.Push(fullSourceDirectory);

            while (pendingDirectories.Count > 0)
            {
                string currentDirectory = pendingDirectories.Pop();
                string normalizedCurrentDirectory =
                    BackupHelper.NormalizeDirectoryPath(currentDirectory);

                if (!visitedDirectories.Add(normalizedCurrentDirectory))
                {
                    continue;
                }

                if (BackupHelper.IsExcludedPath(fullSourceDirectory, currentDirectory, excludedPaths))
                {
                    continue;
                }

                sourceTree.Directories.Add(currentDirectory);

                if (!TryExecuteSourceOperation(
                    currentDirectory,
                    () => Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly),
                    state,
                    out string[]? childDirectories))
                {
                    continue;
                }

                if (!TryExecuteSourceOperation(
                    currentDirectory,
                    () => Directory.GetFiles(currentDirectory, "*", SearchOption.TopDirectoryOnly),
                    state,
                    out string[]? files))
                {
                    continue;
                }

                foreach (string childDirectory in childDirectories ?? Array.Empty<string>())
                {
                    if (BackupHelper.IsExcludedPath(fullSourceDirectory, childDirectory, excludedPaths))
                    {
                        continue;
                    }

                    if (!TryExecuteSourceOperation(
                        childDirectory,
                        () => File.GetAttributes(childDirectory),
                        state,
                        out FileAttributes childDirectoryAttributes))
                    {
                        continue;
                    }

                    if ((childDirectoryAttributes & FileAttributes.ReparsePoint) != 0)
                    {
                        state.RecordSkipped(childDirectory);
                        BackupLogger.WriteNormalLine($"REPARSE POINT SKIPPED | path=\"{childDirectory}\"");
                        continue;
                    }

                    pendingDirectories.Push(childDirectory);
                }

                foreach (string filePath in files ?? Array.Empty<string>())
                {
                    if (BackupHelper.IsExcludedPath(fullSourceDirectory, filePath, excludedPaths))
                    {
                        continue;
                    }

                    if (!TryExecuteSourceOperation(
                        filePath,
                        () => File.GetAttributes(filePath),
                        state,
                        out FileAttributes fileAttributes))
                    {
                        continue;
                    }

                    if ((fileAttributes & FileAttributes.ReparsePoint) != 0)
                    {
                        state.RecordSkipped(filePath);
                        BackupLogger.WriteNormalLine($"REPARSE POINT SKIPPED | path=\"{filePath}\"");
                        continue;
                    }

                    sourceTree.Files.Add(filePath);
                }
            }

            return sourceTree;
        }

        private static bool TryExecuteSourceOperation<T>(
            string path,
            Func<T> operation,
            BackupFileOperationState state,
            out T? result)
        {
            while (true)
            {
                try
                {
                    result = operation();
                    return true;
                }
                catch (Exception exception)
                {
                    BackupFileErrorAction action = state.IgnoreAllErrors
                        ? BackupFileErrorAction.IgnoreAll
                        : state.ErrorHandler?.Invoke(path, exception) ?? BackupFileErrorAction.Abort;

                    if (action == BackupFileErrorAction.Retry)
                    {
                        continue;
                    }

                    if (action == BackupFileErrorAction.IgnoreAll)
                    {
                        state.IgnoreAllErrors = true;
                        state.RecordSkipped(path);
                        result = default;
                        return false;
                    }

                    if (action == BackupFileErrorAction.Skip)
                    {
                        state.RecordSkipped(path);
                        result = default;
                        return false;
                    }

                    throw;
                }
            }
        }

        private static string BuildTemporaryPath(string destinationPath)
        {
            string? parentDirectory = Path.GetDirectoryName(destinationPath);

            if (string.IsNullOrWhiteSpace(parentDirectory))
            {
                throw new IOException($"Destination parent directory could not be determined: {destinationPath}");
            }

            string destinationName = Path.GetFileName(destinationPath);

            for (int attempt = 0; attempt < 100; attempt++)
            {
                string temporaryPath = Path.Combine(
                    parentDirectory,
                    destinationName + ".evbtmp-" + Guid.NewGuid().ToString("N"));

                if (!File.Exists(temporaryPath) && !Directory.Exists(temporaryPath))
                {
                    return temporaryPath;
                }
            }

            throw new IOException($"No temporary destination name could be created for: {destinationPath}");
        }

        private static void PublishTemporaryBackup(
            string temporaryPath,
            string destinationPath,
            bool isFile,
            bool overwriteExisting)
        {
            bool destinationExists = File.Exists(destinationPath) || Directory.Exists(destinationPath);

            if (!destinationExists)
            {
                MovePath(temporaryPath, destinationPath, isFile);
                return;
            }

            if (!overwriteExisting)
            {
                throw new IOException($"Destination already exists: {destinationPath}");
            }

            bool destinationIsFile = File.Exists(destinationPath);

            if (destinationIsFile != isFile)
            {
                throw new IOException($"Destination type does not match the backup type: {destinationPath}");
            }

            string rollbackPath = destinationPath + ".evbold-" + Guid.NewGuid().ToString("N");

            MovePath(destinationPath, rollbackPath, isFile);

            try
            {
                MovePath(temporaryPath, destinationPath, isFile);
            }
            catch
            {
                if (!File.Exists(destinationPath) && !Directory.Exists(destinationPath))
                {
                    MovePath(rollbackPath, destinationPath, isFile);
                }

                throw;
            }

            try
            {
                DeletePathIfExists(rollbackPath);
            }
            catch (Exception exception)
            {
                BackupLogger.WriteLine($"BACKUP CLEANUP WARNING | path=\"{rollbackPath}\" | error=\"{exception.Message}\"");
            }
        }

        private static void MovePath(string sourcePath, string destinationPath, bool isFile)
        {
            if (isFile)
            {
                File.Move(sourcePath, destinationPath);
            }
            else
            {
                Directory.Move(sourcePath, destinationPath);
            }
        }

        private static void TryDeletePathIfExists(string path)
        {
            try
            {
                DeletePathIfExists(path);
            }
            catch (Exception exception)
            {
                BackupLogger.WriteVerboseLine(
                    $"BACKUP TEMPORARY CLEANUP WARNING | path=\"{path}\" | error=\"{exception.Message}\"");
            }
        }

        private static void DeletePathIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return;
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private sealed class BackupFileOperationState
        {
            private readonly HashSet<string> skippedPathSet =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public BackupFileOperationState(
                bool ignoreAllErrors,
                Func<string, Exception, BackupFileErrorAction>? errorHandler)
            {
                IgnoreAllErrors = ignoreAllErrors;
                ErrorHandler = errorHandler;
            }

            public bool IgnoreAllErrors { get; set; }
            public Func<string, Exception, BackupFileErrorAction>? ErrorHandler { get; }
            public List<string> SkippedPaths { get; } = new List<string>();

            public void RecordSkipped(string path)
            {
                if (skippedPathSet.Add(path))
                {
                    SkippedPaths.Add(path);
                }
            }
        }

        private sealed class SourceTree
        {
            public List<string> Directories { get; } = new List<string>();
            public List<string> Files { get; } = new List<string>();
        }
    }
}
