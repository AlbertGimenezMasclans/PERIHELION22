using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("Prefab del proyectil que dispara el enemigo")]
    public GameObject projectilePrefab;
    [Tooltip("Punto desde donde se dispara el proyectil (debe ser hijo del enemigo)")]
    public Transform firePoint;
    [Tooltip("Velocidad del proyectil")]
    public float projectileSpeed = 8f;
    [Tooltip("Frecuencia de disparo (segundos entre disparos)")]
    public float fireRate = 2f;
    [Tooltip("Daño causado al jugador por cada disparo")]
    public float projectileDamage = 5f;

    [Header("Detection and Shooting Range")]
    [Tooltip("Distancia máxima a la que el enemigo detecta al jugador")]
    public float detectionRange = 10f;
    [Tooltip("Distancia máxima del rango de disparo triangular")]
    public float shootingRange = 5f;
    [Tooltip("Ángulo del cono de disparo (en grados, la mitad del cono total)")]
    public float shootingAngle = 30f;

    [Header("Contact Damage Settings")]
    [Tooltip("Daño causado al jugador al tocar al enemigo")]
    public float contactDamage = 4f;
    [Tooltip("Tiempo de enfriamiento después de hacer daño por contacto (segundos)")]
    public float contactCooldown = 1f;

    [Header("Health Settings")]
    [Tooltip("Vida máxima del enemigo")]
    public float maxHealth = 10f;

    [Header("Death Effect Settings")]
    [Tooltip("Prefab for the effect to play when the enemy dies")]
    public GameObject deathEffectPrefab;
    [Tooltip("Duration of the death effect")]
    public float deathEffectDuration = 2f;

    [Header("Movement Settings")]
    [Tooltip("Enable movement between Point A and Point B")]
    [SerializeField] private bool enableMovement = false;
    [Tooltip("Coordinates for Point A")]
    [SerializeField] private Vector2 pointA;
    [Tooltip("Coordinates for Point B")]
    [SerializeField] private Vector2 pointB;
    [Tooltip("Speed when moving between points A and Point B")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Speed when chasing the player")]
    [SerializeField] private float chaseSpeed = 4f;
    [Tooltip("Pause duration at each point (seconds)")]
    [SerializeField] private float pauseDuration = 1f;
    [Tooltip("Start movement at Point A")]
    [SerializeField] private bool startAtPointA = true;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private float nextFireTime;
    private float contactCooldownEnd;
    private bool isOnCooldown;
    private float currentHealth;
    private bool facingRight;
    private bool lastFacingRight; // Orientación durante el movimiento y pausa
    private bool isDead;

    // Variables para el movimiento
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingToB;
    private float pauseTimer;
    private bool isPaused;
    private float journeyLength;
    private float journeyTime;
    private float elapsedTime;
    private float fixedY; // Coordenada Y fija para el movimiento

    void Start()
    {
        // Inicializar referencias
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("No se encontró al jugador con el tag 'Player'. Asegúrate de que el jugador tenga el tag asignado.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró SpriteRenderer en el enemigo. El sprite no podrá voltearse.");
        }

        if (firePoint == null)
        {
            Debug.LogError("FirePoint no está asignado en el Inspector.");
        }

        // Inicializar movimiento
        if (enableMovement)
        {
            fixedY = pointA.y; // Usar la coordenada Y de pointA como fija
            transform.position = startAtPointA ? new Vector2(pointA.x, fixedY) : new Vector2(pointB.x, fixedY);
            startPosition = transform.position;
            targetPosition = startAtPointA ? new Vector2(pointB.x, fixedY) : new Vector2(pointA.x, fixedY);
            movingToB = startAtPointA;

            journeyLength = Mathf.Abs(startPosition.x - targetPosition.x);
            journeyTime = journeyLength > 0 ? journeyLength / moveSpeed : 0f; // Evitar división por cero
            elapsedTime = 0f;
            pauseTimer = 0f;
            isPaused = false;

            // Inicializar orientación
            facingRight = targetPosition.x > transform.position.x;
            lastFacingRight = facingRight;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
        }

        nextFireTime = Time.time;
        contactCooldownEnd = Time.time;
        currentHealth = maxHealth;
        isDead = false;
    }

    void Update()
    {
        if (player == null || isDead) return;

        // Calcular la distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerDetected = distanceToPlayer <= detectionRange;

        // Ajustar la posición del firePoint
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (facingRight ? 1f : -1f);
            firePoint.localPosition = localPos;
        }

        // Manejar disparos
        if (Time.time >= contactCooldownEnd)
        {
            isOnCooldown = false;
        }

        if (playerDetected && IsPlayerInShootingCone(distanceToPlayer) && Time.time >= nextFireTime && !isOnCooldown)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Manejar movimiento y volteo del sprite
        if (enableMovement)
        {
            if (playerDetected)
            {
                // Perseguir al jugador y voltear según su posición
                facingRight = player.position.x > transform.position.x;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = !facingRight;
                }
                MoveTowardsPlayer();
            }
            else
            {
                // Mantener la orientación del sprite durante la pausa
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = !lastFacingRight;
                }
                MoveBetweenPoints();
            }
        }
        else
        {
            // Volteo basado en la posición del jugador si no hay movimiento
            if (playerDetected && spriteRenderer != null)
            {
                facingRight = player.position.x > transform.position.x;
                spriteRenderer.flipX = !facingRight;
            }
        }
    }

    void MoveBetweenPoints()
    {
        if (journeyLength == 0) return; // No moverse si los puntos son iguales

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                startPosition = transform.position;
                targetPosition = movingToB ? new Vector2(pointA.x, fixedY) : new Vector2(pointB.x, fixedY);
                movingToB = !movingToB;
                // Actualizar la orientación para el nuevo movimiento
                facingRight = targetPosition.x > transform.position.x;
                lastFacingRight = facingRight;
                journeyLength = Mathf.Abs(startPosition.x - targetPosition.x);
                journeyTime = journeyLength > 0 ? journeyLength / moveSpeed : 0f;
                elapsedTime = 0f;
            }
            return;
        }

        elapsedTime += Time.deltaTime;
        float fraction = Mathf.Clamp01(elapsedTime / journeyTime);
        float smoothFraction = SmoothCurve(fraction);

        float newX = Mathf.Lerp(startPosition.x, targetPosition.x, smoothFraction);
        transform.position = new Vector2(newX, fixedY);

        if (fraction >= 1f)
        {
            isPaused = true;
            pauseTimer = pauseDuration;
        }
    }

    void MoveTowardsPlayer()
    {
        float minX = Mathf.Min(pointA.x, pointB.x);
        float maxX = Mathf.Max(pointA.x, pointB.x);

        Vector2 targetPos = new Vector2(player.position.x, fixedY);
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
    }

    private float SmoothCurve(float t)
    {
        return t * t * (3f - 2f * t); // SmoothStep
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("ProjectilePrefab o FirePoint no están asignados en el Inspector.");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb == null)
        {
            projectileRb = projectile.AddComponent<Rigidbody2D>();
        }

        projectileRb.gravityScale = 0f;
        Vector2 direction = (player.position - firePoint.position).normalized;
        projectileRb.velocity = direction * projectileSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.SetDamage(projectileDamage);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }

            isOnCooldown = true;
            contactCooldownEnd = Time.time + contactCooldown;
        }
    }

    bool IsPlayerInShootingCone(float distance)
    {
        if (distance > shootingRange) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(forward, directionToPlayer);

        return angleToPlayer <= shootingAngle;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointB, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA, pointB);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Vector3 forward = facingRight ? Vector3.right : Vector3.left;
        Vector3 leftEdge = Quaternion.Euler(0, 0, shootingAngle) * forward * shootingRange;
        Vector3 rightEdge = Quaternion.Euler(0, 0, -shootingAngle) * forward * shootingRange;

        Gizmos.DrawLine(transform.position, transform.position + leftEdge);
        Gizmos.DrawLine(transform.position, transform.position + rightEdge);
        Gizmos.DrawLine(transform.position + leftEdge, transform.position + rightEdge);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        enabled = false;

        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
            if (effectRenderer != null)
            {
                effectRenderer.flipX = Random.value > 0.5f;
            }
            Destroy(effect, deathEffectDuration);
        }
        else
        {
            Debug.LogWarning("DeathEffectPrefab no está asignado en el Inspector. No se mostrará ningún efecto al morir.");
        }

        Destroy(gameObject);
    }
}