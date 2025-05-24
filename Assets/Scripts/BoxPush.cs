using UnityEngine;

public class BoxPush : MonoBehaviour
{
    [Header("Push Settings")]
    [Tooltip("Base force applied when pushed")]
    [SerializeField] private float pushForce = 5f; // Fuerza base de empuje
    [Tooltip("Inverse gravity for the box")]
    [SerializeField] private bool inverseGravity = false; // Checkbox para gravedad invertida

    private Rigidbody2D rb;
    private float normalGravityScale;
    private Vector2 initialPosition; // Almacenar posición inicial
    private Quaternion initialRotation; // Almacenar rotación inicial
    private bool isInGravityField = false; // Rastrear si está en GravityField
    private bool fieldGravityNormal = true; // Almacenar la gravedad del campo
    private bool isInMoveLimit = false; // Nuevo: Rastrear si está dentro del área de movimiento
    private Bounds moveLimitBounds; // Nuevo: Almacenar los límites del collider

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGravityScale = rb.gravityScale;
        initialPosition = transform.position; // Guardar posición inicial
        initialRotation = transform.rotation; // Guardar rotación inicial
    }

    void Update()
    {
        // Aplicar gravedad según el estado
        if (isInGravityField)
        {
            rb.gravityScale = fieldGravityNormal ? normalGravityScale : -normalGravityScale;
        }
        else
        {
            rb.gravityScale = inverseGravity ? -normalGravityScale : normalGravityScale;
        }

        // Restringir movimiento dentro de los límites si está en el área de movimiento
        if (isInMoveLimit)
        {
            RestrictMovement();
        }
    }

    // Método para aplicar empuje desde los colliders laterales
    public void PushBox(Vector2 direction)
    {
        if (direction.x != 0) // Solo empujar si hay entrada horizontal
        {
            float effectiveForce = pushForce / rb.mass;
            rb.AddForce(direction * effectiveForce, ForceMode2D.Force);
        }
    }

    private void RestrictMovement()
    {
        // Obtener la posición actual de la caja
        Vector3 newPosition = transform.position;

        // Obtener los límites del collider de la caja
        Bounds boxBounds = GetComponent<Collider2D>().bounds;

        // Restringir la posición para que la caja no salga del área permitida
        newPosition.x = Mathf.Clamp(newPosition.x, 
            moveLimitBounds.min.x + boxBounds.extents.x, 
            moveLimitBounds.max.x - boxBounds.extents.x);
        newPosition.y = Mathf.Clamp(newPosition.y, 
            moveLimitBounds.min.y + boxBounds.extents.y, 
            moveLimitBounds.max.y - boxBounds.extents.y);

        // Aplicar la posición restringida
        transform.position = newPosition;

        // Opcional: Detener velocidad si la caja está en el borde
        if (Mathf.Abs(transform.position.x - moveLimitBounds.min.x) < 0.01f ||
            Mathf.Abs(transform.position.x - moveLimitBounds.max.x) < 0.01f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Detener movimiento horizontal
        }
        if (Mathf.Abs(transform.position.y - moveLimitBounds.min.y) < 0.01f ||
            Mathf.Abs(transform.position.y - moveLimitBounds.max.y) < 0.01f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); // Detener movimiento vertical
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reiniciar posición si toca un objeto con tag "Limit"
        if (collision.gameObject.CompareTag("Limit"))
        {
            rb.velocity = Vector2.zero; // Detener movimiento
            rb.angularVelocity = 0f; // Detener rotación
            transform.position = initialPosition; // Volver a posición inicial
            transform.rotation = initialRotation; // Volver a rotación inicial
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar entrada en GravityField
        if (other.CompareTag("GravityField"))
        {
            isInGravityField = true;
            GravityTilemap gravityField = other.GetComponent<GravityTilemap>();
            if (gravityField != null)
            {
                fieldGravityNormal = gravityField.isGravityNormal;
            }
            else
            {
                Debug.LogWarning("GravityField no tiene componente GravityTilemap.");
            }
        }

        // Detectar entrada en el área de movimiento
        if (other.CompareTag("MoveLimit"))
        {
            isInMoveLimit = true;
            moveLimitBounds = other.bounds; // Almacenar los límites del collider
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Salir de GravityField
        if (other.CompareTag("GravityField"))
        {
            isInGravityField = false;
        }

        // Salir del área de movimiento
        if (other.CompareTag("MoveLimit"))
        {
            isInMoveLimit = false;
            // Opcional: Reiniciar posición si sale del área
            rb.velocity = Vector2.zero;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }
}