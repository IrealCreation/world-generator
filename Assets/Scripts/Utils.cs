
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Utils
{
    public static Dictionary<string, float> SumValueDictionaries(Dictionary<string, float> dicA, Dictionary<string, float> dicB)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        foreach(KeyValuePair<string, float> kv in dicA)
        {
            result[kv.Key] += kv.Value;
        }
        foreach(KeyValuePair<string, float> kv in dicB)
        {
            result[kv.Key] += kv.Value;
        }

        return result;
    }
    
    public static Dictionary<string, int> SumValueDictionaries(Dictionary<string, int> dicA, Dictionary<string, int> dicB)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        foreach(KeyValuePair<string, int> kv in dicA)
        {
            result[kv.Key] += kv.Value;
        }
        foreach(KeyValuePair<string, int> kv in dicB)
        {
            result[kv.Key] += kv.Value;
        }

        return result;
    }

    public static T SelectByPref<T>(Dictionary<T, int> dic, int total = 0)
    {
        if (total == 0)
        {
            // First, we need to sum it
            total = dic.Sum(x => x.Value);
        }

        int sum = 0;
        int random;
        KeyValuePair<T, int> prefered = dic.First(); // The element with the best score will be the default one
        foreach (KeyValuePair<T, int> kv in dic)
        {
            random = Random.Range(1, total);
            if (kv.Value + sum >= random)
                return kv.Key;

            if (kv.Value > prefered.Value)
                prefered = kv;
        }

        return prefered.Key;
    }
}