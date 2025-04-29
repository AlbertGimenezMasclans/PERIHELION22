using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("DeathZone Settings")]
    [Tooltip("GameObject whose position will be used as the respawn point for the player and camera")]
    [SerializeField] private GameObject respawnPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDeath playerDeath = other.GetComponent<PlayerDeath>();
            if (playerDeath != null && !playerDeath.IsDead())
            {
                // Verificar que el Animator esté presente
                Animator playerAnimator = other.GetComponent<Animator>();
                if (playerAnimator == null)
                {
                    Debug.LogWarning("El jugador no tiene un componente Animator. La lógica de muerte puede no funcionar correctamente.");
                }

                // Actualizar la posición de reaparición y la cámara si se especificó un respawnPoint
                if (respawnPoint != null)
                {
                    playerDeath.SetNewSpawnPositionAndCamera(respawnPoint.transform.position, respawnPoint.transform.position.x);
                    Debug.Log($"Posición de reaparición y cámara actualizadas: Jugador = {respawnPoint.transform.position}, Cámara X = {respawnPoint.transform.position.x}");
                }
                else
                {
                    Debug.LogWarning("No se asignó un respawnPoint en el Inspector. Usando la posición de reaparición predeterminada.");
                }

                // Iniciar la coroutine de reaparición
                StartCoroutine(playerDeath.RespawnCoroutine());
            }
        }
    }
}