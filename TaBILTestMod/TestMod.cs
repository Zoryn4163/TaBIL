using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaBILModLoader;

namespace TaBILTestMod
{
    public class TestMod : ModBase
    {
        public override void Init()
        {
            Log.Out("We loaded a test mod!");
        }

        public override void OnGameLoaded()
        {
            Log.Out("OnGameLoaded function for test mod!");
        }
    }
}
