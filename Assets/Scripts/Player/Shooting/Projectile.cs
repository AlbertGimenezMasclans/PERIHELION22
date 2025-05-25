using UnityEngine;
using UnityEngine.Audio;

public class Projectile : MonoBehaviour
{
    private float lifetime = 2.5f; // Tiempo de vida en segundos
    private float timer = 0f;
    private Rigidbody2D rb; // Referencia al Rigidbody2D para obtener la velocidad
    private SpriteRenderer spriteRenderer; // Referencia al SpriteRenderer para voltear el sprite

    [Header("Damage Settings")]
    [Tooltip("Daño causado a los enemigos al impactar")]
    [SerializeField] private float damageToEnemy = 5f; // Daño al enemigo, configurable en el Inspector

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSound; // Sonido al disparar el proyectil
    [SerializeField] private AudioClip wallBreakSound; // Sonido al destruir una pared
    [SerializeField] private AudioSource audioSource; // AudioSource para reproducir sonidos
    [SerializeField] private AudioMixerGroup outputMixerGroup; // Salida de audio opcional
    [SerializeField, Range(0f, 1f)] private float shootSoundVolume = 1f; // Volumen del sonido de disparo
    [SerializeField, Range(0f, 1f)] private float wallBreakVolume = 1f; // Volumen del sonido de la pared

    void Awake()
    {
        // Obtener los componentes necesarios
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
        {
            Debug.LogError("Projectile necesita un Rigidbody2D para determinar la dirección.", gameObject);
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Projectile no tiene un SpriteRenderer. La funcionalidad de volteo de sprite no estará disponible.", gameObject);
        }
        if (audioSource == null)
        {
            Debug.LogError("Projectile necesita un AudioSource para reproducir el sonido de disparo.", gameObject);
        }
        else if (outputMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = outputMixerGroup; // Asignar salida de audio si existe
        }

        // Asegurar que los volúmenes estén en un rango válido
        shootSoundVolume = Mathf.Clamp(shootSoundVolume, 0f, 1f);
        wallBreakVolume = Mathf.Clamp(wallBreakVolume, 0f, 1f);
    }

    void OnEnable()
    {
        timer = lifetime;
        // Reproducir sonido de disparo
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, shootSoundVolume);
        }
        else if (shootSound == null)
        {
            Debug.LogWarning("No se asignó un AudioClip para el sonido de disparo en el Inspector.", gameObject);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReturnToPool();
        }

        // Voltear el sprite según la dirección en el eje X, si hay un SpriteRenderer
        if (rb != null && spriteRenderer != null)
        {
            float velocityX = rb.velocity.x;
            if (Mathf.Abs(velocityX) > 0.1f) // Solo voltear si hay movimiento significativo
            {
                spriteRenderer.flipX = velocityX > 0f; // Voltear si va a la izquierda
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Aplicar daño si colisiona con un enemigo
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyShooter enemy = collision.gameObject.GetComponent<EnemyShooter>();
            if (enemy != null)
            {
                collision.gameObject.SendMessage("TakeDamage", damageToEnemy, SendMessageOptions.DontRequireReceiver);
            }
        }
        // Destruir pared rompible si colisiona con ella
        else if (collision.gameObject.CompareTag("BreakableWall"))
        {
            // Reproducir sonido de destrucción de pared
            if (wallBreakSound != null)
            {
                Debug.Log($"Reproduciendo wallBreakSound con volumen {wallBreakVolume}", gameObject);
                GameObject tempAudioObject = new GameObject("TempWallBreakSound");
                AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
                tempAudioSource.clip = wallBreakSound;
                tempAudioSource.volume = wallBreakVolume;
                tempAudioSource.spatialBlend = 0f; // Configurar como 2D
                if (outputMixerGroup != null)
                {
                    tempAudioSource.outputAudioMixerGroup = outputMixerGroup;
                }
                tempAudioSource.Play();
                Destroy(tempAudioObject, wallBreakSound.length);
            }
            else
            {
                Debug.LogWarning("No se asignó un AudioClip para el sonido de destrucción de pared en el Inspector.", gameObject);
            }
            Destroy(collision.gameObject);
        }

        // No devolver al pool si colisiona con "Crate"
        if (!collision.gameObject.CompareTag("Crate"))
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        ProjectilePool.Instance.ReturnProjectile(gameObject);
    }
}