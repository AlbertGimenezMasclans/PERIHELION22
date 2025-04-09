using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] private Sprite unpressedSprite; // Sprite cuando no está presionado
    [SerializeField] private Sprite pressedSprite; // Sprite cuando está presionado
    [SerializeField] private GameObject[] switchObjects; // Objetos a desactivar
    [SerializeField] private bool requiresBoxWeight = false; // Checkbox para requerir caja
    [SerializeField] private bool requiresBoxPresence = false; // Checkbox para requerir presencia continua de la caja

    private SpriteRenderer buttonSprite; // Referencia al SpriteRenderer del botón
    private bool isPlayerOnButton; // Verifica si el jugador está encima
    private bool isBoxOnButton; // Verifica si una caja está encima
    private bool isPressed; // Estado del botón (completamente presionado)

    private void Start()
    {
        // Obtener el SpriteRenderer del botón
        buttonSprite = GetComponent<SpriteRenderer>();
        
        // Asegurarse de que el sprite inicial sea el "sin presionar"
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }
        
        isPressed = false;
        isPlayerOnButton = false;
        isBoxOnButton = false;

        // Asegurarse de que los switchObjects estén activos al inicio
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = true;

            // Si no requiere caja, presionar completamente con el jugador
            if (!requiresBoxWeight && !isPressed)
            {
                PressButtonAction(true);
            }
            // Si requiere caja, solo cambiar el sprite sin activar la acción
            else if (requiresBoxWeight && !isPressed)
            {
                ChangeSpriteToPressed();
            }
        }
        // Verificar si es una caja
        else if (other.CompareTag("Box"))
        {
            isBoxOnButton = true;

            // Si requiere caja y no hay jugador, presionar completamente
            if (requiresBoxWeight && !isPlayerOnButton && (!isPressed || requiresBoxPresence))
            {
                PressButtonAction(true);
            }
            // Si no requiere caja o hay jugador, solo cambiar sprite si no está presionado
            else if (!isPressed)
            {
                ChangeSpriteToPressed();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = false;

            // Si requiere caja y hay una caja encima, presionar completamente
            if (requiresBoxWeight && isBoxOnButton && (!isPressed || requiresBoxPresence))
            {
                PressButtonAction(true);
            }
            // Si no está presionado y no hay caja, volver al sprite sin presionar
            else if (!isPressed && !isBoxOnButton)
            {
                ChangeSpriteToUnpressed();
            }
        }
        // Verificar si es una caja
        else if (other.CompareTag("Box"))
        {
            isBoxOnButton = false;

            // Si requiere presencia continua de la caja, despresionar el botón
            if (requiresBoxPresence && isPressed)
            {
                UnpressButtonAction();
            }
            // Si no está presionado y no hay jugador, volver al sprite sin presionar
            else if (!isPressed && !isPlayerOnButton)
            {
                ChangeSpriteToUnpressed();
            }
        }
    }

    private void PressButtonAction(bool fullPress)
    {
        // Cambiar al sprite de "presionado"
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }

        if (fullPress)
        {
            // Marcar como presionado
            isPressed = true;

            // Desactivar los objetos del switch
            foreach (GameObject obj in switchObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void UnpressButtonAction()
    {
        // Volver al sprite "sin presionar"
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }

        // Marcar como no presionado
        isPressed = false;

        // Reactivar los objetos del switch
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void ChangeSpriteToPressed()
    {
        // Solo cambiar el sprite a "presionado" sin activar la acción completa
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }
    }

    private void ChangeSpriteToUnpressed()
    {
        // Volver al sprite "sin presionar"
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }
    }
}