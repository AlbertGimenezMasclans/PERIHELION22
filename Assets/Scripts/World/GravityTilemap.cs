using UnityEngine;

public class GravityTilemap : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("Should the gravity be normal (down) or inverted (up)?")]
    [SerializeField] private bool isGravityNormal = true; // Normal (true) o invertida (false)

    private bool isPlayerInside; // Rastrear si el jugador está dentro del trigger
    private PlayerMovement player; // Referencia al jugador para actualizaciones continuas

    private void OnEnable()
    {
        // Cuando el Tilemap se activa, forzar la gravedad si el jugador ya está dentro
        if (isPlayerInside && player != null && player.IsGravityNormal() != isGravityNormal)
        {
            player.ChangeGravity();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Forzar la gravedad deseada
                if (player.IsGravityNormal() != isGravityNormal)
                {
                    player.ChangeGravity();
                }
                // Bloquear el cambio de gravedad manual
                player.SetGravityLocked(true);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Forzar la gravedad mientras el jugador está dentro
        if (other.CompareTag("Player"))
        {
            if (player != null && player.IsGravityNormal() != isGravityNormal)
            {
                player.ChangeGravity();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (player != null)
            {
                // Desbloquear el cambio de gravedad manual
                player.SetGravityLocked(false);
            }
            player = null; // Limpiar la referencia
        }
    }
}