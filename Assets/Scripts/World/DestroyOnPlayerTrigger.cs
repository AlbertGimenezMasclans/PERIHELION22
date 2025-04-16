using UnityEngine;

public class DestroyOnPlayerTrigger : MonoBehaviour
{
    [Header("Object to Destroy")]
    [Tooltip("The GameObject to destroy when the player enters the trigger")]
    [SerializeField] private GameObject objectToDestroy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el colisionador pertenece al jugador
        if (other.CompareTag("Player"))
        {
            // Destruir el objeto especificado si no es nulo
            if (objectToDestroy != null)
            {
                Destroy(objectToDestroy);
            }
        }
    }
}