using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PoseReceiver : MonoBehaviour
{
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
    public class PoseData
    {
        public Landmark[] landmarks;
    }

    [Header("Visualización")]
    [SerializeField] private GameObject landmarkPrefab;
    [SerializeField] private float worldWidth = 16f;
    [SerializeField] private float worldHeight = 9f;

    [Header("Tracking")]
    [SerializeField, Range(0.01f, 1f)]
    private float smoothing = 0.25f;

    [SerializeField, Range(0f, 1f)]
    private float visibilityThreshold = 0.65f;

    [Header("Filtros")]
    [SerializeField] private float maxNormalizedMargin = 0.05f;
    [SerializeField] private float maxWorldJumpPerFrame = 1.2f;

    [Header("Pérdida temporal")]
    [SerializeField] private float lostLandmarkGraceTime = 0.2f;

    [Header("Detección de persona")]
    [SerializeField] private float personLostTime = 0.5f;

    private GameObject[] landmarkObjects = new GameObject[33];
    private bool[] landmarkVisible = new bool[33];
    private bool[] landmarkInitialized = new bool[33];

    private float[] lastValidTime = new float[33];

    private float lastPersonDetectedTime = -999f;
    private bool personDetected = false;

    private UdpClient udpClient;
    private Thread receiveThread;

    private string latestMessage;
    private readonly object messageLock = new object();

    private void Start()
    {
        CreateLandmarkObjects();

        udpClient = new UdpClient(5052);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("Esperando datos de MediaPipe...");
    }

    private void CreateLandmarkObjects()
    {
        for (int i = 0; i < 33; i++)
        {
            GameObject point = Instantiate(
                landmarkPrefab,
                Vector3.zero,
                Quaternion.identity
            );

            point.name = "Landmark_" + i;

            landmarkObjects[i] = point;
            landmarkVisible[i] = false;
            landmarkInitialized[i] = false;
            lastValidTime[i] = -999f;

            point.SetActive(false);
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint =
            new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                byte[] data =
                    udpClient.Receive(ref remoteEndPoint);

                string message =
                    Encoding.UTF8.GetString(data);

                lock (messageLock)
                {
                    latestMessage = message;
                }
            }
            catch
            {
                break;
            }
        }
    }

    private void Update()
    {
        string message = null;

        lock (messageLock)
        {
            if (latestMessage != null)
            {
                message = latestMessage;
                latestMessage = null;
            }
        }

        if (message == null)
        {
            UpdateLostLandmarks();
            UpdatePersonDetection();
            return;
        }

        PoseData pose =
            JsonUtility.FromJson<PoseData>(message);

        if (pose == null || pose.landmarks == null)
        {
            UpdateLostLandmarks();
            UpdatePersonDetection();
            return;
        }

        bool anyValidLandmark = false;

        foreach (Landmark landmark in pose.landmarks)
        {
            if (landmark.id < 0 ||
                landmark.id >= landmarkObjects.Length)
                continue;

            bool wasValid = ProcessLandmark(landmark);

            if (wasValid)
            {
                anyValidLandmark = true;
            }
        }

        if (anyValidLandmark)
        {
            lastPersonDetectedTime = Time.time;
            personDetected = true;
        }

        UpdateLostLandmarks();
        UpdatePersonDetection();
    }

    private bool ProcessLandmark(Landmark landmark)
    {
        int id = landmark.id;

        GameObject landmarkObject =
            landmarkObjects[id];

        if (landmarkObject == null)
            return false;

        bool valid =
            landmark.visibility >= visibilityThreshold &&
            landmark.x >= -maxNormalizedMargin &&
            landmark.x <= 1f + maxNormalizedMargin &&
            landmark.y >= -maxNormalizedMargin &&
            landmark.y <= 1f + maxNormalizedMargin;

        if (!valid)
        {
            return false;
        }

        float x =
            (landmark.x - 0.5f) * worldWidth;

        float y =
            (0.5f - landmark.y) * worldHeight;

        Vector3 targetPosition =
            new Vector3(x, y, 0f);

        if (!landmarkInitialized[id])
        {
            landmarkObject.transform.position =
                targetPosition;

            landmarkInitialized[id] = true;
            landmarkVisible[id] = true;
            lastValidTime[id] = Time.time;

            landmarkObject.SetActive(true);

            return true;
        }

        float distance =
            Vector3.Distance(
                landmarkObject.transform.position,
                targetPosition
            );

        if (distance > maxWorldJumpPerFrame)
        {
            return false;
        }

        landmarkVisible[id] = true;
        lastValidTime[id] = Time.time;

        landmarkObject.SetActive(true);

        landmarkObject.transform.position =
            Vector3.Lerp(
                landmarkObject.transform.position,
                targetPosition,
                smoothing
            );

        return true;
    }

    private void UpdateLostLandmarks()
    {
        for (int i = 0; i < landmarkObjects.Length; i++)
        {
            if (!landmarkVisible[i])
                continue;

            float timeSinceLastValid =
                Time.time - lastValidTime[i];

            if (timeSinceLastValid >
                lostLandmarkGraceTime)
            {
                HideLandmark(i);
            }
        }
    }

    private void UpdatePersonDetection()
    {
        if (!personDetected)
            return;

        float timeSinceLastPerson =
            Time.time - lastPersonDetectedTime;

        if (timeSinceLastPerson > personLostTime)
        {
            personDetected = false;

            HideAllLandmarks();

            Debug.Log("Persona perdida");
        }
    }

    private void HideAllLandmarks()
    {
        for (int i = 0; i < landmarkObjects.Length; i++)
        {
            HideLandmark(i);
        }
    }

    private void HideLandmark(int id)
    {
        landmarkVisible[id] = false;
        landmarkInitialized[id] = false;

        if (landmarkObjects[id] != null)
        {
            landmarkObjects[id].SetActive(false);
        }
    }

    public GameObject[] GetLandmarkObjects()
    {
        return landmarkObjects;
    }

    public bool IsLandmarkVisible(int index)
    {
        if (index < 0 ||
            index >= landmarkVisible.Length)
            return false;

        return landmarkVisible[index];
    }

    public bool IsPersonDetected()
    {
        return personDetected;
    }

    private void OnDestroy()
    {
        udpClient?.Close();

        if (receiveThread != null &&
            receiveThread.IsAlive)
        {
            receiveThread.Interrupt();
        }
    }
}