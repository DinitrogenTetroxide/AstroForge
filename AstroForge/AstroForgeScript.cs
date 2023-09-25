using UnityEngine;
using NLua;
using System.Collections.Generic;
using SFS.World;

namespace AstroForge.CustomParts
{
    public class AstroForgeScript : MonoBehaviour, Rocket.INJ_Rocket
    {
        public object lua;

        public string loadFrom;

        public SerializableDictionary<string, Object> vars = new SerializableDictionary<string, Object>();

        public Rocket Rocket { get; set; }

        public void Start()
        {
            Lua lua = new Lua();
            
            lua.LoadCLRPackage();

            foreach (KeyValuePair<string, Object> v in vars)
            {
                lua[v.Key] = v.Value;
            }

            lua.DoString(loadFrom);

            (lua["Begin"] as LuaFunction).Call();

            this.lua = lua;
        }

        public void Update()
        {
            if (lua != null) 
            {
                Lua lua = this.lua as Lua;

                lua["rocket"] = Rocket;

                if (lua["Loop"] != null)
                    (lua["Loop"] as LuaFunction).Call();
            }
        }

        public void OnPartUsed()
        {
            if (lua != null && (lua as Lua)["OnPartUsed"] != null)
            {
                Lua lua = this.lua as Lua;
                (lua["OnPartUsed"] as LuaFunction).Call();
            }
        }
    }
}