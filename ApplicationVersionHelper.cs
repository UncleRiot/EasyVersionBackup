using System;
using System.Reflection;

// Semantic Anchor [ApplicationVersionHelper.cs]: Application version display uses the assembly/project version as its source of truth; do not introduce independent version literals.
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
