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
    public float Zoom { get; private set; }
    public bool ZoomHeld { get; private set; }
    bool _aimTight;
    float _aimDist;
    Vector3 _aimWorld;
    Vital _zoomStick;

    public void ClearZoom()
    {
        Zoom = 0f;
        ZoomHeld = false;
        _zoomStick = null;
        _aimTight = false;
        if (_camera != null)
            _camera.SetAim(Vector3.zero, 0f);
    }

    public void ToggleZoom()
    {
        ZoomHeld = !ZoomHeld;
    }

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
            Zoom = 0f;
            _zoomStick = null;
            _camera?.SetAim(Vector3.zero, 0f);
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
            _zoomStick = null;
            Zoom = Mathf.MoveTowards(Zoom, 0f, Time.deltaTime * 3f);
            _camera.SetAim(_aimWorld, Zoom);
            return;
        }

        Camera cam = _camera.GetComponent<Camera>();
        Vital target = null;
        if (driving)
        {
            _zoomStick = null;
            Zoom = 0f;
            _camera.SetAim(Vector3.zero, 0f);
            target = cam == null ? null : PickTarget(cam);
            _aimTight = false;
        }
        else
        {
            target = cam == null ? null : PickTarget(cam);
            float wantZoom = ZoomHeld ? 1f : 0f;
            Zoom = Mathf.MoveTowards(Zoom, wantZoom, Time.deltaTime * 3.4f);
            _camera.SetAim(_aimWorld, ZoomHeld && _aimTight ? Zoom : 0f);
        }

        LockedOn = target != null;

        if (!Input.GetMouseButton(0) || Time.time < _next || cam == null || CityGame.PointerOnAim())
            return;

        _next = Time.time + 0.18f;
        bool behind = driving && CityGame.Instance.ShipBehind;
        Vector3 direction = behind ? CityGame.Instance.DrivingForward() : cam.transform.forward;
        float lead = behind ? 18f : driving ? 9f : 0.6f;
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
        _aimTight = false;
        var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float reach = Mathf.Max(150f, Screen.height * 0.28f);
        float tight = Mathf.Max(42f, Screen.height * 0.07f);
        float hold = tight * 3.2f;
        SoldierFight[] soldiers = Object.FindObjectsByType<SoldierFight>(FindObjectsSortMode.None);
        for (int i = 0; i < soldiers.Length; i++)
        {
            Vital body = soldiers[i].GetComponent<Vital>();
            if (body == null || !body.Alive)
                continue;

            Vector3 chest = soldiers[i].transform.position + Vector3.up * 1.15f;
            float dist = Vector3.Distance(cam.transform.position, chest);
            Vector3 screen = cam.WorldToScreenPoint(chest);
            if (screen.z <= 0.2f)
                continue;
            float pixels = Vector2.Distance(new Vector2(screen.x, screen.y), center);
            bool zooming = body == _zoomStick;
            float limit = pixels <= tight || zooming ? 220f : 85f;
            if (dist > limit || pixels > reach)
                continue;
            if (screen.x < -80f || screen.y < -80f || screen.x > Screen.width + 80f || screen.y > Screen.height + 80f)
                continue;

            if (pixels < bestPixels)
            {
                bestPixels = pixels;
                best = body;
            }

            if (zooming && pixels <= hold && dist <= 220f && ClearView(cam, chest))
            {
                _aimTight = true;
                _aimDist = dist;
                _aimWorld = chest;
            }
        }

        if (_zoomStick != null && !_aimTight)
            _zoomStick = null;

        if (_zoomStick == null && best != null && bestPixels <= tight)
        {
            Vector3 chest = best.transform.position + Vector3.up * 1.15f;
            if (ClearView(cam, chest))
            {
                _zoomStick = best;
                _aimTight = true;
                _aimDist = Vector3.Distance(cam.transform.position, chest);
                _aimWorld = chest;
            }
        }

        return best;
    }

    static bool ClearView(Camera cam, Vector3 chest)
    {
        Vector3 from = cam.transform.position;
        Vector3 delta = chest - from;
        float dist = delta.magnitude;
        if (dist < 0.8f)
            return true;
        return !Physics.Raycast(from, delta / dist, dist - 0.35f, ~(1 << 2), QueryTriggerInteraction.Ignore);
    }
}
