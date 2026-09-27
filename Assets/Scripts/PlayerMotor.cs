using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public float WalkSpeed = 8.5f;
    public float SprintSpeed = 17f;
    public float JumpSpeed = 7.2f;

    CharacterController _body;
    CityCamera _camera;
    Vector3 _velocity;
    bool _locked;

    public float CurrentSpeed { get; private set; }
    public bool Sprinting { get; private set; }

    public bool Locked
    {
        get => _locked;
        set
        {
            _locked = value;
            if (value)
                _velocity = Vector3.zero;
        }
    }

    void Awake()
    {
        _body = GetComponent<CharacterController>();
    }

    public void Bind(CityCamera camera)
    {
        _camera = camera;
    }

    void Update()
    {
        if (!GameMenu.Playing || _locked || _camera == null)
        {
            CurrentSpeed = 0f;
            Sprinting = false;
            return;
        }

        if (_body.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 wish = forward * inputZ + right * inputX;
        if (wish.sqrMagnitude > 1f)
            wish.Normalize();

        Sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = Sprinting ? SprintSpeed : WalkSpeed;
        CurrentSpeed = wish.magnitude * speed;
        if (wish.sqrMagnitude > 0.001f)
        {
            Quaternion face = Quaternion.LookRotation(wish, Vector3.up);
            float turn = Sprinting ? 22f : 16f;
            transform.rotation = Quaternion.Slerp(transform.rotation, face, turn * Time.deltaTime);
        }

        if (_body.isGrounded && Input.GetButtonDown("Jump"))
            _velocity.y = JumpSpeed;

        _velocity.y += Physics.gravity.y * Time.deltaTime;
        Vector3 motion = wish * speed;
        motion.y = _velocity.y;
        _body.Move(motion * Time.deltaTime);
    }
}
