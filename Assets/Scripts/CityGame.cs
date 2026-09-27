using UnityEngine;

[DefaultExecutionOrder(-50)]
public class CityGame : MonoBehaviour
{
    public static CityGame Instance { get; private set; }

    Transform _player;
    PlayerMotor _motor;
    CharacterController _controller;
    Renderer[] _playerRenderers;
    CityCamera _camera;
    Camera _minimap;
    DriveableCar _currentCar;
    DriveableCar _nearestCar;
    bool _cockpit;
    Vector3 _spawn;
    string _prompt;
    Vital _vital;
    PlayerCombat _combat;
    float _downUntil;
    string _banner;
    float _bannerUntil;
    string _deathLine;
    float _runLeft = -1f;
    int _nagMinute = 5;
    bool _runOver;
    bool _runWon;
    bool _gameOver;
    int _lives = 3;
    bool _lightsOn;
    bool _duskWarned;
    Light _sun;
    GUIStyle _runTime;
    GUIStyle _promptStyle;
    GUIStyle _taskTitle;
    GUIStyle _taskName;
    GUIStyle _taskNum;
    GUIStyle _clock;
    GUIStyle _bannerStyle;
    Texture2D _promptBg;
    Texture2D _promptBar;
    Texture2D _taskBg;
    Texture2D _taskIdle;
    Texture2D _taskOn;
    Texture2D _taskDone;
    Texture2D _hpBg;
    Texture2D _hpFill;
    Texture2D _hpLow;

    public Transform Player => _player;
    public Vital Body => _vital;
    public bool Driving => _currentCar != null;
    public bool ShipBehind => _currentCar != null && !_cockpit;

    public Vector3 DrivingForward()
    {
        return _currentCar == null ? Vector3.forward : _currentCar.transform.forward;
    }

    public void Banner(string text)
    {
        _banner = text;
        _bannerUntil = Time.time + 2.4f;
    }

    public void Kill(string reason)
    {
        if (_vital == null || !_vital.Alive || _downUntil > 0f)
            return;
        _deathLine = reason;
        _vital.Hurt(_vital.Max);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<CityGame>() != null)
            return;

        var go = new GameObject("CityGame");
        go.AddComponent<CityGame>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.AddComponent<GameMenu>();
        _spawn = CityBuilder.Build();
        CityBuilder.SpawnShips();
        CityBuilder.SpawnRings();
        CityBuilder.SpawnSkyline();
        FieldScan.SpawnAll();
        CreatePlayer();
        SetupCamera();
        SetupSun();
        LabInterior.Build();
        LabDoor.CreateAt(_spawn + new Vector3(0f, -0.25f, 8f), Quaternion.LookRotation(Vector3.back));
        LabDoor.ActivateClosest(_spawn);
    }

    void CreatePlayer()
    {
        var player = new GameObject("Oyuncu");
        player.layer = 2;
        player.transform.position = _spawn;
        _controller = player.AddComponent<CharacterController>();
        _controller.height = 1.8f;
        _controller.radius = 0.35f;
        _controller.center = new Vector3(0f, 0.9f, 0f);
        _controller.stepOffset = 0.4f;
        _controller.skinWidth = 0.06f;
        _motor = player.AddComponent<PlayerMotor>();
        _vital = player.AddComponent<Vital>();
        _vital.Team = 0;
        _vital.Max = 100f;
        _vital.Current = 100f;
        _vital.Died += OnPlayerDown;
        _combat = player.AddComponent<PlayerCombat>();
        _player = player.transform;

        if (!CharacterVisual.Attach(player.transform, _motor, out _playerRenderers))
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Govde";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Destroy(body.GetComponent<Collider>());
            Paint(body, new Color(0.15f, 0.45f, 0.75f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Kafa";
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0.05f);
            head.transform.localScale = Vector3.one * 0.38f;
            Destroy(head.GetComponent<Collider>());
            Paint(head, new Color(0.82f, 0.68f, 0.54f));
            _playerRenderers = player.GetComponentsInChildren<Renderer>();
        }

        foreach (Transform child in player.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = 2;
    }

    void SetupCamera()
    {
        Camera main = Camera.main;
        if (main == null)
        {
            var camObject = new GameObject("Main Camera");
            camObject.tag = "MainCamera";
            main = camObject.AddComponent<Camera>();
            camObject.AddComponent<AudioListener>();
        }
        else if (main.GetComponent<AudioListener>() == null)
        {
            main.gameObject.AddComponent<AudioListener>();
        }

        main.clearFlags = CameraClearFlags.SolidColor;
        main.backgroundColor = new Color(0.42f, 0.62f, 0.84f);
        main.nearClipPlane = 0.12f;
        main.farClipPlane = 1400f;
        main.fieldOfView = 68f;
        _camera = main.GetComponent<CityCamera>();
        if (_camera == null)
            _camera = main.gameObject.AddComponent<CityCamera>();
        _camera.Follow(_player, new Vector3(0f, 1.5f, 0f), 6.2f, true);
        _motor.Bind(_camera);
        CharacterVisual visual = _player.GetComponentInChildren<CharacterVisual>();
        Transform muzzle = visual != null ? visual.Muzzle : null;
        _combat.Bind(_camera, muzzle);

        var mapObject = new GameObject("Harita");
        _minimap = mapObject.AddComponent<Camera>();
        _minimap.orthographic = true;
        _minimap.orthographicSize = 78f;
        _minimap.rect = new Rect(0.015f, 0.025f, 0.2f, 0.26f);
        _minimap.clearFlags = CameraClearFlags.SolidColor;
        _minimap.backgroundColor = new Color(0.12f, 0.16f, 0.14f);
        _minimap.depth = 10f;
        _minimap.nearClipPlane = 0.3f;
        _minimap.farClipPlane = 200f;
        if (mapObject.GetComponent<AudioListener>() != null)
            Destroy(mapObject.GetComponent<AudioListener>());
    }

    void SetupSun()
    {
        _sun = null;
        Light sun = null;
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type == LightType.Directional)
            {
                sun = lights[i];
                break;
            }
        }

        if (sun == null)
        {
            var sunObject = new GameObject("Gunes");
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        sun.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        sun.intensity = 1.05f;
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.shadows = LightShadows.Soft;
        _sun = sun;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.56f, 0.64f);
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_minimap != null)
            _minimap.enabled = GameMenu.Playing;

        TickRun();

        if (_downUntil > 0f && Time.time >= _downUntil && !_gameOver)
        {
            _downUntil = 0f;
            _vital.Fill();
            _vital.SafeUntil = Time.time + 2.2f;
            Teleport(_spawn, Quaternion.identity, 6.2f);
            if (!_runOver)
                _motor.Locked = false;
        }

        bool driving = _currentCar != null;
        _nearestCar = driving ? null : FindNearestCar(7f);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (driving && _currentCar.Altitude <= 8f)
                ExitCar();
            else if (!driving && _nearestCar != null)
                EnterCar(_nearestCar);
        }

        if (driving && Input.GetKeyDown(KeyCode.V))
            ToggleShipView();

        float speed01 = 0f;

        if (_currentCar != null)
        {
            float lift = 0f;
            if (Input.GetKey(KeyCode.Space))
                lift += 1f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                lift -= 1f;
            _currentCar.SetFlightInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), lift, Input.GetKey(KeyCode.LeftShift));
            if (Input.GetKeyDown(KeyCode.R))
                _currentCar.ResetPose();
            speed01 = Mathf.Clamp01(_currentCar.SpeedKmh / 220f);
            FollowMinimap(_currentCar.transform);
        }
        else
        {
            FollowMinimap(_player);
            if (!LabInterior.IsInside && Input.GetKeyDown(KeyCode.E))
            {
                if (!_lightsOn && StreetLight.Near(_player.position, 4.2f))
                    SwitchLights();
                else
                    FieldScan.TryNearest(_player.position);
            }
            if (!LabInterior.IsInside && _player.position.y < -8f)
                _player.position = _spawn;
        }

        _camera.SpeedPull = 0f;
        if (GameMenu.Playing)
        {
            Camera view = _camera.GetComponent<Camera>();
            float cruise = _currentCar != null ? Mathf.Lerp(74f, 86f, speed01) : 68f;
            float zoom = _currentCar != null || _combat == null ? 0f : _combat.Zoom;
            float want = Mathf.Lerp(cruise, 14f, zoom);
            float blend = 1f - Mathf.Exp(-7f * Time.deltaTime);
            view.fieldOfView = Mathf.Lerp(view.fieldOfView, want, blend);
            ApplyDusk();
        }
    }

    void SwitchLights()
    {
        _lightsOn = true;
        StreetLight.IgniteAll();
        Banner("Lights on");
    }

    void ApplyDusk()
    {
        if (_runLeft < 0f)
            return;

        float elapsed = 300f - _runLeft;
        float dusk = Mathf.Clamp01((elapsed - 120f) / 28f);
        Color dayAmbient = new Color(0.5f, 0.56f, 0.64f);
        Color nightAmbient = _lightsOn ? new Color(0.16f, 0.18f, 0.24f) : new Color(0.02f, 0.025f, 0.04f);
        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, dusk);
        if (_sun != null)
        {
            _sun.intensity = Mathf.Lerp(1.05f, _lightsOn ? 0.18f : 0.02f, dusk);
            _sun.color = Color.Lerp(new Color(1f, 0.96f, 0.88f), new Color(0.35f, 0.42f, 0.62f), dusk);
        }

        if (!LabInterior.IsInside)
        {
            Camera view = _camera.GetComponent<Camera>();
            Color daySky = new Color(0.42f, 0.62f, 0.84f);
            Color nightSky = _lightsOn ? new Color(0.05f, 0.07f, 0.12f) : new Color(0.01f, 0.012f, 0.02f);
            view.backgroundColor = Color.Lerp(daySky, nightSky, dusk);
        }
    }

    void LateUpdate()
    {
        if (_currentCar != null && _currentCar.transform.position.y < -8f)
            _currentCar.ResetPose();

        _prompt = LabInterior.IsInside
            ? LabInterior.Prompt
            : LabDoor.PromptFor(_player.position, Driving);
        if (_currentCar != null)
            _prompt = "V   Camera";
        else if (!LabInterior.IsInside && string.IsNullOrEmpty(_prompt))
            _prompt = FieldScan.PromptNear(_player.position);
        if (_currentCar == null && !LabInterior.IsInside && string.IsNullOrEmpty(_prompt) && !_lightsOn && StreetLight.Near(_player.position, 4.2f))
            _prompt = "E   Turn on the lights";
        if (_currentCar == null && string.IsNullOrEmpty(_prompt) && _nearestCar != null)
            _prompt = "F   Board ship";
    }

    public void Teleport(Vector3 position, Quaternion rotation, float cameraDistance)
    {
        _controller.enabled = false;
        _player.SetPositionAndRotation(position, rotation);
        _controller.enabled = true;
        _motor.Locked = false;
        _camera.Follow(_player, new Vector3(0f, 1.45f, 0f), cameraDistance, true);
        if (_minimap != null)
            _minimap.enabled = !LabInterior.IsInside;
    }

    void OnGUI()
    {
        if (!GameMenu.Playing)
            return;

        EnsureTaskUi();
        DrawHealth();
        DrawRunClock();
        DrawCrosshair();
        DrawScope();
        DrawBanner();
        DrawGameOver();
        DrawAimButton();
        if (LabInterior.IsInside)
            DrawTaskBoard();
        else
            DrawCityBoard();

        if (string.IsNullOrEmpty(_prompt))
            return;

        float promptY = LabInterior.IsInside ? Screen.height - 168f : Screen.height - 72f;
        var rect = new Rect(Screen.width * 0.5f - 210f, promptY, 420f, 36f);
        GUI.DrawTexture(rect, _promptBg);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), _promptBar);
        GUI.Label(rect, _prompt, _promptStyle);
    }

    void EnsureTaskUi()
    {
        if (_promptStyle != null)
            return;

        _promptStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _promptStyle.normal.textColor = new Color(0.9f, 0.96f, 1f);
        _taskTitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _taskTitle.normal.textColor = new Color(0.45f, 0.82f, 0.95f);
        _taskName = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft
        };
        _taskNum = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _clock = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight
        };
        _bannerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _bannerStyle.normal.textColor = new Color(0.9f, 0.96f, 1f);
        _runTime = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _promptBg = Pixel(new Color(0.07f, 0.1f, 0.15f, 0.94f));
        _promptBar = Pixel(new Color(0.25f, 0.72f, 0.9f, 1f));
        _taskBg = Pixel(new Color(0.06f, 0.09f, 0.13f, 0.94f));
        _taskIdle = Pixel(new Color(0.12f, 0.16f, 0.2f, 1f));
        _taskOn = Pixel(new Color(0.12f, 0.32f, 0.42f, 1f));
        _taskDone = Pixel(new Color(0.12f, 0.32f, 0.22f, 1f));
        _hpBg = Pixel(new Color(0.08f, 0.1f, 0.12f, 0.9f));
        _hpFill = Pixel(new Color(0.25f, 0.78f, 0.9f, 1f));
        _hpLow = Pixel(new Color(0.9f, 0.28f, 0.22f, 1f));
    }

    void TickRun()
    {
        if (!GameMenu.Playing || _runOver || _runWon)
            return;

        if (_runLeft < 0f)
        {
            _runLeft = 300f;
            _nagMinute = 5;
            Banner("Five minutes. Finish the mission. Go.");
            return;
        }

        _runLeft -= Time.deltaTime;
        if (MissionClear())
        {
            _runWon = true;
            Banner("Mission complete");
            return;
        }

        if (_runLeft <= 0f)
        {
            _runLeft = 0f;
            _runOver = true;
            if (_currentCar != null)
                ExitCar();
            LabInterior.Eject();
            _motor.Locked = true;
            Banner("Time's up");
            return;
        }

        int minute = Mathf.CeilToInt(_runLeft / 60f);
        if (minute < _nagMinute && minute > 0)
        {
            _nagMinute = minute;
            Banner(Nag(minute));
        }

        if (!_duskWarned && _runLeft <= 180f)
        {
            _duskWarned = true;
            Banner(_lightsOn
                ? "Night is falling. The lights are on."
                : "Turn the lights on first, or you will be left in the dark.");
        }
    }

    static bool MissionClear()
    {
        return LabInterior.ClearedOnce
            && SkyRing.Total > 0 && SkyRing.Cleared >= SkyRing.Total
            && FieldScan.Total > 0 && FieldScan.DoneCount >= FieldScan.Total;
    }

    static string Nag(int minute)
    {
        if (minute >= 4)
            return "Four minutes left. Time is running out. Hurry.";
        if (minute == 3)
            return "Three minutes. Finish your tasks. Move.";
        if (minute == 2)
            return "Two minutes. Your time is slipping. Go.";
        return "One minute. Complete the mission. Now.";
    }

    void DrawRunClock()
    {
        if (_runTime == null)
            return;

        int secs = Mathf.CeilToInt(Mathf.Max(0f, _runLeft));
        _runTime.normal.textColor = secs <= 60 ? new Color(1f, 0.42f, 0.32f) : new Color(0.75f, 0.95f, 1f);
        var panel = new Rect(Screen.width - 132f, 18f, 114f, 58f);
        GUI.DrawTexture(panel, _taskBg);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 3f, panel.height), _promptBar);
        GUI.Label(new Rect(panel.x + 8f, panel.y + 4f, panel.width - 12f, 16f), "TIME", _taskTitle);
        GUI.Label(new Rect(panel.x + 6f, panel.y + 22f, panel.width - 12f, 28f), secs / 60 + ":" + (secs % 60).ToString("00"), _runTime);
    }

    public static bool PointerOnAim()
    {
        if (Instance == null || !GameMenu.Playing || Instance._currentCar != null)
            return false;
        var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        return AimRect().Contains(mouse);
    }

    static Rect AimRect()
    {
        return new Rect(Screen.width - 96f, Screen.height * 0.5f - 38f, 76f, 76f);
    }

    void DrawAimButton()
    {
        if (_currentCar != null || _taskBg == null || _combat == null)
            return;

        Rect rect = AimRect();
        bool held = _combat.ZoomHeld;
        GUI.DrawTexture(rect, held ? _taskOn : _taskBg);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 3f, rect.height), held ? _hpLow : _promptBar);
        if (GUI.Button(rect, "AIM", _promptStyle))
            _combat.ToggleZoom();
    }

    void DrawHealth()
    {
        if (_vital == null)
            return;

        var bar = new Rect(18f, 16f, 210f, 16f);
        GUI.DrawTexture(bar, _hpBg);
        float ratio = _vital.Max <= 0f ? 0f : Mathf.Clamp01(_vital.Current / _vital.Max);
        GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * ratio, bar.height), ratio < 0.35f ? _hpLow : _hpFill);
        GUI.Label(new Rect(bar.x + 8f, bar.y - 1f, 80f, bar.height), "HP", _taskTitle);
        GUI.Label(new Rect(bar.xMax + 10f, bar.y - 1f, 90f, bar.height), "LIVES  " + _lives, _taskTitle);
    }

    void DrawCrosshair()
    {
        if ((_currentCar != null && _cockpit) || _vital == null || !_vital.Alive)
            return;

        const float arm = 7f;
        float x = Screen.width * 0.5f;
        float y = Screen.height * 0.5f;
        Texture2D mark = _combat != null && _combat.LockedOn ? _hpLow : _promptBar;
        GUI.DrawTexture(new Rect(x - 1f, y - arm, 2f, arm * 2f), mark);
        GUI.DrawTexture(new Rect(x - arm, y - 1f, arm * 2f, 2f), mark);
    }

    void DrawScope()
    {
        if (_combat == null || _combat.Zoom < 0.4f || _promptBar == null)
            return;

        float x = Screen.width * 0.5f;
        float y = Screen.height * 0.5f;
        float span = Mathf.Lerp(70f, 128f, _combat.Zoom);
        float len = 22f;
        float thick = 2f;
        Corner(x - span, y - span, len, thick, 1f, 1f);
        Corner(x + span, y - span, len, thick, -1f, 1f);
        Corner(x - span, y + span, len, thick, 1f, -1f);
        Corner(x + span, y + span, len, thick, -1f, -1f);
    }

    void Corner(float x, float y, float len, float thick, float hx, float hy)
    {
        GUI.DrawTexture(new Rect(hx > 0f ? x : x - len, y - thick * 0.5f, len, thick), _promptBar);
        GUI.DrawTexture(new Rect(x - thick * 0.5f, hy > 0f ? y : y - len, thick, len), _promptBar);
    }

    void DrawBanner()
    {
        if (string.IsNullOrEmpty(_banner) || Time.time > _bannerUntil)
            return;
        float width = Mathf.Min(680f, Screen.width - 170f);
        var rect = new Rect((Screen.width - width) * 0.5f, 22f, width, 40f);
        GUI.DrawTexture(rect, _promptBg);
        GUI.Label(rect, _banner, _bannerStyle);
    }

    void OnPlayerDown()
    {
        if (_downUntil > 0f || _gameOver)
            return;

        if (_currentCar != null)
            ExitCar();
        LabInterior.Eject();
        _motor.Locked = true;
        _lives = Mathf.Max(0, _lives - 1);
        if (_lives <= 0)
        {
            _gameOver = true;
            _downUntil = 0f;
            _deathLine = null;
            Banner("Game over");
            return;
        }

        _downUntil = Time.time + 1.25f;
        string why = string.IsNullOrEmpty(_deathLine) ? "You were hit" : _deathLine;
        _deathLine = null;
        Banner(why + "    " + _lives + " left");
    }

    void DrawGameOver()
    {
        if (!_gameOver || _bannerStyle == null)
            return;

        var rect = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.36f, 440f, 72f);
        GUI.DrawTexture(rect, _promptBg);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), _hpLow);
        GUI.Label(rect, "GAME OVER", _bannerStyle);
    }

    void DrawCityBoard()
    {
        float w = 560f;
        float h = 92f;
        var board = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 16f, w, h);
        GUI.DrawTexture(board, _taskBg);
        GUI.DrawTexture(new Rect(board.x, board.y, board.width, 3f), _promptBar);
        GUI.Label(new Rect(board.x + 16f, board.y + 8f, 200f, 18f), "CITY", _taskTitle);
        GUI.Label(new Rect(board.x + 16f, board.y + 32f, board.width - 32f, 22f), "Rings     " + SkyRing.Cleared + " / " + SkyRing.Total, _taskName);
        GUI.Label(new Rect(board.x + 16f, board.y + 56f, board.width - 32f, 22f), "Signal towers     " + FieldScan.DoneCount + " / " + FieldScan.Total, _taskName);
    }

    void DrawTaskBoard()
    {
        int count = LabInterior.StepCount;
        const int columns = 4;
        int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)columns));
        float w = 760f;
        float cellH = 34f;
        float h = 34f + rows * (cellH + 6f);
        var board = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 16f, w, h);
        GUI.DrawTexture(board, _taskBg);
        GUI.DrawTexture(new Rect(board.x, board.y, board.width, 3f), _promptBar);
        GUI.Label(new Rect(board.x + 16f, board.y + 6f, 220f, 18f), "LAB", _taskTitle);
        int secs = Mathf.CeilToInt(Mathf.Max(0f, LabInterior.TimeLeft));
        _clock.normal.textColor = secs <= 15 ? new Color(1f, 0.42f, 0.32f) : new Color(0.75f, 0.95f, 1f);
        GUI.Label(new Rect(board.xMax - 86f, board.y + 6f, 70f, 18f), secs / 60 + ":" + (secs % 60).ToString("00"), _clock);
        GUI.Label(new Rect(board.xMax - 168f, board.y + 6f, 74f, 18f), LabInterior.Completed + " / " + count, _taskTitle);

        float gap = 8f;
        float col = (board.width - 32f - gap * (columns - 1)) / columns;
        for (int i = 0; i < count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            bool done = LabInterior.StepDone(i);
            bool current = !done && i == LabInterior.CurrentStep;
            var cell = new Rect(board.x + 16f + column * (col + gap), board.y + 30f + row * (cellH + 6f), col, cellH);
            GUI.DrawTexture(cell, done ? _taskDone : current ? _taskOn : _taskIdle);
            GUI.DrawTexture(new Rect(cell.x, cell.y, 28f, cell.height), done ? _taskDone : current ? _promptBar : _taskIdle);

            _taskNum.normal.textColor = done ? new Color(0.7f, 1f, 0.8f) : current ? Color.white : new Color(0.55f, 0.62f, 0.68f);
            _taskName.normal.textColor = done ? new Color(0.75f, 0.95f, 0.82f) : current ? Color.white : new Color(0.62f, 0.7f, 0.76f);
            GUI.Label(new Rect(cell.x, cell.y, 28f, cell.height), (i + 1).ToString(), _taskNum);
            GUI.Label(new Rect(cell.x + 34f, cell.y, cell.width - 40f, cell.height), LabInterior.Names[i], _taskName);
        }
    }

    static Texture2D Pixel(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void FollowMinimap(Transform target)
    {
        if (_minimap == null || target == null)
            return;
        _minimap.transform.position = target.position + Vector3.up * 90f;
        _minimap.transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }

    DriveableCar FindNearestCar(float radius)
    {
        DriveableCar[] cars = FindObjectsByType<DriveableCar>(FindObjectsSortMode.None);
        DriveableCar nearest = null;
        float best = radius * radius;
        for (int i = 0; i < cars.Length; i++)
        {
            Vector3 delta = cars[i].transform.position - _player.position;
            float flat = delta.x * delta.x + delta.z * delta.z;
            if (cars[i].Flight && (flat > 64f || Mathf.Abs(delta.y) > 24f))
                continue;
            if (flat < best)
            {
                best = flat;
                nearest = cars[i];
            }
        }
        return nearest;
    }

    void EnterCar(DriveableCar car)
    {
        _currentCar = car;
        car.Occupied = true;
        _motor.Locked = true;
        _controller.enabled = false;
        _player.position = car.transform.position + Vector3.up * 0.5f;
        SetPlayerVisible(false);
        SetShipVisible(car, false);
        _cockpit = true;
        _camera.FirstPerson(car.transform, new Vector3(0f, 0.9f, 0.35f), true);
        if (_combat != null)
            _combat.ClearZoom();
        Camera view = _camera.GetComponent<Camera>();
        if (view != null)
            view.fieldOfView = 74f;
    }

    void ToggleShipView()
    {
        _cockpit = !_cockpit;
        if (_cockpit)
        {
            SetShipVisible(_currentCar, false);
            _camera.FirstPerson(_currentCar.transform, new Vector3(0f, 0.9f, 0.35f), true);
            return;
        }

        SetShipVisible(_currentCar, true);
        _camera.Chase(_currentCar.transform, new Vector3(0f, 1.8f, 0f), 14f, true);
    }

    void ExitCar()
    {
        DriveableCar car = _currentCar;
        Vector3 exit = car.transform.position + car.transform.right * 2.7f;
        if (Physics.CheckSphere(exit + Vector3.up, 0.45f))
            exit = car.transform.position - car.transform.right * 2.7f;
        exit.y = 0.45f;

        SetShipVisible(car, true);
        _cockpit = false;
        car.Park();
        _currentCar = null;
        _player.SetPositionAndRotation(exit, Quaternion.Euler(0f, car.transform.eulerAngles.y, 0f));
        SetPlayerVisible(true);
        _controller.enabled = true;
        _motor.Locked = false;
        _camera.Follow(_player, new Vector3(0f, 1.5f, 0f), 6.2f, true);
    }

    void SetPlayerVisible(bool visible)
    {
        for (int i = 0; i < _playerRenderers.Length; i++)
            _playerRenderers[i].enabled = visible;
    }

    static void SetShipVisible(DriveableCar car, bool visible)
    {
        Renderer[] renderers = car.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }

    void Paint(GameObject go, Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");
        var material = new Material(shader);
        material.color = color;
        go.GetComponent<Renderer>().sharedMaterial = material;
    }
}
