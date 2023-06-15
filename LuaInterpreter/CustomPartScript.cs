using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System;
using Sirenix.OdinInspector;

namespace LuaInterpreter.CustomParts
{
    [Serializable]
    public class CustomPartScript : MonoBehaviour
    {
        public enum LoadIn
        {
            Build,
            World,
            Both
        }

        [Space(10)][TextArea(10, 10)] public string scriptContent;

        [Space(10)]
        public SerializableDictionary<string, Object> variables = new SerializableDictionary<string, Object>() { };

        [Space(10)]
        [ReadOnly]
        public string LIInfo = "LoadIn support coming soon!";
        public LoadIn loadIn;
    }
}

