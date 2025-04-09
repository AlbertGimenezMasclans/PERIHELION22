using UnityEngine;

public class BoxPushable : MonoBehaviour
{
    [Header("Box Properties")]
    public float weight = 1f; // Peso de la caja (mass)
    public bool isGravityNormal = true; // Gravedad normal o invertida
    public float pushSpeedMultiplier = 0.8f; // Multiplicador de velocidad al empujar (menor que 1 para que sea más lento)

    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    private float playerPushSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = weight; // Asignar el peso
        rb.gravityScale = isGravityNormal ? 1f : -1f; // Configurar la gravedad inicial
    }

    void FixedUpdate()
    {
        if (isBeingPushed)
        {
            // Asegurarse de que la caja solo se mueva horizontalmente
            rb.velocity = new Vector2(playerPushSpeed, rb.velocity.y);
        }
    }

    public void StartPushing(float playerSpeed, float direction)
    {
        isBeingPushed = true;
        playerPushSpeed = playerSpeed * pushSpeedMultiplier * direction;
    }

    public void StopPushing()
    {
        isBeingPushed = false;
        rb.velocity = new Vector2(0f, rb.velocity.y); // Detener el movimiento horizontal
    }

    public bool IsGravityNormal()
    {
        return isGravityNormal;
    }
}