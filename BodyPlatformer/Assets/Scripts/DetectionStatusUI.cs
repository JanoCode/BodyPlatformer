using UnityEngine;
using TMPro;
using System.Collections;

public class DetectionStatusUI : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Tiempos")]
    [SerializeField] private float detectedMessageDuration = 3f;

    private bool lastDetectedState;
    private Coroutine hideCoroutine;

    private void Start()
    {
        if (statusText != null)
        {
            statusText.text = "Ponte frente a la cámara";
            statusText.gameObject.SetActive(true);
        }

        lastDetectedState = false;
    }

    private void Update()
    {
        if (poseReceiver == null || statusText == null)
            return;

        bool detected = poseReceiver.IsPersonDetected();

        if (detected == lastDetectedState)
            return;

        lastDetectedState = detected;

        if (detected)
        {
            ShowPersonDetected();
        }
        else
        {
            ShowSearchingMessage();
        }
    }

    private void ShowPersonDetected()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        statusText.gameObject.SetActive(true);
        statusText.text = "Persona detectada";

        hideCoroutine =
            StartCoroutine(HideDetectedMessage());
    }

    private IEnumerator HideDetectedMessage()
    {
        yield return new WaitForSeconds(
            detectedMessageDuration
        );

        statusText.gameObject.SetActive(false);

        hideCoroutine = null;
    }

    private void ShowSearchingMessage()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        statusText.gameObject.SetActive(true);
        statusText.text = "Ponte frente a la cámara";
    }
}