using PdfSharp.Pdf.IO;
using System.Reflection;
using System.Text;
using UglyToad.PdfPig;

namespace ExportParser.Common
{
    public static class InputOutput 
    {
        private static string _csvSeparator = ";";

        public static string PdfSharpReader(string filePath)
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

        public static string PdfPigReader(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            var stringBuilder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                stringBuilder.Append(page.Text);
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

        public static void ConvertPDFToCSV(Func<string, Entry[]> parser, Func<string, string> reader, string file) 
        {
            var input = new FileInfo(file);
            var allText = reader(input.FullName);
            var entries = parser(allText);
            var lines = entries.Select(s => $"{s.date:dd/MM/yyyy}{_csvSeparator}{Math.Abs(s.sum ?? 0)}{_csvSeparator}{s.addr}{_csvSeparator}{s.mcc}").ToArray();

            File.WriteAllLines(Path.Combine(input.DirectoryName, $"{Path.GetFileNameWithoutExtension(input.Name)}.csv"), lines);
        }
    }
}