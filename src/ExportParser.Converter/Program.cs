﻿using ExportParser.Common;

namespace ExportParser.Converter
{

    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello!");

           // InputOutput.ConvertPDFToCSV(Alfa.Parser.Parse, InputOutput.PdfSharpReader, "D:\\учет\\document09.01.23 191216.654.pdf");
            InputOutput.ConvertPDFToCSV(Tinkoff.Parser.Parse, InputOutput.PdfPigReader, "D:\\учет\\5-3DD8KQ9GN_20230109.pdf");

            Console.WriteLine("Hello, World!");
        }
    }
}