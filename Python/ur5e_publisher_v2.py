import paho.mqtt.client as mqtt
import json
import time
import math

BROKER = "localhost"
PORT   = 1883
TOPIC  = "corex/ur5e/data"

client = mqtt.Client()
client.connect(BROKER, PORT)
client.loop_start()

print("CoreX — UR5e Full Data Publisher")
print(f"Topic: {TOPIC}")
print("Press Ctrl+C to stop\n")

t = 0
try:
    while True:
        # Joint positions (radians)
        actual_q  = [round(-0.208 + 0.5 * math.sin(t * 0.3 + i), 4) for i in range(6)]
        target_q  = [round(q + 0.001 * math.sin(t * 2), 4) for q in actual_q]

        # Joint velocities
        actual_qd = [round(0.1 * math.sin(t * 0.5 + i), 4) for i in range(6)]

        # Currents (Amps)
        actual_current = [round(0.25 + 0.05 * math.sin(t * 0.4 + i), 4) for i in range(6)]
        target_current = [round(0.00 + 0.02 * math.sin(t * 0.4 + i), 4) for i in range(6)]

        # Torques (Nm)
        target_moment = [round(-8.99 + 0.5 * math.sin(t * 0.3 + i), 4) for i in range(6)]

        # Temperatures (Celsius) — realistic values from real robot data
        base_temps = [26.4, 26.5, 26.8, 31.3, 34.1, 35.2]
        joint_temperatures = [round(b + 0.3 * math.sin(t * 0.1 + i), 2) for i, b in enumerate(base_temps)]

        # TCP Pose [x, y, z, rx, ry, rz]
        actual_TCP_pose = [
            round(-0.376 + 0.01 * math.sin(t * 0.2), 4),
            round(-0.058 + 0.01 * math.cos(t * 0.2), 4),
            round( 0.340 + 0.005 * math.sin(t * 0.3), 4),
            round(-2.392, 4),
            round(-1.943, 4),
            round( 0.074, 4),
        ]
        target_TCP_pose = [round(v + 0.001, 4) for v in actual_TCP_pose]

        # Robot & joint modes
        robot_mode = 7   # RUNNING
        joint_mode = [7, 7, 7, 7, 7, 7]

        payload = json.dumps({
            "actual_q":          actual_q,
            "target_q":          target_q,
            "actual_qd":         actual_qd,
            "actual_current":    actual_current,
            "target_current":    target_current,
            "target_moment":     target_moment,
            "joint_temperatures":joint_temperatures,
            "actual_TCP_pose":   actual_TCP_pose,
            "target_TCP_pose":   target_TCP_pose,
            "robot_mode":        robot_mode,
            "joint_mode":        joint_mode,
        })

        client.publish(TOPIC, payload)
        print(f"[{time.strftime('%H:%M:%S')}] Published — temps: {[f'{x:.1f}' for x in joint_temperatures]}")

        time.sleep(0.1)
        t += 0.1

except KeyboardInterrupt:
    print("\nStopped.")
    client.loop_stop()
    client.disconnect()
