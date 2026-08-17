using UnityEngine;

public class HandPlatformController : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField]
    private HandOrientationTracker orientationTracker;

    [Header("Body Colliders")]
    [SerializeField]
    private Transform bodyCollidersRoot;

    [Header("Debug")]
    [SerializeField]
    private bool debugLogs = false;

    private Collider2D leftHandCollider;
    private Collider2D rightHandCollider;

    private bool previousLeftState;
    private bool previousRightState;


    private void Update()
    {
        // Los colliders de las manos se crean en runtime,
        // así que los buscamos hasta encontrarlos.
        FindHandColliders();

        if (orientationTracker == null)
            return;

        bool leftUsable =
            orientationTracker.IsLeftHandPlatform();

        bool rightUsable =
            orientationTracker.IsRightHandPlatform();

        UpdateCollider(
            leftHandCollider,
            leftUsable,
            "LEFT",
            ref previousLeftState
        );

        UpdateCollider(
            rightHandCollider,
            rightUsable,
            "RIGHT",
            ref previousRightState
        );
    }


    private void FindHandColliders()
    {
        if (bodyCollidersRoot == null)
            return;

        if (
            leftHandCollider != null &&
            rightHandCollider != null
        )
        {
            return;
        }

        Transform[] children =
            bodyCollidersRoot
                .GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (
                leftHandCollider == null &&
                child.name == "LeftHandCollider"
            )
            {
                leftHandCollider =
                    child.GetComponent<Collider2D>();

                if (
                    debugLogs &&
                    leftHandCollider != null
                )
                {
                    Debug.Log(
                        "LeftHandCollider encontrado."
                    );
                }
            }

            if (
                rightHandCollider == null &&
                child.name == "RightHandCollider"
            )
            {
                rightHandCollider =
                    child.GetComponent<Collider2D>();

                if (
                    debugLogs &&
                    rightHandCollider != null
                )
                {
                    Debug.Log(
                        "RightHandCollider encontrado."
                    );
                }
            }
        }
    }


    private void UpdateCollider(
        Collider2D handCollider,
        bool usable,
        string handName,
        ref bool previousState
    )
    {
        if (handCollider == null)
            return;

        handCollider.enabled =
            usable;

        if (
            debugLogs &&
            usable != previousState
        )
        {
            Debug.Log(
                handName +
                " HAND PLATFORM: " +
                (usable ? "ON" : "OFF")
            );
        }

        previousState =
            usable;
    }
}