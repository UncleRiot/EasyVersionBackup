using System;

namespace EasyVersionBackup
{
    public static class AutoBackupIntervalHelper
    {
        public static string Format(int seconds)
        {
            int safeSeconds = Math.Max(0, seconds);

            if (safeSeconds > 0 && safeSeconds % 3600 == 0)
            {
                return (safeSeconds / 3600).ToString() + "h";
            }

            if (safeSeconds > 0 && safeSeconds % 60 == 0)
            {
                return (safeSeconds / 60).ToString() + "m";
            }

            return safeSeconds.ToString() + "s";
        }

        public static bool TryParseSeconds(string value, out int seconds)
        {
            seconds = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalizedValue = value.Trim().ToLowerInvariant();
            long multiplier = 60;
            string numberText = normalizedValue;

            if (normalizedValue.EndsWith("s", StringComparison.Ordinal))
            {
                multiplier = 1;
                numberText = normalizedValue[..^1];
            }
            else if (normalizedValue.EndsWith("m", StringComparison.Ordinal))
            {
                multiplier = 60;
                numberText = normalizedValue[..^1];
            }
            else if (normalizedValue.EndsWith("h", StringComparison.Ordinal))
            {
                multiplier = 3600;
                numberText = normalizedValue[..^1];
            }

            if (!long.TryParse(numberText, out long valueNumber) || valueNumber < 1)
            {
                return false;
            }

            long calculatedSeconds = valueNumber * multiplier;

            if (calculatedSeconds > int.MaxValue)
            {
                return false;
            }

            seconds = (int)calculatedSeconds;
            return true;
        }

        public static int ParseSecondsOrDefault(string value, int defaultSeconds)
        {
            return TryParseSeconds(value, out int seconds)
                ? seconds
                : Math.Max(1, defaultSeconds);
        }
    }
}
