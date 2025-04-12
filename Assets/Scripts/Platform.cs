using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private Vector2 pointA;          // Punto A en el inspector
    [SerializeField] private Vector2 pointB;          // Punto B en el inspector
    [SerializeField] private float speed = 2f;        // Velocidad base de movimiento
    [SerializeField] private float pauseTime = 1.10f; // Tiempo de pausa en segundos
    [SerializeField] private bool startAtPointA = true; // ¿Comienza en punto A?
    [SerializeField] private bool waitForPlayer = false; // Checkbox para esperar al jugador
    [SerializeField] private bool waitForButton = false; // Checkbox para esperar al botón

    private Vector2 startPosition;                   // Posición de inicio del movimiento actual
    private Vector2 targetPosition;                  // Posición objetivo actual
    private bool movingToB = true;                   // Dirección del movimiento
    private float pauseTimer;                        // Temporizador de pausa
    private bool isPaused;                           // Estado de pausa
    private float journeyLength;                     // Distancia total del trayecto
    private float journeyTime;                       // Tiempo estimado del trayecto
    private float elapsedTime;                       // Tiempo transcurrido en el movimiento
    private Transform playerTransform;               // Referencia al jugador cuando está encima
    private bool hasStartedMoving;                   // Indica si la plataforma ya comenzó a moverse
    private float startDelayTimer;                   // Temporizador para el retraso inicial
    private bool waitingToStart;                     // Indica si está en el retraso inicial
    private bool isActivated;                        // Indica si la plataforma está activada por un botón

    public bool IsMoving => !isPaused;              // Propiedad pública para saber si se está moviendo

    void Start()
    {
        transform.position = startAtPointA ? pointA : pointB;
        startPosition = transform.position;
        targetPosition = startAtPointA ? pointB : pointA;
        movingToB = startAtPointA;
        
        journeyLength = Vector2.Distance(startPosition, targetPosition);
        journeyTime = journeyLength / speed;
        elapsedTime = 0f;
        pauseTimer = 0f;

        // Inicializar según los checkboxes
        isPaused = waitForPlayer || waitForButton;
        hasStartedMoving = !(waitForPlayer || waitForButton); // No se mueve hasta que se cumpla una condición
        startDelayTimer = 0f;
        waitingToStart = false;
        isActivated = false; // Inicialmente no activada por botón
    }

    void Update()
    {
        // Si espera un botón y no está activada, o espera al jugador y no ha comenzado, no moverse
        if ((waitForButton && !isActivated) || (waitForPlayer && !hasStartedMoving))
        {
            if (waitingToStart)
            {
                startDelayTimer -= Time.deltaTime;
                if (startDelayTimer <= 0)
                {
                    waitingToStart = false;
                    hasStartedMoving = true;
                    isPaused = false;
                    startPosition = transform.position;
                    targetPosition = movingToB ? pointB : pointA;
                    journeyLength = Vector2.Distance(startPosition, targetPosition);
                    journeyTime = journeyLength / speed;
                    elapsedTime = 0f;
                }
            }
            return;
        }

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
                startPosition = transform.position;
                targetPosition = movingToB ? pointA : pointB;
                movingToB = !movingToB;
                journeyLength = Vector2.Distance(startPosition, targetPosition);
                journeyTime = journeyLength / speed;
                elapsedTime = 0f;
            }
            return;
        }

        elapsedTime += Time.deltaTime;
        float fraction = Mathf.Clamp01(elapsedTime / journeyTime);
        float smoothFraction = SmoothCurve(fraction);

        Vector2 previousPosition = transform.position;
        transform.position = Vector2.Lerp(startPosition, targetPosition, smoothFraction);

        if (playerTransform != null)
        {
            Vector2 deltaPosition = (Vector2)transform.position - previousPosition;
            playerTransform.position += (Vector3)deltaPosition;
        }

        if (fraction >= 1f)
        {
            isPaused = true;
            pauseTimer = pauseTime;
        }
    }

    private float SmoothCurve(float t)
    {
        return t * t * (3f - 2f * t); // SmoothStep
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointB, 0.1f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTransform = collision.transform;

            if (waitForPlayer && !hasStartedMoving && !waitingToStart && !waitForButton)
            {
                waitingToStart = true;
                startDelayTimer = 0.50f;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTransform = null;
        }
    }

    public void ActivatePlatform()
    {
        isActivated = true;
        if (!hasStartedMoving)
        {
            waitingToStart = true;
            startDelayTimer = 0.50f; // Retraso similar al waitForPlayer
        }
    }

    public void DeactivatePlatform()
    {
        isActivated = false;
        isPaused = true; // Pausar el movimiento
    }
}