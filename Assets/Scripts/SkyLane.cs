using UnityEngine;

public class SkyLane : MonoBehaviour
{
    public Vector3 Center;
    public float Radius = 70f;
    public float Height = 30f;
    public float Speed = 12f;

    float _angle;
    Rigidbody _body;

    void Awake()
    {
        _body = gameObject.AddComponent<Rigidbody>();
        _body.isKinematic = true;
        _body.useGravity = false;
        _body.interpolation = RigidbodyInterpolation.Interpolate;
        var hull = gameObject.AddComponent<BoxCollider>();
        hull.center = new Vector3(0f, 0.35f, 0f);
        hull.size = new Vector3(7.5f, 2.2f, 10.5f);
        gameObject.AddComponent<ShipHull>();
    }

    void Start()
    {
        _angle = Random.Range(0f, Mathf.PI * 2f);
        _body.position = Point(_angle);
    }

    void FixedUpdate()
    {
        float ahead = _angle + Speed / Mathf.Max(12f, Radius) * 0.4f;
        Vector3 next = Point(ahead);
        Vector3 direction = next - _body.position;
        if (direction.sqrMagnitude > 0.01f)
            _body.MoveRotation(Quaternion.Slerp(_body.rotation, Quaternion.LookRotation(direction.normalized, Vector3.up), 3f * Time.fixedDeltaTime));
        _angle += Speed / Mathf.Max(12f, Radius) * Time.fixedDeltaTime;
        _body.MovePosition(Point(_angle));
    }

    Vector3 Point(float angle)
    {
        return Center + new Vector3(Mathf.Cos(angle) * Radius, Height, Mathf.Sin(angle) * Radius);
    }
}
