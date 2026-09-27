using UnityEngine;

public class LabInterior : MonoBehaviour
{
    public static bool IsInside { get; private set; }
    public static string Prompt { get; private set; }
    public static float BlockEntryUntil { get; private set; }
    public const float TimeLimit = 60f;
    public static int StepCount => Names.Length;
    public static int Completed => _instance == null ? 0 : _instance._done;
    public static int CurrentStep => _instance == null ? 0 : _instance.NextStep();
    public static float TimeLeft => _instance == null ? 0f : _instance._timeLeft;

    public static readonly string[] Names =
    {
        "Scan the data",
        "Plant sample",
        "Measure power",
        "Tune antenna",
        "Archive sample",
        "Decode signal",
        "Cool the core",
        "File the report"
    };

    static LabInterior _instance;
    static readonly Vector3 Origin = new Vector3(2500f, 0f, 2500f);
    const string Pack = "Assets/Sci-Fi Styled Modular Pack/Prefabs/";
    const float Cam = 3.2f;
    const int RoomVersion = 10;

    LabDoor _returnDoor;
    ScienceSpot[] _spots;
    int _done;
    Vector3 _spawn;
    float _lock;
    float _timeLeft;
    bool _clock;
    int _roomVersion;

    public static bool StepDone(int index)
    {
        if (_instance == null || _instance._spots == null || index < 0 || index >= _instance._spots.Length)
            return false;
        return _instance._spots[index].Done;
    }

    public static void Build()
    {
        if (_instance != null)
            return;

        LabInterior[] stale = FindObjectsByType<LabInterior>(FindObjectsSortMode.None);
        for (int i = 0; i < stale.Length; i++)
        {
            if (stale[i] != null)
                Destroy(stale[i].gameObject);
        }

        var go = new GameObject("Laboratuvar");
        go.transform.position = Origin;
        _instance = go.AddComponent<LabInterior>();
        _instance.Create();
    }

    public static void Enter(LabDoor door)
    {
        if (_instance == null)
            Build();

        IsInside = true;
        _instance._returnDoor = door;
        _instance._lock = 0.35f;
        _instance.Create();
        _instance._timeLeft = TimeLimit;
        _instance._clock = true;
        _instance.ApplyView();
        CityGame.Instance.Teleport(_instance._spawn, Quaternion.LookRotation(Vector3.right), Cam);
    }

    void Create()
    {
        Wipe();
        _spawn = Origin + new Vector3(6.2f, 0.35f, 8f);
        Shell();

        _spots = new[]
        {
            PlaceSpot(CityArt.Console, new Vector3(8f, 0f, 3.2f), 0),
            PlaceSpot(CityArt.Plant, new Vector3(14f, 0f, 12.6f), 1),
            PlaceSpot(CityArt.Generator, new Vector3(20f, 0f, 3.2f), 2),
            PlaceSpot(CityArt.Console, new Vector3(5.2f, 0f, 12.2f), 3),
            PlaceSpot(CityArt.Plant, new Vector3(18.2f, 0f, 12.2f), 4),
            PlaceSpot(CityArt.Console, new Vector3(21.4f, 0f, 8.2f), 5),
            PlaceSpot(CityArt.Generator, new Vector3(10.2f, 0f, 5.2f), 6),
            PlaceSpot(CityArt.Console, new Vector3(16.2f, 0f, 6.4f), 7)
        };

        Furnish();
        _done = 0;
        _roomVersion = RoomVersion;
    }

    public static void Eject()
    {
        if (_instance != null && IsInside)
            _instance.Leave(false);
    }

    void Wipe()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    void ApplyView()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
    }

    void Shell()
    {
        string[] floors =
        {
            "Floors/floor_1.prefab",
            "Floors/floor_2.prefab",
            "Floors/floor_3.prefab",
            "Floors/floor_5.prefab"
        };
        string[] walls =
        {
            "Walls/decorative_wall_1.prefab",
            "Walls/decorative_wall_2.prefab",
            "Walls/decorative_wall_3.prefab",
            "Walls/decorative_wall_4_computer.prefab",
            "Walls/decorative_wall_5.prefab",
            "Walls/decorative_wall_6.prefab"
        };

        const int tilesX = 6;
        const int tilesZ = 4;
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                string floor = floors[(x + z) % floors.Length];
                var cell = new Vector3(2f + x * 4f, 0f, 2f + z * 4f);
                Module(floor, cell, Vector3.zero, true);
                Module(floor, cell + new Vector3(0f, 4f, 0f), new Vector3(180f, (x + z) * 90f, 0f), false);
                if ((x + z) % 2 == 0)
                    Module("Lights/light_celing_2.prefab", cell + new Vector3(0f, 3.85f, 0f), Vector3.zero, false);
            }
        }

        for (int x = 0; x < tilesX; x++)
        {
            Module(walls[x % walls.Length], new Vector3(2f + x * 4f, 0f, 0f), Vector3.zero, true);
            Module(walls[(x + 3) % walls.Length], new Vector3(2f + x * 4f, 0f, tilesZ * 4f), new Vector3(0f, 180f, 0f), true);
        }

        for (int z = 0; z < tilesZ; z++)
        {
            Module(walls[z % walls.Length], new Vector3(0f, 0f, 2f + z * 4f), new Vector3(0f, 90f, 0f), true);
            Module(walls[(z + 2) % walls.Length], new Vector3(tilesX * 4f, 0f, 2f + z * 4f), new Vector3(0f, -90f, 0f), true);
        }

        for (int i = 0; i < 4; i++)
            Lamp(new Vector3(4f + i * 5f, 3.4f, 8f), new Color(0.8f, 0.9f, 1f), 11f, 1.3f);
    }

    void Furnish()
    {
        Seat("Decorative elements/computer_station.prefab", new Vector3(6.2f, 0f, 3.1f), 0f, 1.15f);
        Seat("Decorative elements/console_screen.prefab", new Vector3(9.4f, 0f, 2.6f), 0f, 1.2f);
        Seat("Decorative elements/decorative_chair.prefab", new Vector3(8f, 0f, 4.6f), 180f, 1f);
        Seat("Decorative elements/cabinet.prefab", new Vector3(4.2f, 0f, 3.2f), 0f, 1.8f);
        Seat("Decorative elements/shelf.prefab", new Vector3(11.2f, 0f, 2.4f), 0f, 1.8f);

        Seat("Decorative elements/hydroponic.prefab", new Vector3(12.2f, 0f, 12.4f), 180f, 1.5f);
        Seat("Decorative elements/decorative_plant.prefab", new Vector3(15.4f, 0f, 12.2f), 20f, 1.15f);
        Seat("Decorative elements/decorative_plant_desk.prefab", new Vector3(16.6f, 0f, 11.2f), 180f, 1f);
        Seat("Decorative elements/decorative_chair.prefab", new Vector3(14f, 0f, 10.4f), 0f, 1f);
        Seat("Decorative elements/Tables/decorative_table_glass.prefab", new Vector3(14f, 0f, 8.8f), 0f, 0.8f);

        Seat("Machines/Battery_big.prefab", new Vector3(18.2f, 0f, 3.2f), 0f, 1.4f);
        Seat("Machines/Battery.prefab", new Vector3(21.4f, 0f, 3.4f), 15f, 0.9f);
        Seat("Machines/Capacitor.prefab", new Vector3(21.2f, 0f, 5.2f), 0f, 1.1f);
        Seat("Decorative elements/decorative_chair.prefab", new Vector3(20f, 0f, 5.2f), 180f, 1f);
        Seat("Decorative elements/console.prefab", new Vector3(18.4f, 0f, 5.4f), 0f, 1.2f);

        Seat("Decorative elements/big_screen.prefab", new Vector3(12f, 0f, 1.6f), 0f, 2.2f);
        Seat("Decorative elements/bulletin_board_big.prefab", new Vector3(1.5f, 0f, 8f), 90f, 1.8f);
        Seat("Machines/vending_machine.prefab", new Vector3(22.2f, 0f, 8f), -90f, 1.8f);
        Seat("Decorative elements/shelf_small.prefab", new Vector3(4.2f, 0f, 12.4f), 180f, 1.5f);
        Seat("Machines/storage_container_small.prefab", new Vector3(6.4f, 0f, 12.6f), 10f, 1f);
        Seat("Decorative elements/computer_station.prefab", new Vector3(10f, 0f, 10.4f), 180f, 1.15f);
        Seat("Decorative elements/decorative_chair.prefab", new Vector3(10f, 0f, 9.2f), 0f, 1f);
        Seat("Decorative elements/Arts/art_1.prefab", new Vector3(16f, 0f, 1.5f), 0f, 1.6f);
        Seat("Lights/light_wall_1.prefab", new Vector3(8f, 2.2f, 1.2f), 0f, 0.6f);
        Seat("Lights/light_wall_1.prefab", new Vector3(16f, 2.2f, 14.6f), 180f, 0.6f);
    }

    void Module(string path, Vector3 local, Vector3 euler, bool solid)
    {
        GameObject prefab = CityArt.LoadProp(Pack + path);
        if (prefab == null)
            return;
        GameObject model = Instantiate(prefab, transform);
        model.transform.localPosition = local;
        model.transform.localRotation = Quaternion.Euler(euler);
        if (!solid)
            Strip(model);
    }

    void Update()
    {
        if (_roomVersion != RoomVersion)
        {
            Create();
            if (IsInside && CityGame.Instance != null)
            {
                ApplyView();
                CityGame.Instance.Teleport(_spawn, Quaternion.LookRotation(Vector3.right), Cam);
            }
        }

        if (!IsInside || CityGame.Instance == null)
        {
            Prompt = null;
            _clock = false;
            return;
        }

        if (_clock)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                Fail();
                return;
            }
        }

        if (_lock > 0f)
            _lock -= Time.deltaTime;

        Transform player = CityGame.Instance.Player;
        if (player.position.y < Origin.y - 2f)
        {
            CityGame.Instance.Teleport(_spawn, Quaternion.LookRotation(Vector3.right), Cam);
            return;
        }

        int step = NextStep();
        bool interact = _lock <= 0f && Input.GetKeyDown(KeyCode.E);
        if (interact && step < _spots.Length && Vector3.Distance(player.position, _spots[step].Point) <= 2.5f)
        {
            _spots[step].Complete();
            _done++;
            interact = false;
        }

        if (interact && Vector3.Distance(player.position, _spawn) < 2.5f)
            Leave();

        RefreshLamps();
        Prompt = Hint(player.position, NextStep());
    }

    int NextStep()
    {
        for (int i = 0; i < _spots.Length; i++)
        {
            if (!_spots[i].Done)
                return i;
        }
        return _spots.Length;
    }

    void RefreshLamps()
    {
        int step = NextStep();
        for (int i = 0; i < _spots.Length; i++)
        {
            if (_spots[i].Lamp == null)
                continue;
            if (_spots[i].Done)
                _spots[i].Lamp.color = new Color(0.25f, 1f, 0.45f);
            else if (i == step)
                _spots[i].Lamp.color = Color.Lerp(new Color(0.2f, 0.75f, 1f), Color.white, Mathf.PingPong(Time.time, 0.6f));
            else
                _spots[i].Lamp.color = new Color(0.25f, 0.28f, 0.32f);

            if (_spots[i].Number != null)
            {
                _spots[i].Number.color = _spots[i].Done
                    ? new Color(0.45f, 1f, 0.6f)
                    : i == step ? new Color(0.7f, 0.95f, 1f) : new Color(0.45f, 0.5f, 0.55f);
                if (Camera.main != null)
                    _spots[i].Number.transform.rotation = Quaternion.LookRotation(_spots[i].Number.transform.position - Camera.main.transform.position);
            }
        }
    }

    string Hint(Vector3 player, int step)
    {
        if (step < _spots.Length && Vector3.Distance(player, _spots[step].Point) <= 2.5f)
            return "E   " + (step + 1) + "  " + Names[step];

        if (Vector3.Distance(player, _spawn) <= 2.5f)
            return step >= _spots.Length ? "E   Tasks done, step outside" : "E   Step outside";

        return null;
    }

    void Fail()
    {
        _clock = false;
        _timeLeft = 0f;
        if (_spots != null)
        {
            for (int i = 0; i < _spots.Length; i++)
                _spots[i].Reset();
        }
        _done = 0;
        if (CityGame.Instance != null)
            CityGame.Instance.Banner("Time's up");
        Leave(true);
    }

    void Leave()
    {
        Leave(true);
    }

    void Leave(bool allowFinish)
    {
        bool finished = allowFinish && _spots != null && _done >= _spots.Length;
        _clock = false;
        LabDoor door = _returnDoor;
        if (finished)
        {
            for (int i = 0; i < _spots.Length; i++)
                _spots[i].Reset();
            _done = 0;
            if (door != null)
                LabDoor.ActivateNext(door.transform.position);
            if (CityGame.Instance != null)
                CityGame.Instance.Banner("Lab complete");
        }

        IsInside = false;
        BlockEntryUntil = Time.time + 0.8f;
        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.42f, 0.62f, 0.84f);
        if (door != null)
        {
            door.ArmCooldown();
            CityGame.Instance.Teleport(door.ExitPoint, door.ExitRotation, 6.2f);
        }
    }

    ScienceSpot PlaceSpot(GameObject prefab, Vector3 local, int index)
    {
        Vector3 world = Origin + local;
        if (prefab != null)
        {
            GameObject model = Instantiate(prefab, transform);
            model.transform.localPosition = local + new Vector3(0f, 0.95f, 0f);
            Vector3 face = new Vector3(0f, 0f, -Mathf.Sign(local.z));
            model.transform.localRotation = Quaternion.LookRotation(face, Vector3.up);
            SeatObject(model, 1.05f, 0.95f);
        }

        var number = new GameObject("GorevNo");
        number.transform.SetParent(transform, false);
        number.transform.localPosition = local + new Vector3(0f, 2.5f, 0f);
        TextMesh text = number.AddComponent<TextMesh>();
        text.text = (index + 1).ToString();
        text.fontSize = 64;
        text.characterSize = 0.05f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(0.7f, 0.95f, 1f);

        return new ScienceSpot
        {
            Point = world + new Vector3(0f, 0f, -Mathf.Sign(local.z) * 1.3f),
            Lamp = Lamp(local + new Vector3(0f, 2.2f, 0f), new Color(0.25f, 0.28f, 0.32f), 4.2f, 1.6f),
            Number = text
        };
    }

    void Seat(string path, Vector3 local, float yaw, float height)
    {
        GameObject prefab = CityArt.LoadProp(Pack + path);
        if (prefab == null)
            return;
        GameObject model = Instantiate(prefab, transform);
        model.transform.localPosition = local;
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        SeatObject(model, height, local.y);
    }

    void SeatObject(GameObject model, float height, float floorY)
    {
        Strip(model);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = BoundsOf(renderers);
        if (bounds.size.y > 0.01f)
            model.transform.localScale *= height / bounds.size.y;

        bounds = BoundsOf(model.GetComponentsInChildren<Renderer>());
        float lift = (Origin.y + floorY) - bounds.min.y;
        model.transform.position += Vector3.up * lift;
    }

    static Bounds BoundsOf(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void Strip(GameObject model)
    {
        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    Light Lamp(Vector3 local, Color color, float range, float intensity)
    {
        var lamp = new GameObject("Isik");
        lamp.transform.SetParent(transform, false);
        lamp.transform.localPosition = local;
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
        return light;
    }

    class ScienceSpot
    {
        public Vector3 Point;
        public Light Lamp;
        public TextMesh Number;
        public bool Done;

        public void Complete()
        {
            Done = true;
        }

        public void Reset()
        {
            Done = false;
        }
    }
}
