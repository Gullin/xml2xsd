using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Användning: dotnet run <sökväg1> <sökväg2> ...");
            Console.WriteLine("Exempel:");
            Console.WriteLine("  dotnet run ./xmlfiles ./enfil.xml C:/temp/data");
            return;
        }

        try
        {
            // Hämta alla XML-filer baserat på input
            var xmlFiles = ResolveXmlFiles(args);

            if (!xmlFiles.Any())
            {
                Console.WriteLine("Inga XML-filer hittades.");
                return;
            }

            Console.WriteLine("Följande XML-filer används:");
            foreach (var f in xmlFiles)
                Console.WriteLine(" - " + f);

            // Skapa schema
            XmlSchemaInference inference = new XmlSchemaInference();
            XmlSchemaSet schemaSet = null;

            foreach (string xmlFile in xmlFiles)
            {
                using var reader = XmlReader.Create(xmlFile);
                schemaSet = schemaSet == null
                    ? inference.InferSchema(reader)
                    : inference.InferSchema(reader, schemaSet);
            }

            // Spara resultat
            string outputFile = Path.Combine(Environment.CurrentDirectory, "combined_schema.xsd");

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                using var writer = XmlWriter.Create(outputFile, new XmlWriterSettings { Indent = true });
                schema.Write(writer);
            }

            Console.WriteLine($"\n✅ XSD-schema genererat: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Fel: " + ex.Message);
        }
    }

    /// <summary>
    /// Tar emot sökvägar (filer/mappar, absoluta/relativa) och returnerar en lista med XML-filer.
    /// </summary>
    static IEnumerable<string> ResolveXmlFiles(string[] inputs)
    {
        List<string> files = new();

        foreach (var input in inputs)
        {
            string path = Path.GetFullPath(input);

            if (File.Exists(path) && path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(path);
            }
            else if (Directory.Exists(path))
            {
                files.AddRange(Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories));
            }
            else
            {
                Console.WriteLine($"⚠️ Hoppade över: {input} (ingen giltig fil eller katalog)");
            }
        }

        // Ta bort dubbletter och sortera
        return files.Distinct().OrderBy(f => f).ToList();
    }
}