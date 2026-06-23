# CoreX — UR5e RTDE to HiveMQ Publisher
# يقرأ البيانات من الروبوت الحقيقي عن طريق RTDE
# ويبعتها على HiveMQ Cloud عشان الداشبورد يستقبلها
#
# Config file -> UR3_min.xml
# Done By: Khalid Metwally

import rtde.rtde as rtde
import rtde.rtde_config as rtde_config
import paho.mqtt.client as mqtt
import json
import time
import ssl

# ── Robot Settings ─────────────────────────────────────────────
ROBOT_IP    = "172.21.0.140"
ROBOT_PORT  = 30004
CONFIG_FILE = "UR3_min.xml"

# ── HiveMQ Cloud Settings ──────────────────────────────────────
BROKER   = "a1d84dccdb22471ea81d9a83b6e53959.s1.eu.hivemq.cloud"
PORT     = 8883
USERNAME = "corex_project"
PASSWORD = "Corex#123"
TOPIC    = "corex/ur5e/data"

ROBOT_MODE_MAP = {
    0:  "DISCONNECTED",
    1:  "CONFIRM_SAFETY",
    2:  "BOOTING",
    3:  "POWER_OFF",
    4:  "POWER_ON",
    5:  "IDLE",
    6:  "BACKDRIVE",
    7:  "RUNNING",
    8:  "UPDATING_FIRMWARE",
}

# ── RUL State (محاكاة تدهور بسيطة لحد ما يكون عندنا موديل حقيقي) ──
rul = [92.0, 88.0, 85.0, 61.0, 54.0, 32.0]
rul_decay_rate = 0.0001   # بينقص كل loop

# ── Connect to HiveMQ ──────────────────────────────────────────
mqtt_client = mqtt.Client()
mqtt_client.username_pw_set(USERNAME, PASSWORD)
mqtt_client.tls_set(tls_version=ssl.PROTOCOL_TLS)

print("Connecting to HiveMQ Cloud...")
mqtt_client.connect(BROKER, PORT)
mqtt_client.loop_start()
print(f"MQTT Connected! Topic: {TOPIC}\n")

# ── Connect to Robot ───────────────────────────────────────────
conf = rtde_config.ConfigFile(CONFIG_FILE)
output_names, output_types = conf.get_recipe("out")

con = rtde.RTDE(ROBOT_IP, ROBOT_PORT)
try:
    con.connect()
    print("Robot Connected!")
except Exception as e:
    print("Failed to connect to robot:", e)
    mqtt_client.loop_stop()
    mqtt_client.disconnect()
    exit(1)

con.send_output_setup(output_names, output_types)
con.send_start()

print("--- Streaming robot data to HiveMQ (Ctrl+C to stop) ---\n")

loop_count = 0
try:
    while True:
        state = con.receive()
        if not state:
            continue

        loop_count += 1

        # ── تحديث RUL ──────────────────────────────────────────
        for i in range(6):
            rul[i] = max(0.0, rul[i] - rul_decay_rate)

        # ── Robot Mode ─────────────────────────────────────────
        mode_id  = int(state.robot_mode)
        mode_str = ROBOT_MODE_MAP.get(mode_id, f"MODE_{mode_id}")

        # ── Build Payload ──────────────────────────────────────
        payload = json.dumps({
            "actual_q":           [round(v, 6) for v in state.actual_q],
            "target_q":           [round(v, 6) for v in state.target_q],
            "actual_qd":          [round(v, 6) for v in state.actual_qd],
            "actual_current":     [round(v, 4) for v in state.actual_current],
            "target_current":     [round(v, 4) for v in state.target_current],
            "target_moment":      [round(v, 4) for v in state.target_moment],
            "joint_temperatures": [round(v, 2) for v in state.joint_temperatures],
            "actual_TCP_pose":    [round(v, 6) for v in state.actual_TCP_pose],
            "target_TCP_pose":    [round(v, 6) for v in state.target_TCP_pose],
            "rul":                [round(v, 1) for v in rul],
            "robot_mode":         mode_str,
            "joint_mode":         list(state.joint_mode),
        })

        mqtt_client.publish(TOPIC, payload)

        # ── Print كل 10 loops عشان مايملاش الـ terminal ────────
        if loop_count % 10 == 0:
            print(
                f"[{time.strftime('%H:%M:%S')}] "
                f"Mode: {mode_str} | "
                f"Q0={state.actual_q[0]:+.3f} | "
                f"Temp={[f'{v:.1f}' for v in state.joint_temperatures]} | "
                f"RUL_J6={rul[5]:.1f}%"
            )

        time.sleep(0.05)   # 20 Hz — مناسب للـ Raspberry Pi

except KeyboardInterrupt:
    print("\nStopped.")

finally:
    con.send_pause()
    con.disconnect()
    mqtt_client.loop_stop()
    mqtt_client.disconnect()
    print("Disconnected from robot and MQTT.")
