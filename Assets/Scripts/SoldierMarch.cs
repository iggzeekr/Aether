using UnityEngine;

public class SoldierMarch : MonoBehaviour
{
    Transform _hipL;
    Transform _hipR;
    Transform _kneeL;
    Transform _kneeR;
    Transform _armL;
    Transform _armR;
    Quaternion _hipL0;
    Quaternion _hipR0;
    Quaternion _kneeL0;
    Quaternion _kneeR0;
    Quaternion _armL0;
    Quaternion _armR0;
    float _phase;
    public bool Aiming;

    void Awake()
    {
        _hipL = FindBone("UpperLeg.L");
        _hipR = FindBone("UpperLeg.R");
        _kneeL = FindBone("LowerLeg.L");
        _kneeR = FindBone("LowerLeg.R");
        _armL = FindBone("UpperArm.l");
        if (_armL == null)
            _armL = FindBone("UpperArm.L");
        _armR = FindBone("UpperArm.r");
        if (_armR == null)
            _armR = FindBone("UpperArm.R");

        _hipL0 = Rest(_hipL);
        _hipR0 = Rest(_hipR);
        _kneeL0 = Rest(_kneeL);
        _kneeR0 = Rest(_kneeR);
        _armL0 = Rest(_armL);
        _armR0 = Rest(_armR);
        _phase = Random.value * Mathf.PI * 2f;
    }

    void LateUpdate()
    {
        float swing = Mathf.Sin(Time.time * 5.5f + _phase);
        if (Aiming)
            swing *= 0.25f;
        Swing(_hipL, _hipL0, swing * 32f);
        Swing(_hipR, _hipR0, -swing * 32f);
        Swing(_kneeL, _kneeL0, Mathf.Max(0f, -swing) * 42f);
        Swing(_kneeR, _kneeR0, Mathf.Max(0f, swing) * 42f);
        Swing(_armL, _armL0, Aiming ? -18f : -swing * 22f);
        Swing(_armR, _armR0, Aiming ? -48f : swing * 22f);
    }

    static void Swing(Transform bone, Quaternion rest, float pitch)
    {
        if (bone == null)
            return;
        bone.localRotation = rest * Quaternion.Euler(pitch, 0f, 0f);
    }

    static Quaternion Rest(Transform bone)
    {
        return bone == null ? Quaternion.identity : bone.localRotation;
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
