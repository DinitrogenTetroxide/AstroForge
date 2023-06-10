using ModLoader.Helpers;
using NLua;
using SFS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuaInterpreter
{
    // Token: 0x02000070 RID: 112
    public class ScriptLoader
    {
        public void Load() 
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
            new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray()));
            SceneHelper.OnSceneLoaded += () =>
            {
                new GameObject().AddComponent<BlankMB>().StartCoroutine(Functions.OnFrame(scrs.ToArray()));
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

            public static IEnumerator OnFrame(Lua[] s)
            {
                while (true)
                {
                    foreach (Lua script in s)
                    {
                        var loop = script["Loop"] as LuaFunction;
                        loop.Call();

                        script["this"] = script;

                        if (PlayerController.main != null) // Not doing this will cause NullRefException outside of World_PC
                        if (PlayerController.main.player.Value as Rocket != null)
                        {
                            script["controllingARocket"] = true;
                            script["currentRocket"] = PlayerController.main.player.Value as Rocket;
                        } else 
                        {
                            script["controllingARocket"] = false;
                            script["currentRocket"] = KeraLua.LuaType.Nil;
                        }
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
