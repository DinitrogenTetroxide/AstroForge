using System;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using AstroForge.CustomParts;
using SFS.Parts;

[Serializable]
public class AstroForgeInjector : MonoBehaviour
{

    public LoadFrom loadScriptFrom;

    //[ShowIf("loadScriptFrom", LoadFrom.String)]
    [Space(10)]
    [TextArea(10, 10)] 
        public string scriptContent;

    /*[ShowIf("loadScriptFrom", LoadFrom.File)]
    [Space(10)]
        public TextAsset scriptFile;*/

    [Space(10)]
        public SerializableDictionary<string, Object> variables = new SerializableDictionary<string, Object>() { };

    [HideInInspector] public MonoBehaviour astroForgeScript;

    public void OnPartUsed(UsePartData data)
    {
        astroForgeScript.Invoke("OnPartUsed", 0f);
    }
}

public enum LoadFrom
{
    String,
    File
}

namespace AstroForge.CustomParts
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
                throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                Add(keys[i], values[i]);
        }
    }
}