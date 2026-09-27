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
    bool _title;
    Vector3 _aimWorld;
    float _aimWeight;

    public void SetAim(Vector3 world, float weight)
    {
        _aimWorld = world;
        _aimWeight = Mathf.Clamp01(weight);
    }

    public void Follow(Transform target, Vector3 pivot, float distance, bool snap)
    {
        _firstPerson = false;
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
        Target = target;
        PivotOffset = seat;
        Distance = 0f;
        SpeedPull = 0f;
        _pitch = 0f;
        _snap = snap;
    }

    void LateUpdate()
    {
        if (Target == null)
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

        if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * LookSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * LookSensitivity * 0.7f;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        }

        Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivot = Target.position + PivotOffset;
        float distance = Distance + SpeedPull;
        Vector3 desired = pivot + orbit * new Vector3(0f, 0.35f, -distance);

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

        Vector3 lookAt = _aimWeight > 0.02f
            ? Vector3.Lerp(pivot + Vector3.up * 0.45f, _aimWorld, _aimWeight)
            : pivot;
        Vector3 lookDir = lookAt - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    void TitleShot()
    {
        Camera view = GetComponent<Camera>();
        if (view != null)
            view.fieldOfView = 36f;

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
