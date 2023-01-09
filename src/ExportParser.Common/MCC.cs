using System.Text.RegularExpressions;

namespace ExportParser.Common
{
    public static class MCC
    {
        private static Regex _textLine = new Regex("\"(?<mcc>\\d{4})\",\"(?<text>.*)\",\"");

        public static Dictionary<string, string> MccCode = InputOutput.ReadManifestData("mcc_codes.csv")
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (mcc: _textLine.Match(s).Groups["mcc"].Value, text: _textLine.Match(s).Groups["text"].Value))
                .Where(s => !string.IsNullOrEmpty(s.mcc))
                .ToDictionary(kv => kv.mcc, kv => kv.text);
    }
}