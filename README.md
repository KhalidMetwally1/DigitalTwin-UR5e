# CoreX - AI-Driven Digital Twin Framework for Predictive Maintenance in Industrial Robotic Systems

<div align="center">

![CoreX](https://img.shields.io/badge/CoreX-Digital%20Twin-005CFF?style=for-the-badge)
![Unity](https://img.shields.io/badge/Unity-6.4-black?style=for-the-badge&logo=unity)
![Python](https://img.shields.io/badge/Python-3.14-blue?style=for-the-badge&logo=python)
![MQTT](https://img.shields.io/badge/MQTT-Mosquitto-purple?style=for-the-badge)
![HiveMQ](https://img.shields.io/badge/HiveMQ-Cloud-yellow?style=for-the-badge)

**Real-time Digital Twin for a UR5e Robotic Arm**  
Graduation Project — Digital Twin Team

</div>

---

## 📌 Overview

CoreX provides a scalable and industry-ready approach for predictive maintenance using Digital Twin and AI technologies. By combining non-invasive hardware, high-fidelity modeling, advanced analytics, and industrial compatibility, the system bridges a critical gap between theoretical predictive systems and practical deployment. The modularity of the system enables adoption by both large-scale factories and smaller facilities lacking centralized monitoring infrastructure.

This repo includes the real-time Digital Twin system for a **Universal Robots UR5e** robotic arm. It streams live joint data from a **Raspberry Pi** over **MQTT**, visualizes the robot in a **Unity 3D** environment, and displays a full monitoring dashboard including joint health, temperature, current, torque, position tracking, and **Remaining Useful Life (RUL)** per joint.

---

## 🗂️ Repository Structure

```
DigitalTwin-UR5e/
│
├── 📁 mqttnet.5.1.0.1559/             # MQTTnet Library (HiveMQ Cloud Support)
│
├── 📁 Python/                         # Python Scripts
│   ├── ur5e_rtde_to_hivemq.py         # Reads live UR5e data via RTDE → publishes to HiveMQ Cloud
│   └── ur5e_simulator.py              # Simulates UR5e data for testing (no real robot needed)
│
├── 📁 UI/                             # Dashboard UI Files
│   ├── 📁 USS/                        # Stylesheets (CoreX Light Theme)
│   ├── 📁 UXML/                       # Dashboard layout and structure
│   └── 📁 Scripts/                    # UI data-binding C# scripts
│
├── 📁 Unity/                          # Unity C# Scripts
│   ├── MqttDashboardReceiver.cs       # Receives data from HiveMQ Cloud → feeds dashboard & robot
│   └── RobotController.cs            # Controls 6-DOF joint motion via ArticulationBody
│
├── 📁 URDF and Meshes/                # UR5e 3D Model
│   ├── ur5e_fixed.urdf                # URDF with corrected mesh paths for Unity import
│   └── meshes/                        # Visual (.dae) and collision (.stl) mesh files
│                                      # with custom materials (silver body + blue joints)
│
└── README.md                          # This file
```

---

## ⚙️ System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     UR5e Real Robot                      │
│              (joint angles, temps, currents)             │
└───────────────────────┬─────────────────────────────────┘
                        │ RTDE (Ethernet)
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   Raspberry Pi                           │
│            ur5e_rtde_to_hivemq.py                        │
└───────────────────────┬─────────────────────────────────┘
                        │ MQTT over TLS (port 8883)
                        ▼
┌─────────────────────────────────────────────────────────┐
│                 HiveMQ Cloud Broker                      │
│              topic: corex/ur5e/data                      │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   Unity 6.4                              │
│                                                          │
│  MqttDashboardReceiver ──►  RobotController             │
│          │                   (3D arm moves)              │
│          └─────────────►  DashboardController            │
│                             (UI panels update)           │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Components

### 1. `mqttnet.5.1.0.1559/` — MQTTnet Library
The **MQTTnet** library (v5.1.0) is included directly in the repository to enable **TLS-secured MQTT communication** with HiveMQ Cloud from within Unity. Unlike the standard M2Mqtt library, MQTTnet fully supports SSL/TLS on port 8883, which is required for cloud broker connections.

---

### 2. `Python/` — Data Publisher & Simulator

| File | Purpose |
|------|---------|
| `ur5e_rtde_to_hivemq.py` | Connects to the real UR5e robot via **RTDE protocol** over Ethernet, reads live joint data (positions, velocities, currents, temperatures, TCP pose, robot mode), and publishes it to **HiveMQ Cloud** over MQTT with TLS. This is the script that runs on the Raspberry Pi next to the robot. |
| `ur5e_simulator.py` | Generates realistic simulated UR5e data using sine waves — mimicking smooth robot motion — and publishes it to HiveMQ Cloud in the exact same JSON format. Used for testing the full pipeline without a physical robot. |

**Dependencies:**
```bash
pip install paho-mqtt ur-rtde
```

---

### 3. `UI/` — Dashboard Interface

The dashboard is built with **Unity UI Toolkit** and split into three layers:

| Folder | Contents |
|--------|---------|
| `USS/` | CoreX Light Theme stylesheet — defines colors (`#005CFF` primary), panel borders, temperature/RUL color thresholds, and layout |
| `UXML/` | Dashboard structure with 8 panels: Status Bar, Joint Temperatures, Current & Torque, UR5e Digital Twin viewport, TCP Pose, Position Tracking, Anomaly Alerts, RUL per Joint, and Maintenance Schedule |
| `Scripts/` | C# data-binding scripts that connect incoming MQTT data to the UI elements in real-time |

---

### 4. `Unity/` — Unity C# Scripts

| File | Purpose |
|------|---------|
| `MqttDashboardReceiver.cs` | Subscribes to the HiveMQ Cloud broker via TLS (port 8883), receives the JSON robot data, parses all fields, and distributes them to `RobotController` (to move the 3D arm) and `DashboardController` (to update all dashboard panels). Handles thread-safety via the main thread dispatcher. |
| `RobotController.cs` | Receives joint angles from `MqttDashboardReceiver` and applies them to the UR5e 3D model using Unity's **ArticulationBody** physics system, enabling real-time synchronized motion between the physical robot and its digital twin. |

---

### 5. `URDF and Meshes/` — UR5e 3D Model

Contains the complete 3D model of the UR5e robotic arm, ready for direct import into Unity:

- **`ur5e_fixed.urdf`** — The official Universal Robots URDF with mesh paths corrected from ROS `package://` format to relative paths compatible with Unity's URDF Importer.
- **`meshes/`** — Visual (`.dae`) and collision (`.stl`) mesh files for all 6 links: base, shoulder, upper arm, forearm, and wrists 1–3.
- **Custom materials** — Robot appearance has been tuned to match the real UR5e: silver metallic body, blue joint caps, and dark grey joint rings.

---

## 🚀 Quick Start

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Unity Hub + Unity | 6.4 (6000.4.5f1) | Main development environment |
| Python | 3.14+ | Data publisher / simulator |
| paho-mqtt | 2.1.0 | Python MQTT library |
| HiveMQ Cloud account | Free tier | Cloud MQTT broker |

### 1. Run the Simulator (no robot needed)
```bash
cd Python/
# Set your HiveMQ credentials in the script first
python ur5e_simulator.py
```

### 2. Run with Real Robot (Raspberry Pi)
```bash
cd Python/
# Set robot IP and HiveMQ credentials in the script
python ur5e_rtde_to_hivemq.py
```

### 3. Open Unity Project
- Import `URDF and Meshes/ur5e_fixed.urdf` using the URDF Importer package
- Copy `Unity/` scripts to `Assets/Scripts/`
- Copy `UI/` folder to `Assets/UI/`
- Set HiveMQ credentials in `MqttDashboardReceiver.cs` Inspector fields
- Press **Play ▶️**

---

## 📡 MQTT Data Format

**Topic:** `corex/ur5e/data`

```json
{
  "actual_q":           [-0.208, -1.936, 1.956, -1.666, -1.566, 0.002],
  "target_q":           [-0.208, -1.936, 1.956, -1.666, -1.566, 0.002],
  "actual_qd":          [0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
  "actual_current":     [0.25, 0.30, 0.28, 0.22, 0.18, 0.15],
  "target_current":     [0.00, 0.00, 0.00, 0.00, 0.00, 0.00],
  "target_moment":      [-8.99, -7.50, 3.20, -1.10, 0.50, 0.10],
  "joint_temperatures": [26.4, 26.5, 26.8, 31.3, 34.1, 35.2],
  "actual_TCP_pose":    [-0.376, -0.058, 0.340, -2.392, -1.943, 0.074],
  "target_TCP_pose":    [-0.376, -0.058, 0.340, -2.392, -1.943, 0.074],
  "robot_mode":         7,
  "joint_mode":         [7, 7, 7, 7, 7, 7]
}
```

### Parameter Reference

| Parameter | Unit | Description |
|-----------|------|-------------|
| `actual_q` | radians | Actual joint positions (6 joints) |
| `target_q` | radians | Target joint positions (6 joints) |
| `actual_qd` | rad/s | Actual joint velocities |
| `actual_current` | Amps | Actual motor current per joint |
| `target_current` | Amps | Target motor current per joint |
| `target_moment` | Nm | Target torque per joint |
| `joint_temperatures` | °C | Temperature per joint |
| `actual_TCP_pose` | m / rad | Tool center point [x,y,z,rx,ry,rz] |
| `robot_mode` | enum | 7=Running, 0=Disconnected |
| `joint_mode` | enum | 7=Running per joint |

---

## 🖥️ Dashboard Panels

| Panel | Data Source | Purpose |
|-------|------------|---------|
| Status Bar | `robot_mode` | Robot state + timestamp |
| Joint Temperatures | `joint_temperatures` | 6-joint temp bars with color thresholds |
| Current & Torque | `actual_current`, `target_moment` | Load monitoring + error delta |
| UR5e Digital Twin | `actual_q`, `actual_TCP_pose` | 3D robot viewport |
| TCP Pose | `actual_TCP_pose` | Tool position in 3D space |
| Position Tracking | `target_q`, `actual_q` | Per-joint error table |
| Anomaly Alerts | AI model output | Real-time anomaly feed |
| RUL per Joint | Computed | Remaining useful life bars |
| Maintenance Schedule | RUL output | Priority maintenance list |

---

## 🤖 Robot Goals

| Goal | Status | Method |
|------|--------|--------|
| Anomaly Detection | 🔄 In Progress | AI/ML model on joint data |
| RUL Calculation | 🔄 In Progress | Temp + current + torque model |
| Predictive Maintenance | 🔄 In Progress | RUL-based scheduling |
| Real-time Digital Twin | ✅ Complete | MQTT → Unity ArticulationBody |
| Live Dashboard | ✅ Complete | Unity UI Toolkit |

---

## 👥 Team

**CoreX - Digital Twin**  
Graduation Project · 2026

---

<div align="center">
<sub>Built with Unity 6.4 · Python 3.14 · MQTTnet · HiveMQ Cloud · UR5e RTDE</sub>
</div>
