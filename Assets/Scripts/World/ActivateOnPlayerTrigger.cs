using UnityEngine;

public class ActivateOnPlayerTrigger : MonoBehaviour
{
    [Header("Object to Activate")]
    [Tooltip("Assign the GameObject to activate when the player enters the trigger in the Inspector")]
    [SerializeField] private GameObject objectToActivate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el colisionador pertenece al jugador
        if (other.CompareTag("Player"))
        {
            // Activar el objeto especificado si no es nulo
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
            else
            {
                Debug.LogWarning("No GameObject assigned to activate in the Inspector!", this);
            }
        }
    }
}