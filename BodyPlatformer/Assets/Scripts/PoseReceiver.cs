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

    private GameObject[] landmarkObjects = new GameObject[33];

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

        if (pose?.landmarks == null)
            return;

        foreach (Landmark landmark in pose.landmarks)
        {
            if (landmark.id < 0 ||
                landmark.id >= landmarkObjects.Length)
                continue;

            float x =
                (landmark.x - 0.5f) * worldWidth;

            float y =
                (0.5f - landmark.y) * worldHeight;

            landmarkObjects[landmark.id]
                .transform.position =
                new Vector3(x, y, 0);
        }
    }

    public GameObject[] GetLandmarkObjects()
    {
        return landmarkObjects;
    }

    private void OnDestroy()
    {
        receiveThread?.Interrupt();
        udpClient?.Close();
    }
}