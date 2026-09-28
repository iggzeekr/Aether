using UnityEngine;

public class CityCamera : MonoBehaviour
{
    public Transform Target;
    public Vector3 PivotOffset = new Vector3(0f, 1.55f, 0f);
    public float Distance = 6.2f;
    public float SpeedPull;
    public float MinPitch = -42f;
    public float MaxPitch = 72f;
    public float LookSensitivity = 3.2f;

    float _yaw;
    float _pitch = 16f;
    bool _snap;
    bool _firstPerson;
    bool _chase;
    bool _title;
    Vector3 _aimWorld;
    float _aimWeight;
    public float LookZoom;

    public void SetAim(Vector3 world, float weight)
    {
        _aimWorld = world;
        _aimWeight = Mathf.Clamp01(weight);
    }

    public void Follow(Transform target, Vector3 pivot, float distance, bool snap)
    {
        _firstPerson = false;
        _chase = false;
        Target = target;
        PivotOffset = pivot;
        Distance = distance;
        if (snap && target != null)
        {
            _yaw = target.eulerAngles.y;
            _pitch = 16f;
            _snap = true;
        }
    }

    public void FirstPerson(Transform target, Vector3 seat, bool snap)
    {
        _firstPerson = true;
        _chase = false;
        Target = target;
        PivotOffset = seat;
        Distance = 0f;
        SpeedPull = 0f;
        _pitch = 0f;
        _snap = snap;
    }

    public void Chase(Transform target, Vector3 pivot, float distance, bool snap)
    {
        Follow(target, pivot, distance, snap);
        _chase = true;
    }

    void LateUpdate()
    {
        if (Target == null || ShipVoyage.Descending)
            return;

        if (!GameMenu.Playing)
        {
            TitleShot();
            _title = true;
            return;
        }

        if (_title)
        {
            _title = false;
            ShipVoyage.RestoreLight();
            ShipVoyage.ShowHull(true);
            _firstPerson = false;
            Distance = 6.2f;
            _yaw = Target.eulerAngles.y;
            _pitch = 16f;
            _snap = true;
            Camera view = GetComponent<Camera>();
            if (view != null)
                view.fieldOfView = 68f;
        }

        if (_firstPerson)
        {
            Cockpit();
            return;
        }

        if (_chase && !Input.GetMouseButton(1))
        {
            float follow = 1f - Mathf.Exp(-6f * Time.deltaTime);
            _yaw = Mathf.LerpAngle(_yaw, Target.eulerAngles.y, follow);
            _pitch = Mathf.Lerp(_pitch, 16f, follow);
        }
        else if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * LookSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * LookSensitivity * 0.7f;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        }

        Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivot = Target.position + PivotOffset;
        float distance = Distance + SpeedPull;
        Vector3 desired = pivot + orbit * new Vector3(0f, 0.35f, -distance);
        if (LookZoom > 0.02f)
        {
            Vector3 aimPos = pivot + orbit * new Vector3(0.8f, 0.85f, -3.2f);
            desired = Vector3.Lerp(desired, aimPos, LookZoom);
        }

        Vector3 offset = desired - pivot;
        float length = offset.magnitude;
        if (length > 0.01f)
        {
            RaycastHit[] hits = Physics.RaycastAll(pivot, offset / length, length, ~ (1 << 2), QueryTriggerInteraction.Ignore);
            float nearest = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (hitTransform == Target || hitTransform.IsChildOf(Target))
                    continue;
                if (hits[i].distance < nearest)
                {
                    nearest = hits[i].distance;
                    desired = hits[i].point + hits[i].normal * 0.3f;
                }
            }
        }

        if (_snap)
        {
            transform.position = desired;
            _snap = false;
        }
        else
        {
            float t = 1f - Mathf.Exp(-12f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }

        Vector3 lookAt = _chase ? pivot + Target.forward * 16f : pivot;
        if (LookZoom > 0.02f)
            lookAt = Vector3.Lerp(lookAt, transform.position + orbit * Vector3.forward * 40f, LookZoom);
        if (_aimWeight > 0.02f)
            lookAt = Vector3.Lerp(lookAt, _aimWorld, _aimWeight);
        Vector3 lookDir = lookAt - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    void FrameShip(Transform ship)
    {
        Camera view = GetComponent<Camera>();
        if (view != null)
        {
            view.clearFlags = CameraClearFlags.Skybox;
            view.fieldOfView = 32f;
        }

        Transform heroine = CityGame.Instance != null ? CityGame.Instance.Player : null;
        Vector3 flat = ship.forward;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.01f)
            flat = Vector3.forward;
        flat.Normalize();
        float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg + Mathf.Sin(Time.unscaledTime * 0.16f) * 3f;
        Vector3 pivot = heroine != null ? heroine.position + Vector3.up * 1.02f : ship.position + Vector3.up * 2f;
        transform.position = pivot + Quaternion.Euler(2f, yaw, 0f) * new Vector3(-0.35f, 0f, 3.5f);
        transform.LookAt(pivot + Vector3.up * 0.08f);
        if (heroine != null)
        {
            Vector3 toCam = transform.position - heroine.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.01f)
                heroine.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        }
        ShipVoyage.ShowHull(false);
        ShipVoyage.AimSky(transform.eulerAngles.y);
        ShipVoyage.FramePlanet(view);
        ShipVoyage.TitleLight();
    }

    void TitleShot()
    {
        Camera view = GetComponent<Camera>();
        if (view != null)
        {
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.42f, 0.62f, 0.84f);
            view.fieldOfView = 36f;
        }

        float yaw = 180f + Mathf.Sin(Time.unscaledTime * 0.38f) * 24f;
        Vector3 pivot = Target.position + new Vector3(0f, 1.2f, 0f);
        Vector3 offset = Quaternion.Euler(-8f, yaw, 0f) * new Vector3(0f, 0.05f, -3.15f);
        transform.position = pivot + offset;
        transform.LookAt(pivot);
        transform.LookAt(pivot - transform.right * 1.15f);
    }

    void Cockpit()
    {
        if (Input.GetMouseButton(1))
        {
            _pitch -= Input.GetAxis("Mouse Y") * LookSensitivity * 0.45f;
            _pitch = Mathf.Clamp(_pitch, -55f, 62f);
        }

        transform.position = Target.TransformPoint(PivotOffset);
        transform.rotation = Target.rotation * Quaternion.Euler(_pitch, 0f, 0f);
    }
}
