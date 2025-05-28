using UnityEngine;
using System.IO;

[System.Serializable]
public class GameData
{
    public Vector2 playerPosition; // Posición del jugador
    public int coinCount; // Número de monedas
    public bool canChangeGravity; // Habilidad de cambio de gravedad
    public bool canShoot; // Habilidad de disparo
    public bool canDismember; // Habilidad de desmembramiento
}

public class SaveSystem : MonoBehaviour
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    // Método para obtener los datos por defecto
    private static GameData GetDefaultGameData()
    {
        return new GameData
        {
            playerPosition = new Vector2(0f, 0f),
            coinCount = 0,
            canChangeGravity = false,
            canShoot = false,
            canDismember = false
        };
    }

    // Método para comparar si los datos guardados son los datos por defecto
    private static bool IsDefaultGameData(GameData data)
    {
        GameData defaultData = GetDefaultGameData();
        return data.playerPosition == defaultData.playerPosition &&
               data.coinCount == defaultData.coinCount &&
               data.canChangeGravity == defaultData.canChangeGravity &&
               data.canShoot == defaultData.canShoot &&
               data.canDismember == defaultData.canDismember;
    }

    public static void SaveGame(PlayerMovement player, CoinControllerUI coinController)
    {
        GameData data = new GameData
        {
            playerPosition = player.transform.position,
            coinCount = KredsManager.Instance != null ? KredsManager.Instance.totalTokens : 0,
            canChangeGravity = player.canChangeGravity,
            canShoot = player.canShoot,
            canDismember = player.canDismember
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        // Activar GameLoaded siempre que se guarde una partida, incluso si es por defecto inicialmente
        PlayerPrefs.SetInt("GameLoaded", 1);
        PlayerPrefs.Save();
        Debug.Log($"Juego guardado en: {SavePath}");
    }

    public static void SaveDefaultGame()
    {
        GameData data = GetDefaultGameData();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        // Los datos son los datos por defecto, desactivar GameLoaded
        PlayerPrefs.SetInt("GameLoaded", 0);
        PlayerPrefs.Save();
        Debug.Log($"Juego por defecto guardado en: {SavePath}");
        Debug.Log("GameLoaded desactivado: Se guardaron los datos por defecto.");
    }

    public static GameData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Verificar si los datos cargados son los datos por defecto
            if (IsDefaultGameData(data))
            {
                PlayerPrefs.SetInt("GameLoaded", 0);
                PlayerPrefs.Save();
                Debug.Log("GameLoaded desactivado: Los datos cargados son los datos por defecto.");
            }
            else
            {
                PlayerPrefs.SetInt("GameLoaded", 1);
                PlayerPrefs.Save();
                Debug.Log("GameLoaded activado: Los datos cargados no son los datos por defecto.");
            }

            return data;
        }
        Debug.LogWarning("No se encontró archivo de guardado.");
        PlayerPrefs.SetInt("GameLoaded", 0); // No hay partida guardada, desactivar GameLoaded
        PlayerPrefs.Save();
        return null;
    }

    public static bool HasSavedGame()
    {
        return File.Exists(SavePath);
    }
}