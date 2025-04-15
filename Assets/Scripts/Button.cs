using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] private Sprite unpressedSprite; // Sprite cuando no está presionado
    [SerializeField] private Sprite pressedSprite; // Sprite cuando está presionado
    [SerializeField] private GameObject[] switchObjects; // Objetos a desactivar
    [SerializeField] private GameObject[] objectsToActivate; // Objetos a activar
    [SerializeField] private bool requiresBoxWeight = false; // Checkbox para requerir caja
    [SerializeField] private bool requiresBoxPresence = false; // Checkbox para requerir presencia continua de la caja
    [SerializeField] private bool activatesPlatform = false; // Checkbox para activar plataforma
    [SerializeField] private Platform platform; // Referencia a la plataforma a activar

    private SpriteRenderer buttonSprite; // Referencia al SpriteRenderer del botón
    private bool isPlayerOnButton; // Verifica si el jugador está encima
    private bool isBoxOnButton; // Verifica si una caja está encima
    private bool isPressed; // Estado del botón (completamente presionado)
    private GameObject boxOnButton; // Referencia a la caja que está encima

    private void Start()
    {
        buttonSprite = GetComponent<SpriteRenderer>();
        
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }
        
        isPressed = false;
        isPlayerOnButton = false;
        isBoxOnButton = false;

        // Inicializar switchObjects (activos por defecto)
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // Inicializar objectsToActivate (desactivados por defecto)
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = true;

            if (!requiresBoxWeight && !isPressed)
            {
                PressButtonAction(true);
            }
            else if (requiresBoxWeight && !isPressed)
            {
                ChangeSpriteToPressed();
            }
        }
        else if (other.CompareTag("PushableBox"))
        {
            isBoxOnButton = true;
            boxOnButton = other.gameObject;

            if (requiresBoxWeight && !isPlayerOnButton && (!isPressed || requiresBoxPresence))
            {
                PressButtonAction(true);
            }
            else if (!isPressed)
            {
                ChangeSpriteToPressed();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = false;

            if (requiresBoxWeight && isBoxOnButton && (!isPressed || requiresBoxPresence))
            {
                PressButtonAction(true);
            }
            else if (!isPressed && !isBoxOnButton)
            {
                ChangeSpriteToUnpressed();
            }
        }
        else if (other.CompareTag("PushableBox"))
        {
            isBoxOnButton = false;
            boxOnButton = null;

            if (requiresBoxPresence && isPressed)
            {
                UnpressButtonAction();
            }
            else if (!isPressed && !isPlayerOnButton)
            {
                ChangeSpriteToUnpressed();
            }
        }
    }

    private void Update()
    {
        // Si ambos checkboxes están activados y hay una caja encima
        if (requiresBoxWeight && requiresBoxPresence && isBoxOnButton && boxOnButton != null)
        {
            // Verificar si la caja está cerca del centro del botón
            Vector2 buttonCenter = (Vector2)transform.position;
            Vector2 boxCenter = boxOnButton.transform.position;
            float distance = Vector2.Distance(buttonCenter, boxCenter);

            if (distance < 0.1f) // Umbral para considerar que está centrada
            {
                // Bloquear la caja
                Rigidbody2D boxRb = boxOnButton.GetComponent<Rigidbody2D>();
                if (boxRb != null)
                {
                    boxRb.bodyType = RigidbodyType2D.Static; // Hacer la caja estática
                    boxOnButton.transform.position = buttonCenter; // Alinear exactamente al centro
                }
            }
        }
    }

    private void PressButtonAction(bool fullPress)
    {
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }

        if (fullPress)
        {
            isPressed = true;

            // Desactivar los switchObjects
            foreach (GameObject obj in switchObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            // Activar los objectsToActivate
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            // Activar la plataforma si el checkbox está marcado
            if (activatesPlatform && platform != null)
            {
                platform.ActivatePlatform();
            }
        }
    }

    private void UnpressButtonAction()
    {
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }

        isPressed = false;

        // Reactivar los switchObjects
        foreach (GameObject obj in switchObjects)
        {
            if (obj != null)
                {
                    obj.SetActive(true);
                }
        }

        // Desactivar los objectsToActivate
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Detener la plataforma si estaba activada
        if (activatesPlatform && platform != null)
        {
            platform.DeactivatePlatform();
        }
    }

    private void ChangeSpriteToPressed()
    {
        if (buttonSprite != null && pressedSprite != null)
        {
            buttonSprite.sprite = pressedSprite;
        }
    }

    private void ChangeSpriteToUnpressed()
    {
        if (buttonSprite != null && unpressedSprite != null)
        {
            buttonSprite.sprite = unpressedSprite;
        }
    }
}