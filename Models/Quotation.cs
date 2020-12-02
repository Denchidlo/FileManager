using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    public class Quotation_
    {
        public string Str;

        public string Author;

        public Quotation_(string author, string str)
        {
            this.Author = author;
            this.Str = str;
        }

        public override string ToString()
        {
            return this.Str + "\n\t" + this.Author;
        }
    }
}
