using System.Collections.Generic;

public static class ValuesRegister
{
    private static Dictionary<string, int> values = new Dictionary<string, int>();

    public delegate void OnValueSet(string key, int val);
    public static event OnValueSet ValueSet;

    public static bool Contains(string key)
    {
        return values.ContainsKey(key);
    }

    public static int GetValue(string key)
    {
        return values[key];
    }

    public static void SetValue(string key, int val)
    {
        if(!Contains(key))
        {
            values.Add(key, val);
        }
        else
        {
            values[key] = val;
        }

        ValueSet?.Invoke(key, val);
    }
}
