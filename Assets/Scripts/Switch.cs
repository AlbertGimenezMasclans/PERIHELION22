using UnityEngine;

public class LeverIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicator; // El GameObject que actúa como indicador
    private SpriteRenderer leverSprite; // El SpriteRenderer de la palanca (este objeto)
    private bool isPlayerNearby; // Para saber si el jugador está dentro del trigger
    private bool isUsed; // Para rastrear si la palanca ya fue usada

    [Header("Switch Trigger")]
    [SerializeField] private GameObject[] switchObjects; // Lista de GameObjects que se desactivarán

    [Header("Flip Options")]
    [SerializeField] private bool flipOnXAxis = true; // Checkbox para girar en el eje X
    [SerializeField] private bool flipOnYAxis = false; // Checkbox para girar en el eje Y

    private void Start()
    {
        // Asegurarnos de que el indicador esté desactivado al inicio
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
        // Obtener el SpriteRenderer de este objeto (la palanca)
        leverSprite = GetComponent<SpriteRenderer>();
        isUsed = false; // Inicializamos la palanca como no usada
    }

    private void Update()
    {
        // Si el jugador está cerca, la palanca no ha sido usada y presiona la tecla "C"
        if (isPlayerNearby && !isUsed && Input.GetKeyDown(KeyCode.C))
        {
            // Cambiar el Flip del sprite de la palanca según las opciones seleccionadas
            if (leverSprite != null)
            {
                if (flipOnXAxis)
                {
                    leverSprite.flipX = !leverSprite.flipX; // Girar en el eje X
                }
                if (flipOnYAxis)
                {
                    leverSprite.flipY = !leverSprite.flipY; // Girar en el eje Y
                }
            }
            // Marcar la palanca como usada
            isUsed = true;
            // Desactivar el indicador permanentemente
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
            // Desactivar los objetos especificados
            foreach (GameObject obj in switchObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el que entra es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Activar el indicador solo si la palanca no ha sido usada
            if (indicator != null && !isUsed)
            {
                indicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verificar si el que sale es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // Desactivar el indicador solo si no ha sido usada
            if (indicator != null && !isUsed)
            {
                indicator.SetActive(false);
            }
        }
    }
}