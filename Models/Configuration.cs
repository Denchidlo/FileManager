using FileManager.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Models
{
    class Configuration
    {
        public string SessionConnectionString { get; set; }

        public static Configuration GetConfiguration(string src)
        {
            using (var file = File.Open(src, FileMode.Open))
            {
                StreamReader reader = new StreamReader(file);
                string xml = reader.ReadToEnd();
                reader.Close();
                Configuration configuration = JsonParser.Deserialize<Configuration>(xml);
                    return configuration;
            }
        }

        public static void SetConfiguration(string src, Configuration configuration)
        {
            using (var file = File.Open(src, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(file);
                string xml = JsonParser.Serialize(configuration);
                writer.Write(xml);
                writer.Close();
            }
        }
    }
}