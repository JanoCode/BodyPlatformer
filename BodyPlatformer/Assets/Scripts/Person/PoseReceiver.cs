using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PoseReceiver : MonoBehaviour
{
    // =========================================================
    // DATOS RECIBIDOS DESDE PYTHON
    // =========================================================

    [Serializable]
    public class Landmark
    {
        public int id;
        public float x;
        public float y;
        public float z;
        public float visibility;
    }

    [Serializable]
    public class HandLandmark
    {
        public int id;
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class HandData
    {
        public string handedness;
        public float confidence;

        public HandLandmark[] landmarks;
        public HandLandmark[] worldLandmarks;
    }

    [Serializable]
    public class PoseData
    {
        public Landmark[] landmarks;
        public HandData[] hands;
    }


    // =========================================================
    // VISUALIZACIÓN
    // =========================================================

    [Header("Visualización cuerpo")]
    [SerializeField] private GameObject landmarkPrefab;

    [Header("Visualización manos")]
    [SerializeField] private GameObject handLandmarkPrefab;

    [SerializeField] private float fallbackWorldWidth = 16f;
    [SerializeField] private float fallbackWorldHeight = 9f;


    // =========================================================
    // TRACKING DEL CUERPO
    // =========================================================

    [Header("Tracking cuerpo")]
    [SerializeField, Range(0.01f, 1f)]
    private float smoothing = 0.25f;

    [SerializeField, Range(0f, 1f)]
    private float visibilityThreshold = 0.65f;


    // =========================================================
    // TRACKING DE MANOS
    // =========================================================

    [Header("Tracking manos")]
    [SerializeField, Range(0.01f, 1f)]
    private float handSmoothing = 0.35f;

    [SerializeField]
    private float handLostTime = 0.25f;

    [SerializeField]
    private float maxHandWorldJumpPerFrame = 1.5f;


    // =========================================================
    // FILTROS
    // =========================================================

    [Header("Filtros")]
    [SerializeField]
    private float maxNormalizedMargin = 0.05f;

    [SerializeField]
    private float maxWorldJumpPerFrame = 1.2f;


    // =========================================================
    // PÉRDIDA TEMPORAL
    // =========================================================

    [Header("Pérdida temporal")]
    [SerializeField]
    private float lostLandmarkGraceTime = 0.2f;


    // =========================================================
    // DETECCIÓN DE PERSONA
    // =========================================================

    [Header("Detección de persona")]
    [SerializeField]
    private float personLostTime = 0.5f;


    // =========================================================
    // LANDMARKS DEL CUERPO
    // =========================================================

    private GameObject[] landmarkObjects =
        new GameObject[33];

    private bool[] landmarkVisible =
        new bool[33];

    private bool[] landmarkInitialized =
        new bool[33];

    private float[] lastValidTime =
        new float[33];


    // =========================================================
    // LANDMARKS DE LAS MANOS
    // =========================================================

    private GameObject[] leftHandObjects =
        new GameObject[21];

    private GameObject[] rightHandObjects =
        new GameObject[21];

    private bool[] leftHandInitialized =
        new bool[21];

    private bool[] rightHandInitialized =
        new bool[21];

    private bool[] leftHandVisible =
        new bool[21];

    private bool[] rightHandVisible =
        new bool[21];

    private float[] leftHandLastValidTime =
        new float[21];

    private float[] rightHandLastValidTime =
        new float[21];

    private bool leftHandDetected = false;
    private bool rightHandDetected = false;

    private float lastLeftHandDetectedTime = -999f;
    private float lastRightHandDetectedTime = -999f;

    private HandData latestLeftHand;
    private HandData latestRightHand;


    // =========================================================
    // ESTADO GENERAL
    // =========================================================

    private float lastPersonDetectedTime = -999f;
    private bool personDetected = false;

    private bool debugLandmarksVisible = true;

    private float runtimeWorldWidth;
    private float runtimeWorldHeight;

    private Camera mainCamera;


    // =========================================================
    // UDP
    // =========================================================

    private UdpClient udpClient;
    private Thread receiveThread;

    private string latestMessage;

    private readonly object messageLock =
        new object();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        mainCamera = Camera.main;

        CalculateWorldSize();

        CreateLandmarkObjects();
        CreateHandLandmarkObjects();

        udpClient =
            new UdpClient(5052);

        receiveThread =
            new Thread(ReceiveData);

        receiveThread.IsBackground = true;

        receiveThread.Start();

        Debug.Log(
            "Esperando datos de MediaPipe..."
        );

        Debug.Log(
            "Tracking World Size: " +
            runtimeWorldWidth +
            " x " +
            runtimeWorldHeight
        );
    }


    // =========================================================
    // TAMAÑO DEL MUNDO
    // =========================================================

    private void CalculateWorldSize()
    {
        if (
            mainCamera != null &&
            mainCamera.orthographic
        )
        {
            runtimeWorldHeight =
                mainCamera.orthographicSize * 2f;

            runtimeWorldWidth =
                runtimeWorldHeight *
                mainCamera.aspect;
        }
        else
        {
            runtimeWorldWidth =
                fallbackWorldWidth;

            runtimeWorldHeight =
                fallbackWorldHeight;
        }
    }


    // =========================================================
    // CREACIÓN LANDMARKS CUERPO
    // =========================================================

    private void CreateLandmarkObjects()
    {
        for (int i = 0; i < 33; i++)
        {
            GameObject point =
                Instantiate(
                    landmarkPrefab,
                    Vector3.zero,
                    Quaternion.identity
                );

            point.name =
                "Landmark_" + i;

            landmarkObjects[i] = point;

            landmarkVisible[i] = false;
            landmarkInitialized[i] = false;

            lastValidTime[i] = -999f;

            point.SetActive(false);
        }
    }


    // =========================================================
    // CREACIÓN LANDMARKS MANOS
    // =========================================================

    private void CreateHandLandmarkObjects()
    {
        GameObject prefab =
            handLandmarkPrefab != null
            ? handLandmarkPrefab
            : landmarkPrefab;

        if (prefab == null)
        {
            Debug.LogWarning(
                "No hay prefab para visualizar landmarks de manos."
            );

            return;
        }

        for (int i = 0; i < 21; i++)
        {
            GameObject leftPoint =
                Instantiate(
                    prefab,
                    Vector3.zero,
                    Quaternion.identity
                );

            leftPoint.name =
                "LeftHand_" + i;

            leftPoint.SetActive(false);

            leftHandObjects[i] =
                leftPoint;

            leftHandVisible[i] = false;
            leftHandInitialized[i] = false;
            leftHandLastValidTime[i] = -999f;


            GameObject rightPoint =
                Instantiate(
                    prefab,
                    Vector3.zero,
                    Quaternion.identity
                );

            rightPoint.name =
                "RightHand_" + i;

            rightPoint.SetActive(false);

            rightHandObjects[i] =
                rightPoint;

            rightHandVisible[i] = false;
            rightHandInitialized[i] = false;
            rightHandLastValidTime[i] = -999f;
        }
    }


    // =========================================================
    // RECEPCIÓN UDP
    // =========================================================

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint =
            new IPEndPoint(
                IPAddress.Any,
                0
            );

        while (true)
        {
            try
            {
                byte[] data =
                    udpClient.Receive(
                        ref remoteEndPoint
                    );

                string message =
                    Encoding.UTF8.GetString(
                        data
                    );

                lock (messageLock)
                {
                    latestMessage =
                        message;
                }
            }
            catch
            {
                break;
            }
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CalculateWorldSize();

        string message = null;

        lock (messageLock)
        {
            if (latestMessage != null)
            {
                message =
                    latestMessage;

                latestMessage =
                    null;
            }
        }

        if (message == null)
        {
            UpdateLostLandmarks();
            UpdateLostHandLandmarks();
            UpdatePersonDetection();
            UpdateHandDetection();

            return;
        }

        PoseData pose =
            JsonUtility.FromJson<PoseData>(
                message
            );

        if (pose == null)
        {
            UpdateLostLandmarks();
            UpdateLostHandLandmarks();
            UpdatePersonDetection();
            UpdateHandDetection();

            return;
        }

        ProcessBody(pose);
        ProcessHands(pose);

        UpdateLostLandmarks();
        UpdateLostHandLandmarks();
        UpdatePersonDetection();
        UpdateHandDetection();
    }


    // =========================================================
    // CUERPO
    // =========================================================

    private void ProcessBody(
        PoseData pose
    )
    {
        if (
            pose.landmarks == null ||
            pose.landmarks.Length == 0
        )
        {
            return;
        }

        bool anyValidLandmark =
            false;

        foreach (
            Landmark landmark
            in pose.landmarks
        )
        {
            if (
                landmark.id < 0 ||
                landmark.id >=
                landmarkObjects.Length
            )
            {
                continue;
            }

            bool wasValid =
                ProcessLandmark(
                    landmark
                );

            if (wasValid)
            {
                anyValidLandmark =
                    true;
            }
        }

        if (anyValidLandmark)
        {
            lastPersonDetectedTime =
                Time.time;

            personDetected =
                true;
        }
    }


    // =========================================================
    // PROCESAR LANDMARK CUERPO
    // =========================================================

    private bool ProcessLandmark(
        Landmark landmark
    )
    {
        int id =
            landmark.id;

        GameObject landmarkObject =
            landmarkObjects[id];

        if (landmarkObject == null)
            return false;

        bool valid =
            landmark.visibility >=
                visibilityThreshold &&

            landmark.x >=
                -maxNormalizedMargin &&

            landmark.x <=
                1f +
                maxNormalizedMargin &&

            landmark.y >=
                -maxNormalizedMargin &&

            landmark.y <=
                1f +
                maxNormalizedMargin;

        if (!valid)
            return false;

        Vector3 targetPosition =
            NormalizedToWorld(
                landmark.x,
                landmark.y
            );

        if (!landmarkInitialized[id])
        {
            landmarkObject
                .transform
                .position =
                targetPosition;

            landmarkInitialized[id] =
                true;

            landmarkVisible[id] =
                true;

            lastValidTime[id] =
                Time.time;

            landmarkObject.SetActive(
                debugLandmarksVisible
            );

            return true;
        }

        float distance =
            Vector3.Distance(
                landmarkObject
                    .transform
                    .position,

                targetPosition
            );

        if (
            distance >
            maxWorldJumpPerFrame
        )
        {
            return false;
        }

        landmarkVisible[id] =
            true;

        lastValidTime[id] =
            Time.time;

        landmarkObject.SetActive(
            debugLandmarksVisible
        );

        landmarkObject
            .transform
            .position =
            Vector3.Lerp(
                landmarkObject
                    .transform
                    .position,

                targetPosition,

                smoothing
            );

        return true;
    }


    // =========================================================
    // MANOS
    // =========================================================

    private void ProcessHands(
        PoseData pose
    )
    {
        if (
            pose.hands == null ||
            pose.hands.Length == 0
        )
        {
            return;
        }

        foreach (
            HandData hand
            in pose.hands
        )
        {
            if (
                hand == null ||
                hand.landmarks == null ||
                hand.landmarks.Length == 0
            )
            {
                continue;
            }

            if (
                string.Equals(
                    hand.handedness,
                    "Left",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                latestLeftHand =
                    hand;

                leftHandDetected =
                    true;

                lastLeftHandDetectedTime =
                    Time.time;

                ProcessHandLandmarks(
                    hand,
                    leftHandObjects,
                    leftHandInitialized,
                    leftHandVisible,
                    leftHandLastValidTime
                );
            }
            else if (
                string.Equals(
                    hand.handedness,
                    "Right",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                latestRightHand =
                    hand;

                rightHandDetected =
                    true;

                lastRightHandDetectedTime =
                    Time.time;

                ProcessHandLandmarks(
                    hand,
                    rightHandObjects,
                    rightHandInitialized,
                    rightHandVisible,
                    rightHandLastValidTime
                );
            }
        }
    }


    // =========================================================
    // PROCESAR LOS 21 PUNTOS DE UNA MANO
    // =========================================================

    private void ProcessHandLandmarks(
        HandData hand,
        GameObject[] objects,
        bool[] initialized,
        bool[] visible,
        float[] lastValidTime
    )
    {
        foreach (
            HandLandmark landmark
            in hand.landmarks
        )
        {
            if (
                landmark.id < 0 ||
                landmark.id >= objects.Length
            )
            {
                continue;
            }

            int id =
                landmark.id;

            GameObject point =
                objects[id];

            if (point == null)
                continue;

            bool valid =
                landmark.x >=
                    -maxNormalizedMargin &&

                landmark.x <=
                    1f +
                    maxNormalizedMargin &&

                landmark.y >=
                    -maxNormalizedMargin &&

                landmark.y <=
                    1f +
                    maxNormalizedMargin;

            if (!valid)
                continue;

            Vector3 targetPosition =
                NormalizedToWorld(
                    landmark.x,
                    landmark.y
                );

            if (!initialized[id])
            {
                point.transform.position =
                    targetPosition;

                initialized[id] =
                    true;

                visible[id] =
                    true;

                lastValidTime[id] =
                    Time.time;

                point.SetActive(
                    debugLandmarksVisible
                );

                continue;
            }

            float distance =
                Vector3.Distance(
                    point.transform.position,
                    targetPosition
                );

            if (
                distance >
                maxHandWorldJumpPerFrame
            )
            {
                continue;
            }

            visible[id] =
                true;

            lastValidTime[id] =
                Time.time;

            point.transform.position =
                Vector3.Lerp(
                    point.transform.position,
                    targetPosition,
                    handSmoothing
                );

            point.SetActive(
                debugLandmarksVisible
            );
        }
    }


    // =========================================================
    // CONVERTIR MEDIAPIPE -> MUNDO UNITY
    // =========================================================

    private Vector3 NormalizedToWorld(
        float x,
        float y
    )
    {
        float worldX =
            (x - 0.5f) *
            runtimeWorldWidth;

        float worldY =
            (0.5f - y) *
            runtimeWorldHeight;

        return new Vector3(
            worldX,
            worldY,
            0f
        );
    }


    // =========================================================
    // PÉRDIDA DE LANDMARKS DEL CUERPO
    // =========================================================

    private void UpdateLostLandmarks()
    {
        for (
            int i = 0;
            i < landmarkObjects.Length;
            i++
        )
        {
            if (!landmarkVisible[i])
                continue;

            float timeSinceLastValid =
                Time.time -
                lastValidTime[i];

            if (
                timeSinceLastValid >
                lostLandmarkGraceTime
            )
            {
                HideLandmark(i);
            }
        }
    }


    // =========================================================
    // PÉRDIDA DE LANDMARKS DE LAS MANOS
    // =========================================================

    private void UpdateLostHandLandmarks()
    {
        UpdateLostHandLandmarksForHand(
            leftHandObjects,
            leftHandInitialized,
            leftHandVisible,
            leftHandLastValidTime
        );

        UpdateLostHandLandmarksForHand(
            rightHandObjects,
            rightHandInitialized,
            rightHandVisible,
            rightHandLastValidTime
        );
    }


    private void UpdateLostHandLandmarksForHand(
        GameObject[] objects,
        bool[] initialized,
        bool[] visible,
        float[] lastValidTime
    )
    {
        for (
            int i = 0;
            i < objects.Length;
            i++
        )
        {
            if (!visible[i])
                continue;

            float timeSinceLastValid =
                Time.time -
                lastValidTime[i];

            if (
                timeSinceLastValid >
                handLostTime
            )
            {
                visible[i] =
                    false;

                initialized[i] =
                    false;

                if (objects[i] != null)
                {
                    objects[i]
                        .SetActive(false);
                }
            }
        }
    }


    // =========================================================
    // PÉRDIDA DE PERSONA
    // =========================================================

    private void UpdatePersonDetection()
    {
        if (!personDetected)
            return;

        float timeSinceLastPerson =
            Time.time -
            lastPersonDetectedTime;

        if (
            timeSinceLastPerson >
            personLostTime
        )
        {
            personDetected =
                false;

            HideAllLandmarks();

            HideHand(
                leftHandObjects,
                leftHandInitialized,
                leftHandVisible
            );

            HideHand(
                rightHandObjects,
                rightHandInitialized,
                rightHandVisible
            );

            leftHandDetected =
                false;

            rightHandDetected =
                false;

            latestLeftHand =
                null;

            latestRightHand =
                null;

            Debug.Log(
                "Persona perdida"
            );
        }
    }


    // =========================================================
    // PÉRDIDA DE MANOS
    // =========================================================

    private void UpdateHandDetection()
    {
        if (
            leftHandDetected &&
            Time.time -
            lastLeftHandDetectedTime >
            handLostTime
        )
        {
            leftHandDetected =
                false;

            latestLeftHand =
                null;

            HideHand(
                leftHandObjects,
                leftHandInitialized,
                leftHandVisible
            );
        }

        if (
            rightHandDetected &&
            Time.time -
            lastRightHandDetectedTime >
            handLostTime
        )
        {
            rightHandDetected =
                false;

            latestRightHand =
                null;

            HideHand(
                rightHandObjects,
                rightHandInitialized,
                rightHandVisible
            );
        }
    }


    // =========================================================
    // OCULTAR CUERPO
    // =========================================================

    private void HideAllLandmarks()
    {
        for (
            int i = 0;
            i < landmarkObjects.Length;
            i++
        )
        {
            HideLandmark(i);
        }
    }


    private void HideLandmark(
        int id
    )
    {
        landmarkVisible[id] =
            false;

        landmarkInitialized[id] =
            false;

        if (
            landmarkObjects[id] != null
        )
        {
            landmarkObjects[id]
                .SetActive(false);
        }
    }


    // =========================================================
    // OCULTAR MANO
    // =========================================================

    private void HideHand(
        GameObject[] objects,
        bool[] initialized,
        bool[] visible
    )
    {
        for (
            int i = 0;
            i < objects.Length;
            i++
        )
        {
            initialized[i] =
                false;

            visible[i] =
                false;

            if (objects[i] != null)
            {
                objects[i]
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // API PÚBLICA - CUERPO
    // =========================================================

    public GameObject[] GetLandmarkObjects()
    {
        return landmarkObjects;
    }


    public bool IsLandmarkVisible(
        int index
    )
    {
        if (
            index < 0 ||
            index >=
            landmarkVisible.Length
        )
        {
            return false;
        }

        return landmarkVisible[index];
    }


    public bool IsPersonDetected()
    {
        return personDetected;
    }


    // =========================================================
    // API PÚBLICA - MANOS
    // =========================================================

    public GameObject[] GetLeftHandLandmarks()
    {
        return leftHandObjects;
    }


    public GameObject[] GetRightHandLandmarks()
    {
        return rightHandObjects;
    }


    public HandData GetLeftHandData()
    {
        return latestLeftHand;
    }


    public HandData GetRightHandData()
    {
        return latestRightHand;
    }


    public bool IsLeftHandDetected()
    {
        return leftHandDetected;
    }


    public bool IsRightHandDetected()
    {
        return rightHandDetected;
    }


    // =========================================================
    // DEBUG
    // =========================================================

    public void SetLandmarksVisible(
        bool visible
    )
    {
        debugLandmarksVisible =
            visible;

        for (
            int i = 0;
            i < landmarkObjects.Length;
            i++
        )
        {
            if (
                landmarkObjects[i] == null
            )
            {
                continue;
            }

            landmarkObjects[i]
                .SetActive(
                    debugLandmarksVisible &&
                    landmarkVisible[i]
                );
        }

        SetHandDebugVisible(
            leftHandObjects,
            leftHandVisible,
            leftHandDetected
        );

        SetHandDebugVisible(
            rightHandObjects,
            rightHandVisible,
            rightHandDetected
        );
    }


    private void SetHandDebugVisible(
        GameObject[] objects,
        bool[] visible,
        bool detected
    )
    {
        for (
            int i = 0;
            i < objects.Length;
            i++
        )
        {
            if (objects[i] == null)
                continue;

            objects[i].SetActive(
                debugLandmarksVisible &&
                detected &&
                visible[i]
            );
        }
    }


    // =========================================================
    // CIERRE
    // =========================================================

    private void OnDestroy()
    {
        udpClient?.Close();

        if (
            receiveThread != null &&
            receiveThread.IsAlive
        )
        {
            receiveThread.Interrupt();
        }
    }
}