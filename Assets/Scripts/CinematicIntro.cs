using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCinematic : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image pantallaNegra;
    [SerializeField] private TMP_Text textoHacker;

    [Header("Texto de Cinemática")]
    [SerializeField, TextArea(3, 10)] private string textoCinematica;
    [SerializeField] private float velocidadEscritura = 0.04f;
    [SerializeField] private float pausaComa = 0.25f;
    [SerializeField] private float pausaPunto = 0.45f;

    [Header("Sonido")]
    [SerializeField] private AudioClip sonidoEscritura;
    private AudioSource audioSource;

    [Header("Activación del Canvas")]
    [SerializeField] private GameObject canvasCinematica;

    private bool yaActivado = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        textoHacker.text = "";
        pantallaNegra.color = new Color(0, 0, 0, 1f); // Negra completa
        textoHacker.color = new Color(textoHacker.color.r, textoHacker.color.g, textoHacker.color.b, 0f); // Invisible

        if (canvasCinematica != null)
            canvasCinematica.SetActive(false); // Desactivado al inicio
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            yaActivado = true;

            if (canvasCinematica != null)
                canvasCinematica.SetActive(true); // Activar UI

            StartCoroutine(ReproducirCinematica());
        }
    }

    private IEnumerator ReproducirCinematica()
    {
        yield return new WaitForSeconds(1.25f); // Espera antes de empezar todo

        yield return StartCoroutine(FadeInTexto());

        yield return StartCoroutine(ReproducirTexto());

        yield return new WaitForSeconds(2f); // Espera antes de desaparecer texto

        yield return StartCoroutine(FadeOutTexto());

        yield return StartCoroutine(FadeOutPantallaNegra());

        pantallaNegra.gameObject.SetActive(false);
        gameObject.SetActive(false); // Fin de la cinemática
    }

    private IEnumerator FadeInTexto()
    {
        float duracion = 0.5f;
        float tiempo = 0f;

        Color colorInicial = new Color(textoHacker.color.r, textoHacker.color.g, textoHacker.color.b, 0f);
        Color colorFinal = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 1f);

        textoHacker.gameObject.SetActive(true);

        while (tiempo < duracion)
        {
            textoHacker.color = Color.Lerp(colorInicial, colorFinal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        textoHacker.color = colorFinal;
    }

    private IEnumerator ReproducirTexto()
    {
        textoHacker.text = "";
        int contadorSonido = 0;

        for (int i = 0; i < textoCinematica.Length; i++)
        {
            char c = textoCinematica[i];
            textoHacker.text += c;

            contadorSonido++;
            if (contadorSonido % 2 == 0 && sonidoEscritura != null)
                audioSource.PlayOneShot(sonidoEscritura);

            if (c == ',')
                yield return new WaitForSecondsRealtime(pausaComa);
            else if (c == '.' || c == ':')
                yield return new WaitForSecondsRealtime(pausaPunto);
            else
                yield return new WaitForSecondsRealtime(velocidadEscritura);
        }
    }

    private IEnumerator FadeOutTexto()
    {
        float duracion = 1f;
        float tiempo = 0f;

        Color colorInicial = textoHacker.color;
        Color colorFinal = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);

        while (tiempo < duracion)
        {
            textoHacker.color = Color.Lerp(colorInicial, colorFinal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        textoHacker.color = colorFinal;
        textoHacker.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutPantallaNegra()
    {
        float duracion = 2f;
        float tiempo = 0f;
        Color colorInicial = pantallaNegra.color;
        Color colorFinal = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);

        while (tiempo < duracion)
        {
            pantallaNegra.color = Color.Lerp(colorInicial, colorFinal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        pantallaNegra.color = colorFinal;
    }
}
