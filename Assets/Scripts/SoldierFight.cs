using UnityEngine;

public class SoldierFight : MonoBehaviour
{
    public const float Damage = 12f;
    const float Range = 13.5f;

    static float _shotWindow;
    static int _shots;

    Vital _vital;
    StreetWalker _walk;
    SoldierMarch _march;
    CapsuleCollider _hit;
    Transform _model;
    Transform _muzzle;
    float _nextShot;
    float _down;

    void Awake()
    {
        _vital = GetComponent<Vital>();
        _walk = GetComponent<StreetWalker>();
        _hit = GetComponent<CapsuleCollider>();
        if (_vital != null)
            _vital.Died += Down;
        _nextShot = Random.Range(0.2f, 1.4f);
    }

    public void Arm(Transform model)
    {
        _model = model;
        _march = model.GetComponent<SoldierMarch>();
        Transform hand = Find(model, "Hand.R");
        if (hand == null)
            hand = model;
        _muzzle = GunMount.Hand(CityArt.Rifle, hand, transform);
    }

    void Update()
    {
        if (_vital == null)
            return;

        if (_down > 0f)
        {
            _down -= Time.deltaTime;
            if (_model != null)
                _model.localRotation = Quaternion.Euler(80f, 0f, 12f);
            if (_down <= 0f)
                Rise();
            return;
        }

        if (!GameMenu.Playing || !_vital.Alive)
            return;

        if (_march != null)
            _march.Aiming = false;

        CityGame game = CityGame.Instance;
        if (game == null || game.Driving || LabInterior.IsInside || game.Body == null || !game.Body.Alive)
        {
            if (_walk != null)
                _walk.Paused = false;
            return;
        }

        Vector3 to = game.Player.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        if (dist > Range || dist < 0.4f || !Sees(game.Player))
        {
            if (_walk != null)
                _walk.Paused = false;
            return;
        }

        if (_walk != null)
            _walk.Paused = true;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 10f * Time.deltaTime);
        if (_march != null)
            _march.Aiming = true;

        if (Time.time < _nextShot || !TakeShot())
            return;

        _nextShot = Time.time + 1.35f;
        Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 1.35f;
        Vector3 aim = game.Player.position + Vector3.up * 1.05f;
        EnergyBolt.Spawn(origin, aim - origin, 1, Damage, new Color(1f, 0.45f, 0.2f));
    }

    bool Sees(Transform player)
    {
        Vector3 from = transform.position + Vector3.up * 1.45f;
        Vector3 to = player.position + Vector3.up * 1f;
        Vector3 delta = to - from;
        if (!Physics.Raycast(from, delta.normalized, out RaycastHit hit, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
            return true;
        Vital body = hit.collider.GetComponentInParent<Vital>();
        return body != null && body.Team == 0;
    }

    static bool TakeShot()
    {
        if (Time.time >= _shotWindow)
        {
            _shotWindow = Time.time + 0.7f;
            _shots = 0;
        }

        if (_shots >= 2)
            return false;
        _shots++;
        return true;
    }

    void Down()
    {
        _down = 6.5f;
        if (_walk != null)
            _walk.Paused = true;
        if (_hit != null)
            _hit.enabled = false;
        if (_march != null)
        {
            _march.Aiming = false;
            _march.enabled = false;
        }
    }

    void Rise()
    {
        _vital.Fill();
        if (_model != null)
            _model.localRotation = Quaternion.identity;
        if (_hit != null)
            _hit.enabled = true;
        if (_march != null)
            _march.enabled = true;
        if (_walk != null)
            _walk.Paused = false;
        _nextShot = 0.8f;
    }

    static Transform Find(Transform rig, string boneName)
    {
        Transform[] bones = rig.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i].name == boneName)
                return bones[i];
        }
        return null;
    }
}
