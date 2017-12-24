using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaBILModLoader
{
    public sealed class ModManifest
    {
        public string InternalName { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Authour { get; set; }

        public string Assembly { get; set; }

        public string AssemblyPath { get; internal set; }
    }
}
