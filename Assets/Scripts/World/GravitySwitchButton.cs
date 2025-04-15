using UnityEngine;

public class GravitySwitchButton : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("Tilemap for normal gravity (activated by default)")]
    [SerializeField] private GameObject normalGravityTilemap;
    [Tooltip("Tilemap for inverted gravity")]
    [SerializeField] private GameObject invertedGravityTilemap;

    [Header("Indicator Settings")]
    [Tooltip("Indicator object shown when player is in trigger")]
    [SerializeField] private GameObject interactionIndicator;

    private bool isPlayerInside; // Rastrear si el jugador está dentro del trigger
    private bool isNormalGravityActive = true; // Estado actual (normal por defecto)

    private void Start()
    {
        // Asegurar el estado inicial: normal activado, invertido desactivado
        if (normalGravityTilemap != null)
        {
            normalGravityTilemap.SetActive(true);
        }
        if (invertedGravityTilemap != null)
        {
            invertedGravityTilemap.SetActive(false);
        }

        // Asegurar que el indicador esté desactivado al inicio
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        // Si el jugador está dentro del trigger y pulsa C, alternar Tilemaps
        if (isPlayerInside && Input.GetKeyDown(KeyCode.C))
        {
            isNormalGravityActive = !isNormalGravityActive;
            if (normalGravityTilemap != null)
            {
                normalGravityTilemap.SetActive(isNormalGravityActive);
            }
            if (invertedGravityTilemap != null)
            {
                invertedGravityTilemap.SetActive(!isNormalGravityActive);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Activar el indicador
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // Desactivar el indicador
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(false);
            }
        }
    }
}