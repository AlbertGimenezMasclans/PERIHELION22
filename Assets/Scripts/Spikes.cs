using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private float damageAmount = 2f; // Cantidad de vida a quitar (float para coincidir con PlayerHealth)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto que entró es el jugador
        if (other.CompareTag("Player"))
        {
            // Obtener el script PlayerMovement para cambiar la gravedad
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Cambiar la gravedad usando el método ChangeGravity
                if (playerMovement.canChangeGravity)
                {
                    playerMovement.ChangeGravity();
                }
                else
                {
                    Debug.LogWarning("El jugador no tiene la habilidad de cambiar gravedad activada.");
                }
            }
            else
            {
                Debug.LogWarning("El jugador no tiene un componente PlayerMovement.");
            }

            // Quitar vida al jugador usando PlayerHealth
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount); // Restar 2 de vida
            }
            else
            {
                Debug.LogWarning("El jugador no tiene un componente PlayerHealth.");
            }
        }
    }
}