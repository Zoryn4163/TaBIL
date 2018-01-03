using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaBILModLoader
{
    public abstract class ModBase
    {
        public ModManifest Manifest { get; set; }

        public virtual void Init() { }

        public virtual void OnGameLoaded() { }

        public virtual void OnScreenUpdate() { }

        public virtual void OnGameUpdate() { }
        public virtual void OnLevelUpdate() { }

        public virtual void Nothing() { }
    }
}
