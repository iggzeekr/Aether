using System.Collections.Generic;
using UnityEngine;

public class LabDoor : MonoBehaviour
{
    public static readonly List<LabDoor> All = new List<LabDoor>();
    public static LabDoor Active { get; private set; }

    static int _created;
    static int _seen;

    Transform _beacon;
    float _cooldown;

    public Vector3 ExitPoint => transform.position + transform.forward * 3.8f;
    public Quaternion ExitRotation => Quaternion.LookRotation(transform.forward, Vector3.up);

    public static void Reset()
    {
        All.Clear();
        Active = null;
        _created = 0;
        _seen = 0;
    }

    public static void TryCreate(Transform building, float depth)
    {
        _seen++;
        if (_created >= 8 || _seen % 5 != 0)
            return;

        var go = new GameObject("LaboratuvarKapisi");
        go.transform.SetParent(building, false);
        go.transform.localPosition = new Vector3(0f, 0f, depth * 0.5f + 3.4f);
        go.transform.localRotation = Quaternion.identity;
        var door = go.AddComponent<LabDoor>();
        door.BuildVisual();
        All.Add(door);
        _created++;
    }

    public static LabDoor CreateAt(Vector3 position, Quaternion rotation)
    {
        var go = new GameObject("LaboratuvarKapisi");
        go.transform.SetPositionAndRotation(position, rotation);
        var door = go.AddComponent<LabDoor>();
        door.BuildVisual();
        All.Add(door);
        return door;
    }

    public static void ActivateClosest(Vector3 from)
    {
        SetActive(Closest(from, null));
    }

    public static void ActivateNext(Vector3 from)
    {
        LabDoor next = Closest(from, Active);
        if (next != null)
            SetActive(next);
    }

    public static string PromptFor(Vector3 player, bool driving)
    {
        if (driving || LabInterior.IsInside || Time.time < LabInterior.BlockEntryUntil)
            return null;
        LabDoor near = Nearest(player, 4.2f);
        if (near == null)
            return null;
        return "E  Enter the lab";
    }

    void BuildVisual()
    {
        if (CityArt.Door != null)
        {
            GameObject model = Instantiate(CityArt.Door, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Collider[] blockers = model.GetComponentsInChildren<Collider>();
            for (int i = 0; i < blockers.Length; i++)
                blockers[i].enabled = false;
        }

        Bar(new Vector3(-0.75f, 1.25f, 0f), new Vector3(0.14f, 2.5f, 0.14f));
        Bar(new Vector3(0.75f, 1.25f, 0f), new Vector3(0.14f, 2.5f, 0.14f));
        Bar(new Vector3(0f, 2.5f, 0f), new Vector3(1.7f, 0.14f, 0.14f));

        var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "Giris";
        pad.transform.SetParent(transform, false);
        pad.transform.localPosition = new Vector3(0f, 0.05f, 0.4f);
        pad.transform.localScale = new Vector3(1.4f, 0.04f, 1.4f);
        Destroy(pad.GetComponent<Collider>());
        pad.GetComponent<Renderer>().sharedMaterial = Glow(new Color(0.2f, 0.85f, 1f));

        var lamp = new GameObject("KapiIsigi");
        lamp.transform.SetParent(transform, false);
        lamp.transform.localPosition = new Vector3(0f, 2.2f, 0.5f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 8f;
        light.intensity = 2.4f;
        light.color = new Color(0.35f, 0.9f, 1f);
        light.shadows = LightShadows.None;

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "Isaret";
        beam.transform.SetParent(transform, false);
        beam.transform.localPosition = new Vector3(0f, 9f, 0f);
        beam.transform.localScale = new Vector3(0.4f, 9f, 0.4f);
        Destroy(beam.GetComponent<Collider>());
        beam.GetComponent<Renderer>().sharedMaterial = Glow(new Color(0.25f, 0.95f, 1f));
        _beacon = beam.transform;
        _beacon.gameObject.SetActive(false);
    }

    void Bar(Vector3 localPosition, Vector3 scale)
    {
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.transform.SetParent(transform, false);
        bar.transform.localPosition = localPosition;
        bar.transform.localScale = scale;
        Destroy(bar.GetComponent<Collider>());
        bar.GetComponent<Renderer>().sharedMaterial = Glow(new Color(0.3f, 0.85f, 1f));
    }

    static Material Glow(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * 1.5f);
        return material;
    }

    static void SetActive(LabDoor door)
    {
        Active = door;
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i]._beacon != null)
                All[i]._beacon.gameObject.SetActive(All[i] == door);
        }
    }

    static LabDoor Closest(Vector3 from, LabDoor except)
    {
        LabDoor best = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i] == except)
                continue;
            float distance = (All[i].transform.position - from).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = All[i];
            }
        }
        return best;
    }

    static LabDoor Nearest(Vector3 player, float radius)
    {
        LabDoor nearest = null;
        float best = radius * radius;
        for (int i = 0; i < All.Count; i++)
        {
            float sqr = (All[i].transform.position - player).sqrMagnitude;
            if (sqr < best)
            {
                best = sqr;
                nearest = All[i];
            }
        }
        return nearest;
    }

    void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
        if (!GameMenu.Playing || LabInterior.IsInside || Time.time < LabInterior.BlockEntryUntil)
            return;
        if (CityGame.Instance == null || CityGame.Instance.Driving || _cooldown > 0f)
            return;

        float distance = Vector3.Distance(CityGame.Instance.Player.position, transform.position);
        bool pressed = Input.GetKeyDown(KeyCode.E);
        if (Nearest(CityGame.Instance.Player.position, 4.2f) != this)
            return;
        if (pressed || distance < 2.1f)
            LabInterior.Enter(this);
    }

    public void ArmCooldown()
    {
        _cooldown = 1f;
    }
}
