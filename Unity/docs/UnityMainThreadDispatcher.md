# UnityMainThreadDispatcher.cs — Documentation

> **Location:** `Unity/UnityMainThreadDispatcher.cs`  
> **Language:** C# (Unity MonoBehaviour)  
> **Purpose:** Safely transfers work from background threads (like MQTT) to Unity's main thread

---

## 📋 What Does This Script Do?

This script solves a critical problem in Unity:

> **Unity's rule:** You can ONLY update UI elements, move GameObjects, or change physics from the **main thread**. If you try to do it from another thread (like an MQTT callback), Unity will crash or throw errors.

MQTT messages arrive on a **background thread**. So before we can update the dashboard or move the robot, we need to "hand off" that work to the main thread safely.

This script acts as a **post office**:
- Background threads **drop off** work (actions) into a queue
- Every frame, the main thread **picks up** and **executes** everything in the queue

---

## 🔧 How It Works (Simple Explanation)

```
MQTT callback (background thread)
        ↓
Calls: UnityMainThreadDispatcher.Instance().Enqueue(() => { ... })
        ↓
Action is added to the queue (thread-safe)
        ↓
Next frame — Update() runs on main thread
        ↓
Picks up all queued actions and executes them
        ↓
Robot moves ✅  Dashboard updates ✅
```

---

## 📦 Variables (Private — Internal Use Only)

| Variable | Type | Description |
|----------|------|-------------|
| `_instance` | UnityMainThreadDispatcher | The single shared instance of this script (Singleton) |
| `_queue` | `Queue<Action>` | Thread-safe queue holding all pending actions |

---

## 🔩 Functions

### `Instance()`

**What it does:**  
Returns the single shared instance of the dispatcher. This is how other scripts access it.

**Design pattern:** Singleton — only one instance exists at a time.

**Takes:** Nothing

**Returns:** `UnityMainThreadDispatcher` — the active instance

**Throws:** `Exception` if no instance exists in the scene (you forgot to add the script to a GameObject)

**Usage example:**
```csharp
UnityMainThreadDispatcher.Instance().Enqueue(() => {
    // This code will run on the main thread
    robotController.joint1 = 45f;
});
```

---

### `Awake()`

**What it does:**  
Runs when the GameObject first loads. Sets up the singleton instance and makes sure it persists between scenes.

**How it works:**
- If no instance exists yet → sets this as the instance
- Calls `DontDestroyOnLoad()` → keeps this GameObject alive when scenes change

**Takes:** Nothing  
**Returns:** Nothing

---

### `Enqueue(action)`

**What it does:**  
Adds a piece of work (an Action) to the queue so it can be executed on the main thread next frame.

**Called by:** `MqttRobotController` — every time a new MQTT message is received

**Takes:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `action` | Action | A block of code to run on the main thread (written as a lambda: `() => { ... }`) |

**Returns:** Nothing

**Thread safety:** Uses `lock(_queue)` to prevent two threads from modifying the queue at the same time.

---

### `Update()`

**What it does:**  
Runs every frame on Unity's main thread. Processes all queued actions.

**How it works:**
1. Locks the queue (prevents new items being added mid-processing)
2. Loops through all queued actions
3. Removes each action from the queue and executes it
4. Releases the lock

**Takes:** Nothing  
**Returns:** Nothing

> ⚡ **Performance Note:** If many MQTT messages arrive between frames, multiple actions may be queued and all executed in a single `Update()` call. This is intentional and safe.

---

## 🔗 Dependencies

| Dependency | Why |
|-----------|-----|
| `UnityEngine` | Core Unity library |
| `System` | For `Action` and `Exception` types |
| `System.Collections.Generic` | For `Queue<Action>` |

---

## ⚠️ Setup Requirements

1. This script must be attached to a **GameObject in the Scene** (e.g. `MqttManager`)
2. Only **one instance** should exist in the scene at a time
3. Must be present **before** any script calls `Instance()` — ensure execution order if needed