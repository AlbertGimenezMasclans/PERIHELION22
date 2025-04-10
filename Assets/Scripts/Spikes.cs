using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private float damageAmount = 2f; // Cantidad de vida a quitar
    [SerializeField] private float bounceForce = 5f; // Fuerza del rebote

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto que entró es el jugador
        if (other.CompareTag("Player"))
        {
            // Obtener los componentes necesarios
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            if (playerHealth == null)
            {
                Debug.LogWarning("El jugador no tiene un componente PlayerHealth.");
                return;
            }
            if (playerRb == null)
            {
                Debug.LogWarning("El jugador no tiene un componente Rigidbody2D.");
                return;
            }

            // Quitar vida al jugador
            playerHealth.TakeDamage(damageAmount);

            // Rebotar al jugador
            Vector2 currentVelocity = playerRb.velocity;
            Vector2 bounceDirection = -currentVelocity.normalized; // Dirección opuesta al movimiento actual
            Vector2 bounceVelocity = bounceDirection * bounceForce;
            playerRb.velocity = bounceVelocity;
        }
    }
}