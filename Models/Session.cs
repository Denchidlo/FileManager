using FileManager.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Models
{
    class Session
    {
        public string lDir { get; set; }
        public string rDir { get; set; }
        public void UpdateLog()
        {
            using (var file = File.Open(Config.sessionTarget, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(file);
                string log = XmlParser.Serialize(this);
                writer.Write(log);
                writer.Close();
            }
        }
    }
}
