using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExportParser.Alfa
{
    public static class Program
    {
        public static string _pageHeader = "Дата проводки Код операции Описание Сумма  в валюте счета";

        public static Regex _alfaLine = new Regex(@"(?<sum>-[0-9 ]+,\d{2} RUR)");

        public static Regex _dateLine = new Regex(@"(?<date>\d{2}\.\d{2}\.\d{2,4}),");

        public static Regex _dateLine2 = new Regex(@"(?<date>\d{2}\.\d{2}\.\d{4})");

        public static Regex _sumLine = new Regex(@"(?<sum>-(\s*[0-9]+)+\,\d{2}) RUR$");
        
        public static Regex _descrLine = new Regex(@"место совершения операции:\s+(?<addr>\d*.*)MCC(?<mcc>\d{4})");
        
        public static Regex _textLine = new Regex("\"(?<mcc>\\d{4})\",\"(?<text>.*)\",\"");

        public static Dictionary<string, string> mccCode = ReadManifestData("mcc_codes.csv")
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (mcc: _textLine.Match(s).Groups["mcc"].Value, text: _textLine.Match(s).Groups["text"].Value))
                .Where(s => !string.IsNullOrEmpty(s.mcc))
                .ToDictionary(kv => kv.mcc, kv => kv.text);

        public static string ReadManifestData(string embeddedFileName)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
         
            return reader.ReadToEnd();
        }

        public static string MatchOr(this Regex first, Regex second, string value)
        {
            var firstMatch = first.Match(value);
            if (firstMatch.Success) 
            {
                return firstMatch.Groups["date"].Value;
            }

            return second.Match(value).Groups["date"].Value;
        }

        public static string TrimStartByText(this string input, string text)
        {
            var splitInput = input.Split(text);
            if (splitInput.Length == 1)
            {
                return input;
            }

            return string.Join(text, splitInput.Skip(1));
        }
       
        public static string ReadText(string filePath)
        {
            using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
            using var extractor = new PdfSharpTextExtractor.Extractor(document);
            var stringBuilder = new StringBuilder();
            foreach (var page in document.Pages)
            {
                extractor.ExtractText(page, stringBuilder);
            }

            return stringBuilder.ToString();
        }

        public static string[] ReadAllLines(string rawText)
        {
            var lines = _alfaLine.Split(rawText);
            return lines
                .Select((x, i) => $"{x}{lines.ElementAtOrDefault(i + 1)}")
                .Where((_, i) => i % 2 == 0)
                .Select(r => r.TrimStartByText(_pageHeader))
                .ToArray();
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key) => source.TryGetValue(key, out TValue value) ? value : default(TValue);

        public static DateTime? TryDate(string source) => DateTime.TryParse(source, out DateTime date) ? date : new DateTime?();

        public static decimal? TrySum(string source) => decimal.TryParse(source, out decimal sum) ? sum : new decimal?();

        static void Main(string[] args)
        {
            var inputPdf = new FileInfo("D:\\учет\\document09.01.23 191216.654.pdf");
            var allText = ReadText(inputPdf.FullName);
            var lines = ReadAllLines(allText);

            var parsedlines = lines                
                .Select(s => 
                (
                    date: _dateLine.MatchOr(_dateLine2, s), 
                    sum: _sumLine.Match(s).Groups["sum"].Value, 
                    addr: _descrLine.Match(s).Groups["addr"].Value, 
                    mcc: _descrLine.Match(s).Groups["mcc"].Value)
                )
                .Select(s => (date: TryDate(s.date), sum: TrySum(s.sum), s.addr, mcc: mccCode.TryGet(s.mcc)))
                .OrderBy(s => s.date)
                .ThenBy(s => s.sum)
                .Select(s => $"{s.date:dd/MM/yyyy};{Math.Abs(s.sum ?? 0)};{s.addr};{s.mcc}")
                .ToArray();

            File.WriteAllLines(Path.Combine(inputPdf.DirectoryName, $"{Path.GetFileNameWithoutExtension(inputPdf.Name)}.csv"), parsedlines);
        }
    }
}
