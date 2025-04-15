using UnityEngine;
using UnityEngine.Tilemaps;

public class GravityTilemap : MonoBehaviour
{
    [Tooltip("Is gravity normal in this field?")]
    public bool isGravityNormal = true;
    private Tilemap tilemap;
    private bool isPlayerInside = false;
    private PlayerMovement player;

    private bool originalCanChangeGravity;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap component not found on " + gameObject.name);
        }
        gameObject.tag = "GravityField";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                ApplyGravity(player);

                // Guarda el estado original y desactiva la habilidad
                originalCanChangeGravity = player.canChangeGravity;
                player.canChangeGravity = false;

                Debug.Log("Entró en zona de gravedad: NO puede usar Z para cambiar gravedad.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && player != null)
        {
            // Restaura el estado original
            player.canChangeGravity = originalCanChangeGravity;
            Debug.Log("Salió de zona de gravedad: AHORA puede usar Z para cambiar gravedad.");

            isPlayerInside = false;
            player = null;
        }
    }
    private void ApplyGravity(PlayerMovement player)
    {
        if (player.IsGrounded() && player.IsGravityNormal() != isGravityNormal)
        {
            float targetGravityScale = isGravityNormal ? Mathf.Abs(player.rb.gravityScale) : -Mathf.Abs(player.rb.gravityScale);
            player.rb.gravityScale = targetGravityScale;
            player.isGravityNormal = isGravityNormal;

            Vector3 center = player.boxCollider.bounds.center;
            player.transform.RotateAround(center, Vector3.forward, 180f);
            player.transform.RotateAround(center, Vector3.up, 180f);
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0f);

            Debug.Log($"Gravity applied: isGravityNormal={isGravityNormal}, gravityScale={player.rb.gravityScale}");
        }
    }
}