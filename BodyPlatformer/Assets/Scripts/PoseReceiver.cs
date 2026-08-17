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

    private GameObject[] landmarkObjects = new GameObject[33];
    private bool[] landmarkVisible = new bool[33];
    private bool[] landmarkInitialized = new bool[33];

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
            return;

        PoseData pose =
            JsonUtility.FromJson<PoseData>(message);

        if (pose == null || pose.landmarks == null)
            return;

        foreach (Landmark landmark in pose.landmarks)
        {
            if (landmark.id < 0 ||
                landmark.id >= landmarkObjects.Length)
                continue;

            ProcessLandmark(landmark);
        }
    }

    private void ProcessLandmark(Landmark landmark)
    {
        int id = landmark.id;

        GameObject landmarkObject =
            landmarkObjects[id];

        if (landmarkObject == null)
            return;

        // 1. Confianza de MediaPipe
        if (landmark.visibility < visibilityThreshold)
        {
            HideLandmark(id);
            return;
        }

        // 2. Rechazar puntos claramente fuera de la imagen
        if (
            landmark.x < -maxNormalizedMargin ||
            landmark.x > 1f + maxNormalizedMargin ||
            landmark.y < -maxNormalizedMargin ||
            landmark.y > 1f + maxNormalizedMargin
        )
        {
            HideLandmark(id);
            return;
        }

        float x =
            (landmark.x - 0.5f) * worldWidth;

        float y =
            (0.5f - landmark.y) * worldHeight;

        Vector3 targetPosition =
            new Vector3(x, y, 0);

        // Primer frame válido del punto
        if (!landmarkInitialized[id])
        {
            landmarkObject.transform.position =
                targetPosition;

            landmarkInitialized[id] = true;
            landmarkVisible[id] = true;

            landmarkObject.SetActive(true);

            return;
        }

        float distance =
            Vector3.Distance(
                landmarkObject.transform.position,
                targetPosition
            );

        // 3. Rechazar teletransportes absurdos
        if (distance > maxWorldJumpPerFrame)
        {
            HideLandmark(id);
            return;
        }

        landmarkVisible[id] = true;
        landmarkObject.SetActive(true);

        landmarkObject.transform.position =
            Vector3.Lerp(
                landmarkObject.transform.position,
                targetPosition,
                smoothing
            );
    }

    private void HideLandmark(int id)
    {
        landmarkVisible[id] = false;

        if (landmarkObjects[id] != null)
        {
            landmarkObjects[id].SetActive(false);
        }

        // Cuando vuelva a aparecer,
        // aceptamos su nueva posición como inicial.
        landmarkInitialized[id] = false;
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