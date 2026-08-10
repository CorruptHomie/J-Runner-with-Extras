
namespace JRunner
{
    public static class StaticVersion
    {
        public const string Ver = variables.staticversion;

        // Ver used to always be 4 plain numeric dot-separated parts (e.g. "1.0.0.0"), so
        // this class just did Ver.Split('.') + int.Parse on each piece. Now that
        // variables.staticversion carries a real pre-release tag (e.g. "4.0.0pre2"),
        // that would throw a FormatException on "pre1" the moment this class is touched.
        // Match the numeric major.minor.build core with a regex instead, and keep
        // whatever trailing text follows it (if any) as a separate string, so both old
        // plain-numeric strings and new tagged ones parse without crashing.
        private static readonly System.Text.RegularExpressions.Match _match =
            System.Text.RegularExpressions.Regex.Match(Ver, @"^(\d+)\.(\d+)\.(\d+)(.*)$");

        public static int Betaversion = _match.Success ? int.Parse(_match.Groups[1].Value) : 0;
        public static int version = _match.Success ? int.Parse(_match.Groups[2].Value) : 0;
        public static int Build = _match.Success ? int.Parse(_match.Groups[3].Value) : 0;

        // Free-form qualifier after the numeric core - "pre1", "-rc2", "" for a final
        // release, etc. Replaces the old 4th numeric "Dev" component, which a pre-release
        // tag doesn't fit into.
        public static string PreRelease = _match.Success ? _match.Groups[4].Value.TrimStart('-', '.', ' ') : "";

        // Kept so any old code still expecting a 4-element array doesn't break; the 4th
        // element is now the pre-release tag instead of a numeric dev build count.
        public static string[] versions = { Betaversion.ToString(), version.ToString(), Build.ToString(), PreRelease };
    }
}
