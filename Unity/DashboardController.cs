using UnityEngine;
using UnityEngine.UIElements;

public class DashboardController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Robot Camera")]
    public RenderTexture robotRenderTexture;

    // UI Elements
    private Label robotModeLabel;
    private Label timestampLabel;

    // Temperature labels & bars
    private Label[] tempLabels = new Label[6];
    private VisualElement[] tempBars = new VisualElement[6];

    // Current & Torque
    private Label currentVal;
    private Label targetCurrentVal;
    private Label torqueVal;
    private Label currentError;

    // TCP Pose
    private Label tcpX, tcpY, tcpZ, tcpRX, tcpRY, tcpRZ;

    // Position Tracking
    private Label[] tqLabels = new Label[6];
    private Label[] aqLabels = new Label[6];
    private Label[] eqLabels = new Label[6];

    // RUL bars & values
    private VisualElement[] rulBars = new VisualElement[6];
    private Label[] rulVals = new Label[6];

    // Viewport
    private VisualElement twinViewport;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        // Status
        robotModeLabel = root.Q<Label>("robot-mode-label");
        timestampLabel = root.Q<Label>("timestamp-label");

        // Temperatures
        for (int i = 0; i < 6; i++)
        {
            tempLabels[i] = root.Q<Label>($"temp-val-{i}");
            tempBars[i]   = root.Q<VisualElement>($"temp-bar-{i}");
        }

        // Current & Torque
        currentVal       = root.Q<Label>("current-val");
        targetCurrentVal = root.Q<Label>("target-current-val");
        torqueVal        = root.Q<Label>("torque-val");
        currentError     = root.Q<Label>("current-error");

        // TCP
        tcpX  = root.Q<Label>("tcp-x");
        tcpY  = root.Q<Label>("tcp-y");
        tcpZ  = root.Q<Label>("tcp-z");
        tcpRX = root.Q<Label>("tcp-rx");
        tcpRY = root.Q<Label>("tcp-ry");
        tcpRZ = root.Q<Label>("tcp-rz");

        // Position tracking
        for (int i = 0; i < 6; i++)
        {
            tqLabels[i] = root.Q<Label>($"tq-{i}");
            aqLabels[i] = root.Q<Label>($"aq-{i}");
            eqLabels[i] = root.Q<Label>($"eq-{i}");
        }

        // RUL
        for (int i = 0; i < 6; i++)
        {
            rulBars[i] = root.Q<VisualElement>($"rul-bar-{i}");
            rulVals[i] = root.Q<Label>($"rul-val-{i}");
        }

        // Viewport — set render texture as background
        twinViewport = root.Q<VisualElement>("twin-viewport");
        if (robotRenderTexture != null)
        {
            twinViewport.style.backgroundImage =
                Background.FromRenderTexture(robotRenderTexture);
        }
    }

    void Update()
    {
        // Update timestamp
        if (timestampLabel != null)
            timestampLabel.text = System.DateTime.Now.ToString("HH:mm:ss");
    }

    // ── Called by MqttRobotController when new data arrives ──────────────────
    public void UpdateTemperatures(float[] temps)
    {
        float[] thresholds = { 35f, 35f, 35f, 35f, 35f, 35f };
        for (int i = 0; i < 6 && i < temps.Length; i++)
        {
            if (tempLabels[i] != null)
                tempLabels[i].text = $"{temps[i]:F1}°C";

            if (tempBars[i] != null)
            {
                float pct = Mathf.Clamp01(temps[i] / 60f);
                tempBars[i].style.width = Length.Percent(pct * 100f);

                tempBars[i].RemoveFromClassList("temp-normal");
                tempBars[i].RemoveFromClassList("temp-warn");
                tempBars[i].RemoveFromClassList("temp-danger");

                if (temps[i] < 35f)
                    tempBars[i].AddToClassList("temp-normal");
                else if (temps[i] < 50f)
                    tempBars[i].AddToClassList("temp-warn");
                else
                    tempBars[i].AddToClassList("temp-danger");
            }
        }
    }

    public void UpdateCurrentTorque(float[] actualCurrent, float[] targetCurrent, float[] torque)
    {
        if (actualCurrent.Length > 0 && currentVal != null)
            currentVal.text = $"{actualCurrent[1]:F3} A";

        if (targetCurrent.Length > 0 && targetCurrentVal != null)
            targetCurrentVal.text = $"{targetCurrent[1]:F3} A";

        if (torque.Length > 1 && torqueVal != null)
            torqueVal.text = $"{torque[1]:F2} Nm";

        if (currentError != null && actualCurrent.Length > 0 && targetCurrent.Length > 0)
        {
            float err = Mathf.Abs(actualCurrent[1] - targetCurrent[1]);
            currentError.text = $"{err:F3} A";
        }
    }

    public void UpdatePositions(float[] targetQ, float[] actualQ)
    {
        for (int i = 0; i < 6; i++)
        {
            if (tqLabels[i] != null) tqLabels[i].text = $"{targetQ[i]:F3}";
            if (aqLabels[i] != null) aqLabels[i].text = $"{actualQ[i]:F3}";

            if (eqLabels[i] != null)
            {
                float err = Mathf.Abs(targetQ[i] - actualQ[i]);
                eqLabels[i].text = $"{err:F3}";

                eqLabels[i].RemoveFromClassList("ok");
                eqLabels[i].RemoveFromClassList("warn");
                eqLabels[i].RemoveFromClassList("danger");

                if (err < 0.01f)       eqLabels[i].AddToClassList("ok");
                else if (err < 0.05f)  eqLabels[i].AddToClassList("warn");
                else                   eqLabels[i].AddToClassList("danger");
            }
        }
    }

    public void UpdateTCP(float[] tcp)
    {
        if (tcp.Length < 6) return;
        if (tcpX != null) tcpX.text  = $"{tcp[0]:F3}";
        if (tcpY != null) tcpY.text  = $"{tcp[1]:F3}";
        if (tcpZ != null) tcpZ.text  = $"{tcp[2]:F3}";
        if (tcpRX != null) tcpRX.text = $"{tcp[3]:F3}";
        if (tcpRY != null) tcpRY.text = $"{tcp[4]:F3}";
        if (tcpRZ != null) tcpRZ.text = $"{tcp[5]:F3}";
    }

    public void UpdateRUL(float[] rulPercents)
    {
        string[] classes = { "rul-good", "rul-warn", "rul-danger" };
        string[] pctClasses = { "good", "warn", "danger" };

        for (int i = 0; i < 6 && i < rulPercents.Length; i++)
        {
            float pct = Mathf.Clamp01(rulPercents[i] / 100f);

            if (rulBars[i] != null)
            {
                rulBars[i].style.height = Length.Percent(pct * 100f);
                foreach (var c in classes) rulBars[i].RemoveFromClassList(c);

                if (rulPercents[i] > 70f)      rulBars[i].AddToClassList("rul-good");
                else if (rulPercents[i] > 40f) rulBars[i].AddToClassList("rul-warn");
                else                           rulBars[i].AddToClassList("rul-danger");
            }

            if (rulVals[i] != null)
            {
                rulVals[i].text = $"{rulPercents[i]:F0}%";
                foreach (var c in pctClasses) rulVals[i].RemoveFromClassList(c);

                if (rulPercents[i] > 70f)      rulVals[i].AddToClassList("good");
                else if (rulPercents[i] > 40f) rulVals[i].AddToClassList("warn");
                else                           rulVals[i].AddToClassList("danger");
            }
        }
    }

    public void SetRobotMode(string mode)
    {
        if (robotModeLabel == null) return;
        robotModeLabel.text = $"● {mode.ToUpper()}";
    }
}