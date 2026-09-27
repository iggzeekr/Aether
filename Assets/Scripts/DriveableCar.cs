using UnityEngine;

public class DriveableCar : MonoBehaviour
{
    public bool Occupied;
    public bool Flight;
    public float RestHeight = 2.6f;
    public Transform Beacon;
    public Transform Glow;
    public Transform[] FrontWheels;
    public Transform[] Wheels;

    public float Lift { get; private set; }
    public bool Boost { get; private set; }
    public float Altitude => transform.position.y;

    Rigidbody _body;
    float _throttle;
    float _steer;
    float _wheelPitch;

    public float SpeedKmh => _body == null ? 0f : _body.linearVelocity.magnitude * 3.6f;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
    }

    public bool Handbrake { get; private set; }

    const float MaxSpeed = 38f;

    public void SetInput(float throttle, float steer, bool handbrake)
    {
        _throttle = Mathf.Clamp(throttle, -1f, 1f);
        _steer = Mathf.Clamp(steer, -1f, 1f);
        Handbrake = handbrake;
    }

    public void SetFlightInput(float throttle, float steer, float lift, bool boost)
    {
        SetInput(throttle, steer, false);
        Lift = Mathf.Clamp(lift, -1f, 1f);
        Boost = boost;
    }

    public void Park()
    {
        Occupied = false;
        _throttle = 0f;
        _steer = 0f;
        Handbrake = false;
        Lift = 0f;
        Boost = false;
    }

    public void ResetPose()
    {
        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        Vector3 position = transform.position;
        position.y = Flight ? RestHeight : 0.6f;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
    }

    void Update()
    {
        if (Beacon != null)
        {
            Beacon.gameObject.SetActive(!Occupied);
            if (!Occupied)
                Beacon.localPosition = new Vector3(0f, 2.55f + Mathf.Sin(Time.time * 3f) * 0.18f, 0f);
        }

        float spin = _body.linearVelocity.magnitude * (_throttle < -0.05f || Vector3.Dot(_body.linearVelocity, transform.forward) < 0f ? -1f : 1f);
        _wheelPitch += spin * 90f * Time.deltaTime;
        Quaternion roll = Quaternion.Euler(_wheelPitch, 0f, 90f);
        if (Wheels != null)
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                if (Wheels[i] != null)
                    Wheels[i].localRotation = roll;
            }
        }

        if (Glow != null)
        {
            float power = Occupied ? Mathf.Clamp01(Mathf.Abs(_throttle) + Mathf.Abs(Lift) * 0.6f + (Boost ? 0.7f : 0f)) : 0.2f;
            Glow.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.6f, power);
        }

        if (FrontWheels != null)
        {
            Quaternion steered = Quaternion.Euler(_wheelPitch, _steer * 28f, 90f);
            for (int i = 0; i < FrontWheels.Length; i++)
            {
                if (FrontWheels[i] != null)
                    FrontWheels[i].localRotation = steered;
            }
        }
    }

    void FixedUpdate()
    {
        if (Flight)
        {
            Fly();
            return;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 velocity = _body.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, forward);
        float sideSpeed = Vector3.Dot(velocity, right);
        float dt = Time.fixedDeltaTime;

        if (!Occupied)
        {
            forwardSpeed = Mathf.MoveTowards(forwardSpeed, 0f, 12f * dt);
            sideSpeed = Mathf.MoveTowards(sideSpeed, 0f, 20f * dt);
            _body.linearVelocity = forward * forwardSpeed + right * sideSpeed + Vector3.up * velocity.y;
            return;
        }

        float target = _throttle * MaxSpeed;
        bool opposing = Mathf.Abs(_throttle) > 0.05f && Mathf.Abs(forwardSpeed) > 1f && Mathf.Sign(target) != Mathf.Sign(forwardSpeed);
        float accel = Mathf.Abs(_throttle) < 0.05f ? 8f : (opposing ? 28f : 13f);
        if (Handbrake && Mathf.Abs(_throttle) < 0.15f)
            accel = 16f;
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, target, accel * dt);

        float absSpeed = Mathf.Abs(forwardSpeed);
        float steerAuthority = Mathf.InverseLerp(0.3f, 7f, absSpeed);
        float highSpeed = Mathf.Lerp(1f, 0.4f, Mathf.InverseLerp(16f, MaxSpeed, absSpeed));
        if (Handbrake)
            highSpeed = 1.25f;
        float direction = absSpeed < 0.8f
            ? Mathf.Sign(Mathf.Abs(_throttle) > 0.05f ? _throttle : 1f)
            : Mathf.Sign(forwardSpeed);
        float turn = _steer * 100f * steerAuthority * highSpeed * direction;
        float yaw = turn * dt;
        _body.MoveRotation(Quaternion.Euler(0f, yaw, 0f) * _body.rotation);
        forward = Quaternion.Euler(0f, yaw, 0f) * forward;
        right = Vector3.Cross(Vector3.up, forward);

        float grip = Handbrake ? 4f : Mathf.Lerp(26f, 12f, absSpeed / MaxSpeed);
        sideSpeed = Mathf.MoveTowards(sideSpeed, 0f, grip * dt);
        _body.linearVelocity = forward * forwardSpeed + right * sideSpeed + Vector3.up * velocity.y;
    }

    void Fly()
    {
        float dt = Time.fixedDeltaTime;
        float yaw = transform.eulerAngles.y;
        if (!Occupied)
        {
            Lift = 0f;
            Boost = false;
            _body.linearVelocity = Vector3.MoveTowards(_body.linearVelocity, Vector3.zero, 18f * dt);
            _body.angularVelocity = Vector3.zero;
            Vector3 rest = _body.position;
            rest.y = Mathf.MoveTowards(rest.y, RestHeight, 3f * dt);
            _body.MovePosition(rest);
            _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, Quaternion.Euler(0f, yaw, 0f), 50f * dt));
            return;
        }

        yaw += _steer * 78f * dt;
        float pitch = -Lift * 16f;
        float roll = -_steer * 26f;
        _body.angularVelocity = Vector3.zero;
        _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, Quaternion.Euler(pitch, yaw, roll), 140f * dt));

        float maxSpeed = Boost ? 74f : 42f;
        float climb = Lift * (Boost ? 34f : 18f);
        Vector3 wish = (_body.rotation * Vector3.forward) * (_throttle * maxSpeed) + Vector3.up * climb;
        if (Mathf.Abs(_throttle) < 0.04f && Mathf.Abs(Lift) < 0.04f)
            wish *= 0.15f;
        _body.linearVelocity = Vector3.MoveTowards(_body.linearVelocity, wish, (Boost ? 46f : 24f) * dt);

        Vector3 position = _body.position;
        Vector3 velocity = _body.linearVelocity;
        float limit = CityLayout.Half - 6f;
        if (position.x < -limit) { position.x = -limit; velocity.x = 0f; }
        if (position.x > limit) { position.x = limit; velocity.x = 0f; }
        if (position.z < -limit) { position.z = -limit; velocity.z = 0f; }
        if (position.z > limit) { position.z = limit; velocity.z = 0f; }
        if (position.y < 3.2f) { position.y = 3.2f; velocity.y = Mathf.Max(0f, velocity.y); }
        if (position.y > 90f) { position.y = 90f; velocity.y = Mathf.Min(0f, velocity.y); }
        _body.linearVelocity = velocity;
        _body.MovePosition(position);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!Occupied || !Flight || collision.relativeVelocity.magnitude < 9f)
            return;
        if (collision.collider.GetComponentInParent<ShipHull>() == null)
            return;
        if (CityGame.Instance != null)
            CityGame.Instance.Kill("Crashed");
    }
}

public class ShipHull : MonoBehaviour
{
}
