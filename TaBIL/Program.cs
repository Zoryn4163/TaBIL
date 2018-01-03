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
                var ld = Path.Combine(ExeFolder, "TheyAreBillions.bak.exe");
                var ver = GetAssemblyInfo(ld);
                var ver2 = Assembly.UnsafeLoadFrom(ld);
                Console.WriteLine(ver);
                Console.WriteLine(ver2);
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (cacheInfo.Exists)
            {
                cacheInfo.Delete();
            }

            if (backupInfo.Exists && exeInfo.Exists)
            {
                Console.WriteLine("Backup and Original found, checking versions...");

                var einfo = GetAssemblyInfo(exeInfo.FullName);
                var binfo = GetAssemblyInfo(backupInfo.FullName);

                Console.WriteLine($"Original: [{einfo.Version}] | Backup: [{binfo.Version}]");

                if (einfo.Version > binfo.Version)
                {
                    Console.WriteLine("Original game exe more updated than backup, will replace backup with: " + exeInfo.FullName);
                    Console.Write("Press Y to confirm > ");
                    var k = Console.ReadKey();
                    if (k.Key != ConsoleKey.Y)
                    {
                        Console.WriteLine("\n'Y' was not pressed. The program will now exit.");
                        Environment.Exit(0);
                    }
                    BaseExeIsOrig = true;
                    Console.WriteLine("\nRemoving backup exe...");
                    backupInfo.Delete();
                    Console.WriteLine("Copying original exe to cache...");
                    exeInfo.CopyTo(cacheInfo.FullName, false);
                }
                else
                {
                    Console.WriteLine("Loading backed-up game exe: " + backupInfo.FullName);
                    Console.WriteLine("Copying backup exe to cache...");
                    backupInfo.CopyTo(cacheInfo.FullName, false);
                }
            }
            else if (backupInfo.Exists)
            {
                Console.WriteLine("Loading backed-up game exe: " + backupInfo.FullName);
                backupInfo.CopyTo(cacheInfo.FullName, false);
            }
            else if (exeInfo.Exists)
            {
                Console.WriteLine("Loading original game exe: " + exeInfo.FullName);
                Console.Write("Press Y to confirm > ");
                var k = Console.ReadKey();
                if (k.Key != ConsoleKey.Y)
                {
                    Console.WriteLine("\n'Y' was not pressed. The program will now exit.");
                    Environment.Exit(0);
                }
                BaseExeIsOrig = true;
                Console.WriteLine("\nRemoving backup exe...");
                backupInfo.Delete();
                Console.WriteLine("Copying original exe to cache...");
                exeInfo.CopyTo(cacheInfo.FullName, false);
            }
            else
            {
                ShowNotOriginalMsg(new AssemblyInfo());
            }

            Console.WriteLine("Testing cached exe for original assembly definition...");
            var cai = GetAssemblyInfo(cacheInfo.FullName);
            if (!ExeIsOriginal(cai))
            {
                ShowNotOriginalMsg(cai);
            }

            if (BaseExeIsOrig)
            {
                Console.WriteLine("Copying cache to original exe...");
                exeInfo.CopyTo(backupInfo.FullName, false);
            }

            Console.WriteLine("Reading cached exe for injection...");
            var inj = new Inject();
            inj.TargAssembly = AssemblyDefinition.ReadAssembly(cacheInfo.FullName);
            inj.InjAssembly = AssemblyDefinition.ReadAssembly(typeof(Inject).Assembly.Location);

            Console.WriteLine("Processing injections...");
            inj.ProcessIlHooks();

            //inj.TargAssembly.MainModule.AssemblyReferences.Add(AssemblyNameReference.Parse(inj.InjAssembly.FullName));

            Console.WriteLine("Rewriting assembly definition to modified definition...");
            inj.TargAssembly.Name.Name = inj.TargAssembly.Name.Name + "_TaBIL";

            Console.WriteLine("Writing out modified exe to original exe...");
            inj.TargAssembly.MainModule.Write(exeInfo.FullName);

            Console.WriteLine("Finished, executing: " + ExecuteWhenFinished);
            ProcessStartInfo psi = new ProcessStartInfo("cmd");
            psi.WorkingDirectory = ExeFolder;
            psi.Arguments = "/c \"" + ExecuteWhenFinished + "\"";
            Process.Start(psi);
        }

        public static AssemblyInfo GetAssemblyInfo(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine("Error: File not found: " + assemblyPath);
                Console.WriteLine("The program cannot continue. Press any key to throw the exception.");
                Console.ReadKey();
                throw new FileNotFoundException(assemblyPath);
            }

            AppDomain d = AppDomain.CreateDomain("GetVersion", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);

            AssemblyInfo ret = new AssemblyInfo();

            try
            {
                Type t = typeof(Proxy);
                var v = (Proxy) d.CreateInstanceAndUnwrap(t.Assembly.FullName, t.FullName);
                ret = v.GetAssemblyInfo(assemblyPath);
                Console.WriteLine($"Retreived assembly info for [{assemblyPath}]: " + ret.FullName);
            }
            finally
            {
                AppDomain.Unload(d);
            }

            return ret;
        }

        public static bool ExeIsOriginal(AssemblyInfo ai)
        {
            return ai.DisplayName == "TheyAreBillions";
        }

        public static void ShowNotOriginalMsg(AssemblyInfo ai)
        {
            Console.WriteLine("Original game exe could not be found. Please verify the integrity of your game files and try again.");

            if (!ai.Equals(new AssemblyInfo()))
                Console.WriteLine($"Expected: 'TheyAreBillions' | Got: '{ai.DisplayName}'");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }

    public class Proxy : MarshalByRefObject
    {
        public AssemblyInfo GetAssemblyInfo(string assemblyPath)
        {
            try
            {
                //Console.WriteLine("\n\nLoading assembly: " + assemblyPath);
                var a = Assembly.UnsafeLoadFrom(assemblyPath);
                //Console.WriteLine("Assembly is: " + a.FullName);

                var ret = new AssemblyInfo();
                ret.FullName = a.FullName;
                ret.Version = a.GetName().Version;
                ret.DisplayName = a.GetName().Name;
                ret.Location = a.Location;
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new AssemblyInfo();
                // throw new InvalidOperationException(ex);
            }
        }
    }

    [Serializable]
    public struct AssemblyInfo
    {
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Location { get; set; }
        public Version Version { get; set; }

        public override string ToString()
        {
            return $"{FullName} [{Location}]";
        }
    }
}