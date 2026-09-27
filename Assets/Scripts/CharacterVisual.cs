using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    Animator _animator;
    PlayerMotor _motor;
    string _state = "";
    Vector3 _restPos;
    bool _restCaptured;
    Transform _wink;
    Vector3 _eyeScale;

    public Transform Muzzle { get; private set; }

    public static bool Attach(Transform player, PlayerMotor motor, out Renderer[] renderers)
    {
        renderers = null;
        CityArt.Load();
        if (CityArt.Character == null)
            return false;

        GameObject model = Object.Instantiate(CityArt.Character, player);
        model.name = "Asuna";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        Renderer[] skinned = model.GetComponentsInChildren<Renderer>();
        if (skinned.Length == 0 || !FitToHeight(model, player, skinned, 1.72f))
        {
            Object.Destroy(model);
            return false;
        }

        Animator animator = model.GetComponent<Animator>();
        if (animator == null)
            animator = model.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = CityArt.Moves;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (CityArt.Moves != null)
            animator.CrossFade("Idle1", 0f);

        CharacterVisual visual = model.AddComponent<CharacterVisual>();
        visual._animator = animator;
        visual._motor = motor;
        visual.Muzzle = GunMount.Socket(CityArt.Pistol, model.transform, "Firearm_SocketPistol_R");

        foreach (Transform child in model.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = 2;

        renderers = model.GetComponentsInChildren<Renderer>();
        return true;
    }

    static bool FitToHeight(GameObject model, Transform player, Renderer[] renderers, float targetHeight)
    {
        Bounds bounds = WorldBounds(renderers);
        if (bounds.size.y < 0.01f)
            return false;

        float scale = targetHeight / bounds.size.y;
        model.transform.localScale = Vector3.one * scale;
        bounds = WorldBounds(renderers);
        float feet = bounds.min.y - player.position.y;
        model.transform.localPosition = new Vector3(0f, -feet, 0f);
        return true;
    }

    static Bounds WorldBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    void Update()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        if (!GameMenu.Playing)
            return;

        transform.localRotation = Quaternion.identity;
        if (_motor == null)
            return;

        string next = "Idle1";
        if (!_motor.Locked && _motor.CurrentSpeed > 0.2f)
            next = _motor.Sprinting ? "Run" : "Walk";

        _animator.speed = _motor.Sprinting ? 1.35f : 1f;
        if (next == _state)
            return;

        _state = next;
        _animator.CrossFade(next, 0.12f);
    }

    void LateUpdate()
    {
        if (_animator == null)
            return;

        if (!GameMenu.Playing)
        {
            TitleMotion();
            return;
        }

        if (_restCaptured)
            transform.localPosition = _restPos;
        if (_wink != null)
            _wink.localScale = _eyeScale;
    }

    void TitleMotion()
    {
        if (!_restCaptured)
        {
            _restPos = transform.localPosition;
            _restCaptured = true;
            _wink = FindBone("EyeLeft");
            if (_wink == null)
                _wink = FindBone("Eye_L");
            if (_wink != null)
                _eyeScale = _wink.localScale;
        }

        float t = Mathf.Repeat(Time.unscaledTime, 3.4f);
        bool jumping = t < 0.72f;
        float hop = jumping ? Mathf.Sin(t / 0.72f * Mathf.PI) * 0.62f : 0f;
        transform.localPosition = _restPos + Vector3.up * hop;

        string pose = jumping ? "PistolAim" : "Idle1";
        _animator.speed = 1f;
        if (pose != _state)
        {
            _state = pose;
            _animator.CrossFade(_state, 0.12f);
        }

        if (_wink == null)
            return;

        bool wink = t > 0.28f && t < 0.46f;
        Vector3 scale = _eyeScale;
        scale.y = wink ? _eyeScale.y * 0.05f : _eyeScale.y;
        _wink.localScale = scale;
    }

    Transform FindBone(string boneName)
    {
        Transform[] bones = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i].name == boneName)
                return bones[i];
        }

        return null;
    }
}
