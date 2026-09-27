using UnityEngine;

public class EnergyBolt : MonoBehaviour
{
    int _team;
    float _damage;
    float _life;
    Vector3 _direction;
    float _speed;

    public static void Spawn(Vector3 position, Vector3 direction, int team, float damage, Color color)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Atis";
        Destroy(go.GetComponent<Collider>());
        go.transform.position = position + direction.normalized * 0.45f;
        go.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        go.transform.localScale = new Vector3(0.08f, 0.08f, 0.42f);

        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        if (shader != null && shader.name == "Standard")
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.2f);
        }
        go.GetComponent<Renderer>().sharedMaterial = material;

        EnergyBolt bolt = go.AddComponent<EnergyBolt>();
        bolt._team = team;
        bolt._damage = damage;
        bolt._life = 1.6f;
        bolt._direction = direction.normalized;
        bolt._speed = 90f;
    }

    void Update()
    {
        float step = _speed * Time.deltaTime;
        Vector3 pos = transform.position;
        float traveled = 0f;
        int guard = 0;
        while (traveled < step && guard < 8)
        {
            guard++;
            float left = step - traveled;
            if (!Physics.SphereCast(pos, 0.12f, _direction, out RaycastHit hit, left, ~0, QueryTriggerInteraction.Collide))
            {
                pos += _direction * left;
                break;
            }

            Vital body = hit.collider.GetComponentInParent<Vital>();
            if (body == null && hit.collider.isTrigger)
            {
                float skip = Mathf.Max(0.05f, hit.distance + 0.08f);
                pos += _direction * skip;
                traveled += skip;
                continue;
            }

            if (body != null && body.Team == _team)
            {
                float skip = Mathf.Max(0.05f, hit.distance + 0.08f);
                pos += _direction * skip;
                traveled += skip;
                continue;
            }

            if (body != null)
                body.Hurt(_damage);
            Destroy(gameObject);
            return;
        }

        transform.position = pos;
        _life -= Time.deltaTime;
        if (_life <= 0f)
            Destroy(gameObject);
    }
}
