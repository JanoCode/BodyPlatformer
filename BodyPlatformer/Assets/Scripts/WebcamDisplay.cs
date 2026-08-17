using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    [SerializeField] private RawImage display;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;

    private WebCamTexture webcamTexture;

    private void Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No se encontró ninguna cámara.");
            return;
        }

        string cameraName = WebCamTexture.devices[0].name;

        webcamTexture = new WebCamTexture(
            cameraName,
            1280,
            720,
            30
        );

        display.texture = webcamTexture;
        webcamTexture.Play();

        Debug.Log("Cámara iniciada: " + cameraName);
    }

    private void Update()
    {
        if (webcamTexture == null)
            return;

        if (webcamTexture.width > 16 && webcamTexture.height > 16)
        {
            float aspect =
                (float)webcamTexture.width /
                webcamTexture.height;

            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectRatio = aspect;
            }
        }
    }

    public WebCamTexture GetWebCamTexture()
    {
        return webcamTexture;
    }

    private void OnDestroy()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}