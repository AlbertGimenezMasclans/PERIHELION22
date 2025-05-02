using UnityEngine;
using UnityEngine.Playables;

public class CinematicTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector timeline; // El Timeline que se reproducirá
    [SerializeField] private GameObject cinematicObject; // El GameObject que se activará
    [SerializeField] private bool playOnce = true; // ¿Se activa solo una vez?
    private bool hasBeenTriggered = false;

    private void Start()
    {
        // Asegurarse de que el cinematicObject esté desactivado al inicio, si está asignado
        if (cinematicObject != null)
        {
            cinematicObject.SetActive(false);
        }

        // Validar que el timeline esté asignado
        if (timeline == null)
        {
            Debug.LogError("No se ha asignado un PlayableDirector en el CinematicTrigger.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el collider pertenece al jugador
        if (collision.CompareTag("Player") && (!hasBeenTriggered || !playOnce))
        {
            // Marcar como activado si solo se ejecuta una vez
            if (playOnce) hasBeenTriggered = true;

            // Desactivar el control del jugador
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false; // Desactivar el script de movimiento
            }

            // Activar el cinematicObject (si está asignado) y reproducir el Timeline
            if (cinematicObject != null)
            {
                cinematicObject.SetActive(true);
            }

            if (timeline != null)
            {
                timeline.Play();
            }
            else
            {
                Debug.LogError("No se puede iniciar la cinemática: el PlayableDirector no está asignado.");
            }

            // Opcional: Desactivar el trigger si solo se usa una vez
            if (playOnce) DisableTrigger();
        }
    }

    // Método para reactivar el control del jugador (llamado desde el Timeline)
    public void EnablePlayerControl()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Desactivar el cinematicObject al finalizar, si está asignado
        if (cinematicObject != null)
        {
            cinematicObject.SetActive(false);
        }
    }

    // Desactivar el trigger después de usarlo
    private void DisableTrigger()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}