using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace TaBILTest
{
    public static class Program
    {
        public static string ExeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ExeName = @"TheyAreBillions.exe";
        public static readonly string OutExeName = @"TheyAreBillions.test.exe";
        public static string ExePath => Path.Combine(ExeFolder, ExeName);
        public static string OutExePath => Path.Combine(ExeFolder, OutExeName);

        static void Main(string[] args)
        {
            Console.WriteLine("This program simply takes in the TheyAreBillions.exe and spits it back out after being injested by Mono.Cecil.");
            Console.WriteLine("This program does not perform any modding hooks or IL rewrites and should not affect the game at all.");
            Console.WriteLine();

            var exeInfo = new FileInfo(ExePath);
            var outInfo = new FileInfo(OutExePath);

            if (outInfo.Exists)
            {
                Console.WriteLine("Deleting output: " + outInfo.FullName);
                outInfo.Delete();
            }

            Console.WriteLine("Reading in exe: " + exeInfo.FullName);
            var exeIn = AssemblyDefinition.ReadAssembly(exeInfo.FullName);

            Console.WriteLine("Writing out exe: " + outInfo.FullName);
            exeIn.Write(outInfo.FullName);

            Console.WriteLine("Finished");
        }
    }
}
