using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private ActiveButton[] controlledButtons; // Lista de botones controlados por este controlador
    [SerializeField] private GameObject[] objectsToDeactivate; // Objetos a desactivar cuando se pulsan 3 botones
    [SerializeField] private float resetDelay = 0.5f; // Tiempo de retraso para reiniciar el contador

    private int triggerCount = 0; // Contador local de triggers
    private bool hasDeactivatedObjects = false; // Indica si los objetos ya fueron desactivados

    private void Start()
    {
        // Validar que controlledButtons no esté vacío
        if (controlledButtons == null || controlledButtons.Length == 0)
        {
            Debug.LogWarning("No se han asignado botones controlados en el ButtonController.");
            return;
        }

        // Validar que objectsToDeactivate no esté vacío
        if (objectsToDeactivate == null || objectsToDeactivate.Length == 0)
        {
            Debug.LogWarning("No se han asignado objetos a desactivar en el ButtonController.");
        }

        // Inicializar los botones controlados
        foreach (ActiveButton button in controlledButtons)
        {
            if (button != null)
            {
                button.SetController(this); // Asignar este controlador al botón
            }
        }
    }

    // Método para registrar la pulsación de un botón
    public void RegisterButtonPress()
    {
        triggerCount++;
        Debug.Log($"TriggerCount incrementado a {triggerCount} por un botón controlado.");

        // Actualizar sprites de todos los botones controlados
        UpdateAllButtonSprites();

        // Verificar si se alcanzó el conteo de 3
        if (triggerCount == 3 && !hasDeactivatedObjects)
        {
            DeactivateObjects();
            StartCoroutine(ResetTriggerCountAfterDelay());
        }
    }

    // Método para actualizar los sprites de todos los botones controlados
    private void UpdateAllButtonSprites()
    {
        foreach (ActiveButton button in controlledButtons)
        {
            if (button != null)
            {
                button.UpdateSpriteBasedOnCount(triggerCount);
            }
        }
    }

    // Método para desactivar los objetos
    private void DeactivateObjects()
    {
        hasDeactivatedObjects = true;
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Objeto {obj.name} desactivado porque TriggerCount alcanzó 3.");
            }
        }
    }

    // Corrutina para reiniciar el contador después de un retraso
    private System.Collections.IEnumerator ResetTriggerCountAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        triggerCount = 0;
        hasDeactivatedObjects = false;
        UpdateAllButtonSprites(); // Restaurar sprites base
        Debug.Log($"TriggerCount reiniciado a 0 después de {resetDelay} segundos.");
    }

    // Método para reiniciar el estado (útil para reiniciar el nivel)
    public void ResetController()
    {
        triggerCount = 0;
        hasDeactivatedObjects = false;
        UpdateAllButtonSprites();
        Debug.Log("ButtonController reiniciado.");
    }

    // Método para obtener el conteo actual (opcional, para depuración o UI)
    public int GetTriggerCount()
    {
        return triggerCount;
    }
}