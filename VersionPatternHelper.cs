using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            return Regex.IsMatch(value, "yyyy|yy|dd|hh|HH|mm|MM", RegexOptions.IgnoreCase);
        }

        public static string CreateVersionFromPattern(string pattern)
        {
            if (!IsDatePattern(pattern))
            {
                return pattern.Trim();
            }

            return BuildDateVersion(pattern.Trim(), DateTime.Now);
        }

        public static bool IsValidVersionValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        public static string IncrementVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "0.0.1";
            }

            Match match = Regex.Match(version, @"^(?<prefix>[A-Za-z]*)(?<numbers>\d+(\.\d+)*)$");

            if (!match.Success)
            {
                return version;
            }

            string prefix = match.Groups["prefix"].Value;
            string[] parts = match.Groups["numbers"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);
            List<int> numbers = new List<int>();

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int number))
                {
                    return version;
                }

                numbers.Add(number);
            }

            if (numbers.Count == 0)
            {
                return version;
            }

            numbers[numbers.Count - 1]++;

            return prefix + string.Join(".", numbers);
        }

        public static string GetHighestCompatibleVersion(string baseVersion, IEnumerable<string> versions)
        {
            string highest = string.Empty;

            foreach (string version in versions)
            {
                if (string.IsNullOrWhiteSpace(version))
                {
                    continue;
                }

                if (!IsCompatibleVersion(baseVersion, version))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(highest) || IsVersionGreater(version, highest))
                {
                    highest = version;
                }
            }

            return highest;
        }

        private static bool IsCompatibleVersion(string baseVersion, string version)
        {
            Match baseMatch = Regex.Match(baseVersion, @"^(?<prefix>[A-Za-z]*)(?<numbers>\d+(\.\d+)*)$");
            Match versionMatch = Regex.Match(version, @"^(?<prefix>[A-Za-z]*)(?<numbers>\d+(\.\d+)*)$");

            if (!baseMatch.Success || !versionMatch.Success)
            {
                return false;
            }

            if (!string.Equals(baseMatch.Groups["prefix"].Value, versionMatch.Groups["prefix"].Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return baseMatch.Groups["numbers"].Value.Split('.').Length == versionMatch.Groups["numbers"].Value.Split('.').Length;
        }

        private static bool IsVersionGreater(string left, string right)
        {
            int[] leftParts = ParseVersionNumbers(left);
            int[] rightParts = ParseVersionNumbers(right);

            int maxLength = Math.Max(leftParts.Length, rightParts.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int leftValue = i < leftParts.Length ? leftParts[i] : 0;
                int rightValue = i < rightParts.Length ? rightParts[i] : 0;

                if (leftValue > rightValue)
                {
                    return true;
                }

                if (leftValue < rightValue)
                {
                    return false;
                }
            }

            return false;
        }

        private static int[] ParseVersionNumbers(string version)
        {
            Match match = Regex.Match(version, @"^(?<prefix>[A-Za-z]*)(?<numbers>\d+(\.\d+)*)$");

            if (!match.Success)
            {
                return Array.Empty<int>();
            }

            return match.Groups["numbers"].Value
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => int.TryParse(part, out int number) ? number : 0)
                .ToArray();
        }

        private static string BuildDateVersion(string pattern, DateTime dateTime)
        {
            string result = string.Empty;

            for (int i = 0; i < pattern.Length;)
            {
                if (StartsWithToken(pattern, i, "yyyy"))
                {
                    result += dateTime.ToString("yyyy");
                    i += 4;
                    continue;
                }

                if (StartsWithToken(pattern, i, "yy"))
                {
                    result += dateTime.ToString("yy");
                    i += 2;
                    continue;
                }

                if (StartsWithToken(pattern, i, "dd"))
                {
                    result += dateTime.ToString("dd");
                    i += 2;
                    continue;
                }

                if (StartsWithToken(pattern, i, "hh") || StartsWithToken(pattern, i, "HH"))
                {
                    result += dateTime.ToString("HH");
                    i += 2;
                    continue;
                }

                if (StartsWithToken(pattern, i, "mm") || StartsWithToken(pattern, i, "MM"))
                {
                    bool isMinute = HasHourTokenBefore(pattern, i);
                    result += isMinute ? dateTime.ToString("mm") : dateTime.ToString("MM");
                    i += 2;
                    continue;
                }

                result += pattern[i];
                i++;
            }

            return result;
        }

        private static bool StartsWithToken(string pattern, int index, string token)
        {
            return pattern.Length >= index + token.Length &&
                   string.Compare(pattern, index, token, 0, token.Length, true) == 0;
        }

        private static bool HasHourTokenBefore(string pattern, int index)
        {
            string leftPart = pattern.Substring(0, index);
            return leftPart.Contains("hh", StringComparison.OrdinalIgnoreCase);
        }
    }
}