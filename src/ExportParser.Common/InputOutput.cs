using PdfSharp.Pdf.IO;
using System.Reflection;
using System.Text;

namespace ExportParser.Common
{
    public static class InputOutput 
    {
        private static string _csvSeparator = ",";

        private static string ReadText(string filePath)
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

        public static string ReadManifestData(string embeddedFileName)
        {
            var assembly = typeof(MCC).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static void ConvertPDFToCSV(Func<string, Entry[]> parser, string file) 
        {
            var input = new FileInfo(file);
            var allText = ReadText(input.FullName);
            var entries = parser(allText);
            var lines = entries.Select(s => $"{s.date:dd/MM/yyyy}{_csvSeparator}{Math.Abs(s.sum ?? 0)}{_csvSeparator}{s.addr}{_csvSeparator}{s.mcc}").ToArray();

            File.WriteAllLines(Path.Combine(input.DirectoryName, $"{Path.GetFileNameWithoutExtension(input.Name)}.csv"), lines);
        }
    }
}