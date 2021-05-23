
using System.Collections.Generic;

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
}