using ModLoader.UI;
using UnityEngine;
using System;
using System.Reflection;
using NLua;
using System.Linq;
using SFS.World;

namespace LuaInterpreter
{
    /// <summary>
    /// A class that will make the plugin appear in the mod menu,
    /// also might be useful for the future features
    /// </summary>
    public class LuaPlugin
    {
        public string ID = "";
        public string Name = "Lua plugin";
        public string Author = "Default Author";
        public string Version = "1.0.0";
        public string Description = "Default Lua plugin description.";

        public void OnUpdate(Lua script){}

        /// <summary>
        /// Initialize the LuaPlugin and add the plugin to the mod menu.
        /// </summary>
        public void Init()
        {
            ModsListElement.ModData data = new ModsListElement.ModData()
            {
                author = Author,
                saveName = ID == "" || ID == null ? Name.Replace(" ", "") : ID,
                name = "(Lua plugin) " + Name,
                version = Version,
                description = Description,
                type = ModsListElement.ModType.Mod
            };

            ModsMenu.AddElement(data);
            Debug.Log("Loaded " + Name + " by " + Author);
        }
    }
}