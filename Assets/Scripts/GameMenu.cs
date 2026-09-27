using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public static bool Playing { get; private set; }

    GUIStyle _title;
    GUIStyle _kicker;
    GUIStyle _section;
    GUIStyle _key;
    GUIStyle _action;
    GUIStyle _note;
    GUIStyle _story;
    GUIStyle _quote;
    GUIStyle _button;
    Texture2D _dim;
    Texture2D _panel;
    Texture2D _bar;
    Texture2D _chip;
    Texture2D _line;
    Texture2D _buttonBg;
    float _scale = -1f;
    bool _help;

    void Awake()
    {
        Playing = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        _dim = Pixel(new Color(0.02f, 0.04f, 0.08f, 0.9f));
        _panel = Pixel(new Color(0.07f, 0.1f, 0.15f, 0.96f));
        _bar = Pixel(new Color(0.25f, 0.72f, 0.9f, 1f));
        _chip = Pixel(new Color(0.12f, 0.2f, 0.28f, 1f));
        _line = Pixel(new Color(0.25f, 0.72f, 0.9f, 0.35f));
        _buttonBg = Pixel(new Color(0.16f, 0.42f, 0.55f, 1f));
    }

    void Update()
    {
        if (Playing)
            return;
        if (_help)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _help = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Begin();
    }

    void Begin()
    {
        Playing = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (!Playing)
        {
            if (_help)
                DrawHelp();
            else
                DrawStart();
        }
    }

    void DrawStart()
    {
        float w = Mathf.Min(500f, Screen.width * 0.46f);
        float h = Screen.height - 28f;
        float scale = Mathf.Clamp(Mathf.Min(w / 500f, h / 640f), 0.62f, 1f);
        EnsureStyles(scale);

        var panel = new Rect(14f, 14f, w, h);
        GUI.DrawTexture(panel, _panel);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 4f * scale, panel.height), _bar);

        float pad = 20f * scale;
        GUI.Label(new Rect(panel.x + pad, panel.y + 16f * scale, panel.width - pad * 2f, 16f * scale), "SCIENCE CITY", _kicker);
        GUI.Label(new Rect(panel.x + pad, panel.y + 32f * scale, panel.width - pad * 2f, 40f * scale), "AETHER", _title);
        GUI.Label(new Rect(panel.x + pad, panel.y + 72f * scale, panel.width - pad * 2f, 16f * scale), "EPISODE I    THE LAST LIGHT", _note);
        float lineY = panel.y + 94f * scale;
        GUI.DrawTexture(new Rect(panel.x + pad, lineY, panel.width - pad * 2f, 1f), _line);

        const string story =
            "A long time from now, in a city that is still awake...\n\n" +
            "The core of AETHER is failing. In five minutes the lights go out.\n\n" +
            "ASUNA is the last scientist inside the walls. The lab is unfinished. The signal towers are silent. The sky rings are still open.\n\n" +
            "The guard robots have forgotten their orders. They fire on sight.\n\n" +
            "She has three lives, one ship, and one clock.\n\n" +
            "Finish the work, and the city lives. Fall three times, and it ends.\n\n" +
            "This is her story. This is why you play.";

        float buttonH = 36f * scale;
        float gap = 8f * scale;
        float buttonsY = panel.yMax - pad - buttonH;
        var crawl = new Rect(panel.x + pad, lineY + 10f * scale, panel.width - pad * 2f, buttonsY - lineY - 62f * scale);
        DrawCrawl(crawl, story);
        GUI.Label(new Rect(panel.x + pad, buttonsY - 42f * scale, panel.width - pad * 2f, 34f * scale), "Hold the light. Five minutes.\nShe is the one who steps in.", _quote);

        float half = (panel.width - pad * 2f - gap) * 0.5f;
        var help = new Rect(panel.x + pad, buttonsY, half, buttonH);
        var start = new Rect(help.xMax + gap, buttonsY, half, buttonH);
        GUI.DrawTexture(help, _chip);
        GUI.DrawTexture(start, _buttonBg);
        if (GUI.Button(help, "HOW TO PLAY", _button))
            _help = true;
        if (GUI.Button(start, "ENTER    START", _button))
            Begin();
    }

    void DrawCrawl(Rect view, string story)
    {
        float contentH = 640f * _scale;
        float travel = contentH + view.height;
        float y = view.height - Mathf.Repeat(Time.unscaledTime * 14f * _scale, travel);
        GUI.BeginGroup(view);
        GUI.Label(new Rect(8f, y, view.width - 16f, contentH), story, _story);
        GUI.EndGroup();
        float fade = 22f * _scale;
        GUI.DrawTexture(new Rect(view.x, view.y, view.width, fade), _panel);
        GUI.DrawTexture(new Rect(view.x, view.yMax - fade, view.width, fade), _panel);
    }

    void DrawHelp()
    {
        float w = Mathf.Min(500f, Screen.width * 0.46f);
        float h = Screen.height - 28f;
        float scale = Mathf.Clamp(Mathf.Min(w / 500f, h / 640f), 0.62f, 1f);
        EnsureStyles(scale);

        var panel = new Rect(14f, 14f, w, h);
        GUI.DrawTexture(panel, _panel);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 4f * scale, panel.height), _bar);

        float pad = 20f * scale;
        GUI.Label(new Rect(panel.x + pad, panel.y + 18f * scale, panel.width - pad * 2f, 28f * scale), "HOW TO PLAY", _title);
        float lineY = panel.y + 56f * scale;
        GUI.DrawTexture(new Rect(panel.x + pad, lineY, panel.width - pad * 2f, 1f), _line);

        float left = panel.x + pad;
        float colW = (panel.width - pad * 3f) * 0.5f;
        float right = left + colW + pad;
        float yLeft = lineY + 16f * scale;
        float yRight = yLeft;
        float rowH = 26f * scale;
        float gap = 8f * scale;

        Section(ref yLeft, left, "MOVE");
        Row(ref yLeft, left, colW, rowH, gap, "WASD", "Walk");
        Row(ref yLeft, left, colW, rowH, gap, "SHIFT", "Sprint");
        Row(ref yLeft, left, colW, rowH, gap, "SPACE", "Jump  /  climb");
        Row(ref yLeft, left, colW, rowH, gap, "CTRL", "Descend");
        Row(ref yLeft, left, colW, rowH, gap, "MOUSE", "Look");
        Row(ref yLeft, left, colW, rowH, gap, "V", "Ship camera");

        Section(ref yRight, right, "MISSION");
        Row(ref yRight, right, colW, rowH, gap, "AIM", "Zoom button");
        Row(ref yRight, right, colW, rowH, gap, "CLICK", "Fire");
        Row(ref yRight, right, colW, rowH, gap, "F", "Board ship");
        Row(ref yRight, right, colW, rowH, gap, "E", "Do a task");
        Row(ref yRight, right, colW, rowH, gap, "DOOR", "Cyan lab");
        Row(ref yRight, right, colW, rowH, gap, "TIME", "5:00");

        float buttonH = 36f * scale;
        var back = new Rect(panel.x + pad, panel.yMax - pad - buttonH, panel.width - pad * 2f, buttonH);
        GUI.DrawTexture(back, _buttonBg);
        if (GUI.Button(back, "BACK", _button))
            _help = false;
    }

    void Section(ref float y, float x, string title)
    {
        GUI.Label(new Rect(x, y, 280f, 16f * _scale), title, _section);
        y += 18f * _scale;
    }

    void Row(ref float y, float x, float width, float rowH, float gap, string key, string action)
    {
        float chipW = Mathf.Min(78f * _scale, width * 0.34f);
        GUI.DrawTexture(new Rect(x, y, chipW, rowH), _chip);
        GUI.Label(new Rect(x, y, chipW, rowH), key, _key);
        GUI.Label(new Rect(x + chipW + 8f * _scale, y, width - chipW - 8f * _scale, rowH), action, _action);
        y += rowH + gap;
    }

    void EnsureStyles()
    {
        EnsureStyles(1f);
    }

    void EnsureStyles(float scale)
    {
        if (_title != null && Mathf.Abs(_scale - scale) < 0.02f)
            return;

        _scale = scale;
        _title = Make(Mathf.RoundToInt(36f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.82f, 0.95f, 1f));
        _kicker = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.35f, 0.78f, 0.92f));
        _section = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.55f, 0.86f, 0.95f));
        _key = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.75f, 0.94f, 1f));
        _action = Make(Mathf.RoundToInt(13f * scale), FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.9f, 0.94f, 0.97f));
        _note = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.28f));
        _story = Make(Mathf.RoundToInt(14f * scale), FontStyle.Bold, TextAnchor.UpperCenter, new Color(1f, 0.86f, 0.32f));
        _story.wordWrap = true;
        _quote = Make(Mathf.RoundToInt(13f * scale), FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.9f, 0.55f));
        _quote.wordWrap = true;
        _button = Make(Mathf.RoundToInt(13f * scale), FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
    }

    static GUIStyle Make(int size, FontStyle style, TextAnchor anchor, Color color)
    {
        var gui = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = style,
            alignment = anchor
        };
        gui.normal.textColor = color;
        return gui;
    }

    static Texture2D Pixel(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}

public class Hover : MonoBehaviour
{
    Vector3 _origin;

    void Start()
    {
        _origin = transform.position;
    }

    void Update()
    {
        transform.position = _origin + Vector3.up * Mathf.Sin(Time.time * 0.7f) * 1.2f;
    }
}
