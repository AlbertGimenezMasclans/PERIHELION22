using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CoinControllerUI coinController;

    void Start()
    {
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement no está asignado en GameLoader.");
            return;
        }
        if (coinController == null)
        {
            Debug.LogError("CoinControllerUI no está asignado en GameLoader.");
            return;
        }

        // Cargar datos del JSON
        GameData data = SaveSystem.LoadGame();
        if (data != null)
        {
            // Aplicar posición del jugador
            playerMovement.transform.position = data.playerPosition;
            Debug.Log($"Cargada posición del jugador: {data.playerPosition}");

            // Aplicar monedas
            if (KredsManager.Instance != null)
            {
                KredsManager.Instance.totalTokens = data.coinCount;
                KredsManager.Instance.displayedTokens = data.coinCount;
                KredsManager.Instance.UpdateHUD();
                Debug.Log($"Cargadas {data.coinCount} monedas.");

                // Forzar la HUD a la posición oculta
                if (KredsManager.Instance.uiContainer != null)
                {
                    KredsManager.Instance.uiContainer.anchoredPosition = KredsManager.Instance.hiddenUIPosition;
                    Debug.Log("HUD movida a posición oculta.");
                }
                else
                {
                    Debug.LogWarning("uiContainer no asignado en KredsManager.");
                }
            }
            else
            {
                Debug.LogWarning("KredsManager.Instance no encontrado al cargar monedas.");
            }

            // Aplicar habilidades
            playerMovement.canChangeGravity = data.canChangeGravity;
            playerMovement.canShoot = data.canShoot;
            playerMovement.canDismember = data.canDismember;
            Debug.Log($"Habilidades cargadas: Gravity={data.canChangeGravity}, Shoot={data.canShoot}, Dismember={data.canDismember}");

            Debug.Log("Datos del juego cargados desde JSON.");
        }
        else
        {
            Debug.LogWarning("No se pudo cargar el juego. Usando valores por defecto.");
        }
    }
}