import cv2
import mediapipe as mp
import time
import socket
import json

MODEL_PATH = "pose_landmarker_lite.task"

# UDP hacia Unity
UDP_IP = "127.0.0.1"
UDP_PORT = 5052
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

BaseOptions = mp.tasks.BaseOptions
PoseLandmarker = mp.tasks.vision.PoseLandmarker
PoseLandmarkerOptions = mp.tasks.vision.PoseLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

options = PoseLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=MODEL_PATH),
    running_mode=VisionRunningMode.VIDEO,
    num_poses=1,
    min_pose_detection_confidence=0.5,
    min_pose_presence_confidence=0.5,
    min_tracking_confidence=0.5
)

cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("No se pudo abrir la cámara.")
    exit()

connections = mp.tasks.vision.PoseLandmarksConnections.POSE_LANDMARKS

with PoseLandmarker.create_from_options(options) as landmarker:

    start_time = time.time()

    while True:
        ret, frame = cap.read()

        if not ret:
            break

        # Espejo
        frame = cv2.flip(frame, 1)

        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        mp_image = mp.Image(
            image_format=mp.ImageFormat.SRGB,
            data=rgb_frame
        )

        timestamp_ms = int((time.time() - start_time) * 1000)

        result = landmarker.detect_for_video(
            mp_image,
            timestamp_ms
        )

        if result.pose_landmarks:

            landmarks = result.pose_landmarks[0]

            # --------------------------
            # ENVIAR LANDMARKS A UNITY
            # --------------------------

            data = {
                "landmarks": []
            }

            for i, landmark in enumerate(landmarks):
                data["landmarks"].append({
                    "id": i,
                    "x": landmark.x,
                    "y": landmark.y,
                    "z": landmark.z,
                    "visibility": landmark.visibility
                })

            message = json.dumps(data)

            sock.sendto(
                message.encode("utf-8"),
                (UDP_IP, UDP_PORT)
            )

            # --------------------------
            # DIBUJAR ESQUELETO
            # --------------------------

            height, width, _ = frame.shape

            for connection in connections:

                landmark1 = landmarks[connection.start]
                landmark2 = landmarks[connection.end]

                x1 = int(landmark1.x * width)
                y1 = int(landmark1.y * height)

                x2 = int(landmark2.x * width)
                y2 = int(landmark2.y * height)

                cv2.line(
                    frame,
                    (x1, y1),
                    (x2, y2),
                    (0, 255, 0),
                    2
                )

            for landmark in landmarks:

                x = int(landmark.x * width)
                y = int(landmark.y * height)

                cv2.circle(
                    frame,
                    (x, y),
                    5,
                    (0, 0, 255),
                    -1
                )

        cv2.imshow("Body Tracking", frame)

        if cv2.waitKey(1) & 0xFF == ord("q"):
            break

cap.release()
sock.close()
cv2.destroyAllWindows()