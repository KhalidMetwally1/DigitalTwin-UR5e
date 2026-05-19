using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class MqttRobotController : MonoBehaviour
{
    [Header("MQTT Settings")]
    public string brokerAddress = "localhost";
    public int brokerPort = 1883;
    public string topic = "corex/ur5e/data";

    [Header("References")]
    public RobotController robotController;
    public DashboardController dashboardController;

    private MqttClient client;

    void Start()
    {
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        client.MqttMsgPublishReceived += OnMessageReceived;

        string clientId = "UnityCorex_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        client.Connect(clientId);

        if (client.IsConnected)
        {
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            Debug.Log("Connected to MQTT topic: " + topic);
        }
        else
        {
            Debug.LogError("Failed to connect to MQTT broker!");
        }
    }

    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.Message);
        try
        {
            float[] actual_q           = ParseFloatArray(payload, "actual_q");
            float[] target_q           = ParseFloatArray(payload, "target_q");
            float[] actual_current     = ParseFloatArray(payload, "actual_current");
            float[] target_current     = ParseFloatArray(payload, "target_current");
            float[] target_moment      = ParseFloatArray(payload, "target_moment");
            float[] joint_temperatures = ParseFloatArray(payload, "joint_temperatures");
            float[] actual_TCP_pose    = ParseFloatArray(payload, "actual_TCP_pose");
            int     robot_mode         = ParseInt(payload, "robot_mode");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (robotController != null && actual_q.Length >= 6)
                {
                    robotController.joint1 = actual_q[0] * Mathf.Rad2Deg;
                    robotController.joint2 = actual_q[1] * Mathf.Rad2Deg;
                    robotController.joint3 = actual_q[2] * Mathf.Rad2Deg;
                    robotController.joint4 = actual_q[3] * Mathf.Rad2Deg;
                    robotController.joint5 = actual_q[4] * Mathf.Rad2Deg;
                    robotController.joint6 = actual_q[5] * Mathf.Rad2Deg;
                }

                if (dashboardController != null)
                {
                    dashboardController.UpdateTemperatures(joint_temperatures);
                    dashboardController.UpdateCurrentTorque(actual_current, target_current, target_moment);
                    dashboardController.UpdatePositions(target_q, actual_q);
                    dashboardController.UpdateTCP(actual_TCP_pose);
                    dashboardController.UpdateRUL(EstimateRUL(joint_temperatures, actual_current));
                    string mode = robot_mode == 7 ? "RUNNING" : robot_mode == 0 ? "DISCONNECTED" : "IDLE";
                    dashboardController.SetRobotMode(mode);
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MQTT parse error: " + ex.Message);
        }
    }

    float[] EstimateRUL(float[] temps, float[] currents)
    {
        float[] rul = new float[6];
        for (int i = 0; i < 6; i++)
        {
            float t = 1f - Mathf.Clamp01((temps[i] - 25f) / 55f);
            float c = 1f - Mathf.Clamp01(currents[i] / 2f);
            rul[i] = Mathf.Round((t * 0.6f + c * 0.4f) * 100f);
        }
        return rul;
    }

    float[] ParseFloatArray(string json, string key)
    {
        int start = json.IndexOf("\"" + key + "\"");
        if (start < 0) return new float[6];
        int arrStart = json.IndexOf('[', start);
        int arrEnd   = json.IndexOf(']', arrStart);
        if (arrStart < 0 || arrEnd < 0) return new float[6];
        string inner = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
        string[] parts = inner.Split(',');
        float[] result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            float.TryParse(parts[i].Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out result[i]);
        return result;
    }

    int ParseInt(string json, string key)
    {
        int start = json.IndexOf("\"" + key + "\"");
        if (start < 0) return 0;
        int colon = json.IndexOf(':', start);
        int end   = json.IndexOfAny(new char[] { ',', '}' }, colon);
        string val = json.Substring(colon + 1, end - colon - 1).Trim();
        int.TryParse(val, out int result);
        return result;
    }

    void OnDestroy()
    {
        if (client != null && client.IsConnected)
            client.Disconnect();
    }
}