using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using TaBILModWrapper;

namespace TaBIL
{
    public static class Program
    {
        //public static readonly string ExeFolder = @"D:\#Network-Steam\SteamRepo\steamapps\common\They Are Billions";
        public static string ExeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ExeName = @"TheyAreBillions.exe";
        public static readonly string BackupExeName = @"TheyAreBillions.bak.exe";
        public static readonly string CacheExeName = @"TheyAreBillions.cache.exe";

        public static string ExePath => Path.Combine(ExeFolder, ExeName);
        public static string BackupExePath => Path.Combine(ExeFolder, BackupExeName);
        public static string CacheExePath => Path.Combine(ExeFolder, CacheExeName);

        public static readonly string ExecuteWhenFinished = ExeName;

        public static bool BaseExeIsOrig = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("Operating in: " + ExeFolder);
            
            var exeInfo = new FileInfo(ExePath);
            var backupInfo = new FileInfo(BackupExePath);
            var cacheInfo = new FileInfo(CacheExePath);

            

            if (false)
            {
                var test = AssemblyDefinition.ReadAssembly(exeInfo.FullName);
                Console.WriteLine(test.Name.Name);
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (cacheInfo.Exists)
            {
                cacheInfo.Delete();
            }

            if (backupInfo.Exists)
            {
                Console.WriteLine("Loading backed-up game exe: " + backupInfo.FullName);
                backupInfo.CopyTo(cacheInfo.FullName, false);
            }
            else if (exeInfo.Exists)
            {
                Console.WriteLine("Loading original game exe: " + exeInfo.FullName);
                Console.Write("Press ENTER to confirm > ");
                Console.ReadLine();
                BaseExeIsOrig = true;
                exeInfo.CopyTo(cacheInfo.FullName, false);
            }
            else
            {
                ShowNotOriginalMsg();
            }

            if (!ExeIsOriginal(cacheInfo))
            {
                ShowNotOriginalMsg();
            }

            if (BaseExeIsOrig)
            {
                exeInfo.CopyTo(backupInfo.FullName, false);
            }

            var inj = new Inject();
            inj.TargAssembly = AssemblyDefinition.ReadAssembly(cacheInfo.FullName);
            inj.InjAssembly = AssemblyDefinition.ReadAssembly(typeof(Inject).Assembly.Location);

            inj.ProcessIlHooks();

            //inj.TargAssembly.MainModule.AssemblyReferences.Add(AssemblyNameReference.Parse(inj.InjAssembly.FullName));

            inj.TargAssembly.Name.Name = inj.TargAssembly.Name.Name + "_TaBIL";
            inj.TargAssembly.MainModule.Write(exeInfo.FullName);

            Console.WriteLine("Finished, executing: " + ExecuteWhenFinished);
            ProcessStartInfo psi = new ProcessStartInfo("cmd");
            psi.WorkingDirectory = ExeFolder;
            psi.Arguments = "/c \"" + ExecuteWhenFinished + "\"";
            Process.Start(psi);
        }

        /// <summary>
        /// Only call this on cached exe because it becomes read-only until program closes
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static bool ExeIsOriginal(FileInfo fi)
        {
            var ass = AssemblyDefinition.ReadAssembly(fi.FullName);
            return ass.Name.Name == "TheyAreBillions";
        }

        public static void ShowNotOriginalMsg()
        {
            Console.WriteLine("Original game exe could not be found. Please verify the integrity of your game files and try again.");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}