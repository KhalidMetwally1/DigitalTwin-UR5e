import paho.mqtt.client as mqtt
import json
import time
import math
import random

# ── MQTT Settings ──────────────────────────────────────────────
BROKER   = "a1d84dccdb22471ea81d9a83b6e53959.s1.eu.hivemq.cloud"
PORT     = 8883
USERNAME = "corex_project"
PASSWORD = "Corex#123"
TOPIC    = "corex/ur5e/data"

client = mqtt.Client()
client.username_pw_set(USERNAME, PASSWORD)
client.tls_set()
client.connect(BROKER, PORT)
client.loop_start()

print("CoreX — UR5e Simulator Started")
print(f"Topic : {TOPIC}")
print("Press Ctrl+C to stop\n")

base_q    = [-0.208, -1.936,  1.956, -1.666, -1.566,  0.002]
base_temp = [ 26.4,   26.5,   26.8,  31.3,   34.1,   35.2 ]
base_rul  = [ 92.0,   88.0,   85.0,  61.0,   54.0,   32.0 ]

t = 0.0

try:
    while True:
        t += 0.05

        actual_q = [round(base_q[i] + 0.05 * math.sin(t * 0.3 + i), 6) for i in range(6)]
        target_q = [round(base_q[i] + 0.05 * math.sin(t * 0.3 + i + 0.02), 6) for i in range(6)]

        actual_current = [round(0.25 + 0.05 * math.sin(t * 0.5 + i) + random.uniform(-0.01, 0.01), 4) for i in range(6)]
        target_current = [round(0.00 + 0.03 * math.sin(t * 0.5 + i), 4) for i in range(6)]

        torque = [round(-8.99 + 0.5 * math.sin(t * 0.4 + i) + random.uniform(-0.1, 0.1), 4) for i in range(6)]

        tcp = [
            round(-0.376 + 0.01 * math.sin(t * 0.2),  4),
            round(-0.058 + 0.01 * math.cos(t * 0.2),  4),
            round( 0.340 + 0.005 * math.sin(t * 0.3), 4),
            round(-2.392 + 0.01 * math.sin(t * 0.15), 4),
            round(-1.943 + 0.01 * math.cos(t * 0.15), 4),
            round( 0.074 + 0.005 * math.sin(t * 0.1), 4),
        ]

        temperatures = [round(base_temp[i] + 0.001 * t + 0.3 * math.sin(t * 0.1 + i), 2) for i in range(6)]
        rul          = [round(max(0.0, base_rul[i] - 0.002 * t + 0.1 * math.sin(t * 0.05 + i)), 1) for i in range(6)]

        payload = json.dumps({
            "actual_q":        actual_q,
            "target_q":        target_q,
            "actual_current":  actual_current,
            "target_current":  target_current,
            "torque":          torque,
            "tcp":             tcp,
            "temperatures":    temperatures,
            "rul":             rul,
            "mode":            "RUNNING"
        })

        client.publish(TOPIC, payload)
        print(f"[t={t:.1f}] Q0={actual_q[0]:.3f} | Temp0={temperatures[0]}°C | RUL_J6={rul[5]}%")
        time.sleep(0.05)

except KeyboardInterrupt:
    print("\nStopped.")
    client.loop_stop()
    client.disconnect()
