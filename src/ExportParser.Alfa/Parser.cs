using ExportParser.Common;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExportParser.Alfa
{
    public static class Parser
    {
        private static string _pageHeader = "Дата проводки Код операции Описание Сумма  в валюте счета";
        private static Regex _alfaLine = new Regex(@"(?<sum>-[0-9 ]+,\d{2} RUR)");
        private static Regex _dateLine = new Regex(@"(?<date>\d{2}\.\d{2}\.\d{2,4}),");
        private static Regex _dateLine2 = new Regex(@"(?<date>\d{2}\.\d{2}\.\d{4})");
        private static Regex _sumLine = new Regex(@"(?<sum>-(\s*[0-9]+)+\,\d{2}) RUR$");
        private static Regex _descrLine = new Regex(@"место совершения операции:\s+(?<addr>\d*.*)MCC(?<mcc>\d{4})");
        private static Regex _textLine = new Regex("\"(?<mcc>\\d{4})\",\"(?<text>.*)\",\"");

        private static string MatchOr(this Regex first, Regex second, string value)
        {
            var firstMatch = first.Match(value);
            if (firstMatch.Success) 
            {
                return firstMatch.Groups["date"].Value;
            }

            return second.Match(value).Groups["date"].Value;
        }

        private static string TrimStartByText(this string input, string text)
        {
            var splitInput = input.Split(text);
            if (splitInput.Length == 1)
            {
                return input;
            }

            return string.Join(text, splitInput.Skip(1));
        }

        private static string[] ReadAllLines(string rawText)
        {
            var lines = _alfaLine.Split(rawText);
            return lines
                .Select((x, i) => $"{x}{lines.ElementAtOrDefault(i + 1)}")
                .Where((_, i) => i % 2 == 0)
                .Select(r => r.TrimStartByText(_pageHeader))
                .ToArray();
        }

        private static DateTime? TryDate(string source) => DateTime.TryParse(source, out DateTime date) ? date : new DateTime?();

        private static decimal? TrySum(string source) => decimal.TryParse(source, out decimal sum) ? sum : new decimal?();

        public static Entry[] Parse(string allText)
        {
            var lines = ReadAllLines(allText);

            return lines
                .Select(s =>
                (
                    date: _dateLine.MatchOr(_dateLine2, s),
                    sum: _sumLine.Match(s).Groups["sum"].Value,
                    addr: _descrLine.Match(s).Groups["addr"].Value,
                    mcc: _descrLine.Match(s).Groups["mcc"].Value)
                )
                .Select(s => new Entry(TryDate(s.date), TrySum(s.sum), s.addr, MCC.Codes.TryGet(s.mcc), Bank.Alfa))
                .OrderBy(s => s.date)
                .ThenBy(s => s.sum)
                .ToArray();
        }
    }
}
