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

    private List<ArticulationBody> joints = new List<ArticulationBody>();

    void Start()
    {
        // Get all revolute joints in order
        ArticulationBody[] bodies = GetComponentsInChildren<ArticulationBody>();
        foreach (var body in bodies)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                joints.Add(body);
            }
        }
        Debug.Log("Found " + joints.Count + " joints");
    }

    void Update()
    {
        float[] targets = { joint1, joint2, joint3, joint4, joint5, joint6 };

        for (int i = 0; i < joints.Count && i < targets.Length; i++)
        {
            MoveJoint(joints[i], targets[i]);
        }
    }

    void MoveJoint(ArticulationBody joint, float targetDegrees)
    {
        var drive = joint.xDrive;
        drive.target = targetDegrees;
        drive.stiffness = 10000f;
        drive.damping = 100f;
        drive.forceLimit = 1000f;
        joint.xDrive = drive;
    }
}