using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimatorBridge : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Collider2D playerCollider;

    [Header("Suelo")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float minimumGroundNormalY = 0.2f;

    [Header("Movimiento")]
    [SerializeField] private float moveThreshold = 0.1f;

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];

    private void Update()
    {
        if (animator == null ||
            playerRigidbody == null ||
            playerCollider == null)
        {
            return;
        }

        float moveInput = GetMoveInput();
        bool grounded = CheckGrounded();

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("Grounded", grounded);
        animator.SetFloat(
            "VerticalVelocity",
            playerRigidbody.linearVelocity.y
        );

        UpdateFacing(moveInput);
    }

    private float GetMoveInput()
    {
        // Mando
        if (Gamepad.current != null)
        {
            float stickX =
                Gamepad.current.leftStick.x.ReadValue();

            if (Mathf.Abs(stickX) > moveThreshold)
            {
                return stickX;
            }
        }

        // Teclado como respaldo para pruebas
        if (Keyboard.current != null)
        {
            float keyboardInput = 0f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                keyboardInput -= 1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                keyboardInput += 1f;
            }

            return keyboardInput;
        }

        return 0f;
    }

    private bool CheckGrounded()
    {
        ContactFilter2D filter = new ContactFilter2D();

        filter.useLayerMask = true;
        filter.layerMask = groundLayer;

        filter.useTriggers = false;

        int contactCount =
            playerCollider.GetContacts(filter, contacts);

        for (int i = 0; i < contactCount; i++)
        {
            // Una normal positiva en Y significa que algo
            // está sosteniendo al jugador desde abajo.
            if (contacts[i].normal.y >= minimumGroundNormalY)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateFacing(float moveInput)
    {
        if (spriteRenderer == null)
            return;

        if (moveInput > moveThreshold)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < -moveThreshold)
        {
            spriteRenderer.flipX = true;
        }
    }
}