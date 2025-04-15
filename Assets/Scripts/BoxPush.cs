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
    private Vector2 initialPosition; // Nuevo: Almacenar posición inicial
    private Quaternion initialRotation; // Nuevo: Almacenar rotación inicial
    private bool isInGravityField = false; // Nuevo: Rastrear si está en GravityField
    private bool fieldGravityNormal = true; // Nuevo: Almacenar la gravedad del campo

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
            // Si está en un GravityField, usar la gravedad del campo
            rb.gravityScale = fieldGravityNormal ? normalGravityScale : -normalGravityScale;
        }
        else
        {
            // Si no, usar la gravedad del checkbox
            rb.gravityScale = inverseGravity ? -normalGravityScale : normalGravityScale;
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
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Salir de GravityField
        if (other.CompareTag("GravityField"))
        {
            isInGravityField = false;
        }
    }
}