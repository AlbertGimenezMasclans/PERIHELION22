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
        PlayerPrefs.SetInt("GameLoaded", 1); // Marcar que hay una partida guardada
        PlayerPrefs.Save();
        Debug.Log($"Juego guardado en: {SavePath}");
    }

    public static void SaveDefaultGame()
    {
        GameData data = new GameData
        {
            playerPosition = new Vector2(0f, 0f), // Posición por defecto
            coinCount = 0,
            canChangeGravity = false,
            canShoot = false,
            canDismember = false
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        PlayerPrefs.SetInt("GameLoaded", 1); // Marcar que hay una partida guardada
        PlayerPrefs.Save();
        Debug.Log($"Juego por defecto guardado en: {SavePath}");
    }

    public static GameData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameData>(json);
        }
        Debug.LogWarning("No se encontró archivo de guardado.");
        return null;
    }

    public static bool HasSavedGame()
    {
        return File.Exists(SavePath);
    }
}