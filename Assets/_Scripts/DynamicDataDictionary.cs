using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "dynamicDataDictionary.asset", menuName = "DynamicDatDictionary")]
public class DynamicDataDictionary : ScriptableObject
{
    [SerializeField] private Dictionary<string, AnimationData> _dataDictionary;

    public AnimationData GetValue(string key)
    {
        AnimationData transform;
        _dataDictionary.TryGetValue(key, out transform);
        return transform;
    }
    
    public void Set(string key, AnimationData data)
    {
        if(_dataDictionary == null) { _dataDictionary = new Dictionary<string, AnimationData>(); }

        if (_dataDictionary.ContainsKey(key))
        {
            _dataDictionary[key] = data;
        }
        else
        {
            _dataDictionary.Add(key, data);
        }
    }
}
