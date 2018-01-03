using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TaBILModWrapper
{
    public class Inject
    {
        public AssemblyDefinition InjAssembly = null;
        public AssemblyDefinition TargAssembly = null;
        
        public void ProcessIlHooks()
        {
            Console.WriteLine("Performing IL Hooks!");
            PublicizeTypes();
            IlApi();
            IlOnGameLoaded();
            //IlOnScreenUpdate();
            //IlOnGameUpdate();
            IlOnLevelUpdate();
        }

        private void PublicizeTypes()
        {
            foreach (var t in TargAssembly.MainModule.Types)
            {
                if (!t.IsPublic)
                    t.IsPublic = true;
            }
        }

        private void IlApi()
        {
            var initClassType = InjAssembly.MainModule.GetType("TaBILModWrapper.InjectMethods");
            var initMethod = initClassType.Methods.Single(x => x.Name == "InitializeApi");
            var impMethod = TargAssembly.MainModule.ImportReference(initMethod);

            var il = TargAssembly.MainModule.EntryPoint.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();
            il.InsertBefore(fi, il.Create(OpCodes.Call, impMethod));
        }

        private void IlOnGameLoaded()
        {
            var initClassType = InjAssembly.MainModule.GetType("TaBILModWrapper.InjectMethods");
            var initMethod = initClassType.Methods.Single(x => x.Name == "OnGameLoaded");
            var impMethod = TargAssembly.MainModule.ImportReference(initMethod);

            var tgt = TargAssembly.MainModule.GetType("ZX.ZXGame");
            var tgm = tgt.Methods.Single(x => x.Name == "OnLoad");

            var il = tgm.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();
            il.InsertBefore(fi, il.Create(OpCodes.Call, impMethod));
        }

        private void IlOnScreenUpdate()
        {
            var initClassType = InjAssembly.MainModule.GetType("TaBILModWrapper.InjectMethods");
            var initMethod = initClassType.Methods.Single(x => x.Name == "OnScreenUpdate");
            var impMethod = TargAssembly.MainModule.ImportReference(initMethod);

            var tgt = TargAssembly.MainModule.GetType("ZX.ZXGame");
            var tgm = tgt.Methods.Single(x => x.Name == "OnStartFrame");

            var il = tgm.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();
            il.InsertBefore(fi, il.Create(OpCodes.Call, impMethod));
        }

        private void IlOnGameUpdate()
        {
            var initClassType = InjAssembly.MainModule.GetType("TaBILModWrapper.InjectMethods");
            var initMethod = initClassType.Methods.Single(x => x.Name == "OnGameUpdate");
            var impMethod = TargAssembly.MainModule.ImportReference(initMethod);

            var tgt = TargAssembly.MainModule.GetType("ZX.ZXSystem_GameLevel");
            var tgm = tgt.Methods.Single(x => x.Name == "OnStartUpdate");

            var il = tgm.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();
            il.InsertBefore(fi, il.Create(OpCodes.Call, impMethod));
        }

        private void IlOnLevelUpdate()
        {
            var initClassType = InjAssembly.MainModule.GetType("TaBILModWrapper.InjectMethods");
            var initMethod = initClassType.Methods.Single(x => x.Name == "OnLevelUpdate");
            var impMethod = TargAssembly.MainModule.ImportReference(initMethod);

            var tgt = TargAssembly.MainModule.GetType("ZX.ZXLevelState");
            var tgm = tgt.Methods.Single(x => x.Name == "OnUpdate");

            var il = tgm.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();
            il.InsertBefore(fi, il.Create(OpCodes.Call, impMethod));
        }

        public void IlThrowOnExecution()
        {
            var exr = typeof(Exception);
            var exc = exr.GetConstructor(new Type[] { });

            var cr = TargAssembly.MainModule.ImportReference(exc);

            var il = TargAssembly.MainModule.EntryPoint.Body.GetILProcessor();
            var fi = il.Body.Instructions.First();

            il.InsertBefore(fi, il.Create(OpCodes.Newobj, cr));
            il.InsertBefore(fi, il.Create(OpCodes.Throw));
        }
    }

    public static class InjectMethods
    {
        public static void InitializeApi()
        {
            TaBILModLoader.ModLoader.Initialize();
        }

        public static void OnGameLoaded()
        {
            TaBILModLoader.ModLoader.OnGameLoaded();
        }

        public static void OnScreenUpdate()
        {
            TaBILModLoader.ModLoader.OnScreenUpdate();
        }

        public static void OnGameUpdate()
        {
            TaBILModLoader.ModLoader.OnGameUpdate();
        }

        public static void OnLevelUpdate()
        {
            TaBILModLoader.ModLoader.OnLevelUpdate();
        }
    }
}
