using UnityEngine;
using System.Collections;

public class LaserCollision : MonoBehaviour
{
    [SerializeField] private float damageAmount = 2f; // Cantidad de vida a quitar
    [SerializeField] private float teleportDistance = 0.80f; // Distancia de teletransporte en X
    [SerializeField] private float damageCooldown = 0.75f; // Cooldown en segundos entre daños
    [SerializeField] private float movementLockDuration = 0.3f; // Duración en segundos del bloqueo de movimiento

    private float lastDamageTime = -1f; // Tiempo del último daño (inicializado para permitir daño inmediato)

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Obtener los componentes necesarios
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            Transform playerTransform = collision.gameObject.transform;
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            Animator playerAnimator = collision.gameObject.GetComponent<Animator>();

            if (playerHealth == null)
            {
                Debug.LogWarning("El jugador no tiene un componente PlayerHealth.");
                return;
            }
            if (playerTransform == null)
            {
                Debug.LogWarning("El jugador no tiene un componente Transform.");
                return;
            }
            if (playerMovement == null)
            {
                Debug.LogWarning("El jugador no tiene un componente PlayerMovement.");
                return;
            }
            if (playerAnimator == null)
            {
                Debug.LogWarning("El jugador no tiene un componente Animator.");
                return;
            }

            // Verificar si ha pasado el cooldown desde el último daño
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                // Quitar vida al jugador
                playerHealth.TakeDamage(damageAmount);

                // Determinar la dirección del contacto usando la normal de la colisión
                ContactPoint2D contact = collision.GetContact(0);
                Vector2 contactNormal = contact.normal;

                // Calcular la dirección de teletransporte (solo en X)
                float teleportDirectionX = -Mathf.Sign(contactNormal.x); // Dirección contraria en X
                if (Mathf.Abs(contactNormal.x) < 0.1f) // Si el contacto es casi completamente vertical
                {
                    // Usar la velocidad del jugador para determinar la dirección si la normal no es clara
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null && playerRb.velocity.magnitude > 0.1f)
                    {
                        teleportDirectionX = -Mathf.Sign(playerRb.velocity.x);
                    }
                    else
                    {
                        teleportDirectionX = playerTransform.localScale.x > 0 ? -1f : 1f; // Usar la orientación del jugador como fallback
                    }
                }

                // Calcular la nueva posición
                Vector3 currentPosition = playerTransform.position;
                Vector3 newPosition = currentPosition + new Vector3(teleportDirectionX * teleportDistance, 0f, 0f);

                // Teletransportar al jugador
                playerTransform.position = newPosition;

                // Bloquear el movimiento y forzar la animación de estar quieto
                playerMovement.SetMovementLocked(true);
                SetIdleAnimation(playerAnimator, playerMovement.IsGrounded());

                // Desbloquear el movimiento y restaurar el control de la animación después de 0.5 segundos
                StartCoroutine(UnlockMovementAfterDelay(playerMovement, playerAnimator));

                // Actualizar el tiempo del último daño
                lastDamageTime = Time.time;
            }
        }
    }

    private void SetIdleAnimation(Animator animator, bool isGrounded)
    {
        // Forzar la animación a estado quieto
        animator.SetBool("MoveRight", false);
        animator.SetBool("MoveLeft", false);
        animator.SetFloat("VerticalSpeed", 0f);
        animator.SetBool("IsGrounded", isGrounded); // Mantener el estado de suelo actual
    }

    private IEnumerator UnlockMovementAfterDelay(PlayerMovement playerMovement, Animator playerAnimator)
    {
        yield return new WaitForSeconds(movementLockDuration);
        playerMovement.SetMovementLocked(false);
        // No restauramos parámetros específicos aquí, ya que PlayerMovement los actualizará en Update
    }
}