using UnityEngine;
using System.Collections.Generic;

public class LongPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Primer punto de la trayectoria.")]
    [SerializeField] private Vector2 pointA;
    [Tooltip("Segundo punto de la trayectoria.")]
    [SerializeField] private Vector2 pointB;
    [Tooltip("Tercer punto de la trayectoria.")]
    [SerializeField] private Vector2 pointC;
    [Tooltip("Cuarto punto de la trayectoria.")]
    [SerializeField] private Vector2 pointD;
    [Tooltip("Quinto punto de la trayectoria.")]
    [SerializeField] private Vector2 pointE;
    [Tooltip("Sexto punto de la trayectoria.")]
    [SerializeField] private Vector2 pointF;

    [Header("Movement Settings")]
    [Tooltip("Velocidad de movimiento de la plataforma (unidades por segundo).")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Índice del punto inicial (0 = Point A, 1 = Point B, ..., 5 = Point F).")]
    [SerializeField] [Range(0, 5)] private int startPointIndex = 0;
    [Tooltip("Índice del punto donde la plataforma puede detenerse si Stop At Last Point está marcado (0 = Point A, ..., 5 = Point F).")]
    [SerializeField] [Range(0, 5)] private int lastPointIndex = 5;
    [Tooltip("Si está marcado, la plataforma espera a que el jugador la toque para empezar a moverse.")]
    [SerializeField] private bool waitForPlayer = false;
    [Tooltip("Si está marcado, la plataforma se detiene permanentemente al llegar al punto especificado en Last Point Index.")]
    [SerializeField] private bool stopAtLastPoint = false;

    private List<Vector2> waypoints = new List<Vector2>(); // Lista de puntos a recorrer
    private int currentWaypointIndex;                // Índice del punto objetivo actual
    private Vector2 startPosition;                   // Posición de inicio del movimiento actual
    private Vector2 targetPosition;                  // Posición objetivo actual
    private bool isPaused;                           // Estado de pausa (solo usado para inicio o detención final)
    private float journeyLength;                     // Distancia total del trayecto
    private float journeyTime;                       // Tiempo estimado del trayecto
    private float elapsedTime;                       // Tiempo transcurrido en el movimiento
    private Transform playerTransform;               // Referencia al jugador cuando está encima
    private bool hasStartedMoving;                   // Indica si la plataforma ya comenzó a moverse
    private float startDelayTimer;                   // Temporizador para el retraso inicial
    private bool waitingToStart;                     // Indica si está en el retraso inicial
    private bool hasStoppedPermanently;              // Indica si la plataforma se ha detenido permanentemente
    private bool movingForward = true;               // Indica si va en orden normal (A→F) o inverso (F→A)
    private bool isFirstMove = true;                 // Indica si es el primer movimiento desde el punto inicial

    public bool IsMoving => !isPaused && !hasStoppedPermanently; // Propiedad pública para saber si se está moviendo

    void Start()
    {
        // Configurar los puntos de la trayectoria
        waypoints.Clear();
        waypoints.Add(pointA);
        waypoints.Add(pointB);
        waypoints.Add(pointC);
        waypoints.Add(pointD);
        waypoints.Add(pointE);
        waypoints.Add(pointF);

        // Validar los índices
        startPointIndex = Mathf.Clamp(startPointIndex, 0, waypoints.Count - 1);
        lastPointIndex = Mathf.Clamp(lastPointIndex, 0, waypoints.Count - 1);

        // Establecer posición inicial
        currentWaypointIndex = startPointIndex;
        transform.position = waypoints[currentWaypointIndex];
        startPosition = transform.position;
        targetPosition = waypoints[GetNextWaypointIndex()];
        
        journeyLength = Vector2.Distance(startPosition, targetPosition);
        journeyTime = journeyLength / speed;
        elapsedTime = 0f;

        // Inicializar según el checkbox
        isPaused = waitForPlayer;
        hasStartedMoving = !waitForPlayer;
        startDelayTimer = 0f;
        waitingToStart = false;
        hasStoppedPermanently = false;
        movingForward = true;
        isFirstMove = true;
    }

    void Update()
    {
        // Si está detenido permanentemente, no hacer nada
        if (hasStoppedPermanently)
        {
            return;
        }

        // Si espera al jugador y no ha comenzado, no moverse
        if (waitForPlayer && !hasStartedMoving)
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
                    targetPosition = waypoints[GetNextWaypointIndex()];
                    journeyLength = Vector2.Distance(startPosition, targetPosition);
                    journeyTime = journeyLength / speed;
                    elapsedTime = 0f;
                    isFirstMove = true;
                }
            }
            return;
        }

        if (isPaused)
        {
            // Solo se usa isPaused para el estado inicial o detención final
            return;
        }

        elapsedTime += Time.deltaTime;
        float fraction = Mathf.Clamp01(elapsedTime / journeyTime);

        // Determinar el tipo de suavizado según el contexto
        bool isApproachingLastPoint = currentWaypointIndex == lastPointIndex;
        float smoothFraction = SmoothCurve(fraction, isFirstMove, isApproachingLastPoint);

        Vector2 previousPosition = transform.position;
        transform.position = Vector2.Lerp(startPosition, targetPosition, smoothFraction);

        if (playerTransform != null)
        {
            Vector2 deltaPosition = (Vector2)transform.position - previousPosition;
            playerTransform.position += (Vector3)deltaPosition;
        }

        if (fraction >= 1f)
        {
            // Llegó al punto actual
            // Verificar si es el último punto y debe detenerse
            if (stopAtLastPoint && currentWaypointIndex == lastPointIndex)
            {
                hasStoppedPermanently = true;
                isPaused = true;
                return;
            }

            // Actualizar dirección al llegar al extremo
            if (!stopAtLastPoint)
            {
                if (movingForward && currentWaypointIndex == waypoints.Count - 1)
                {
                    movingForward = false; // Cambiar a orden inverso
                }
                else if (!movingForward && currentWaypointIndex == 0)
                {
                    movingForward = true; // Volver a orden normal
                }
            }

            // Desactivar isFirstMove después del primer movimiento
            if (isFirstMove)
            {
                isFirstMove = false;
            }

            // Mover al siguiente punto
            currentWaypointIndex = GetNextWaypointIndex();
            startPosition = transform.position;
            targetPosition = waypoints[currentWaypointIndex];
            journeyLength = Vector2.Distance(startPosition, targetPosition);
            journeyTime = journeyLength / speed;
            elapsedTime = 0f;
        }
    }

    private int GetNextWaypointIndex()
    {
        if (movingForward)
        {
            return (currentWaypointIndex + 1) % waypoints.Count;
        }
        else
        {
            return currentWaypointIndex == 0 ? waypoints.Count - 1 : currentWaypointIndex - 1;
        }
    }

    private float SmoothCurve(float t, bool isFirstMove, bool isApproachingLastPoint)
    {
        if (isFirstMove)
        {
            // Suavizado muy ligero para el primer movimiento
            // Usamos una mezcla cercana a lineal para un inicio casi directo
            return t * (1f + t * (0.5f - 0.5f * t)); // Curva suave pero mínima
        }
        else if (isApproachingLastPoint)
        {
            // Suavizado pronunciado para el frenado en el último punto
            // Usamos SmoothStep original para una desaceleración notable
            return t * t * (3f - 2f * t);
        }
        else
        {
            // Suavizado intermedio para movimientos entre puntos
            return t * t * (2f - t);
        }

        // Opciones para ajustar el suavizado manualmente:
        // - return t; // Movimiento completamente lineal (sin suavizado)
        // - return t * t * (3f - 2f * t); // SmoothStep original (suavizado fuerte)
        // - return (t + t * t * (2f - t)) / 2; // Mezcla de lineal y suavizado intermedio
        // - return Mathf.Sin(t * Mathf.PI / 2); // Suavizado sinusoidal
    }

    void OnDrawGizmos()
    {
        // Dibujar todos los puntos con colores distintos
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointB, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pointC, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointD, 0.1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(pointE, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pointF, 0.1f);

        // Resaltar el último punto seleccionado si stopAtLastPoint está activado
        if (stopAtLastPoint && waypoints.Count > lastPointIndex)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(waypoints[lastPointIndex], 0.15f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTransform = collision.transform;

            if (waitForPlayer && !hasStartedMoving && !waitingToStart)
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

    public void SetPointA(Vector2 newPointA)
    {
        pointA = newPointA;
        if (waypoints.Count > 0) waypoints[0] = newPointA;
    }

    public void SetPointB(Vector2 newPointB)
    {
        pointB = newPointB;
        if (waypoints.Count > 1) waypoints[1] = newPointB;
    }

    public void SetPointC(Vector2 newPointC)
    {
        pointC = newPointC;
        if (waypoints.Count > 2) waypoints[2] = newPointC;
    }

    public void SetPointD(Vector2 newPointD)
    {
        pointD = newPointD;
        if (waypoints.Count > 3) waypoints[3] = newPointD;
    }

    public void SetPointE(Vector2 newPointE)
    {
        pointE = newPointE;
        if (waypoints.Count > 4) waypoints[4] = newPointE;
    }

    public void SetPointF(Vector2 newPointF)
    {
        pointF = newPointF;
        if (waypoints.Count > 5) waypoints[5] = newPointF;
    }
}