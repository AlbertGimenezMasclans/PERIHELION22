using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // El cuerpo del jugador
    public Transform headTarget; // La cabeza del jugador
    public Transform habSelector;

    [SerializeField] private Vector3 offset;
    public float minX = float.MinValue;
    public float maxX = float.MaxValue;
    public float minY = float.MinValue;
    public float maxY = float.MaxValue;
    public float lookAheadFactor = 2f;
    public float lookAheadSpeed = 0.1f;
    // Eliminamos headOffsetY porque ya no se usa
    // public float headOffsetY = 1f; 

    private float initialMinX;
    private float initialMaxX;
    private float initialMinY;
    private float initialMaxY;
    private float lookAheadOffset = 0f;
    private PlayerMovement playerMovement;
    private PlayerDeath playerDeath;
    private Zone currentZone;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Por favor asigna un target (jugador) a la cámara.");
            return;
        }

        playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("El target no tiene el componente PlayerMovement.");
        }

        playerDeath = target.GetComponent<PlayerDeath>();
        if (playerDeath == null)
        {
            Debug.LogError("El target no tiene el componente PlayerDeath.");
        }

        offset = transform.position - target.position;

        initialMinX = minX;
        initialMaxX = maxX;
        initialMinY = minY;
        initialMaxY = maxY;
    }

    void LateUpdate()
    {
        if (target == null || (playerDeath != null && playerDeath.IsDead())) return;

        Vector2 targetPosition = target.position;
        Zone newZone = FindZoneAtPosition(targetPosition);

        if (newZone != currentZone)
        {
            currentZone = newZone;
            if (currentZone != null)
            {
                float newMinX, newMaxX, newMinY, newMaxY;
                currentZone.GetCameraLimits(out newMinX, out newMaxX, out newMinY, out newMaxY);
                minX = newMinX;
                maxX = newMaxX;
                minY = newMinY;
                maxY = newMaxY;
                Debug.Log($"Límites de la cámara actualizados según la zona ({currentZone.gameObject.name}): minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
            }
            else
            {
                minX = initialMinX;
                maxX = initialMaxX;
                minY = initialMinY;
                maxY = initialMaxY;
                Debug.Log($"No se encontró una zona en la posición {targetPosition}. Restaurando límites iniciales: minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
            }
        }

        // Determinar el objetivo actual (cuerpo o cabeza)
        Transform currentTarget = target;
        if (playerMovement != null && playerMovement.isDismembered && headTarget != null)
        {
            currentTarget = headTarget; // Seguir a la cabeza si el jugador está desmembrado
        }

        // Calcular el look-ahead basado en la velocidad del objetivo actual
        float targetVelocityX = currentTarget.GetComponent<Rigidbody2D>()?.velocity.x ?? 0f;
        float targetLookAhead = Mathf.Lerp(lookAheadOffset, targetVelocityX * lookAheadFactor, lookAheadSpeed);
        lookAheadOffset = targetLookAhead;

        // Posición deseada de la cámara: siempre sigue al objetivo actual con el offset
        Vector3 desiredPosition = new Vector3(
            currentTarget.position.x + offset.x + lookAheadOffset,
            currentTarget.position.y + offset.y,
            transform.position.z
        );

        // Aplicar los límites de la cámara
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        transform.position = desiredPosition;

        // Actualizar la posición del habSelector si existe
        if (habSelector != null)
        {
            habSelector.position = new Vector3(
                transform.position.x,
                transform.position.y,
                habSelector.position.z
            );
        }
    }

    public void TeleportCamera(Vector2 newPosition)
    {
        Vector3 cameraPosition = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        transform.position = cameraPosition;
        Debug.Log($"Cámara teletransportada a: {cameraPosition}");

        Zone newZone = FindZoneAtPosition(newPosition);
        if (newZone != null)
        {
            currentZone = newZone;
            float newMinX, newMaxX, newMinY, newMaxY;
            newZone.GetCameraLimits(out newMinX, out newMaxX, out newMinY, out newMaxY);
            minX = newMinX;
            maxX = newMaxX;
            minY = newMinY;
            maxY = newMaxY;
            Debug.Log($"Límites de la cámara actualizados después de teletransportar según la zona ({currentZone.gameObject.name}): minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
        }
        else
        {
            minX = initialMinX;
            maxX = initialMaxX;
            minY = initialMinY;
            maxY = initialMaxY;
            Debug.Log($"No se encontró una zona en la posición {newPosition}. Restaurando límites iniciales: minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
        }
    }

    public void UpdateCameraLimits(float newMinX, float newMaxX)
    {
        minX = newMinX;
        maxX = newMaxX;
        Debug.Log($"Nuevos límites de la cámara (solo X): minX = {minX}, maxX = {maxX}");
    }

    public void UpdateCameraLimits(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
        Debug.Log($"Nuevos límites de la cámara: minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
    }

    public bool IsPositionWithinLimits(Vector2 position)
    {
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    public void RestoreInitialLimits()
    {
        minX = initialMinX;
        maxX = initialMaxX;
        minY = initialMinY;
        maxY = initialMaxY;
        Debug.Log($"Límites de la cámara restaurados a los iniciales: minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
    }

    private Zone FindZoneAtPosition(Vector2 position)
    {
        Zone[] zones = FindObjectsOfType<Zone>();
        foreach (Zone zone in zones)
        {
            if (zone.ContainsPosition(position))
            {
                return zone;
            }
        }
        return null;
    }
}