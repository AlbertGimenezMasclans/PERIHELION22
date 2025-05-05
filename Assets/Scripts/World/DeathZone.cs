using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("DeathZone Settings")]
    [Tooltip("GameObject whose position will be used as the respawn point for the player and camera")]
    [SerializeField] private GameObject respawnPoint;

    void Start()
    {
        // Validar que respawnPoint esté asignado
        if (respawnPoint == null)
        {
            Debug.LogError($"No se asignó un respawnPoint en el Inspector para {gameObject.name}. El jugador reaparecerá en la posición inicial predeterminada.");
        }
        else
        {
            Debug.Log($"RespawnPoint asignado para {gameObject.name}: {respawnPoint.transform.position}");
        }

        // Validar que el GameObject tenga un BoxCollider2D configurado como trigger
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null || !collider.isTrigger)
        {
            Debug.LogError($"El GameObject {gameObject.name} debe tener un BoxCollider2D configurado como trigger para funcionar como DeathZone.");
        }
    }

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

                // Actualizar la posición de reaparición y la cámara
                if (respawnPoint != null)
                {
                    Vector3 respawnPosition = respawnPoint.transform.position;
                    // Usar la posición X del respawnPoint para la cámara, manteniendo Y y Z de la cámara inicial
                    playerDeath.SetNewSpawnPositionAndCamera(respawnPosition, respawnPosition.x);
                    Debug.Log($"DeathZone {gameObject.name} actualizó la posición de reaparición: Jugador = {respawnPosition}, Cámara X = {respawnPosition.x}");

                    // Invalidar el checkpoint activo para priorizar el respawnPoint del DeathZone
                    System.Reflection.FieldInfo activeCheckpointField = typeof(PlayerDeath).GetField("activeCheckpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (activeCheckpointField != null)
                    {
                        activeCheckpointField.SetValue(playerDeath, null);
                        Debug.Log("Checkpoint activo invalidado para priorizar el respawnPoint del DeathZone.");
                    }
                }
                else
                {
                    Debug.LogWarning($"No se asignó un respawnPoint en el Inspector para {gameObject.name}. Usando la posición de reaparición predeterminada.");
                }

                // Iniciar la coroutine de reaparición
                StartCoroutine(playerDeath.RespawnCoroutine());
            }
            else
            {
                Debug.LogWarning("No se encontró PlayerDeath en el objeto del jugador o el jugador ya está muerto.");
            }
        }
    }
}