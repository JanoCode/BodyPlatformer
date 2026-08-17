import cv2
import mediapipe as mp
import time
import socket
import json
import struct
import numpy as np

POSE_MODEL_PATH = "pose_landmarker_lite.task"
HAND_MODEL_PATH = "hand_landmarker.task"

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

HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions

VisionRunningMode = mp.tasks.vision.RunningMode


pose_options = PoseLandmarkerOptions(
    base_options=BaseOptions(
        model_asset_path=POSE_MODEL_PATH
    ),
    running_mode=VisionRunningMode.VIDEO,
    num_poses=1,
    min_pose_detection_confidence=0.5,
    min_pose_presence_confidence=0.5,
    min_tracking_confidence=0.5
)


hand_options = HandLandmarkerOptions(
    base_options=BaseOptions(
        model_asset_path=HAND_MODEL_PATH
    ),
    running_mode=VisionRunningMode.VIDEO,
    num_hands=2,
    min_hand_detection_confidence=0.5,
    min_hand_presence_confidence=0.5,
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


with (
    PoseLandmarker.create_from_options(
        pose_options
    ) as pose_landmarker,

    HandLandmarker.create_from_options(
        hand_options
    ) as hand_landmarker
):

    try:
        while True:

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

            # -----------------------
            # POSE
            # -----------------------

            pose_result = pose_landmarker.detect_for_video(
                mp_image,
                timestamp_ms
            )

            # -----------------------
            # HANDS
            # -----------------------

            hand_result = hand_landmarker.detect_for_video(
                mp_image,
                timestamp_ms
            )

            data = {
                "landmarks": [],
                "hands": []
            }

            # -----------------------
            # BODY LANDMARKS
            # -----------------------

            if pose_result.pose_landmarks:

                pose_landmarks = (
                    pose_result.pose_landmarks[0]
                )

                for i, landmark in enumerate(
                    pose_landmarks
                ):

                    data["landmarks"].append({
                        "id": i,
                        "x": landmark.x,
                        "y": landmark.y,
                        "z": landmark.z,
                        "visibility": landmark.visibility
                    })

            # -----------------------
            # HAND LANDMARKS
            # -----------------------

            for hand_index, landmarks in enumerate(
                hand_result.hand_landmarks
            ):

                handedness = "Unknown"
                confidence = 0.0

                if (
                    hand_index
                    < len(hand_result.handedness)
                    and
                    len(
                        hand_result.handedness[
                            hand_index
                        ]
                    ) > 0
                ):

                    category = (
                        hand_result.handedness[
                            hand_index
                        ][0]
                    )

                    handedness = (
                        category.category_name
                    )

                    confidence = (
                        category.score
                    )

                hand_data = {
                    "handedness": handedness,
                    "confidence": confidence,
                    "landmarks": [],
                    "worldLandmarks": []
                }

                # Coordenadas normalizadas
                for i, landmark in enumerate(
                    landmarks
                ):

                    hand_data[
                        "landmarks"
                    ].append({
                        "id": i,
                        "x": landmark.x,
                        "y": landmark.y,
                        "z": landmark.z
                    })

                # Coordenadas 3D reales aproximadas
                if (
                    hand_index
                    < len(
                        hand_result.hand_world_landmarks
                    )
                ):

                    world_landmarks = (
                        hand_result.hand_world_landmarks[
                            hand_index
                        ]
                    )

                    for i, landmark in enumerate(
                        world_landmarks
                    ):

                        hand_data[
                            "worldLandmarks"
                        ].append({
                            "id": i,
                            "x": landmark.x,
                            "y": landmark.y,
                            "z": landmark.z
                        })

                data["hands"].append(
                    hand_data
                )

            message = json.dumps(data)

            udp_socket.sendto(
                message.encode("utf-8"),
                (
                    UNITY_IP,
                    UNITY_PORT
                )
            )

    except Exception as e:
        print(
            "Error:",
            e
        )

    finally:
        connection.close()
        server_socket.close()
        udp_socket.close()