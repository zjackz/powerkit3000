using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GenerateDuplicateJson
{
    public static void Main(string[] args)
    {
        string sourceFilePath = "/home/dministrator/code/HappyTools/consoleapp/data/sample2_part1.json";
        string destFilePath = "/home/dministrator/code/HappyTools/consoleapp/data/test_duplicate.json";

        try
        {
            var allJsonObjects = new List<JObject>();
            using (StreamReader file = File.OpenText(sourceFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                reader.SupportMultipleContent = true;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                        allJsonObjects.Add(jsonObject);
                    }
                }
            }

            if (allJsonObjects.Any())
            {
                // Duplicate the first object
                JObject firstObject = allJsonObjects.First();
                allJsonObjects.Insert(0, firstObject); // Insert at the beginning to ensure it's processed early
            }

            using (StreamWriter file = File.CreateText(destFilePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.None; // Keep it compact
                foreach (var obj in allJsonObjects)
                {
                    obj.WriteTo(writer);
                    writer.WriteRaw(Environment.NewLine); // Ensure each object is on a new line
                }
            }
            Console.WriteLine($"Successfully generated {destFilePath} with duplicate entry.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating JSON file: {ex.Message}");
        }
    }
}
