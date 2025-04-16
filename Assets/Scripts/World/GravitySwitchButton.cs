using UnityEngine;
using System.Collections.Generic;

public class GravitySwitchButton : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("Tilemap for normal gravity (activated by default)")]
    [SerializeField] private GameObject normalGravityTilemap;
    [Tooltip("Tilemap for inverted gravity")]
    [SerializeField] private GameObject invertedGravityTilemap;

    [Header("Object Activation Settings")]
    [Tooltip("Objects to activate when button is pressed (if deactivated at start)")]
    [SerializeField] private GameObject[] objectsToActivate;
    [Tooltip("Objects to toggle (switch) when button is pressed")]
    [SerializeField] private GameObject[] switchObjects;

    [Header("Indicator Settings")]
    [Tooltip("Indicator object shown when player is in trigger")]
    [SerializeField] private GameObject interactionIndicator;

    [Header("Sprite Settings")]
    [Tooltip("Sprite for normal gravity state")]
    [SerializeField] private Sprite normalSprite;
    [Tooltip("Sprite for inverted gravity state")]
    [SerializeField] private Sprite invertedSprite;

    [Header("Sound Settings")]
    [Tooltip("Sound played when button is activated")]
    [SerializeField] private AudioClip activationSound;

    private bool isPlayerInside; // Rastrear si el jugador está dentro del trigger
    private static bool isNormalGravityActive = true; // Estado global de la gravedad (estático)
    private PlayerMovement player; // Referencia al componente PlayerMovement
    private SpriteRenderer spriteRenderer; // Referencia al SpriteRenderer del botón
    private AudioSource audioSource; // Componente para reproducir el sonido

    // Diccionario para almacenar el estado inicial de objectsToActivate
    private Dictionary<GameObject, bool> initialActiveStateActivate;

    private void Awake()
    {
        // Inicializar el diccionario
        initialActiveStateActivate = new Dictionary<GameObject, bool>();

        // Guardar el estado inicial de objectsToActivate
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                initialActiveStateActivate[obj] = obj.activeSelf;
            }
        }
    }

    private void Start()
    {
        // Obtener el SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Obtener o añadir el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Asegurar el estado inicial: normal activado, invertido desactivado
        if (normalGravityTilemap != null)
        {
            normalGravityTilemap.SetActive(isNormalGravityActive);
        }
        if (invertedGravityTilemap != null)
        {
            invertedGravityTilemap.SetActive(!isNormalGravityActive);
        }

        // Asegurar que el indicador esté desactivado al inicio
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }

        // Establecer el sprite inicial
        UpdateSprite();

        isPlayerInside = false;
    }

    private void Update()
    {
        // Si el jugador está dentro del trigger, tocando el suelo y pulsa C, alternar Tilemaps
        if (isPlayerInside && player != null && player.IsGrounded() && Input.GetKeyDown(KeyCode.C))
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

            // Activar objetos que estaban desactivados al inicio
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null && initialActiveStateActivate.ContainsKey(obj) && !initialActiveStateActivate[obj])
                {
                    obj.SetActive(true);
                }
            }

            // Alternar el estado de los switchObjects
            foreach (GameObject obj in switchObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }

            // Reproducir el sonido de activación
            if (audioSource != null && activationSound != null)
            {
                audioSource.PlayOneShot(activationSound);
            }

            // Actualizar el sprite de todos los botones
            UpdateAllButtonsSprites();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();
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
            player = null;
            // Desactivar el indicador
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(false);
            }
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isNormalGravityActive && normalSprite != null ? normalSprite : invertedSprite;
        }
    }

    private void UpdateAllButtonsSprites()
    {
        // Encontrar todas las instancias de GravitySwitchButton en la escena
        GravitySwitchButton[] allButtons = FindObjectsOfType<GravitySwitchButton>();
        foreach (GravitySwitchButton button in allButtons)
        {
            button.UpdateSprite();
        }
    }
}