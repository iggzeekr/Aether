using UnityEngine;

public class DeliveryMission : MonoBehaviour
{
    public static DeliveryMission Instance { get; private set; }

    public int Cash { get; private set; }
    public string Objective { get; private set; }
    public Vector3 Target => transform.position;

    enum Phase { Pickup, Dropoff }

    Phase _phase;
    Vector3 _pickup;
    Material _beamMaterial;
    float _toastUntil;
    string _toast;
    bool _ready;

    void Awake()
    {
        Instance = this;
        var box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 3f, 0f);
        box.size = new Vector3(13f, 8f, 13f);

        var body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "Isin";
        beam.transform.SetParent(transform, false);
        beam.transform.localPosition = new Vector3(0f, 20f, 0f);
        beam.transform.localScale = new Vector3(1.6f, 20f, 1.6f);
        Destroy(beam.GetComponent<Collider>());

        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        _beamMaterial = new Material(shader);
        beam.GetComponent<Renderer>().sharedMaterial = _beamMaterial;

        var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "Halka";
        pad.transform.SetParent(transform, false);
        pad.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        pad.transform.localScale = new Vector3(8f, 0.06f, 8f);
        Destroy(pad.GetComponent<Collider>());
        pad.GetComponent<Renderer>().sharedMaterial = _beamMaterial;
    }

    public void Begin(Vector3 near)
    {
        PlacePickup(near);
        _ready = true;
    }

    public float DistanceFrom(Vector3 world)
    {
        world.y = 0f;
        Vector3 flat = transform.position;
        flat.y = 0f;
        return Vector3.Distance(world, flat);
    }

    public string Toast => Time.time < _toastUntil ? _toast : null;

    void PlacePickup(Vector3 near)
    {
        _phase = Phase.Pickup;
        transform.position = PickIntersection(near, 75f, 170f);
        _pickup = transform.position;
        Objective = "Mission: Go to the yellow light and pick up the package";
        SetColor(new Color(1f, 0.75f, 0.12f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_ready)
            return;

        DriveableCar car = other.GetComponentInParent<DriveableCar>();
        if (car == null || !car.Occupied)
            return;

        if (_phase == Phase.Pickup)
        {
            _phase = Phase.Dropoff;
            transform.position = PickIntersection(_pickup, 140f, 380f);
            Objective = "Mission: Deliver it to the green light";
            SetColor(new Color(0.25f, 0.9f, 0.35f));
            return;
        }

        int pay = Mathf.RoundToInt(60f + Vector3.Distance(_pickup, transform.position) * 0.45f);
        Cash += pay;
        _toast = "Delivered  +$" + pay;
        _toastUntil = Time.time + 3.5f;
        PlacePickup(transform.position);
    }

    void SetColor(Color color)
    {
        if (_beamMaterial != null)
            _beamMaterial.color = color;
    }

    static Vector3 PickIntersection(Vector3 away, float minDist, float maxDist)
    {
        Vector3 best = CityLayout.Intersection(CityLayout.Blocks / 2, 2);
        float bestScore = float.MaxValue;
        for (int i = 0; i < 40; i++)
        {
            int ix = Random.Range(1, CityLayout.Blocks);
            int iz = Random.Range(1, CityLayout.Blocks);
            Vector3 point = CityLayout.Intersection(ix, iz);
            float distance = Vector3.Distance(new Vector3(point.x, 0f, point.z), new Vector3(away.x, 0f, away.z));
            if (distance >= minDist && distance <= maxDist)
                return point;

            float score = Mathf.Abs(distance - (minDist + maxDist) * 0.5f);
            if (distance >= minDist * 0.8f && score < bestScore)
            {
                bestScore = score;
                best = point;
            }
        }

        return best;
    }
}
