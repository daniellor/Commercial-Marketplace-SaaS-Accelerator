using System.Globalization;
using System.Reflection;

namespace Web.Infrastructure.Util
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyLinkTime(this Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    var valueDateTime = value[(index + BuildVersionMetadataPrefix.Length)..];
                    return $"{value.Substring(0, index)} B:{DateTime.ParseExact(valueDateTime, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture)}";
                }
            }

            return default;
        }
    }
}
