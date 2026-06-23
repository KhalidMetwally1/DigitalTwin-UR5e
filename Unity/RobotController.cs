using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Joint Targets (degrees)")]
    public float joint1 = 0f;
    public float joint2 = 0f;
    public float joint3 = 0f;
    public float joint4 = 0f;
    public float joint5 = 0f;
    public float joint6 = 0f;

    [Header("Speed")]
    public float speed = 50f;

    // ---- MQTT يكتب هنا ----
    [HideInInspector] public float mqttJoint1 = 0f;
    [HideInInspector] public float mqttJoint2 = 0f;
    [HideInInspector] public float mqttJoint3 = 0f;
    [HideInInspector] public float mqttJoint4 = 0f;
    [HideInInspector] public float mqttJoint5 = 0f;
    [HideInInspector] public float mqttJoint6 = 0f;

    [Header("MQTT Control")]
    public bool useMqtt = false;   // شغّله من Inspector لما تيجي تشغّل Python

    private List<ArticulationBody> joints = new List<ArticulationBody>();

    void Start()
    {
        ArticulationBody[] bodies = GetComponentsInChildren<ArticulationBody>();
        foreach (var body in bodies)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
                joints.Add(body);
        }
        Debug.Log("Found " + joints.Count + " joints");
    }

    void Update()
    {
        float[] targets;

        if (useMqtt)
        {
            // القيم جاية من Python عن طريق MQTT
            targets = new float[] { mqttJoint1, mqttJoint2, mqttJoint3,
                                    mqttJoint4, mqttJoint5, mqttJoint6 };
        }
        else
        {
            // القيم من Inspector يدوي
            targets = new float[] { joint1, joint2, joint3,
                                    joint4, joint5, joint6 };
        }

        for (int i = 0; i < joints.Count && i < targets.Length; i++)
            MoveJoint(joints[i], targets[i]);
    }

    void MoveJoint(ArticulationBody joint, float targetDegrees)
    {
        var drive = joint.xDrive;
        drive.target     = targetDegrees;
        drive.stiffness  = 10000f;
        drive.damping    = 100f;
        drive.forceLimit = 1000f;
        joint.xDrive = drive;
    }
}
