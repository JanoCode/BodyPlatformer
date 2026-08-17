using UnityEngine;

public class PlayerDetectionGate : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PoseReceiver poseReceiver;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Transform respawnPoint;

    [Header("Opciones")]
    [SerializeField] private bool hidePlayerWhenNoPerson = true;
    [SerializeField] private bool respawnWhenPersonReturns = true;

    private bool playerActive = false;
    private bool hasBeenDetectedBefore = false;

    private void Start()
    {
        SetPlayerActive(false);
    }

    private void Update()
    {
        if (poseReceiver == null)
            return;

        bool personDetected =
            poseReceiver.IsPersonDetected();

        if (personDetected == playerActive)
            return;

        if (personDetected)
        {
            ActivatePlayer();
        }
        else
        {
            DeactivatePlayer();
        }
    }

    private void ActivatePlayer()
    {
        if (
            respawnWhenPersonReturns &&
            respawnPoint != null
        )
        {
            transform.position =
                respawnPoint.position;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.simulated = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (
            playerRenderer != null &&
            hidePlayerWhenNoPerson
        )
        {
            playerRenderer.enabled = true;
        }

        playerActive = true;
        hasBeenDetectedBefore = true;
    }

    private void DeactivatePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.simulated = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (
            playerRenderer != null &&
            hidePlayerWhenNoPerson
        )
        {
            playerRenderer.enabled = false;
        }

        playerActive = false;
    }

    private void SetPlayerActive(bool active)
    {
        if (active)
        {
            ActivatePlayer();
        }
        else
        {
            DeactivatePlayer();
        }
    }
}