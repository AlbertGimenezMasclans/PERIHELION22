using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Laser Settings")]
    [Tooltip("List of laser GameObjects to control")]
    [SerializeField] private GameObject[] lasers; // Lista de láseres
    [Tooltip("Time in seconds for each state switch")]
    [SerializeField] private float switchTime = 1.2f; // Tiempo en segundos para cada estado
    [Tooltip("Enable to use three-object mode (one deactivated, two activated)")]
    [SerializeField] private bool useThreeObjectMode = false; // Checkbox para modo de 3 objetos

    private float timer; // Temporizador para controlar el cambio
    private int currentLaserIndex; // Índice del láser actualmente desactivado (en modo 3 objetos) o activo (modo normal)

    void Start()
    {
        // Verificar que la lista de láseres no esté vacía
        if (lasers == null || lasers.Length == 0)
        {
            Debug.LogError("La lista de láseres está vacía o no asignada en el Inspector.", this);
            enabled = false; // Deshabilitar el componente
            return;
        }

        // Verificar que no haya elementos nulos en la lista
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null)
            {
                Debug.LogError($"El láser en la posición {i} de la lista está asignado como null.", this);
                enabled = false; // Deshabilitar el componente
                return;
            }
        }

        // En modo de tres objetos, verificar que haya al menos 3 láseres
        if (useThreeObjectMode && lasers.Length < 3)
        {
            Debug.LogError("El modo de tres objetos requiere al menos 3 láseres asignados.", this);
            enabled = false; // Deshabilitar el componente
            return;
        }

        // Inicializar el estado
        currentLaserIndex = 0;
        UpdateLaserStates();

        // Inicializar el temporizador
        timer = switchTime;
    }

    void Update()
    {
        // Reducir el temporizador con el tiempo transcurrido
        timer -= Time.deltaTime;

        // Cuando el temporizador llegue a 0, cambiar al siguiente estado
        if (timer <= 0f)
        {
            // Avanzar al siguiente índice (volver al inicio si se llega al final)
            currentLaserIndex = (currentLaserIndex + 1) % (useThreeObjectMode ? 3 : lasers.Length);

            // Actualizar el estado de los láseres
            UpdateLaserStates();

            // Reiniciar el temporizador
            timer = switchTime;
        }
    }

    private void UpdateLaserStates()
    {
        if (useThreeObjectMode)
        {
            // Modo de tres objetos: desactivar uno, mantener los otros dos activos
            for (int i = 0; i < 3; i++)
            {
                lasers[i].SetActive(i != currentLaserIndex);
            }
        }
        else
        {
            // Modo normal: activar solo el láser actual, desactivar los demás
            for (int i = 0; i < lasers.Length; i++)
            {
                lasers[i].SetActive(i == currentLaserIndex);
            }
        }
    }
}