# DashboardController.cs — Documentation

> **Location:** `Unity/DashboardController.cs`  
> **Language:** C# (Unity MonoBehaviour)  
> **Purpose:** Receives live robot data and updates all 8 dashboard panels in real-time using Unity UI Toolkit

---

## 📋 What Does This Script Do?

This is the **"display manager"** of the dashboard.

It holds references to every UI element on the dashboard (labels, bars, panels) and exposes simple functions that `MqttRobotController` calls whenever new data arrives. Each function updates the relevant panel with the new values and applies color coding automatically.

---

## 🔧 How It Works (Simple Explanation)

```
MqttRobotController receives new data
        ↓
Calls DashboardController.UpdateTemperatures([26.4, 26.5, ...])
        ↓
DashboardController finds the UI labels and bars
        ↓
Updates text + bar width + color class
        ↓
Dashboard panel updates instantly ✅
```

---

## 📦 Variables (Public — Visible in Unity Inspector)

| Variable | Type | Description |
|----------|------|-------------|
| `uiDocument` | UIDocument | The Unity UI Document containing the dashboard UXML |
| `robotRenderTexture` | RenderTexture | The render texture from RobotCam — displayed in the 3D viewport panel |

---

## 📦 Variables (Private — Internal Use Only)

All private variables are references to specific UI elements, found at startup using their UXML `name` attribute.

| Variable | UI Element | Panel |
|----------|-----------|-------|
| `robotModeLabel` | Label | Status Bar |
| `timestampLabel` | Label | Status Bar |
| `tempLabels[6]` | Label × 6 | Joint Temperatures |
| `tempBars[6]` | VisualElement × 6 | Joint Temperatures |
| `currentVal` | Label | Current & Torque |
| `targetCurrentVal` | Label | Current & Torque |
| `torqueVal` | Label | Current & Torque |
| `currentError` | Label | Current & Torque |
| `tcpX/Y/Z/RX/RY/RZ` | Label × 6 | TCP Pose |
| `tqLabels[6]` | Label × 6 | Position Tracking |
| `aqLabels[6]` | Label × 6 | Position Tracking |
| `eqLabels[6]` | Label × 6 | Position Tracking |
| `rulBars[6]` | VisualElement × 6 | RUL per Joint |
| `rulVals[6]` | Label × 6 | RUL per Joint |
| `twinViewport` | VisualElement | Digital Twin Viewport |

---

## 🔩 Functions

### `Start()`

**What it does:**  
Runs once at startup. Finds and stores references to every UI element on the dashboard.

**How it works:**
- Uses `root.Q<Label>("name")` and `root.Q<VisualElement>("name")` to find elements by their UXML `name` attribute
- Sets the 3D viewport's background image to the RobotCam render texture

**Takes:** Nothing  
**Returns:** Nothing

---

### `Update()`

**What it does:**  
Runs every frame. Updates the live clock in the status bar.

**Takes:** Nothing  
**Returns:** Nothing

---

### `UpdateTemperatures(temps)`

**What it does:**  
Updates the Joint Temperatures panel — sets the text value and bar width for each of the 6 joints, and applies a color class based on temperature thresholds.

**Color thresholds:**

| Class Applied | Condition | Color |
|--------------|-----------|-------|
| `temp-normal` | temp < 35°C | 🟢 Green |
| `temp-warn` | 35°C ≤ temp < 50°C | 🟡 Amber |
| `temp-danger` | temp ≥ 50°C | 🔴 Red |

**Bar width:** Calculated as `(temp / 60) × 100%` — clamped between 0–100%

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `temps` | float[] | Temperature in °C for each of the 6 joints |

**Returns:** Nothing

---

### `UpdateCurrentTorque(actualCurrent, targetCurrent, torque)`

**What it does:**  
Updates the Current & Torque panel with live readings from Joint 2 (index 1) — the joint under the most load.

**Displays:**
- Actual current (A)
- Target current (A)
- Torque reading (Nm)
- Current error = |actual − target| shown in amber if non-zero

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `actualCurrent` | float[] | Actual motor current for all 6 joints (Amps) |
| `targetCurrent` | float[] | Target motor current for all 6 joints (Amps) |
| `torque` | float[] | Target torque for all 6 joints (Nm) |

**Returns:** Nothing

---

### `UpdatePositions(targetQ, actualQ)`

**What it does:**  
Updates the Position Tracking table — shows target angle, actual angle, and error for each of the 6 joints. Applies color coding to the error column.

**Error color thresholds:**

| Class Applied | Condition | Color |
|--------------|-----------|-------|
| `ok` | error < 0.01 rad | 🟢 Green |
| `warn` | 0.01 ≤ error < 0.05 rad | 🟡 Amber |
| `danger` | error ≥ 0.05 rad | 🔴 Red |

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `targetQ` | float[] | Target joint positions in radians (6 values) |
| `actualQ` | float[] | Actual joint positions in radians (6 values) |

**Returns:** Nothing

---

### `UpdateTCP(tcp)`

**What it does:**  
Updates the TCP Pose panel below the 3D viewport with the current tool center point coordinates.

**Displays:** X, Y, Z (meters) and RX, RY, RZ (radians) — 6 values total

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `tcp` | float[] | Tool center point pose [x, y, z, rx, ry, rz] |

**Returns:** Nothing

---

### `UpdateRUL(rulPercents)`

**What it does:**  
Updates the Remaining Useful Life panel — sets bar height and percentage label for each of the 6 joints, with color coding.

**Bar height:** Set directly as a percentage of the bar container height

**Color thresholds:**

| Class Applied | Condition | Color |
|--------------|-----------|-------|
| `rul-good` / `good` | RUL > 70% | 🟢 Green |
| `rul-warn` / `warn` | 40% ≤ RUL ≤ 70% | 🟡 Amber |
| `rul-danger` / `danger` | RUL < 40% | 🔴 Red |

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `rulPercents` | float[] | RUL percentage (0–100) for each of the 6 joints |

**Returns:** Nothing

---

### `SetRobotMode(mode)`

**What it does:**  
Updates the robot mode label in the status bar.

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `mode` | string | Mode string — e.g. "RUNNING", "IDLE", "DISCONNECTED" |

**Returns:** Nothing

**Display format:** `● RUNNING` (bullet + uppercase mode name)

---

## 🔗 Dependencies

| Dependency | Why |
|-----------|-----|
| `UnityEngine.UIElements` | UI Toolkit — all UI element types |
| `UIDocument` | The root document holding the UXML |
| `RenderTexture` | For displaying the 3D robot camera in the viewport |
| `MqttRobotController` | Calls all `Update*()` methods with live data |