using UnityEngine;

public class SkyRing : MonoBehaviour
{
    public static int Total { get; private set; }
    public static int Cleared { get; private set; }

    public Material Glow;

    public static void ResetCount()
    {
        Total = 0;
        Cleared = 0;
    }

    bool _done;
    Light _light;

    void Awake()
    {
        Total++;
        _light = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (!_done)
            transform.Rotate(0f, 0f, 18f * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_done || other.GetComponentInParent<DriveableCar>() == null)
            return;

        _done = true;
        Cleared++;
        var done = new Color(0.25f, 0.9f, 0.55f);
        if (Glow != null)
        {
            Glow.color = done;
            if (Glow.HasProperty("_EmissionColor"))
                Glow.SetColor("_EmissionColor", done * 0.6f);
        }
        if (_light != null)
            _light.color = done;
    }
}
