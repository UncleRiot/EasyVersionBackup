using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace EasyVersionBackup
{
    public static class VersionPatternHelper
    {
        public static bool IsDatePattern(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string pattern = value.Trim();
            bool containsDateToken = false;

            for (int index = 0; index < pattern.Length;)
            {
                char character = pattern[index];

                if (character == '\\')
                {
                    if (index + 1 >= pattern.Length)
                    {
                        return false;
                    }

                    index += 2;
                    continue;
                }

                if (character == '\'' || character == '"')
                {
                    char quoteCharacter = character;
                    index++;
                    bool quoteClosed = false;

                    while (index < pattern.Length)
                    {
                        if (pattern[index] == quoteCharacter)
                        {
                            if (index + 1 < pattern.Length &&
                                pattern[index + 1] == quoteCharacter)
                            {
                                index += 2;
                                continue;
                            }

                            index++;
                            quoteClosed = true;
                            break;
                        }

                        index++;
                    }

                    if (!quoteClosed)
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '%')
                {
                    if (index + 1 >= pattern.Length ||
                        !IsSupportedDatePatternLetter(
                            pattern[index + 1]))
                    {
                        return false;
                    }

                    containsDateToken = true;
                    index += 2;
                    continue;
                }

                if (!char.IsLetter(character))
                {
                    index++;
                    continue;
                }

                if (!IsSupportedDatePatternLetter(character))
                {
                    return false;
                }

                int tokenLength = 1;

                while (index + tokenLength < pattern.Length &&
                    pattern[index + tokenLength] == character)
                {
                    tokenLength++;
                }

                if (!IsSupportedDateTokenLength(
                    character,
                    tokenLength))
                {
                    return false;
                }

                containsDateToken = true;
                index += tokenLength;
            }

            return containsDateToken;
        }

        private static bool IsSupportedDatePatternLetter(
            char character)
        {
            return character == 'y' ||
                character == 'M' ||
                character == 'd' ||
                character == 'H' ||
                character == 'h' ||
                character == 'm' ||
                character == 's' ||
                character == 'f' ||
                character == 'F' ||
                character == 't' ||
                character == 'z' ||
                character == 'K' ||
                character == 'g';
        }

        private static bool IsSupportedDateTokenLength(
            char character,
            int tokenLength)
        {
            return character switch
            {
                'y' => tokenLength <= 5,
                'M' => tokenLength <= 4,
                'd' => tokenLength <= 4,
                'H' or 'h' or 'm' or 's' or 't' or 'g' =>
                    tokenLength <= 2,
                'f' or 'F' => tokenLength <= 7,
                'z' => tokenLength <= 3,
                'K' => tokenLength == 1,
                _ => false
            };
        }

        public static bool TryCreateVersionFromPattern(
            string pattern,
            out string version)
        {
            version = string.Empty;

            if (!IsDatePattern(pattern))
            {
                return false;
            }

            try
            {
                string generatedVersion = DateTime.Now.ToString(
                    pattern.Trim(),
                    CultureInfo.InvariantCulture);

                if (!IsValidVersionValue(generatedVersion))
                {
                    return false;
                }

                version = generatedVersion;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string CreateVersionFromPattern(string pattern)
        {
            if (!IsDatePattern(pattern))
            {
                return pattern.Trim();
            }

            if (!TryCreateVersionFromPattern(pattern, out string version))
            {
                throw new FormatException(
                    "The date versioning pattern does not produce a valid file name.");
            }

            return version;
        }

        public static bool IsValidVersionValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string trimmedValue = value.Trim();

            if (trimmedValue.Length == 0 ||
                trimmedValue.EndsWith(".", StringComparison.Ordinal) ||
                trimmedValue.EndsWith(" ", StringComparison.Ordinal))
            {
                return false;
            }

            return trimmedValue.IndexOfAny(
                Path.GetInvalidFileNameChars()) < 0;
        }

        public static bool IsValidVersioningValue(string value)
        {
            if (string.Equals(
                value,
                "none",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (IsDatePattern(value))
            {
                return TryCreateVersionFromPattern(
                    value,
                    out _);
            }

            return IsValidVersionValue(value);
        }

        public static string IncrementVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "0.0.1";
            }

            Match match = Regex.Match(
                version,
                @"^(?<prefix>[A-Za-z]*)(?<numbers>\d+(\.\d+)*)$");

            if (!match.Success)
            {
                return version;
            }

            string prefix = match.Groups["prefix"].Value;
            string[] parts = match.Groups["numbers"].Value.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries);

            List<int> numbers = new List<int>();

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int number))
                {
                    return version;
                }

                numbers.Add(number);
            }

            if (numbers.Count == 0 ||
                numbers[numbers.Count - 1] == int.MaxValue)
            {
                return version;
            }

            numbers[numbers.Count - 1]++;
            return prefix + string.Join(".", numbers);
        }

        public static string GetHighestCompatibleVersion(
            string baseVersion,
            IEnumerable<string> versions)
        {
            string highest = string.Empty;

            foreach (string version in versions)
            {
                if (string.IsNullOrWhiteSpace(version) ||
                    !IsCompatibleVersion(baseVersion, version))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(highest) ||
                    IsVersionGreater(version, highest))
                {
                    highest = version;
                }
            }

            return highest;
        }

        private static bool IsCompatibleVersion(
            string baseVersion,
            string version)
        {
            Match baseMatch = Regex.Match(
                baseVersion,
                @"^(?<prefix>.*?)(?<number>\d+)$");

            Match versionMatch = Regex.Match(
                version,
                @"^(?<prefix>.*?)(?<number>\d+)$");

            if (!baseMatch.Success || !versionMatch.Success)
            {
                return false;
            }

            return string.Equals(
                baseMatch.Groups["prefix"].Value,
                versionMatch.Groups["prefix"].Value,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVersionGreater(
            string left,
            string right)
        {
            Match leftMatch = Regex.Match(
                left,
                @"^(?<prefix>.*?)(?<number>\d+)$");

            Match rightMatch = Regex.Match(
                right,
                @"^(?<prefix>.*?)(?<number>\d+)$");

            if (!leftMatch.Success || !rightMatch.Success ||
                !string.Equals(
                    leftMatch.Groups["prefix"].Value,
                    rightMatch.Groups["prefix"].Value,
                    StringComparison.OrdinalIgnoreCase) ||
                !long.TryParse(
                    leftMatch.Groups["number"].Value,
                    out long leftNumber) ||
                !long.TryParse(
                    rightMatch.Groups["number"].Value,
                    out long rightNumber))
            {
                return false;
            }

            return leftNumber > rightNumber;
        }
    }
}
