using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpness
{
    public class Enumeration
    {
        public Enumeration()
        {
            Options = new List<string>();
        }

        public string ClassName;
        public string Name;
        public List<string> Options;
    }

    public class Metadata
    {
        public Metadata()
        {
            Enums = new List<Enumeration>();
            Defines = new List<string>();
        }

        public List<Enumeration> Enums;
        public List<string> Defines;

        public string ClassName { get; set; }
    }
}
