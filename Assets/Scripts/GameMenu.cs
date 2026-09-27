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
    GUIStyle _button;
    Texture2D _dim;
    Texture2D _panel;
    Texture2D _bar;
    Texture2D _chip;
    Texture2D _line;
    Texture2D _buttonBg;
    float _scale = -1f;

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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            Begin();
    }

    void Begin()
    {
        Playing = true;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!Playing)
            DrawStart();
    }

    void DrawStart()
    {
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _dim);

        float w = Mathf.Min(640f, Screen.width - 28f);
        float h = Mathf.Min(430f, Screen.height - 28f);
        float scale = Mathf.Clamp(Mathf.Min(w / 640f, h / 430f), 0.62f, 1f);
        EnsureStyles(scale);

        var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        GUI.DrawTexture(panel, _panel);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 4f * scale, panel.height), _bar);

        float pad = 18f * scale;
        float titleH = 28f * scale;
        GUI.Label(new Rect(panel.x + pad, panel.y + 8f * scale, panel.width - pad * 2f, 14f * scale), "SCIENCE CITY", _kicker);
        GUI.Label(new Rect(panel.x + pad, panel.y + 20f * scale, panel.width - pad * 2f, titleH), "AETHER", _title);
        float lineY = panel.y + 52f * scale;
        GUI.DrawTexture(new Rect(panel.x + pad, lineY, panel.width - pad * 2f, 1f), _line);

        float left = panel.x + pad;
        float colW = (panel.width - pad * 3f) * 0.5f;
        float right = left + colW + pad;
        float yLeft = lineY + 10f * scale;
        float yRight = yLeft;
        float rowH = 22f * scale;
        float gap = 5f * scale;

        Section(ref yLeft, left, "MOVE");
        Row(ref yLeft, left, colW, rowH, gap, "WASD", "Walk");
        Row(ref yLeft, left, colW, rowH, gap, "SHIFT", "Sprint");
        Row(ref yLeft, left, colW, rowH, gap, "SPACE", "Jump  /  climb");
        Row(ref yLeft, left, colW, rowH, gap, "CTRL", "Descend");
        Row(ref yLeft, left, colW, rowH, gap, "F", "Board ship");

        Section(ref yRight, right, "LAB");
        Row(ref yRight, right, colW, rowH, gap, "DOOR", "Cyan gate");
        Row(ref yRight, right, colW, rowH, gap, "E", "Finish task");
        Row(ref yRight, right, colW, rowH, gap, "TIME", "60 seconds");
        Row(ref yRight, right, colW, rowH, gap, "FIRE", "Left click");
        Row(ref yRight, right, colW, rowH, gap, "MOUSE", "Look around");

        float buttonH = 32f * scale;
        var button = new Rect(panel.x + pad, panel.yMax - pad - buttonH, panel.width - pad * 2f, buttonH);
        GUI.DrawTexture(button, _buttonBg);
        GUI.Label(button, "ENTER    START", _button);
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
        _title = Make(Mathf.RoundToInt(26f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.82f, 0.95f, 1f));
        _kicker = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.35f, 0.78f, 0.92f));
        _section = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.55f, 0.86f, 0.95f));
        _key = Make(Mathf.RoundToInt(11f * scale), FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.75f, 0.94f, 1f));
        _action = Make(Mathf.RoundToInt(13f * scale), FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.9f, 0.94f, 0.97f));
        _note = Make(Mathf.RoundToInt(12f * scale), FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.7f, 0.8f, 0.86f));
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
