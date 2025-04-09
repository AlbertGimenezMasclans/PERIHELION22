using UnityEngine;

public class PressButton : MonoBehaviour
{
    [SerializeField] private Sprite unpressedSprite; // Sprite cuando no está presionado
    [SerializeField] private Sprite pressedSprite; // Sprite cuando está presionado
    [SerializeField] private GameObject[] switchObjects; // Objetos a desactivar
    
    private SpriteRenderer buttonSprite; // Referencia al SpriteRenderer del botón
    private bool isPlayerOnButton; // Verifica si el jugador está encima
    private bool isPressed; // Estado del botón

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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = true;
            
            // Presionar el botón automáticamente al entrar
            if (!isPressed)
            {
                PressButtonAction();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = false;
        }
    }

    private void PressButtonAction()
    {
        // Cambiar al sprite de "presionado"
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }
        
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