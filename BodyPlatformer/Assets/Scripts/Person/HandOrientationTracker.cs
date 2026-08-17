using UnityEngine;

public class HandOrientationTracker : MonoBehaviour
{
    public enum HandOrientation
    {
        NotDetected,
        Flat,
        Sideways
    }

    [Header("Referencia")]
    [SerializeField] private PoseReceiver poseReceiver;

    [Header("Configuración")]
    [SerializeField, Range(0f, 1f)]
    private float flatThreshold = 0.60f;

    [SerializeField]
    private bool debugLogs = true;

    private HandOrientation leftOrientation =
        HandOrientation.NotDetected;

    private HandOrientation rightOrientation =
        HandOrientation.NotDetected;

    private HandOrientation previousLeftOrientation =
        HandOrientation.NotDetected;

    private HandOrientation previousRightOrientation =
        HandOrientation.NotDetected;


    private void Update()
    {
        leftOrientation =
            CalculateOrientation(
                poseReceiver != null
                    ? poseReceiver.GetLeftHandData()
                    : null,
                poseReceiver != null &&
                poseReceiver.IsLeftHandDetected()
            );

        rightOrientation =
            CalculateOrientation(
                poseReceiver != null
                    ? poseReceiver.GetRightHandData()
                    : null,
                poseReceiver != null &&
                poseReceiver.IsRightHandDetected()
            );

        if (debugLogs)
        {
            PrintChanges();
        }
    }


    private HandOrientation CalculateOrientation(
        PoseReceiver.HandData hand,
        bool detected
    )
    {
        if (
            !detected ||
            hand == null ||
            hand.worldLandmarks == null ||
            hand.worldLandmarks.Length < 18
        )
        {
            return HandOrientation.NotDetected;
        }

        // MediaPipe Hand Landmarks:
        // 0  = Wrist
        // 5  = Index MCP
        // 17 = Pinky MCP

        Vector3 wrist =
            ToVector3(
                hand.worldLandmarks[0]
            );

        Vector3 index =
            ToVector3(
                hand.worldLandmarks[5]
            );

        Vector3 pinky =
            ToVector3(
                hand.worldLandmarks[17]
            );

        Vector3 wristToIndex =
            index - wrist;

        Vector3 wristToPinky =
            pinky - wrist;

        Vector3 palmNormal =
            Vector3.Cross(
                wristToIndex,
                wristToPinky
            ).normalized;

        /*
         * Si la palma está mirando principalmente
         * hacia arriba o hacia abajo, la normal
         * tendrá bastante componente vertical.
         *
         * Abs() hace que nos dé igual:
         *
         * palma arriba
         * o
         * palma abajo
         */

        float verticalAmount =
            Mathf.Abs(
                palmNormal.y
            );

        if (
            verticalAmount >=
            flatThreshold
        )
        {
            return HandOrientation.Flat;
        }

        return HandOrientation.Sideways;
    }


    private Vector3 ToVector3(
        PoseReceiver.HandLandmark landmark
    )
    {
        return new Vector3(
            landmark.x,
            landmark.y,
            landmark.z
        );
    }


    private void PrintChanges()
    {
        if (
            leftOrientation !=
            previousLeftOrientation
        )
        {
            Debug.Log(
                "LEFT HAND: " +
                leftOrientation
            );

            previousLeftOrientation =
                leftOrientation;
        }

        if (
            rightOrientation !=
            previousRightOrientation
        )
        {
            Debug.Log(
                "RIGHT HAND: " +
                rightOrientation
            );

            previousRightOrientation =
                rightOrientation;
        }
    }


    // =========================================================
    // API PÚBLICA
    // =========================================================

    public HandOrientation GetLeftOrientation()
    {
        return leftOrientation;
    }

    public HandOrientation GetRightOrientation()
    {
        return rightOrientation;
    }

    public bool IsLeftHandPlatform()
    {
        return
            leftOrientation ==
            HandOrientation.Flat;
    }

    public bool IsRightHandPlatform()
    {
        return
            rightOrientation ==
            HandOrientation.Flat;
    }
}