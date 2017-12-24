using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DXVision;
using Newtonsoft.Json;

namespace TaBILModLoader
{
    public static class ModLoader
    {
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        public static readonly string ExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ModDir = Path.Combine(ExeDir, "mods");
        public static int ModsLoaded { get; private set; }
        public static ModBase[] Mods { get; private set; }
        public static List<ModBase> ModsToRemove { get; private set; }

        public static DXGame DxGame { get; private set; }

        public static void TestReeeeeeeeeeee()
        {
            string p = @"E:\test\" + Path.GetTempFileName().Split('\\').Last();
            Directory.CreateDirectory(p);
        }

        public static void Initialize()
        {
            string lp = Path.Combine(ExeDir, "TaBIL_Log.latest.txt");
            Log.Initialize(lp);

            Log.Out("TaBILModLoader Initialized");
            Log.Out("Executing in: " + ExeDir);
            Log.Out("Checking for mods in: " + ModDir);

            var modIdx = IndexMods();
            Mods = LoadMods(modIdx);

            Log.Out($"Loaded {ModsLoaded} mods.");
            Log.Out("Calling mod Init functions...");
            foreach (var mod in Mods)
            {
                try
                {
                    Log.Out("Init for: " + mod.Manifest.InternalName);
                    mod.Init();
                }
                catch (Exception ex)
                {
                    Log.Out($"[{mod.Manifest.InternalName}] encountered an error in its OnGameLoaded function.");
                }
            }
        }

        public static void OnGameLoaded()
        {
            Log.Out("Game OnLoad Triggered");
            Log.Out("Renaming main game window...");

            try
            {
                Log.Out("Getting ZXGame from assembly...");
                var zgt = Assembly.GetEntryAssembly().GetType("ZX.ZXGame");
                var zgp = zgt.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
                var zgv = zgp.GetValue(null);

                DxGame = (DXGame) zgv;

                Log.Out("Setting modified window title...");
                SetWindowText(DxGame.Platform.WindowHandle, $"{DxGame.Name} (TaBIL)");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }

            Log.Out("Calling mod OnGameLoaded functions...");
            foreach (var mod in Mods)
            {
                try
                {
                    Log.Out("OnGameLoaded for: " + mod.Manifest.InternalName);
                    mod.OnGameLoaded();
                }
                catch (Exception ex)
                {
                    Log.Out($"[{mod.Manifest.InternalName}] encountered an error in its OnGameLoaded function.");
                }
            }
        }

        public static void OnUpdate()
        {
            if (Mods == null)
                Mods = new ModBase[0];

            if (ModsToRemove == null)
                ModsToRemove = new List<ModBase>();

            if (ModsToRemove.Any())
            {
                List<ModBase> mods = Mods.ToList();
                foreach (var mod in ModsToRemove)
                {
                    if (mods.Remove(mod))
                    {
                        Log.Out($"Removed [{mod.Manifest.InternalName}] from the update pool.");
                    }
                }
                Mods = mods.ToArray();
            }

            foreach (var mod in Mods)
            {
                try
                {
                    mod.OnUpdate();
                }
                catch (Exception ex)
                {
                    Log.Out($"[{mod.Manifest.InternalName}] encountered an error in its Update function.");
                    ModsToRemove.Add(mod);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static ModManifest[] IndexMods()
        {
            if (!Directory.Exists(ModDir))
                Directory.CreateDirectory(ModDir);

            List<ModManifest> ret = new List<ModManifest>();

            string[] modDirs = Directory.GetDirectories(ModDir);
            foreach (var dir in modDirs)
            {
                string modManifestPath = Path.Combine(dir, "manifest.json");
                FileInfo manifestInfo = new FileInfo(modManifestPath);
                if (!manifestInfo.Exists)
                    continue;

                ModManifest manifest = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(manifestInfo.FullName));
                if (manifest == null)
                    continue;

                manifest.AssemblyPath = Path.Combine(Path.GetDirectoryName(manifestInfo.FullName), manifest.Assembly);

                if (!File.Exists(manifest.AssemblyPath))
                    continue;

                ret.Add(manifest);
            }

            return ret.OrderBy(x=>x.Assembly).ToArray();
        }

        public static ModBase[] LoadMods(ModManifest[] manifests)
        {
            List<ModBase> loadedMods = new List<ModBase>();
            foreach (var manifest in manifests)
            {
                var ass = Assembly.UnsafeLoadFrom(manifest.AssemblyPath);
                if (ass.DefinedTypes.Any(x=>x.BaseType == typeof(ModBase)))
                {
                    Log.Out($"Loading mod: {manifest.Name} {manifest.Version} [{manifest.InternalName}]");
                    var tar = ass.DefinedTypes.First(x => x.BaseType == typeof(ModBase));
                    var m = (ModBase)ass.CreateInstance(tar.ToString());

                    if (m == null)
                    {
                        Log.Out("Failed to create instance of: " + tar.ToString());
                        continue;
                    }

                    m.Manifest = manifest;

                    loadedMods.Add(m);
                    ModsLoaded += 1;
                    Log.Out("Mod loaded successfully.");
                }
            }

            return loadedMods.ToArray();
        }
    }
}
