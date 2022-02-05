using System.Reflection;

namespace RouteDokan.Passport {
    internal class DokanGuidance {
        internal static readonly string MessageHeader =
            "Route Dokan Passport v" +
            Assembly.GetExecutingAssembly().GetName().Version.ToString() +
            " \u00A9 Jenrikku (JkKU)\n";

        internal static readonly string ParametersList =
            "\n\nList of parameters (case-sensitive):\n" +
            GenerateParam();

        internal static readonly string SetArgument =
            "-s / --set <parameter> [content]" +
            "\nModifies the configuration." +
            ParametersList;

        internal static readonly string DefaultHelp =
            "\n--set\n\n" +
            "Use --help with one of the above arguments to check its usage.";

        protected static string GenerateParam() {
            string result = string.Empty;

            foreach(string value in DokanConfiguration.AvailableKeys)
                result += value + ",\n";

            return result.Remove(result.Length - 2);
        }
    }
}
