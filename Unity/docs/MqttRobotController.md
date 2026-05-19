# MqttRobotController.cs — Documentation

> **Location:** `Unity/MqttRobotController.cs`  
> **Language:** C# (Unity MonoBehaviour)  
> **Purpose:** Connects to the MQTT broker, receives live UR5e robot data, parses it, and distributes it to the RobotController and DashboardController

---

## 📋 What Does This Script Do?

This is the **"brain"** of the data pipeline inside Unity.

It listens on an MQTT topic for JSON messages containing live robot data. When a message arrives, it parses all the values (joint angles, temperatures, currents, etc.) and sends them to:
1. `RobotController` → to move the 3D arm
2. `DashboardController` → to update all dashboard panels

---

## 🔧 How It Works (Simple Explanation)

```
Mosquitto MQTT Broker sends a JSON message
        ↓
MqttRobotController receives it (on background thread)
        ↓
Parses all values from the JSON string
        ↓
Queues updates on Unity main thread (thread safety)
        ↓
RobotController moves the 3D arm
DashboardController updates all panels
```

---

## 📦 Variables (Public — Visible in Unity Inspector)

### MQTT Settings

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `brokerAddress` | string | "localhost" | IP address of the MQTT broker. Change to Pi's IP when using real robot |
| `brokerPort` | int | 1883 | MQTT port (1883 is the standard default) |
| `topic` | string | "corex/ur5e/data" | The MQTT topic to subscribe to |

### References

| Variable | Type | Description |
|----------|------|-------------|
| `robotController` | RobotController | Reference to the script that moves the 3D arm |
| `dashboardController` | DashboardController | Reference to the script that updates the dashboard |

---

## 📦 Variables (Private — Internal Use Only)

| Variable | Type | Description |
|----------|------|-------------|
| `client` | MqttClient | The MQTT connection object |

---

## 🔩 Functions

### `Start()`

**What it does:**  
Runs once at startup. Creates the MQTT connection and subscribes to the data topic.

**Steps:**
1. Creates a new `MqttClient` pointed at the broker address and port
2. Registers `OnMessageReceived` as the callback for incoming messages
3. Generates a unique client ID (prevents conflicts if multiple instances run)
4. Connects to the broker
5. If connected successfully → subscribes to the topic
6. If failed → logs an error to Console

**Takes:** Nothing  
**Returns:** Nothing

---

### `OnMessageReceived(sender, e)`

**What it does:**  
Called automatically every time a new MQTT message arrives. This is the main data processing function.

**Steps:**
1. Converts the raw message bytes to a UTF-8 string
2. Parses all data fields from the JSON string
3. Queues the UI and robot updates on Unity's main thread

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `sender` | object | The MQTT client that received the message (not used directly) |
| `e` | MqttMsgPublishEventArgs | Contains the raw message bytes |

**Returns:** Nothing

**Data parsed from each message:**

| Field | Parser Used | Sent To |
|-------|------------|---------|
| `actual_q` | `ParseFloatArray()` | `RobotController` (joint motion) |
| `target_q` | `ParseFloatArray()` | `DashboardController` (position tracking) |
| `actual_current` | `ParseFloatArray()` | `DashboardController` (current panel) |
| `target_current` | `ParseFloatArray()` | `DashboardController` (current panel) |
| `target_moment` | `ParseFloatArray()` | `DashboardController` (torque panel) |
| `joint_temperatures` | `ParseFloatArray()` | `DashboardController` (temp panel) |
| `actual_TCP_pose` | `ParseFloatArray()` | `DashboardController` (TCP panel) |
| `robot_mode` | `ParseInt()` | `DashboardController` (status bar) |

> ⚠️ **Thread Safety Note:** MQTT callbacks run on a background thread. Unity's UI and physics can only be updated from the main thread. That's why all updates are queued through `UnityMainThreadDispatcher`.

---

### `EstimateRUL(temps, currents)`

**What it does:**  
Calculates a simple Remaining Useful Life (RUL) percentage for each of the 6 joints.

> 🔄 **Placeholder:** This will be replaced by the real AI/ML model output later.

**Formula used:**
```
tempFactor    = 1 - clamp((temp - 25) / 55)     → how close to max temp (80°C)
currentFactor = 1 - clamp(current / 2)           → how close to max current (2A)
RUL           = round((tempFactor×0.6 + currentFactor×0.4) × 100)
```

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `temps` | float[] | Temperature of each joint in °C (6 values) |
| `currents` | float[] | Current draw of each joint in Amps (6 values) |

**Returns:** `float[]` — RUL percentage (0–100) for each of the 6 joints

---

### `ParseFloatArray(json, key)`

**What it does:**  
Extracts an array of float numbers from a JSON string for a given key — without needing any external JSON library.

**Example:**
```
Input:  json = '{"actual_q": [-0.208, -1.936, 1.956]}', key = "actual_q"
Output: [-0.208, -1.936, 1.956]
```

**How it works:**
1. Finds the position of the key name in the string
2. Finds the `[` and `]` brackets around the array
3. Splits the content by commas
4. Parses each value as a float

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | string | The full JSON message string |
| `key` | string | The field name to extract (e.g. "actual_q") |

**Returns:** `float[]` — Array of parsed float values. Returns `new float[6]` (all zeros) if key not found.

---

### `ParseInt(json, key)`

**What it does:**  
Extracts a single integer value from a JSON string for a given key.

**Example:**
```
Input:  json = '{"robot_mode": 7}', key = "robot_mode"
Output: 7
```

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `json` | string | The full JSON message string |
| `key` | string | The field name to extract (e.g. "robot_mode") |

**Returns:** `int` — The parsed integer value. Returns `0` if key not found.

---

### `OnDestroy()`

**What it does:**  
Runs automatically when the GameObject is destroyed (e.g. when you stop Play mode). Cleanly disconnects from the MQTT broker to prevent connection leaks.

**Takes:** Nothing  
**Returns:** Nothing

---

## 🔗 Dependencies

| Dependency | Why |
|-----------|-----|
| `M2Mqtt` (uPLibrary) | MQTT client library for Unity |
| `RobotController` | Receives joint angles to move 3D arm |
| `DashboardController` | Receives all data to update dashboard |
| `UnityMainThreadDispatcher` | Safely runs Unity updates from MQTT thread |

---

## 🔄 To Switch from Simulation to Real Robot

Change only **one value** in the Unity Inspector:

| Setting | Simulation | Real Robot |
|---------|-----------|-----------|
| `brokerAddress` | `localhost` | Raspberry Pi's IP (e.g. `192.168.1.100`) |
| `topic` | `corex/ur5e/data` | `corex/ur5e/data` (same) |

Everything else stays the same.