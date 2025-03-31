using UnityEngine;

public class Zone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Minimum X limit for the camera in this zone")]
    public float minXLimit = -10f;
    [Tooltip("Maximum X limit for the camera in this zone")]
    public float maxXLimit = 10f;
    [Tooltip("Minimum Y limit for the camera in this zone")]
    public float minYLimit = -10f; // Nuevo límite mínimo en Y
    [Tooltip("Maximum Y limit for the camera in this zone")]
    public float maxYLimit = 10f;  // Nuevo límite máximo en Y

    private BoxCollider2D zoneCollider;

    void Awake()
    {
        // Asegurarse de que la zona tenga un BoxCollider2D
        zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<BoxCollider2D>();
            zoneCollider.isTrigger = true; // Hacer que sea un trigger
        }
    }

    // Método para verificar si una posición está dentro de la zona
    public bool ContainsPosition(Vector2 position)
    {
        if (zoneCollider != null)
        {
            return zoneCollider.OverlapPoint(position);
        }
        return false;
    }

    // Método actualizado para obtener los límites de la cámara en X e Y
    public void GetCameraLimits(out float minX, out float maxX, out float minY, out float maxY)
    {
        minX = minXLimit;
        maxX = maxXLimit;
        minY = minYLimit;
        maxY = maxYLimit;
        Debug.Log($"Zone ({gameObject.name}): Estableciendo límites de cámara: minX = {minX}, maxX = {maxX}, minY = {minY}, maxY = {maxY}");
    }
}