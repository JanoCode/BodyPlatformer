using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 7f;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;

    private int bodyContacts = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            moveInput = 0f;
            return;
        }

        moveInput = gamepad.leftStick.x.ReadValue();

        // A en Xbox / X en PlayStation
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            jumpRequested = true;

            Debug.Log("Botón de salto presionado");
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        if (jumpRequested && bodyContacts > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            Debug.Log("SALTO");
        }

        jumpRequested = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            bodyContacts++;

            Debug.Log("Tocando cuerpo");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            bodyContacts--;

            if (bodyContacts < 0)
                bodyContacts = 0;
        }
    }
}