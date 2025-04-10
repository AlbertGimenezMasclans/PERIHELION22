using UnityEngine;

public class LaserController : MonoBehaviour
{
    [SerializeField] private GameObject[] lasers; // Lista de láseres
    [SerializeField] private float switchTime = 1.2f; // Tiempo en segundos para cada estado

    private float timer; // Temporizador para controlar el cambio
    private int currentLaserIndex; // Índice del láser actualmente activo

    void Start()
    {
        // Verificar que la lista de láseres no esté vacía
        if (lasers == null || lasers.Length == 0)
        {
            Debug.LogError("La lista de láseres está vacía o no asignada en el Inspector.");
            return;
        }

        // Verificar que no haya elementos nulos en la lista
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null)
            {
                Debug.LogError($"El láser en la posición {i} de la lista está asignado como null.");
                return;
            }
        }

        // Inicializar el estado: el primer láser activo, los demás desactivados
        currentLaserIndex = 0;
        for (int i = 0; i < lasers.Length; i++)
        {
            lasers[i].SetActive(i == currentLaserIndex);
        }

        // Inicializar el temporizador
        timer = switchTime;
    }

    void Update()
    {
        // Reducir el temporizador con el tiempo transcurrido
        timer -= Time.deltaTime;

        // Cuando el temporizador llegue a 0, cambiar al siguiente láser
        if (timer <= 0f)
        {
            // Desactivar el láser actual
            lasers[currentLaserIndex].SetActive(false);

            // Avanzar al siguiente láser en la lista (volver al inicio si se llega al final)
            currentLaserIndex = (currentLaserIndex + 1) % lasers.Length;

            // Activar el nuevo láser
            lasers[currentLaserIndex].SetActive(true);

            // Reiniciar el temporizador
            timer = switchTime;
        }
    }
}