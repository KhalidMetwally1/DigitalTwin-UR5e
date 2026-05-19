# CoreX - AI-Driven Digital Twin Framework for Predictive Maintenance in Industrial Robotic Systems

<div align="center">

![CoreX](https://img.shields.io/badge/CoreX-Digital%20Twin-005CFF?style=for-the-badge)
![Unity](https://img.shields.io/badge/Unity-6.4-black?style=for-the-badge&logo=unity)
![Python](https://img.shields.io/badge/Python-3.14-blue?style=for-the-badge&logo=python)
![MQTT](https://img.shields.io/badge/MQTT-Mosquitto-purple?style=for-the-badge)

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
CoreX/
│
├── 📁 Unity/                          # Unity C# Scripts
│   ├── RobotController.cs             # Controls 6-DOF joint motion via ArticulationBody
│   ├── MqttRobotController.cs         # MQTT subscriber — receives & parses robot data
│   ├── UnityMainThreadDispatcher.cs   # Thread-safe dispatcher for MQTT callbacks
│   ├── DashboardController.cs         # Binds live data to Unity UI Toolkit dashboard
│   │
│   └── 📁 docs/
│       ├── RobotController.md         # Full documentation — RobotController.cs
│       ├── MqttRobotController.md     # Full documentation — MqttRobotController.cs
│       ├── UnityMainThreadDispatcher.md
│       └── DashboardController.md
│
├── 📁 Python/                         # Python Scripts
│   ├── ur5e_publisher_v2.py           # Simulated UR5e data publisher over MQTT
│   │
│   └── 📁 docs/
│       └── ur5e_publisher_v2.md       # Full documentation — publisher script
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
│          Python RTDE → MQTT Publisher                    │
└───────────────────────┬─────────────────────────────────┘
                        │ MQTT  topic: corex/ur5e/data
                        ▼
┌─────────────────────────────────────────────────────────┐
│              Mosquitto MQTT Broker                       │
│                  localhost:1883                          │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   Unity 6.4                              │
│                                                          │
│  MqttRobotController  ──►  RobotController              │
│          │                  (3D arm moves)               │
│          └──────────────►  DashboardController           │
│                             (UI panels update)           │
└─────────────────────────────────────────────────────────┘
```

---

## 🚀 Quick Start

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Unity Hub + Unity | 6.4 (6000.4.5f1) | Main development environment |
| Python | 3.14+ | Data publisher / Pi script |
| Mosquitto | Latest | MQTT Broker |
| paho-mqtt | 2.1.0 | Python MQTT library |
| Git | Latest | Version control |

### 1. Start the MQTT Broker
```bash
net start mosquitto
```

### 2. Run the Data Publisher (simulation)
```bash
cd Python/
python ur5e_publisher_v2.py
```

### 3. Open Unity Project
- Open `Unity/` folder in Unity Hub
- Press **Play ▶️**
- Dashboard and 3D arm will start updating automatically

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
<sub>Built with Unity 6.4 · Python 3.14 · MQTT · UR5e RTDE</sub>
</div>
