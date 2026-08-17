using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;

public class CameraFrameSender : MonoBehaviour
{
    [Header("Webcam")]
    [SerializeField] private WebcamDisplay webcamDisplay;

    [Header("Tracking")]
    [SerializeField] private int trackingWidth = 640;
    [SerializeField] private int trackingHeight = 360;

    [SerializeField, Range(1, 30)]
    private int trackingFPS = 15;

    [SerializeField, Range(1, 100)]
    private int jpegQuality = 55;

    private TcpClient client;
    private NetworkStream stream;

    private Texture2D frameTexture;

    private Coroutine sendingCoroutine;

    private void Start()
    {
        frameTexture = new Texture2D(
            trackingWidth,
            trackingHeight,
            TextureFormat.RGB24,
            false
        );

        StartCoroutine(
            ConnectToTracker()
        );
    }

    private IEnumerator ConnectToTracker()
    {
        // Damos tiempo al pose_tracking.exe
        // para abrir el servidor TCP.
        yield return new WaitForSeconds(1f);

        while (client == null || !client.Connected)
        {
            try
            {
                client = new TcpClient();

                client.Connect(
                    "127.0.0.1",
                    5053
                );

                stream =
                    client.GetStream();

                Debug.Log(
                    "Conectado al Body Tracker."
                );

                sendingCoroutine =
                    StartCoroutine(
                        SendFrames()
                    );

                yield break;
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                Debug.Log(
                    "Esperando Body Tracker..."
                );
            }

            yield return new WaitForSeconds(
                0.5f
            );
        }
    }

    private IEnumerator SendFrames()
    {
        float delay =
            1f / trackingFPS;

        WaitForSeconds wait =
            new WaitForSeconds(delay);

        while (true)
        {
            SendCurrentFrame();

            yield return wait;
        }
    }

    private void SendCurrentFrame()
    {
        if (
            stream == null ||
            client == null ||
            !client.Connected
        )
        {
            return;
        }

        WebCamTexture webcam =
            webcamDisplay.GetWebCamTexture();

        if (
            webcam == null ||
            !webcam.isPlaying ||
            webcam.width <= 16
        )
        {
            return;
        }

        RenderTexture renderTexture =
            RenderTexture.GetTemporary(
                trackingWidth,
                trackingHeight,
                0,
                RenderTextureFormat.ARGB32
            );

        Graphics.Blit(
            webcam,
            renderTexture
        );

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active =
            renderTexture;

        frameTexture.ReadPixels(
            new Rect(
                0,
                0,
                trackingWidth,
                trackingHeight
            ),
            0,
            0
        );

        frameTexture.Apply();

        RenderTexture.active =
            previous;

        RenderTexture.ReleaseTemporary(
            renderTexture
        );

        byte[] jpg =
            frameTexture.EncodeToJPG(
                jpegQuality
            );

        byte[] size =
            System.BitConverter.GetBytes(
                jpg.Length
            );

        try
        {
            stream.Write(
                size,
                0,
                size.Length
            );

            stream.Write(
                jpg,
                0,
                jpg.Length
            );
        }
        catch
        {
            Debug.LogWarning(
                "Se perdió la conexión con Body Tracker."
            );
        }
    }

    private void OnDestroy()
    {
        if (sendingCoroutine != null)
        {
            StopCoroutine(
                sendingCoroutine
            );
        }

        stream?.Close();
        client?.Close();

        if (frameTexture != null)
        {
            Destroy(frameTexture);
        }
    }
}