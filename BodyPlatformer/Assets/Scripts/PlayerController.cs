using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Cuerpo")]
    [SerializeField] private string bodyLayerName = "Body";

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;

    private int bodyContacts = 0;
    private Vector2 groundNormal = Vector2.up;

    private Transform currentBodySurface;
    private Vector3 lastSurfacePosition;
    private Vector2 surfaceVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
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

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        bool grounded = bodyContacts > 0;

        UpdateSurfaceVelocity();

        if (grounded)
        {
            MoveAlongSurface();
        }
        else
        {
            MoveInAir();
        }

        if (jumpRequested && grounded)
        {
            Jump();
        }

        jumpRequested = false;
    }

    private void MoveAlongSurface()
    {
        Vector2 tangent = new Vector2(
            groundNormal.y,
            -groundNormal.x
        );

        if (tangent.x < 0)
        {
            tangent = -tangent;
        }

        tangent.Normalize();

        Vector2 playerMovement =
            tangent * moveInput * moveSpeed;

        // Sumamos el movimiento de la superficie
        rb.linearVelocity =
            playerMovement + surfaceVelocity;
    }

    private void MoveInAir()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        bodyContacts = 0;
        currentBodySurface = null;
        surfaceVelocity = Vector2.zero;
    }

    private void UpdateSurfaceVelocity()
    {
        if (currentBodySurface == null)
        {
            surfaceVelocity = Vector2.zero;
            return;
        }

        Vector3 currentPosition =
            currentBodySurface.position;

        Vector3 movement =
            currentPosition - lastSurfacePosition;

        surfaceVelocity =
            movement / Time.fixedDeltaTime;

        lastSurfacePosition =
            currentPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsBody(collision.gameObject))
            return;

        bodyContacts++;

        currentBodySurface =
            collision.transform;

        lastSurfacePosition =
            currentBodySurface.position;

        UpdateGroundNormal(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsBody(collision.gameObject))
            return;

        currentBodySurface =
            collision.transform;

        UpdateGroundNormal(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsBody(collision.gameObject))
            return;

        bodyContacts--;

        if (bodyContacts < 0)
            bodyContacts = 0;

        if (bodyContacts == 0)
        {
            currentBodySurface = null;
            surfaceVelocity = Vector2.zero;
            groundNormal = Vector2.up;
        }
    }

    private bool IsBody(GameObject obj)
    {
        return obj.layer ==
            LayerMask.NameToLayer(bodyLayerName);
    }

    private void UpdateGroundNormal(Collision2D collision)
    {
        if (collision.contactCount == 0)
            return;

        Vector2 bestNormal = Vector2.up;
        float bestUp = -1f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            if (contact.normal.y > bestUp)
            {
                bestUp =
                    contact.normal.y;

                bestNormal =
                    contact.normal;
            }
        }

        if (bestNormal.y > 0.1f)
        {
            groundNormal =
                bestNormal;
        }
    }
}