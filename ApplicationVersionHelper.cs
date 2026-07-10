using System;
using System.Reflection;

namespace EasyVersionBackup
{
    public static class ApplicationVersionHelper
    {
        public static string GetApplicationVersionText()
        {
            Assembly assembly = typeof(ApplicationVersionHelper).Assembly;
            AssemblyInformationalVersionAttribute? informationalVersionAttribute =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (!string.IsNullOrWhiteSpace(informationalVersionAttribute?.InformationalVersion))
            {
                return informationalVersionAttribute.InformationalVersion.Split('+')[0];
            }

            Version? version = assembly.GetName().Version;

            if (version == null)
            {
                return "unknown";
            }

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
