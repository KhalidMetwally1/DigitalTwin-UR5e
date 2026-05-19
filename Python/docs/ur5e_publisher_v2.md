# ur5e_publisher_v2.py — Documentation

> **Location:** `Python/ur5e_publisher_v2.py`  
> **Language:** Python 3.14+  
> **Purpose:** Simulates live UR5e robot data and publishes it over MQTT at 10Hz — used for testing until the real Raspberry Pi is connected

---

## 📋 What Does This Script Do?

This script acts as a **stand-in for the real robot** during development.

Instead of reading data from the actual UR5e arm (which requires a Raspberry Pi connected to the robot), this script **generates realistic fake data** using sine wave math — similar to how a real robot moves smoothly between positions.

It publishes this data to the MQTT broker every 100ms (10 times per second), and Unity picks it up and displays it on the dashboard just like it would with real data.

---

## 🔧 How It Works (Simple Explanation)

```
Script starts → connects to MQTT broker
        ↓
Every 100ms:
  - Calculate new joint angles using sine waves
  - Calculate temperatures, currents, torques
  - Pack everything into a JSON string
  - Publish to MQTT topic: corex/ur5e/data
        ↓
Unity receives the message and updates the dashboard
```

---

## ⚙️ Configuration Variables

These are defined at the top of the script and can be changed:

| Variable | Default | Description |
|----------|---------|-------------|
| `BROKER` | `"localhost"` | IP address of the MQTT broker. Change to Pi IP for real robot |
| `PORT` | `1883` | MQTT port number (standard default) |
| `TOPIC` | `"corex/ur5e/data"` | The topic Unity is subscribed to |

---

## 📦 Data Published

Every message contains a JSON object with the following fields:

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| `actual_q` | list[6] | radians | Simulated actual joint positions |
| `target_q` | list[6] | radians | Simulated target joint positions (slightly ahead of actual) |
| `actual_qd` | list[6] | rad/s | Simulated joint velocities |
| `actual_current` | list[6] | Amps | Simulated motor currents |
| `target_current` | list[6] | Amps | Simulated target currents |
| `target_moment` | list[6] | Nm | Simulated joint torques |
| `joint_temperatures` | list[6] | °C | Simulated temperatures (based on real robot baseline values) |
| `actual_TCP_pose` | list[6] | m / rad | Simulated tool center point [x,y,z,rx,ry,rz] |
| `target_TCP_pose` | list[6] | m / rad | Target TCP (slightly offset from actual) |
| `robot_mode` | int | enum | Always 7 (RUNNING) |
| `joint_mode` | list[6] | enum | Always [7,7,7,7,7,7] (all RUNNING) |

---

## 🔩 How the Simulated Data is Generated

### Joint Positions (`actual_q`)
```python
actual_q[i] = base_position + 0.5 × sin(t × 0.3 + i)
```
- Starts from real robot positions captured from our UR5e
- Oscillates smoothly using sine waves
- Each joint has a phase offset (`+ i`) so they don't all move together

### Joint Velocities (`actual_qd`)
```python
actual_qd[i] = 0.1 × sin(t × 0.5 + i)
```
- Small oscillating velocities — realistic for slow robot motion

### Currents (`actual_current`)
```python
actual_current[i] = 0.25 + 0.05 × sin(t × 0.4 + i)
```
- Based on real readings from our UR5e (avg ~0.25A at rest)
- Small fluctuations simulate real motor behavior

### Temperatures (`joint_temperatures`)
```python
joint_temperatures[i] = base_temp[i] + 0.3 × sin(t × 0.1 + i)
```
Base values taken from real robot data:
| Joint | Base Temp |
|-------|-----------|
| J1 | 26.4°C |
| J2 | 26.5°C |
| J3 | 26.8°C |
| J4 | 31.3°C |
| J5 | 34.1°C |
| J6 | 35.2°C |

> 💡 J4–J6 run warmer because they carry more load in typical operations.

### TCP Pose
```python
actual_TCP_pose = [
    -0.376 + 0.01 × sin(t × 0.2),   # X
    -0.058 + 0.01 × cos(t × 0.2),   # Y
     0.340 + 0.005 × sin(t × 0.3),  # Z
    -2.392,                           # RX (fixed)
    -1.943,                           # RY (fixed)
     0.074                            # RZ (fixed)
]
```
- Position oscillates slightly around the real robot's resting pose
- Rotation stays fixed (robot isn't rotating its tool)

---

## 🔩 Script Flow

### Setup (runs once)
1. Creates MQTT client
2. Connects to broker at `localhost:1883`
3. Starts background loop for MQTT keep-alive
4. Prints confirmation to terminal

### Main Loop (runs every 100ms)
1. Increments time variable `t` by 0.1
2. Calculates all 6 joint values using sine math
3. Builds JSON string with `json.dumps()`
4. Publishes to `corex/ur5e/data`
5. Prints a status line showing current temperatures
6. Sleeps for 100ms

### Shutdown (on Ctrl+C)
1. Stops the MQTT background loop
2. Disconnects from broker cleanly
3. Prints "Stopped."

---

## 🔄 How to Switch to Real Robot Data

When the Raspberry Pi is ready, replace this script with one that:
1. Uses `ur-rtde` library to read from the real UR5e
2. Packages the same JSON format
3. Publishes to the same topic: `corex/ur5e/data`

**Unity does not need any changes** — it only cares about the MQTT topic and JSON format, not where the data comes from.

```python
# Real robot version (future)
import rtde_receive
r = rtde_receive.RTDEReceiveInterface("192.168.1.X")  # Robot IP

actual_q = r.getActualQ()
joint_temperatures = r.getJointTemperatures()
# ... same JSON format, same topic
```

---

## 🚀 How to Run

```bash
# Install dependency (once)
pip install paho-mqtt

# Run
python ur5e_publisher_v2.py
```

**Expected terminal output:**
```
CoreX — UR5e Full Data Publisher
Topic: corex/ur5e/data
Press Ctrl+C to stop

[06:48:00] Published — temps: ['26.4', '26.5', '26.8', '31.3', '34.1', '35.2']
[06:48:00] Published — temps: ['26.4', '26.5', '26.9', '31.3', '34.1', '35.2']
...
```

**To stop:** Press `Ctrl+C`

---

## 🔗 Dependencies

| Library | Install | Purpose |
|---------|---------|---------|
| `paho-mqtt` | `pip install paho-mqtt` | MQTT client for Python |
| `json` | Built-in | JSON serialization |
| `math` | Built-in | Sine/cosine for data simulation |
| `time` | Built-in | Sleep and timestamp |