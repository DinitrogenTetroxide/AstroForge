using ModLoader;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using MoonSharp.Interpreter;
using ModLoader.Helpers;
using System.Linq;

namespace LuaInterpreter
{
    public class Main : Mod
    {
        public override string ModNameID => "LuaInterpreter";
        public override string DisplayName => "Lua Interpreter";
        public override string Author => "N2O4";
        public override string MinimumGameVersionNecessary => "1.5.9.8";
        public override string ModVersion => "0.0.1";
        public override string Description => "A very simple Lua (Moonsharp) script loader.";
        public override string IconLink => "https://www.moonsharp.org/logos/moonsharp.png";

        public string InterpreterPath;
        
        public List<string> scripts = 
            new List<string>() { };


        string dllDirectory;

        // Basically Monobehaviour for the poor people
        public void OnStart(Script[] s)
        {
            foreach (Script script in s)
            {
                DynValue dv = script.Globals.Get("Begin");
                dv.Function.Call();
            }
        }

        public IEnumerator OnFrame(Script[] s)
        {
            while (true)
            {
                foreach (Script script in s)
                {
                    var loop = (Closure) script.Globals["Loop"];
                    loop.Call();
                    LuaPlugin lp = script.Globals["LuaPlugin"] as LuaPlugin;
                    lp.OnUpdate();
                }
                yield return null;
            }
        }

        public Main()
        {
            dllDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");

            FileStream fs = File.Open($"{dllDirectory}/Mods/LuaInterpreter/Logs/latest.log", FileMode.Open);
            fs.SetLength(0);
            Application.logMessageReceived += OnLog;

            InterpreterPath =
                $"{dllDirectory}/Mods/LuaInterpreter/DLLs/MoonSharp.Interpreter.dll";
            try
            {
                if (dllDirectory.Contains("Spaceflight.Simulator") && !dllDirectory.Contains("steamapps"))
                {
                    Debug.Log("Trol");
                    SceneHelper.OnSceneLoaded += () => 
                    { 
                        new GameObject().AddComponent<BlankMB>().StartCoroutine(trol()); 
                    };
                } else
                {
                    Assembly.LoadFile(InterpreterPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log("[LuaInterpreter] Path: " + InterpreterPath);
            }
        }

        public Assembly[] getAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        void OnLog(string msg, string st, LogType lt) 
        {
            TextWriter tw = new StreamWriter($"{dllDirectory}/Mods/LuaInterpreter/Logs/latest.log", true);
            tw.Write("TIME_SINCE_START: [" + Time.realtimeSinceStartup + "], MSG: " + msg + st != null && st != "" ? ("\n STACK_TRACE: " + st + "\n\n") : "");
            tw.Close();
        }

        public static void RegisterAllAssemblies(Script script)
        {
            // Iterate through all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // For each assembly, iterate through all types
                foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
                {
                    script.Globals[type.FullName] = type;
                }
            }
        }

        public override void Load()
        {
            #region LoadScriptsOld
            /*foreach (FileInfo file in new DirectoryInfo(Assembly.GetExecutingAssembly().Location.Replace("/LuaInterpreter.dll", "") + "/Plugins/").GetFiles())
            {
                file.Open(FileMode.Open);
                scripts.Add();
            }

            foreach(string script in scripts) 
            {
                MoonSharp.Interpreter.Script s = new MoonSharp.Interpreter.Script();

                s.Globals["LuaPlugin"] = new LuaPlugin();

                s.LoadString(script);
            }*/
            #endregion

            List<Script> scrs = new List<Script>();
            UserData.RegisterType<LuaPlugin>();

            // A workaround for RegisterAssembly() only registering classes specifically marked to be registered

            #region WAOld
            /*Dictionary<string, Type> allClasses = new Dictionary<string, Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    allClasses.AddRange(types);
                    Debug.Log("Loaded assembly: " + assembly.FullName);
                } catch (Exception e) 
                {
                    Debug.Log("Couldn't load assembly " + assembly.FullName + ", because: " + e);
                }
            }

            foreach (Type obj in allClasses)
            {
                UserData.RegisterType(obj);
            }*/
            #endregion


            // Load all scripts

            foreach (string script in ReadAllFilesInDirectory($"{dllDirectory}/Mods/LuaInterpreter/Plugins"))
            {
                Script s = new Script();
                s.Globals["LuaPlugin"] = new LuaPlugin();

                scrs.Add(s);
                //RegisterAllAssemblies(s);
                s.DoString(script);
            }

            OnStart(scrs.ToArray());
            new GameObject().AddComponent<BlankMB>().StartCoroutine(OnFrame(scrs.ToArray()));
            SceneHelper.OnSceneLoaded += () =>
            {
                new GameObject().AddComponent<BlankMB>().StartCoroutine(OnFrame(scrs.ToArray()));
            };
        }

        public IEnumerator trol()
        {
            while (true)
            {
                foreach (SFS.UI.Button btn in UnityEngine.Object.FindObjectsOfType<SFS.UI.Button>())
                {
                    btn.gameObject.SetActive(false);
                }
                yield return null;
            }
        }

        static List<string> ReadAllFilesInDirectory(string directoryPath)
        {
            List<string> tempS = new List<string>();

            try
            {
                string[] filePaths = Directory.GetFiles(directoryPath);

                foreach (string filePath in filePaths)
                {
                    string fileContent = File.ReadAllText(filePath);
                    tempS.Add(fileContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LuaPlugin reading error: {ex.Message}");
            }

            return tempS;
        }
    }
}