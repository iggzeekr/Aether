using System.Collections.Generic;
using UnityEngine;

public class StreetLight : MonoBehaviour
{
    public static readonly List<StreetLight> All = new List<StreetLight>();

    public static void Clear()
    {
        All.Clear();
    }

    Light _light;

    void Awake()
    {
        _light = GetComponent<Light>();
        if (_light != null)
            _light.enabled = false;
        All.Add(this);
    }

    void OnDestroy()
    {
        All.Remove(this);
    }

    public void Ignite()
    {
        if (_light == null)
            return;
        _light.enabled = true;
        _light.intensity = 2.8f;
        _light.range = 22f;
    }

    public static bool Near(Vector3 position, float radius)
    {
        float reach = radius * radius;
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i] == null)
                continue;
            Vector3 delta = All[i].transform.position - position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= reach)
                return true;
        }

        return false;
    }

    public static void IgniteAll()
    {
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i] != null)
                All[i].Ignite();
        }
    }
}
