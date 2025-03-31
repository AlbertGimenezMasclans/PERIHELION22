using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class GameOver : MonoBehaviour
{
    public Image blackPanel;    // Referencia al panel negro en la UI
    private float fadeDuration = 1f; // Duración del fade en segundos
    private float fadeTimer = 0f;    // Contador de tiempo
    private bool fadeCompleted = false; // Para saber si el fade terminó

    void Start()
    {
        // Asegurarse de que el panel empiece completamente opaco
        if (blackPanel != null)
        {
            Color startColor = blackPanel.color;
            startColor.a = 1f;
            blackPanel.color = startColor;
        }
    }

    void Update()
    {
        // Manejar el fade-out
        if (blackPanel != null && fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));
            Color currentColor = blackPanel.color;
            currentColor.a = alpha;
            blackPanel.color = currentColor;
            
            // Marcar cuando el fade se completa
            if (fadeTimer >= fadeDuration)
            {
                fadeCompleted = true;
            }
        }

        // Verificar si se presiona C después de completar el fade
        if (fadeCompleted && Input.GetKeyDown(KeyCode.C))
        {
            SceneManager.LoadScene("TestZone");
        }
    }
}