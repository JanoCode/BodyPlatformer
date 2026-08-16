using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    [SerializeField] private RawImage display;

    private WebCamTexture WebCamTexture;

    void Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No se encontró ninguna cámara.");
            return;
        }

        string cameraName = WebCamTexture.devices[0].name;

        WebCamTexture = new WebCamTexture(
            cameraName,
            1280,
            720,
            30
        );

        display.texture = WebCamTexture;
        WebCamTexture.Play();

        Debug.Log("Cámara iniciada: " + cameraName);

    }

    void ODestroy()
    {
        if (WebCamTexture != null)
        {
                WebCamTexture.Stop();
        }
    }
}
