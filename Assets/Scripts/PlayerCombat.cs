using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public const float Damage = 36f;

    CityCamera _camera;
    Transform _muzzle;
    Vital _vital;
    float _next;
    bool _arm;

    public bool LockedOn { get; private set; }

    public void Bind(CityCamera camera, Transform muzzle)
    {
        _camera = camera;
        _muzzle = muzzle;
        _vital = GetComponent<Vital>();
    }

    void Update()
    {
        if (!GameMenu.Playing || _camera == null || _vital == null || !_vital.Alive)
        {
            _arm = true;
            return;
        }

        if (_arm)
        {
            _arm = false;
            _next = Time.time + 0.2f;
            return;
        }

        bool driving = CityGame.Instance != null && CityGame.Instance.Driving;
        PlayerMotor motor = GetComponent<PlayerMotor>();
        if (motor != null && motor.Locked && !driving)
        {
            LockedOn = false;
            return;
        }

        Camera cam = _camera.GetComponent<Camera>();
        Vital target = cam == null ? null : PickTarget(cam);
        LockedOn = target != null;

        if (!Input.GetMouseButton(0) || Time.time < _next || cam == null)
            return;

        _next = Time.time + 0.18f;
        Vector3 direction = cam.transform.forward;
        float lead = driving ? 9f : 0.6f;
        Vector3 origin = cam.transform.position + direction * lead;

        if (target != null)
        {
            Vector3 chest = target.transform.position + Vector3.up * 1.15f;
            if (!driving)
            {
                Vector3 face = chest - transform.position;
                face.y = 0f;
                if (face.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(face, Vector3.up);
            }
            target.Hurt(Damage);
            EnergyBolt.Spawn(origin, chest - origin, 0, 0f, new Color(0.35f, 0.85f, 1f));
            return;
        }

        EnergyBolt.Spawn(origin, direction, 0, Damage, new Color(0.35f, 0.85f, 1f));
    }

    Vital PickTarget(Camera cam)
    {
        Vital best = null;
        float bestPixels = float.MaxValue;
        var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float reach = Mathf.Max(150f, Screen.height * 0.28f);
        SoldierFight[] soldiers = Object.FindObjectsByType<SoldierFight>(FindObjectsSortMode.None);
        for (int i = 0; i < soldiers.Length; i++)
        {
            Vital body = soldiers[i].GetComponent<Vital>();
            if (body == null || !body.Alive)
                continue;

            Vector3 chest = soldiers[i].transform.position + Vector3.up * 1.15f;
            float dist = Vector3.Distance(cam.transform.position, chest);
            if (dist > 85f)
                continue;

            Vector3 screen = cam.WorldToScreenPoint(chest);
            if (screen.z <= 0.2f)
                continue;
            if (screen.x < -80f || screen.y < -80f || screen.x > Screen.width + 80f || screen.y > Screen.height + 80f)
                continue;

            float pixels = Vector2.Distance(new Vector2(screen.x, screen.y), center);
            if (pixels > reach)
                continue;
            if (pixels < bestPixels)
            {
                bestPixels = pixels;
                best = body;
            }
        }

        return best;
    }
}
