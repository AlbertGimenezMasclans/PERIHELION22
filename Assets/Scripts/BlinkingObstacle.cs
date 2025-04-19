using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlinkingObstacle : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // GameObject al que se aplicará el efecto
    [SerializeField] private float visibleDuration = 0.2f; // Duración en segundos que el objeto está completamente visible
    [SerializeField] private float invisibleDuration = 0.2f; // Duración en segundos que el objeto está completamente invisible
    [SerializeField] private float longPauseMultiplier = 2.5f; // Multiplicador para la pausa larga (2x a 3x)

    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>(); // Lista de SpriteRenderers en los hijos
    private BoxCollider2D boxCollider; // Para activar/desactivar colisiones

    void Start()
    {
        // Validar que se haya asignado un GameObject
        if (targetObject == null)
        {
            Debug.LogError("No se ha asignado un GameObject en el campo Target Object.");
            return;
        }

        // Obtener todos los SpriteRenderers de los hijos
        spriteRenderers.AddRange(targetObject.GetComponentsInChildren<SpriteRenderer>());
        if (spriteRenderers.Count == 0)
        {
            Debug.LogError("El GameObject especificado no tiene SpriteRenderers en sus hijos.");
            return;
        }

        // Obtener el BoxCollider2D del GameObject especificado
        boxCollider = targetObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("El GameObject especificado no tiene BoxCollider2D.");
            return;
        }

        // Iniciar el ciclo de parpadeo
        StartCoroutine(BlinkCycle());
    }

    IEnumerator BlinkCycle()
    {
        while (true)
        {
            // Realizar 3 parpadeos rápidos
            for (int i = 0; i < 3; i++)
            {
                // Hacer visible
                SetVisibility(true);
                boxCollider.enabled = true; // Activar colisiones cuando es visible
                yield return new WaitForSeconds(visibleDuration);

                // Hacer invisible
                SetVisibility(false);
                boxCollider.enabled = false; // Desactivar colisiones cuando es invisible
                yield return new WaitForSeconds(invisibleDuration);
            }

            // Después del tercer parpadeo, hacer una pausa larga (invisible)
            SetVisibility(false);
            boxCollider.enabled = false;
            yield return new WaitForSeconds(invisibleDuration * longPauseMultiplier);
        }
    }

    private void SetVisibility(bool isVisible)
    {
        // Activar o desactivar todos los SpriteRenderers
        foreach (var renderer in spriteRenderers)
        {
            renderer.enabled = isVisible;
        }
    }
}