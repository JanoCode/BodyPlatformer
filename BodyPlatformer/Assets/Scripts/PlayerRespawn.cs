using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private float deathY = -6f;
    [SerializeField] private Transform respawnPoint;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (transform.position.y < deathY)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        if (respawnPoint == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = respawnPoint.position;
    }
}