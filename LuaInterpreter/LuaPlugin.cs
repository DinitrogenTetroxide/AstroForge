using ModLoader.UI;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System.Linq;

namespace LuaInterpreter
{
    /// <summary>
    /// A class that will make the plugin appear in the mod menu,
    /// also might be useful for the future features
    /// </summary>
    public class LuaPlugin
    {
        public string ID = "";
        public string Name = "MoonSharp plugin";
        public string Author = "Default Author";
        public string Version = "1.0.0";
        public string Description = "Default MoonSharp plugin description.";

        public void OnUpdate() 
        {
            // Might be useful later in the development of this mod
        }

        /// <summary>
        /// Initialize the LuaPlugin and add the plugin to the mod menu.
        /// </summary>
        public void Init()
        {
            ModsListElement.ModData data = new ModsListElement.ModData()
            {
                author = Author,
                saveName = ID,
                name = "[LUA] " + Name,
                version = Version,
                description = Description,
                type = ModsListElement.ModType.Mod
            };

            ModsMenu.AddElement(data);
            Debug.Log("Loaded" + Name + " by " + Author);
        }

        // Methods below were AI-generated, but I randomly read that MoonSharp can simply access C# methods by using clr, so they are unnecessary

    /*public Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName.Replace("%", "."));
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public object GetValue(string propertyPath, bool onlyValues = true, object obj = null)
        {
            string[] pathParts = propertyPath.Split('.');
            object currentObject = obj;

            foreach (string part in pathParts)
            {
                if (currentObject == null)
                {
                    // Attempt to get the static property or field
                    Type type = FindType(part);
                    if (type != null && (onlyValues ? type.IsValueType : true))
                    {
                        currentObject = type;
                        continue;
                    }
                }

                PropertyInfo propInfo = currentObject.GetType().GetProperty(part);
                if (propInfo != null)
                {
                    currentObject = propInfo.GetValue(currentObject);
                }
                else
                {
                    FieldInfo fieldInfo = currentObject.GetType().GetField(part);
                    if (fieldInfo != null)
                    {
                        currentObject = fieldInfo.GetValue(currentObject);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return currentObject;
        }

        public bool SetValue(string propertyPath, object value, object obj = null)
        {
            string[] pathParts = propertyPath.Split('.');
            object currentObject = obj;

            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];

                if (currentObject == null)
                {
                    // Attempt to get the static property or field
                    Type type = FindType(part);
                    if (type != null && type.IsValueType)
                    {
                        currentObject = type;
                        continue;
                    }
                }

                PropertyInfo propInfo = currentObject.GetType().GetProperty(part);
                FieldInfo fieldInfo = currentObject.GetType().GetField(part);

                if (i == pathParts.Length - 1) // Last part of the path
                {
                    if (propInfo != null)
                    {
                        propInfo.SetValue(currentObject, value);
                        return true;
                    }
                    else if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(currentObject, value);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (propInfo != null)
                    {
                        currentObject = propInfo.GetValue(currentObject);
                    }
                    else if (fieldInfo != null)
                    {
                        currentObject = fieldInfo.GetValue(currentObject);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }*/

        public object CallMethod(string methodPath, DynValue[] methodArgs = null, object obj = null)
        {
            // Split the method path to get the type and method names.
            string[] parts = methodPath.Split('.');
            string typeName = string.Join(".", parts.Take(parts.Length - 1));
            string methodName = parts.Last();

            // Get the type.
            Type type = Type.GetType(typeName);

            // If the type is not found, search in all loaded assemblies.
            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        break;
                    }
                }
            }

            // If the type is still not found, throw an exception.
            if (type == null)
            {
                throw new InvalidOperationException($"Type '{typeName}' not found.");
            }

            // Convert DynValue[] to object[] and infer parameter types.
            object[] args = null;
            Type[] argTypes = null;

            if (methodArgs != null)
            {
                args = methodArgs.Select(dynValue => dynValue.ToObject()).ToArray();
                argTypes = methodArgs.Select(dynValue => dynValue.ToObject()?.GetType() ?? typeof(object)).ToArray();
            }

            // Get the method with matching parameter types.
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, argTypes, null);

            // If the method is not found, throw an exception.
            if (method == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in type '{typeName}'.");
            }

            // Invoke the method.
            return method.Invoke(obj, args);
        }
    }
}