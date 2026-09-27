using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    Animator _animator;
    PlayerMotor _motor;
    string _state = "";

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
        if (_animator == null || _animator.runtimeAnimatorController == null || _motor == null)
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
}
