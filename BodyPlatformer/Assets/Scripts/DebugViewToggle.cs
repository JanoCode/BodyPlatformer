using UnityEngine;
using UnityEngine.InputSystem;

public class DebugViewToggle : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject poseSkeletonObject;
    [SerializeField] private PoseReceiver poseReceiver;

    [Header("Estado")]
    [SerializeField] private bool debugVisible = true;

    private void Start()
    {
        ApplyDebugState();
    }

    private void Update()
    {
        // Teclado: F1
        if (Keyboard.current != null &&
            Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ToggleDebug();
        }

        // Mando: Select / Back
        if (Gamepad.current != null &&
            Gamepad.current.selectButton.wasPressedThisFrame)
        {
            ToggleDebug();
        }
    }

    private void ToggleDebug()
    {
        debugVisible = !debugVisible;

        ApplyDebugState();
    }

    private void ApplyDebugState()
    {
        if (poseSkeletonObject != null)
        {
            poseSkeletonObject.SetActive(debugVisible);
        }

        if (poseReceiver != null)
        {
            poseReceiver.SetLandmarksVisible(debugVisible);
        }
    }
}