using UnityEngine;

public class FieldScan : MonoBehaviour
{
    public static int Total { get; private set; }
    public static int DoneCount { get; private set; }

    public string Title;
    public bool Done { get; private set; }

    Light _light;
    Renderer _renderer;

    public static void ResetCount()
    {
        Total = 0;
        DoneCount = 0;
    }

    public static void SpawnAll()
    {
        ResetCount();
        Vector3[] spots =
        {
            new Vector3(24f, 0.2f, 18f),
            new Vector3(-18f, 0.2f, 28f),
            new Vector3(52f, 0.2f, 8f),
            new Vector3(8f, 0.2f, -28f),
            new Vector3(-36f, 0.2f, 6f),
            new Vector3(70f, 0.2f, 46f),
            new Vector3(-54f, 0.2f, 48f),
            new Vector3(34f, 0.2f, 72f),
            new Vector3(-12f, 0.2f, -46f),
            new Vector3(88f, 0.2f, -16f)
        };
        string[] titles =
        {
            "Meteor sample",
            "Radiation check",
            "Signal log",
            "Soil test",
            "Air sample",
            "Satellite align",
            "Geothermal trace",
            "Crystal scan",
            "Dust sample",
            "Wind reading"
        };
        for (int i = 0; i < spots.Length; i++)
            Create(spots[i], titles[i]);
    }

    static void Create(Vector3 position, string title)
    {
        var root = new GameObject("Sinyal");
        root.transform.position = position;

        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "Direk";
        pillar.transform.SetParent(root.transform, false);
        pillar.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        pillar.transform.localScale = new Vector3(0.28f, 3.4f, 0.28f);
        var scan = root.AddComponent<FieldScan>();
        scan.Title = title;
        scan._renderer = pillar.GetComponent<Renderer>();
        scan.Paint(new Color(0.2f, 0.85f, 1f));

        var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.transform.SetParent(root.transform, false);
        cap.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        cap.transform.localScale = Vector3.one * 0.55f;
        Object.Destroy(cap.GetComponent<Collider>());
        cap.GetComponent<Renderer>().sharedMaterial = scan._renderer.sharedMaterial;

        var lamp = new GameObject("Isik");
        lamp.transform.SetParent(root.transform, false);
        lamp.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 14f;
        light.intensity = 2.4f;
        light.shadows = LightShadows.None;
        scan._light = light;
        light.color = new Color(0.3f, 0.9f, 1f);
    }

    void Awake()
    {
        Total++;
    }

    void Paint(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
            material.SetColor("_EmissionColor", color * 0.8f);
        if (_renderer != null)
            _renderer.sharedMaterial = material;
    }

    public static string PromptNear(Vector3 player)
    {
        FieldScan near = Closest(player, 3.4f);
        if (near == null || near.Done)
            return null;
        return "E   " + near.Title;
    }

    public static bool TryNearest(Vector3 player)
    {
        FieldScan near = Closest(player, 3.4f);
        if (near == null || near.Done)
            return false;
        near.Finish();
        return true;
    }

    static FieldScan Closest(Vector3 player, float radius)
    {
        FieldScan[] all = FindObjectsByType<FieldScan>(FindObjectsSortMode.None);
        FieldScan best = null;
        float limit = radius * radius;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].Done)
                continue;
            float sqr = (all[i].transform.position - player).sqrMagnitude;
            if (sqr < limit)
            {
                limit = sqr;
                best = all[i];
            }
        }
        return best;
    }

    void Finish()
    {
        Done = true;
        DoneCount++;
        var color = new Color(0.3f, 1f, 0.5f);
        Paint(color);
        if (_light != null)
            _light.color = color;
    }
}
