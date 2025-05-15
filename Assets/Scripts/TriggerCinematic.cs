using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TriggerCinematic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image blackScreen; // Imagen para el fade-in negro

    [Header("Configuración")]
    [SerializeField] private float fadeInDuration = 3.5f; // Duración del fade-in
    [SerializeField] private string targetScene = "Chapter1-Ending"; // Escena destino

    private bool isCinematicTriggered = false;

    private void Start()
    {
        // Verificar que la pantalla negra esté asignada
        if (blackScreen == null)
        {
            Debug.LogError("BlackScreen no está asignado en el Inspector.", this);
        }
        else
        {
            // Inicializar la pantalla negra como transparente
            blackScreen.color = new Color(0, 0, 0, 0f);
            blackScreen.gameObject.SetActive(true);
            Debug.Log("BlackScreen inicializado correctamente.", blackScreen);
        }
    }

    private IEnumerator PlayCinematic()
    {
        // Desactivar el movimiento del jugador (si existe)
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("Movimiento del jugador desactivado.", playerMovement);
        }
        else
        {
            Debug.LogWarning("PlayerMovement no encontrado en la escena.", this);
        }

        // Ejecutar fade-in de la pantalla negra
        if (blackScreen != null)
        {
            Debug.Log("Iniciando fade-in de la pantalla negra.", blackScreen);
            yield return StartCoroutine(FadeIn(blackScreen, fadeInDuration));
        }
        else
        {
            Debug.LogError("No se puede realizar el fade-in: BlackScreen es null.", this);
        }

        // Cargar la escena destino
        Debug.Log($"Cargando escena: {targetScene}", this);
        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator FadeIn(Image image, float duration)
    {
        if (image == null)
        {
            Debug.LogError("FadeIn no puede ejecutarse: la imagen es null.", this);
            yield break;
        }

        float timer = 0f;
        Color start = new Color(0, 0, 0, 0f); // Negro transparente
        Color end = new Color(0, 0, 0, 1f);   // Negro opaco
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Usar tiempo no escalado
            image.color = Color.Lerp(start, end, timer / duration);
            yield return null;
        }
        image.color = end;
        Debug.Log("Fade-in completado.", image);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCinematicTriggered)
        {
            isCinematicTriggered = true;
            Debug.Log("Jugador entró en el trigger. Iniciando cinemática.", collision);
            StartCoroutine(PlayCinematic());
        }
    }
}