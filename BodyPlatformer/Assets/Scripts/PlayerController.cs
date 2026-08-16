using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Detección")]
    [SerializeField] private string bodyLayerName = "Body";

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;

    private int bodyContacts;

    private Vector2 groundNormal = Vector2.up;
    private bool grounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Evita que la cápsula se caiga/rote físicamente.
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
        grounded = bodyContacts > 0;

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
        // La tangente es perpendicular a la normal del suelo.
        Vector2 tangent = new Vector2(
            groundNormal.y,
            -groundNormal.x
        );

        // Nos aseguramos de que "derecha" en el stick
        // siga significando derecha en pantalla.
        if (tangent.x < 0)
        {
            tangent = -tangent;
        }

        Vector2 movement =
            tangent.normalized * moveInput * moveSpeed;

        // Conservamos una pequeña componente normal,
        // pero desplazamos principalmente sobre la superficie.
        rb.linearVelocity = movement;
    }

    private void MoveInAir()
    {
        // En el aire seguimos teniendo control horizontal.
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        // Saltamos principalmente hacia arriba.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        bodyContacts = 0;
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer !=
            LayerMask.NameToLayer(bodyLayerName))
        {
            return;
        }

        bodyContacts++;

        UpdateGroundNormal(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer !=
            LayerMask.NameToLayer(bodyLayerName))
        {
            return;
        }

        UpdateGroundNormal(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer !=
            LayerMask.NameToLayer(bodyLayerName))
        {
            return;
        }

        bodyContacts--;

        if (bodyContacts < 0)
        {
            bodyContacts = 0;
        }

        if (bodyContacts == 0)
        {
            groundNormal = Vector2.up;
        }
    }

    private void UpdateGroundNormal(Collision2D collision)
    {
        if (collision.contactCount == 0)
            return;

        Vector2 bestNormal = Vector2.up;
        float bestUpValue = -1f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            // Buscamos la superficie cuya normal apunte más hacia arriba.
            if (contact.normal.y > bestUpValue)
            {
                bestUpValue = contact.normal.y;
                bestNormal = contact.normal;
            }
        }

        // Solo consideramos "suelo" una superficie
        // que tenga algo de orientación hacia arriba.
        if (bestNormal.y > 0.1f)
        {
            groundNormal = bestNormal;
        }
    }
}