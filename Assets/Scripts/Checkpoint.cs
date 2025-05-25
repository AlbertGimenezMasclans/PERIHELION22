using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Time (in seconds) the checkpoint remains in cooldown after healing the player")]
    public float healCooldown = 10f;

    [Header("Zone Management")]
    [Tooltip("The visual zone (GameObject) associated with this checkpoint")]
    public GameObject visualZone;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the checkpoint is activated")]
    public AudioClip activationSound;

    [Tooltip("Sound to play when the player is healed")]
    public AudioClip healSound;

    private Animator animator;
    private bool isActivated = false;
    private bool isInCooldown = false;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (animator == null)
        {
            Debug.LogError("Animator no encontrado en el Checkpoint.", this);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource no encontrado. Se agregó uno automáticamente.");
        }

        Vector2 checkpointPosition = transform.position;
        Zone zone = FindZoneAtPosition(checkpointPosition);
        if (zone == null)
        {
            Debug.LogWarning($"El checkpoint {gameObject.name} no está dentro de ninguna zona de cámara.");
        }
        else
        {
            Debug.Log($"El checkpoint {gameObject.name} está dentro de la zona de cámara {zone.gameObject.name}.");
        }

        if (visualZone == null)
        {
            Debug.LogWarning($"VisualZone no asignado en el checkpoint {gameObject.name}.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Debug.Log("Colisión detectada con el jugador.");

        if (!isActivated)
        {
            if (animator != null)
            {
                animator.SetBool("Active", true);
                Debug.Log("Activando animación 'Checkpoint-Active'.");
                StartCoroutine(TransitionToCheckpointC());
            }

            isActivated = true;

            // Sonido al activar el checkpoint
            if (activationSound != null)
            {
                audioSource.PlayOneShot(activationSound);
            }

            PlayerDeath playerDeath = collision.GetComponent<PlayerDeath>();
            if (playerDeath != null)
            {
                Vector3 checkpointPosition = transform.position;
                Vector3 newSpawnPosition = new Vector3(checkpointPosition.x, checkpointPosition.y + 0.50f, checkpointPosition.z);
                playerDeath.SetNewSpawnPositionAndCamera(newSpawnPosition, checkpointPosition.x, this);
                Debug.Log($"Nueva posición de reaparición del jugador: {newSpawnPosition}");
            }
        }

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth && !isInCooldown)
        {
            Debug.Log("Curando al jugador...");
            StartCoroutine(HealPlayer(playerHealth));
        }
    }

    private IEnumerator TransitionToCheckpointC()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Checkpoint-Active"));

        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        animator.SetBool("IsCheckpointC", true);
    }

    private IEnumerator HealPlayer(PlayerHealth playerHealth)
    {
        isInCooldown = true;

        playerHealth.ShowObjects();
        playerHealth.currentHealth = playerHealth.maxHealth;

        HealthBar healthBar = FindObjectOfType<HealthBar>();
        if (healthBar != null)
        {
            healthBar.UpdateHealth(playerHealth.currentHealth);
            playerHealth.UpdateHealthCounterText();
        }

        // Sonido al curar al jugador
        if (healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        yield return new WaitForSeconds(1f);
        playerHealth.ForceHideObjects();
        yield return new WaitForSeconds(healCooldown);

        isInCooldown = false;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    private Zone FindZoneAtPosition(Vector2 position)
    {
        Zone[] zones = FindObjectsOfType<Zone>();
        foreach (Zone zone in zones)
        {
            if (zone.ContainsPosition(position))
                return zone;
        }
        return null;
    }

    public GameObject GetVisualZone()
    {
        return visualZone;
    }
}
