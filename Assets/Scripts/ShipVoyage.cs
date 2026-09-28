using UnityEngine;

public class ShipVoyage : MonoBehaviour
{
    public static Transform Exterior => _instance == null ? null : _instance._ship;
    public static bool Landed => _instance == null || _instance._landed;
    public static bool Descending => _instance != null && _instance._step == Step.Fall;
    public static string Prompt => _instance == null ? null : _instance._prompt;
    public static string Goal => _instance == null ? null : _instance.GoalLine();

    static readonly int[] Order = { 1, 0, 2 };
    static ShipVoyage _instance;

    enum Step { Order, Core, Hatch, Fall, Done }

    Transform _ship;
    Transform _planet;
    Light _cabin;
    Transform _room;
    Vector3 _inside;
    Vector3 _hatch;
    Vector3 _core;
    Transform[] _marks;
    GameObject _orb;
    BoxCollider _seal;
    Step _step = Step.Order;
    int _order;
    bool _landed;
    string _prompt;
    float _fall;
    Vector3 _fallFrom;
    Vector3 _fallTo;

    public static void PlaceHero(Transform player)
    {
        if (_instance == null || _instance._ship == null || player == null)
            return;

        CharacterController body = player.GetComponent<CharacterController>();
        if (body != null)
            body.enabled = false;
        Vector3 flat = _instance._ship.forward;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.01f)
            flat = Vector3.forward;
        flat.Normalize();
        player.SetPositionAndRotation(
            _instance._ship.position + flat * 6.4f + Vector3.up * 0.15f,
            Quaternion.LookRotation(flat, Vector3.up));
        if (body != null)
            body.enabled = true;
    }

    public static void Build()
    {
        if (_instance != null)
            return;
        var go = new GameObject("GemiYolculugu");
        _instance = go.AddComponent<ShipVoyage>();
        _instance.CreateShip();
        _instance.CreateBay();
    }

    public static void TitleLight()
    {
        Light sun = Directional();
        if (sun == null)
            return;
        float yaw = _instance != null && _instance._ship != null ? _instance._ship.eulerAngles.y : 24f;
        sun.transform.rotation = Quaternion.Euler(16f, yaw + 78f, 0f);
        sun.intensity = 1.45f;
        sun.shadows = LightShadows.Soft;
        RenderSettings.ambientLight = new Color(0.14f, 0.18f, 0.26f);
    }

    public static void RestoreLight()
    {
        Light sun = Directional();
        if (sun == null)
            return;
        sun.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        sun.intensity = 1.05f;
        sun.color = new Color(1f, 0.96f, 0.88f);
        RenderSettings.ambientLight = new Color(0.5f, 0.56f, 0.64f);
    }

    void SetCabin(Step step)
    {
        if (_cabin == null)
            return;
        if (step == Step.Order)
        {
            _cabin.color = new Color(0.25f, 0.48f, 0.95f);
            _cabin.intensity = 0.85f;
            RenderSettings.ambientLight = new Color(0.04f, 0.05f, 0.09f);
        }
        else if (step == Step.Core)
        {
            _cabin.color = new Color(1f, 0.68f, 0.22f);
            _cabin.intensity = 2.4f;
            RenderSettings.ambientLight = new Color(0.1f, 0.07f, 0.03f);
        }
        else
        {
            _cabin.color = new Color(0.65f, 0.95f, 1f);
            _cabin.intensity = 3.4f;
            RenderSettings.ambientLight = new Color(0.08f, 0.14f, 0.16f);
        }
    }

    void DimSun()
    {
        Light sun = Directional();
        if (sun == null)
            return;
        sun.intensity = 0.04f;
    }

    static Light Directional()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
                return lights[i];
        }

        return null;
    }

    public static void Begin(CityGame game)
    {
        if (_instance != null)
            _instance.Board(game);
    }

    public static void Advance(CityGame game)
    {
        if (_instance != null)
            _instance.Tick(game);
    }

    public void Board(CityGame game)
    {
        _landed = false;
        _step = Step.Order;
        _order = 0;
        _prompt = "E   Touch mark 2";
        if (_seal != null)
            _seal.enabled = true;
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.025f, 0.04f);
        }
        if (_planet != null)
            _planet.gameObject.SetActive(false);
        DimSun();
        SetCabin(Step.Order);
        game.Teleport(_inside, Quaternion.LookRotation(Vector3.forward), 4.4f);
        game.Banner("Wake the breath core. The hatch stays shut.");
    }

    public void Tick(CityGame game)
    {
        if (_step == Step.Fall)
        {
            _prompt = null;
            _fall += Time.deltaTime;
            float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_fall / 7f));
            if (_ship != null)
                _ship.position = Vector3.Lerp(_fallFrom, _fallTo, u);
            if (_fall >= 7f)
                Arrive(game);
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E) || game.Player == null)
            return;
        Vector3 feet = game.Player.position;
        if (_step == Step.Order)
            TouchMark(feet);
        else if (_step == Step.Core && Flat(feet, _core) < 2.4f)
            LoadCore();
        else if (_step == Step.Hatch && Flat(feet, _hatch) < 2.6f)
            OpenHatch(game);
    }

    string GoalLine()
    {
        if (_landed || _step == Step.Fall)
            return null;
        if (_step == Step.Order)
            return "Wake order    2   -   1   -   3";
        if (_step == Step.Core)
            return "Seat the breath core. Then the hatch can open.";
        return "The core is in. Open the hatch and land.";
    }

    void LateUpdate()
    {
        if (_step != Step.Fall || _ship == null)
            return;
        Camera cam = Camera.main;
        if (cam == null)
            return;
        Vector3 pivot = _ship.position + Vector3.up * 3f;
        cam.transform.position = Vector3.Lerp(cam.transform.position, pivot - _ship.forward * 42f + Vector3.up * 12f, 1f - Mathf.Exp(-4f * Time.deltaTime));
        cam.transform.LookAt(pivot);
        cam.clearFlags = CameraClearFlags.Skybox;
        AimSky(cam.transform.eulerAngles.y);
    }

    void TouchMark(Vector3 feet)
    {
        int nearest = -1;
        float best = 2.3f;
        for (int i = 0; i < _marks.Length; i++)
        {
            float distance = Flat(feet, _marks[i].position);
            if (distance < best)
            {
                best = distance;
                nearest = i;
            }
        }

        if (nearest < 0)
            return;
        if (nearest != Order[_order])
        {
            _order = 0;
            _prompt = "Wrong mark. Start again at 2.";
            return;
        }

        _order++;
        if (_order >= Order.Length)
        {
            _step = Step.Core;
            SetCabin(Step.Core);
            _prompt = "E   Load the breath core";
            if (_orb != null)
                _orb.GetComponent<Renderer>().sharedMaterial = Glow(new Color(1f, 0.82f, 0.35f));
            return;
        }

        _prompt = "E   Touch mark " + (Order[_order] + 1);
    }

    void LoadCore()
    {
        _step = Step.Hatch;
        SetCabin(Step.Hatch);
        _prompt = "E   Open the hatch";
        if (_orb != null)
            _orb.transform.position = _hatch + Vector3.up * 1.3f;
    }

    void OpenHatch(CityGame game)
    {
        _step = Step.Fall;
        _prompt = null;
        if (_seal != null)
            _seal.enabled = false;
        _fall = 0f;
        _fallFrom = _ship.position;
        _fallTo = game.SpawnPoint + new Vector3(0f, 28f, 16f);
        game.LockForLanding();
        game.Banner("The city can breathe. We are going down.");
    }

    void Arrive(CityGame game)
    {
        _landed = true;
        _step = Step.Done;
        _prompt = null;
        if (_ship != null)
            _ship.gameObject.SetActive(false);
        game.Teleport(game.SpawnPoint, Quaternion.identity, 6.2f);
        game.UnlockAfterLanding();
        RestoreLight();
        game.Banner("You are down. Five minutes. Hold the light.");
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.42f, 0.62f, 0.84f);
        }
    }

    void CreateShip()
    {
        _ship = new GameObject("GeceGemisi").transform;
        _ship.position = new Vector3(0f, 170f, 4200f);
        _ship.rotation = Quaternion.Euler(-2f, 8f, -4f);
        CityArt.Load();
        if (CityArt.Bomber != null)
        {
            GameObject model = Instantiate(CityArt.Bomber, _ship);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            FitLength(model, 36f);
            Collider[] colliders = model.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }
        else
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(_ship, false);
            body.transform.localScale = new Vector3(16f, 1.4f, 28f);
            Destroy(body.GetComponent<Collider>());
        }

        UseSpace();
        CreatePlanet();
        var lamp = new GameObject("GemiIsigi");
        lamp.transform.SetParent(_ship, false);
        lamp.transform.localPosition = new Vector3(-7f, 2.2f, -8f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 46f;
        light.intensity = 3.4f;
        light.color = new Color(0.4f, 0.65f, 1f);
    }

    void CreateBay()
    {
        var root = new GameObject("GemiIci");
        _room = root.transform;
        Vector3 origin = new Vector3(5400f, 40f, 5400f);
        _room.position = origin;
        const float width = 16f;
        const float depth = 14f;
        Box(root.transform, origin + new Vector3(0f, -0.1f, 0f), new Vector3(width, 0.2f, depth), new Color(0.16f, 0.18f, 0.22f), true);
        Box(root.transform, origin + new Vector3(0f, 2.1f, -depth * 0.5f), new Vector3(width, 4.2f, 0.3f), new Color(0.2f, 0.24f, 0.3f), true);
        Box(root.transform, origin + new Vector3(-width * 0.5f, 2.1f, 0f), new Vector3(0.3f, 4.2f, depth), new Color(0.2f, 0.24f, 0.3f), true);
        Box(root.transform, origin + new Vector3(width * 0.5f, 2.1f, 0f), new Vector3(0.3f, 4.2f, depth), new Color(0.2f, 0.24f, 0.3f), true);
        Box(root.transform, origin + new Vector3(-4.2f, 2.1f, depth * 0.5f), new Vector3(7f, 4.2f, 0.3f), new Color(0.12f, 0.14f, 0.18f), true);
        Box(root.transform, origin + new Vector3(4.2f, 2.1f, depth * 0.5f), new Vector3(7f, 4.2f, 0.3f), new Color(0.12f, 0.14f, 0.18f), true);
        Box(root.transform, origin + new Vector3(0f, 4.15f, 0f), new Vector3(width, 0.3f, depth), new Color(0.1f, 0.12f, 0.15f), true);
        _seal = Box(root.transform, origin + new Vector3(0f, 1.4f, depth * 0.5f), new Vector3(2.2f, 2.8f, 0.35f), new Color(0.15f, 0.55f, 0.7f), true).GetComponent<BoxCollider>();
        _hatch = origin + new Vector3(0f, 0f, depth * 0.5f - 1.2f);
        Dress(origin, width, depth);
        _inside = origin + new Vector3(0f, 0.4f, -3.5f);
        _core = origin + new Vector3(0f, 0f, 1.2f);
        _marks = new Transform[3];
        _marks[0] = Mark(origin + new Vector3(-3.2f, 0f, -1.2f), new Color(0.3f, 0.85f, 1f));
        _marks[1] = Mark(origin + new Vector3(0f, 0f, -1.2f), new Color(1f, 0.78f, 0.28f));
        _marks[2] = Mark(origin + new Vector3(3.2f, 0f, -1.2f), new Color(0.85f, 0.9f, 0.95f));
        _orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _orb.name = "NefesCekirdegi";
        _orb.transform.SetParent(root.transform, false);
        _orb.transform.position = _core + Vector3.up * 1.15f;
        _orb.transform.localScale = Vector3.one * 0.55f;
        Destroy(_orb.GetComponent<Collider>());
        _orb.GetComponent<Renderer>().sharedMaterial = Glow(new Color(0.25f, 0.28f, 0.32f));

        var lamp = new GameObject("KabinIsigi");
        lamp.transform.position = origin + new Vector3(0f, 3.4f, 0f);
        _cabin = lamp.AddComponent<Light>();
        _cabin.type = LightType.Point;
        _cabin.range = 22f;
        _cabin.intensity = 0.9f;
        _cabin.color = new Color(0.25f, 0.45f, 0.9f);
        _cabin.shadows = LightShadows.Soft;
    }

    void Dress(Vector3 origin, float width, float depth)
    {
        if (CityArt.BayFloor == null)
            return;
        float span = 4f;
        for (float x = -width * 0.5f + 2f; x < width * 0.5f; x += span)
        {
            for (float z = -depth * 0.5f + 2f; z < depth * 0.5f; z += span)
                Piece(CityArt.BayFloor, origin + new Vector3(x, 0.02f, z), Quaternion.identity);
        }

        Piece(CityArt.BayDoor, _room.position + new Vector3(0f, 0f, depth * 0.5f - 0.2f), Quaternion.LookRotation(Vector3.back));
        if (CityArt.BayWall != null)
        {
            Piece(CityArt.BayWall, origin + new Vector3(-width * 0.5f + 0.4f, 0f, 0f), Quaternion.LookRotation(Vector3.right));
            Piece(CityArt.BayWall, origin + new Vector3(width * 0.5f - 0.4f, 0f, 0f), Quaternion.LookRotation(Vector3.left));
        }

        if (CityArt.BayRoof != null)
            Piece(CityArt.BayRoof, origin + new Vector3(0f, 4.5f, -depth * 0.5f + 1.5f), Quaternion.identity);
    }

    static void Piece(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return;
        GameObject model = Instantiate(prefab, position, rotation);
        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    Transform Mark(Vector3 position, Color color)
    {
        var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mark.transform.SetParent(_room, false);
        mark.transform.position = position + Vector3.up * 0.55f;
        mark.transform.localScale = new Vector3(0.7f, 0.55f, 0.7f);
        Destroy(mark.GetComponent<Collider>());
        mark.GetComponent<Renderer>().sharedMaterial = Glow(color);
        return mark.transform;
    }

    static GameObject Box(Transform parent, Vector3 position, Vector3 size, Color color, bool solid)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = size;
        if (!solid)
            Destroy(box.GetComponent<Collider>());
        Renderer renderer = box.GetComponent<Renderer>();
        renderer.sharedMaterial = Glow(color);
        return box;
    }

    static Material _sky;
    static Material _stars;

    void CreatePlanet()
    {
        CityArt.Load();
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Dunya";
        Destroy(quad.GetComponent<Collider>());
        _planet = quad.transform;
        Renderer renderer = quad.GetComponent<Renderer>();
        Shader shader = Shader.Find("Unlit/Texture");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        if (CityArt.EarthFace != null)
            material.mainTexture = CityArt.EarthFace;
        material.color = Color.white;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        var back = GameObject.CreatePrimitive(PrimitiveType.Quad);
        back.name = "DunyaArka";
        back.transform.SetParent(quad.transform, false);
        back.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        Destroy(back.GetComponent<Collider>());
        Renderer backRenderer = back.GetComponent<Renderer>();
        backRenderer.sharedMaterial = material;
        backRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        backRenderer.receiveShadows = false;
        quad.SetActive(false);
    }

    public static void ShowHull(bool on)
    {
        if (_instance == null || _instance._ship == null)
            return;
        Renderer[] renderers = _instance._ship.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = on;
    }

    public static void FramePlanet(Camera cam)
    {
        if (_instance == null || _instance._planet == null || cam == null)
            return;
        Transform plate = _instance._planet;
        plate.gameObject.SetActive(true);
        if (CityArt.StarSky != null)
        {
            if (_stars == null)
                _stars = new Material(CityArt.StarSky);
            RenderSettings.skybox = _stars;
        }
        cam.clearFlags = CameraClearFlags.Skybox;
        float dist = 36f;
        float halfH = dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * Mathf.Max(0.5f, cam.aspect);
        float quadW = halfW * 1.8f;
        float quadH = halfH * 3.1f;
        plate.localScale = new Vector3(quadW, quadH, 1f);
        float centerX = halfW * 0.62f;
        plate.position = cam.transform.position + cam.transform.forward * dist + cam.transform.right * centerX;
        plate.rotation = Quaternion.LookRotation(cam.transform.position - plate.position, cam.transform.up);
    }

    public static void AimSky(float yaw)
    {
        if (_sky != null)
            _sky.SetFloat("_Rotation", yaw - 18f);
    }

    static void UseSpace()
    {
        CityArt.Load();
        if (CityArt.SpaceSky == null)
            return;
        if (_sky == null)
            _sky = new Material(CityArt.SpaceSky);
        RenderSettings.skybox = _sky;
        Camera cam = Camera.main;
        if (cam != null)
            cam.clearFlags = CameraClearFlags.Skybox;
    }

    static void FitLength(GameObject model, float length)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        float longest = Mathf.Max(bounds.size.x, bounds.size.z);
        if (longest < 0.01f)
            return;
        model.transform.localScale = Vector3.one * (length / longest);
    }

    static float Flat(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static Material Glow(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        if (shader != null && shader.name == "Standard")
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.8f);
        }
        return material;
    }
}
