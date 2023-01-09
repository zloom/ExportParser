using PdfSharp.Pdf.IO;
using System.Reflection;
using System.Text;

namespace ExportParser.Common
{
    public static class InputOutput 
    {
        public static string ReadManifestData(string embeddedFileName)
        {
            var assembly = typeof(MCC).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
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

    }
}