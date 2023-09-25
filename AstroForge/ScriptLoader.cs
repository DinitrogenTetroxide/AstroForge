using ModLoader.Helpers;
using NLua;
using SFS.Parts;
using SFS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using AstroForge.CustomParts;
using System.Reflection;
using SFS;

namespace AstroForge
{
    public class ScriptLoader
    {
        public void LoadPlugins() 
        {
            var a = SceneManager.GetActiveScene().name;
            List<Lua> scrs = new List<Lua>();

            // Load all scripts

            foreach (string script in Utility.ReadAllFilesInDirectory($"{Main.dllDirectory}/Mods/AstroForge/Plugins"))
            {
                Lua s = new Lua();
                s["LuaPlugin"] = new LuaPlugin();
                s["this"] = s;
                s.LoadCLRPackage();
                s.DoString(script);

                scrs.Add(s);
            }

            Functions.OnStart(scrs.ToArray());
            new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray()));
            SceneHelper.OnSceneLoaded += () =>
            {
                new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray()));
            };
        }

        public void LoadCustomParts()
        {
            try
            {
                AstroForgeInjector injector;

                foreach (Part part in Base.partsLoader.parts.Values)
                {
                    injector = part.GetComponent<AstroForgeInjector>();

                    if (injector)
                    {
                        var scr = part.GetOrAddComponent<AstroForgeScript>();

                        scr.loadFrom = injector.scriptContent;
                        scr.vars = (injector.variables == null ? new SerializableDictionary<string, UnityEngine.Object>() : injector.variables);

                        injector.astroForgeScript = scr;
                    }
                }
            } catch (Exception) { } // In case there are no plugins with AstroForgeUnity assembly loaded
        }

        public class Functions
        {
            public static void OnStart(Lua[] s)
            {
                foreach (Lua script in s)
                {
                    
                    var plugin = script["LuaPlugin"] as LuaPlugin;
                    plugin.Init();
                    var bgn = script["Begin"] as LuaFunction;
                    bgn.Call();
                    script["controllingARocket"] = false;
                }
            }

            public static IEnumerator OnFrame(Lua[] s)
            {
                //int scene;
                while (true)
                {
                    foreach (Lua script in s)
                    {
                        if (PlayerController.main != null) // Not doing this will cause NullRefException outside of World_PC
                        {
                            var rkt = PlayerController.main.player.Value as Rocket;
                            if (rkt != null)
                            {
                                script["controllingARocket"] = true;
                                script["currentRocket"] = rkt.GetType();
                            }
                            else
                            {
                                script["controllingARocket"] = false;
                                script["currentRocket"] = KeraLua.LuaType.Nil;
                            }
                        } else
                            script["controllingARocket"] = false;

                        var loop = script["Loop"] as LuaFunction;
                        loop.Call();

                        script["this"] = script;
                    }
                    yield return null;
                }
            }

            public static bool AFUnityRunning { // will delete later
                get {
                    foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (asm.FullName == "AstroForgeUnity") return true;
                    }
                    return false;
                } 
            }
        }
        public class Utility
        {
            public static List<string> ReadAllFilesInDirectory(string directoryPath)
            {
                List<string> result = new List<string>();

                try
                {
                    string[] filePaths = Directory.GetFiles(directoryPath);

                    foreach (string filePath in filePaths)
                    {
                        string fileContent = File.ReadAllText(filePath);
                        result.Add(fileContent);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"LuaPlugin reading error: {ex.Message}");
                }

                return result;
            }
        }
    }
}
