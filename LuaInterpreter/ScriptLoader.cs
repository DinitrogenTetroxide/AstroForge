using ModLoader.Helpers;
using NLua;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using LuaInterpreter.CustomParts;

namespace LuaInterpreter
{
    public class ScriptLoader
    {
        public void LoadPlugins() 
        {
            var a = SceneManager.GetActiveScene().name;
            List<Lua> scrs = new List<Lua>();

            // Load all scripts

            foreach (string script in Utility.ReadAllFilesInDirectory($"{Main.dllDirectory}/Mods/LuaInterpreter/Plugins"))
            {
                Lua s = new Lua();
                s["LuaPlugin"] = new LuaPlugin();
                s["this"] = s;
                s.LoadCLRPackage();
                s.DoString(script);

                scrs.Add(s); // We have to make a local list, otherwise C# will cry because I didn't load the required DLL
            }

            Functions.OnStart(scrs.ToArray());
            new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray(), false));
            SceneHelper.OnSceneLoaded += () =>
            {
                new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray(), false));
            };
        }

        public void LoadCustomParts()
        {
            List<CustomPartScript> cps = new List<CustomPartScript>();

            foreach (var part in ResourcesLoader.GetFiles_Array<Part>("Parts").ToList()) 
            {
                CustomPartScript partScript = part.GetComponent<CustomPartScript>();
                if (partScript != null && partScript.enabled)
                {
                    cps.Add(partScript);
                }
                CustomPartScript[] cic = part.GetComponentsInChildren<CustomPartScript>(false);
                if (cic != null)
                {
                    foreach(var c in cic)
                    {
                        cps.Add(c);
                    }
                }
            }
            List<Lua> luas = new List<Lua>();
            foreach (CustomPartScript cpScript in cps)
            {
                Lua l = new Lua();
                l.LoadCLRPackage();
                foreach(var scriptVars in cpScript.variables)
                {
                    l[scriptVars.Key] = scriptVars.Value;
                }
                l["internal_exCondId"] = (int)cpScript.loadIn;
                l.DoString(cpScript.scriptContent);
                luas.Add(l);
            }

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode m) =>
            {
                if (scene.name == "World_PC" || scene.name == "Build_PC")
                    new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(luas.ToArray(), true));
            };
        }

        class Functions
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

            public static IEnumerator OnFrame(Lua[] s, bool fromPack)
            {
                int scene;
                while (true)
                {
                    switch (SceneManager.GetActiveScene().name)
                    {
                        case "Build_PC":
                            scene = 0;
                            break;
                        case "World_PC":
                            scene = 1;
                            break;
                        default:
                            scene = -1;
                            break;
                    }
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
        }
        class Utility
        {
            public static List<string> ReadAllFilesInDirectory(string directoryPath)
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
                    Debug.Log($"LuaPlugin reading error: {ex.Message}");
                }

                return tempS;
            }
        }
    }
}
