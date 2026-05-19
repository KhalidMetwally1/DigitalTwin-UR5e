# RobotController.cs — Documentation

> **Location:** `Unity/RobotController.cs`  
> **Language:** C# (Unity MonoBehaviour)  
> **Purpose:** Controls the 6 joints of the UR5e 3D model in Unity using ArticulationBody physics

---

## 📋 What Does This Script Do?

Think of this script as the **"muscle"** of the Digital Twin.

When the robot receives joint angle values (either from the Inspector manually, or from the MQTT script automatically), this script takes those angles and physically moves the 3D robot model's joints to match them.

It uses Unity's **ArticulationBody** system — which is specially designed for robotic arms and provides realistic joint physics.

---

## 🔧 How It Works (Simple Explanation)

```
Joint angle value arrives (e.g. Joint2 = -90 degrees)
        ↓
RobotController finds the matching ArticulationBody joint
        ↓
Sets the joint's target angle using a "drive" system
        ↓
Unity physics moves the joint smoothly to that angle
```

---

## 📦 Variables (Public — Visible in Unity Inspector)

### Joint Targets

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `joint1` | float | 0 | Target angle for Joint 1 (shoulder pan) in **degrees** |
| `joint2` | float | 0 | Target angle for Joint 2 (shoulder lift) in **degrees** |
| `joint3` | float | 0 | Target angle for Joint 3 (elbow) in **degrees** |
| `joint4` | float | 0 | Target angle for Joint 4 (wrist 1) in **degrees** |
| `joint5` | float | 0 | Target angle for Joint 5 (wrist 2) in **degrees** |
| `joint6` | float | 0 | Target angle for Joint 6 (wrist 3) in **degrees** |
| `speed` | float | 50 | How fast the joints move to their target |

> 💡 **Note:** Values come in as **radians** from the real robot (RTDE), and get converted to degrees by `MqttRobotController` before being passed here.

---

## 📦 Variables (Private — Internal Use Only)

| Variable | Type | Description |
|----------|------|-------------|
| `joints` | `List<ArticulationBody>` | List of all 6 revolute joints found in the robot hierarchy |

---

## 🔩 Functions

### `Start()`

**What it does:**  
Runs once when the game starts. Finds all 6 revolute joints in the UR5e robot hierarchy automatically.

**How it works:**
- Searches all child objects of `ur5e_robot` for `ArticulationBody` components
- Filters only joints with type `RevoluteJoint` (rotation joints)
- Stores them in order in the `joints` list
- Logs how many joints were found to the Console

**Takes:** Nothing  
**Returns:** Nothing  

```
Expected Console output: "Found 6 joints"
```

---

### `Update()`

**What it does:**  
Runs every single frame (60+ times per second). Reads the current target angles and applies them to each joint.

**How it works:**
- Puts all 6 joint target values into an array
- Loops through each joint and calls `MoveJoint()`

**Takes:** Nothing  
**Returns:** Nothing  

---

### `MoveJoint(joint, targetDegrees)`

**What it does:**  
The core function that actually moves a single joint to a target angle.

**How it works:**
- Gets the current "drive" settings of the joint
- Sets the target angle
- Sets physics parameters (stiffness, damping, force limit) to control how the joint moves
- Applies the updated drive back to the joint

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `joint` | ArticulationBody | The specific joint to move |
| `targetDegrees` | float | The angle to move to, in degrees |

**Returns:** Nothing

**Physics Parameters Used:**

| Parameter | Value | Effect |
|-----------|-------|--------|
| Stiffness | 10,000 | How strongly the joint holds its position |
| Damping | 100 | Reduces oscillation / wobbling |
| Force Limit | 1,000 | Maximum force applied (prevents unrealistic snapping) |

---

## 🔗 Dependencies

| Dependency | Why |
|-----------|-----|
| `UnityEngine` | Core Unity library |
| `ArticulationBody` | Unity's physics component for robot joints |
| `MqttRobotController` | Writes to `joint1`–`joint6` variables with live data |

---

## ⚠️ Important Notes

- The script reads joint values in **degrees** — conversion from radians happens in `MqttRobotController`
- `base_link` must have its `ArticulationBody` set to **Immovable** — otherwise the robot base will fall or slide
- **Use Gravity** must be **Disabled** on the `ur5e_robot` — otherwise joints collapse under gravity