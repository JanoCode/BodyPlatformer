import cv2
import mediapipe as mp
import time
import socket
import json
import struct
import numpy as np

MODEL_PATH = "pose_landmarker_lite.task"

# Unity -> Python
FRAME_IP = "127.0.0.1"
FRAME_PORT = 5053

# Python -> Unity
UNITY_IP = "127.0.0.1"
UNITY_PORT = 5052

udp_socket = socket.socket(
    socket.AF_INET,
    socket.SOCK_DGRAM
)

BaseOptions = mp.tasks.BaseOptions
PoseLandmarker = mp.tasks.vision.PoseLandmarker
PoseLandmarkerOptions = mp.tasks.vision.PoseLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

options = PoseLandmarkerOptions(
    base_options=BaseOptions(
        model_asset_path=MODEL_PATH
    ),
    running_mode=VisionRunningMode.VIDEO,
    num_poses=1,
    min_pose_detection_confidence=0.5,
    min_pose_presence_confidence=0.5,
    min_tracking_confidence=0.5
)


def receive_exact(sock, size):
    data = b""

    while len(data) < size:
        packet = sock.recv(size - len(data))

        if not packet:
            return None

        data += packet

    return data


server_socket = socket.socket(
    socket.AF_INET,
    socket.SOCK_STREAM
)

server_socket.setsockopt(
    socket.SOL_SOCKET,
    socket.SO_REUSEADDR,
    1
)

server_socket.bind(
    (FRAME_IP, FRAME_PORT)
)

server_socket.listen(1)

print("Esperando conexión de Unity...")

connection, address = server_socket.accept()

print("Unity conectado:", address)

start_time = time.time()

with PoseLandmarker.create_from_options(options) as landmarker:

    try:
        while True:

            # Primero recibimos 4 bytes con el tamaño
            header = receive_exact(
                connection,
                4
            )

            if header is None:
                break

            frame_size = struct.unpack(
                "<I",
                header
            )[0]

            frame_data = receive_exact(
                connection,
                frame_size
            )

            if frame_data is None:
                break

            # JPEG -> imagen OpenCV
            numpy_data = np.frombuffer(
                frame_data,
                dtype=np.uint8
            )

            frame = cv2.imdecode(
                numpy_data,
                cv2.IMREAD_COLOR
            )

            if frame is None:
                continue

            # BGR -> RGB
            rgb_frame = cv2.cvtColor(
                frame,
                cv2.COLOR_BGR2RGB
            )

            mp_image = mp.Image(
                image_format=mp.ImageFormat.SRGB,
                data=rgb_frame
            )

            timestamp_ms = int(
                (time.time() - start_time) * 1000
            )

            result = landmarker.detect_for_video(
                mp_image,
                timestamp_ms
            )

            if not result.pose_landmarks:
                continue

            landmarks = result.pose_landmarks[0]

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

            udp_socket.sendto(
                message.encode("utf-8"),
                (UNITY_IP, UNITY_PORT)
            )

    except Exception as e:
        print("Error:", e)

    finally:
        connection.close()
        server_socket.close()
        udp_socket.close()