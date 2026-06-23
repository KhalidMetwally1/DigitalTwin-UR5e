using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System.Collections.Generic;

public class MqttDashboardReceiver : MonoBehaviour
{
    [Header("HiveMQ Settings")]
    public string brokerAddress = "a1d84dccdb22471ea81d9a83b6e53959.s1.eu.hivemq.cloud";
    public int    brokerPort    = 8883;
    public string username      = "corex_project";
    public string password      = "Corex#123";
    public string topic         = "corex/ur5e/data";

    [Header("References")]
    public RobotController     robotController;
    public DashboardController dashboard;

    private MqttClient client;
    private readonly object lockObj = new object();
    private string pendingPayload = null;

    void Start()
    {
        try
        {
            client = new MqttClient(brokerAddress, brokerPort, true, null, null,
                                    MqttSslProtocols.TLSv1_2);
            client.MqttMsgPublishReceived += OnMessage;
            client.Connect("UnityDash_" + Random.Range(0, 9999), username, password);
            client.Subscribe(new string[] { topic },
                             new byte[]   { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            Debug.Log("MqttDashboardReceiver connected!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MQTT connect failed: " + ex.Message);
        }
    }

    void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.Message);
        lock (lockObj) { pendingPayload = payload; }
    }

    void Update()
    {
        string payload = null;
        lock (lockObj)
        {
            if (pendingPayload == null) return;
            payload = pendingPayload;
            pendingPayload = null;
        }

        try { ProcessPayload(payload); }
        catch (System.Exception ex) { Debug.LogWarning("Parse error: " + ex.Message); }
    }

    void ProcessPayload(string payload)
    {
        var d = SimpleJson(payload);

        float[] actualQ        = ParseArray(d, "actual_q");
        float[] targetQ        = ParseArray(d, "target_q");
        float[] actualCurrent  = ParseArray(d, "actual_current");
        float[] targetCurrent  = ParseArray(d, "target_current");
        float[] torque         = ParseArray(d, "torque");
        float[] tcp            = ParseArray(d, "tcp");
        float[] temperatures   = ParseArray(d, "temperatures");
        float[] rul            = ParseArray(d, "rul");

        // ── حرّك الروبوت (rad -> deg) ──────────────────────
        if (robotController != null && actualQ.Length == 6)
        {
            robotController.mqttJoint1 = actualQ[0] * Mathf.Rad2Deg;
            robotController.mqttJoint2 = actualQ[1] * Mathf.Rad2Deg;
            robotController.mqttJoint3 = actualQ[2] * Mathf.Rad2Deg;
            robotController.mqttJoint4 = actualQ[3] * Mathf.Rad2Deg;
            robotController.mqttJoint5 = actualQ[4] * Mathf.Rad2Deg;
            robotController.mqttJoint6 = actualQ[5] * Mathf.Rad2Deg;
        }

        // ── حدّث الداشبورد ──────────────────────────────────
        if (dashboard != null)
        {
            if (temperatures.Length == 6)  dashboard.UpdateTemperatures(temperatures);
            if (actualCurrent.Length > 0)  dashboard.UpdateCurrentTorque(actualCurrent, targetCurrent, torque);
            if (targetQ.Length == 6)       dashboard.UpdatePositions(targetQ, actualQ);
            if (tcp.Length == 6)           dashboard.UpdateTCP(tcp);
            if (rul.Length == 6)           dashboard.UpdateRUL(rul);

            if (d.TryGetValue("mode", out string mode))
                dashboard.SetRobotMode(mode);
        }
    }

    // ── JSON Helpers ──────────────────────────────────────────
    Dictionary<string, string> SimpleJson(string json)
    {
        var dict = new Dictionary<string, string>();
        json = json.Trim().TrimStart('{').TrimEnd('}');

        int depth = 0, start = 0;
        string currentKey = null;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '[') depth++;
            else if (c == ']') depth--;
            else if (c == ':' && depth == 0 && currentKey == null)
            {
                currentKey = json.Substring(start, i - start).Trim().Trim('"');
                start = i + 1;
            }
            else if (c == ',' && depth == 0 && currentKey != null)
            {
                dict[currentKey] = json.Substring(start, i - start).Trim().Trim('"');
                currentKey = null;
                start = i + 1;
            }
        }
        if (currentKey != null)
            dict[currentKey] = json.Substring(start).Trim().Trim('"');

        return dict;
    }

    float[] ParseArray(Dictionary<string, string> d, string key)
    {
        if (!d.TryGetValue(key, out string raw)) return new float[0];
        raw = raw.Trim().TrimStart('[').TrimEnd(']');
        if (string.IsNullOrEmpty(raw)) return new float[0];

        string[] parts = raw.Split(',');
        float[] result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            result[i] = float.Parse(parts[i].Trim(),
                        System.Globalization.CultureInfo.InvariantCulture);
        return result;
    }

    void OnDestroy()
    {
        if (client != null && client.IsConnected)
            client.Disconnect();
    }
}
