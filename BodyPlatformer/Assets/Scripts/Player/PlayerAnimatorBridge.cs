using UnityEngine;

public class PlayerAnimatorBridge : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Ajustes")]
    [SerializeField] private float moveThreshold = 0.1f;
    [SerializeField] private float groundedVelocityThreshold = 0.15f;

    private void Update()
    {
        if (animator == null || playerRigidbody == null)
            return;

        float horizontalSpeed =
            Mathf.Abs(playerRigidbody.linearVelocity.x);

        float verticalVelocity =
            playerRigidbody.linearVelocity.y;

        bool grounded =
            Mathf.Abs(verticalVelocity) <
            groundedVelocityThreshold;

        animator.SetFloat(
            "Speed",
            horizontalSpeed
        );

        animator.SetBool(
            "Grounded",
            grounded
        );

        animator.SetFloat(
            "VerticalVelocity",
            verticalVelocity
        );

        UpdateFacing(
            playerRigidbody.linearVelocity.x
        );
    }

    private void UpdateFacing(float velocityX)
    {
        if (spriteRenderer == null)
            return;

        if (velocityX > moveThreshold)
        {
            spriteRenderer.flipX = false;
        }
        else if (velocityX < -moveThreshold)
        {
            spriteRenderer.flipX = true;
        }
    }
}