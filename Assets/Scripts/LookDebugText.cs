using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookDebugText : MonoBehaviour
{
    public InputActionReference LookAction; // drag your Look action here

    private TextMesh textMesh;
    private float maxXSeen = 0f;
    private float maxYSeen = 0f;

    [SerializeField] private float epsilon = 0.0001f; // threshold to treat as non-zero

    [SerializeField] private float windowSeconds = 1f; // rolling max window (seconds)

    [SerializeField] private bool showOverlay = true;

    // rolling buffers for windowed max (store time + |value|)
    private readonly List<float> xTimes = new();
    private readonly List<float> xVals  = new();
    private readonly List<float> yTimes = new();
    private readonly List<float> yVals  = new();

    private float lastNonZeroX = 0f;
    private float lastNonZeroY = 0f;

    private double sumAbsX = 0.0;
    private double sumAbsY = 0.0;
    private long samplesX = 0;
    private long samplesY = 0;

    private float windowMaxX = 0f;
    private float windowMaxY = 0f;

    private float medianX = 0f, medianY = 0f;
    private float p95X = 0f, p95Y = 0f;

    Vector2 look;

    void Awake()
    {
        // Make sure there's a TextMesh component
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.fontSize = 32;
            textMesh.color = Color.green;
            textMesh.anchor = TextAnchor.UpperLeft;
        }
    }

    void Update()
    {
        if (LookAction == null) return;

        look = LookAction.action.ReadValue<Vector2>();

        if (Mathf.Abs(look.x) > epsilon)
        {
            lastNonZeroX = look.x;
            maxXSeen = Mathf.Max(maxXSeen, Mathf.Abs(look.x));
            sumAbsX += Mathf.Abs(look.x);
            samplesX++;
        }
        if (Mathf.Abs(look.y) > epsilon)
        {
            lastNonZeroY = look.y;
            maxYSeen = Mathf.Max(maxYSeen, Mathf.Abs(look.y));
            sumAbsY += Mathf.Abs(look.y);
            samplesY++;
        }

        float avgAbsX = samplesX > 0 ? (float)(sumAbsX / samplesX) : 0f;
        float avgAbsY = samplesY > 0 ? (float)(sumAbsY / samplesY) : 0f;

        float now = Time.unscaledTime;

        if (Mathf.Abs(look.x) > epsilon)
        {
            xTimes.Add(now);
            xVals.Add(Mathf.Abs(look.x));
        }
        if (Mathf.Abs(look.y) > epsilon)
        {
            yTimes.Add(now);
            yVals.Add(Mathf.Abs(look.y));
        }

        // prune samples outside the window
        while (xTimes.Count > 0 && now - xTimes[0] > windowSeconds)
        {
            xTimes.RemoveAt(0);
            xVals.RemoveAt(0);
        }
        while (yTimes.Count > 0 && now - yTimes[0] > windowSeconds)
        {
            yTimes.RemoveAt(0);
            yVals.RemoveAt(0);
        }

        // recompute window maxima
        windowMaxX = 0f;
        for (int i = 0; i < xVals.Count; i++)
            if (xVals[i] > windowMaxX) windowMaxX = xVals[i];

        windowMaxY = 0f;
        for (int i = 0; i < yVals.Count; i++)
            if (yVals[i] > windowMaxY) windowMaxY = yVals[i];

        medianX = Percentile(xVals, 0.5f);
        medianY = Percentile(yVals, 0.5f);
        p95X = Percentile(xVals, 0.95f);
        p95Y = Percentile(yVals, 0.95f);

        if (!showOverlay)
        {
            textMesh.text = string.Empty;
            return;
        }

        textMesh.text =
            $"Win Max |X| ({windowSeconds:F1}s): {windowMaxX:F2}\n" +
            $"Win Max |Y| ({windowSeconds:F1}s): {windowMaxY:F2}\n" +
            $"Median |X|: {medianX:F2}    P95 |X|: {p95X:F2}\n" +
            $"Median |Y|: {medianY:F2}    P95 |Y|: {p95Y:F2}\n" +
            $"Avg |X|: {avgAbsX:F2}       Max |X|: {maxXSeen:F2}\n" +
            $"Avg |Y|: {avgAbsY:F2}       Max |Y|: {maxYSeen:F2}";
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F1)
        {
            showOverlay = !showOverlay;
        }
        if (!showOverlay)
        {
            return;
        }

        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float avgAbsX = samplesX > 0 ? (float)(sumAbsX / samplesX) : 0f;
        float avgAbsY = samplesY > 0 ? (float)(sumAbsY / samplesY) : 0f;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
        {
            ResetStats();
        }

        string text =
            $"Win Max |X| ({windowSeconds:F1}s): {windowMaxX:F2}\n" +
            $"Win Max |Y| ({windowSeconds:F1}s): {windowMaxY:F2}\n" +
            $"Median |X|: {medianX:F2}    P95 |X|: {p95X:F2}\n" +
            $"Median |Y|: {medianY:F2}    P95 |Y|: {p95Y:F2}\n" +
            $"Avg |X|: {avgAbsX:F2}       Max |X|: {maxXSeen:F2}\n" +
            $"Avg |Y|: {avgAbsY:F2}       Max |Y|: {maxYSeen:F2}\n" +
            $"[F1] Toggle  [R] Reset";
        GUI.Label(rect, text, style);
    }

    private static float Percentile(List<float> data, float q)
    {
        if (data == null || data.Count == 0) return 0f;
        var arr = data.ToArray();
        Array.Sort(arr);
        float pos = (arr.Length - 1) * Mathf.Clamp01(q);
        int lo = Mathf.FloorToInt(pos);
        int hi = Mathf.CeilToInt(pos);
        if (lo == hi) return arr[lo];
        float frac = pos - lo;
        return Mathf.Lerp(arr[lo], arr[hi], frac);
    }

    private void ResetStats()
    {
        maxXSeen = maxYSeen = 0f;
        lastNonZeroX = lastNonZeroY = 0f;
        sumAbsX = sumAbsY = 0.0;
        samplesX = samplesY = 0;
        xTimes.Clear(); xVals.Clear();
        yTimes.Clear(); yVals.Clear();
        windowMaxX = windowMaxY = 0f;
        medianX = medianY = p95X = p95Y = 0f;
        if (!showOverlay && textMesh != null) textMesh.text = string.Empty;
    }
}