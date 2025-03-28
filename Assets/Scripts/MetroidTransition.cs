using UnityEngine;
using System.Collections;

public class MetroidTransition : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("The destination door where the player will appear")]
    public GameObject destinationDoor;

    [Header("Zone Management")]
    [Tooltip("Zone to activate after transition (destination)")]
    public GameObject zoneToActivate;
    [Tooltip("Zone to deactivate after transition (origin)")]
    public GameObject zoneToDeactivate;

    [Header("Fade Settings")]
    [Tooltip("UI Image for the fade effect")]
    public UnityEngine.UI.Image fadePanel;
    [Tooltip("Duration of the fade-in effect (seconds)")]
    public float fadeInTime = 0.45f;
    [Tooltip("Duration of the black screen (seconds)")]
    public float blackScreenDuration = 1.10f;
    [Tooltip("Duration of the fade-out effect (seconds)")]
    public float fadeOutTime = 0.45f;

    [Header("Player Movement")]
    [Tooltip("Speed at which the player walks after transition")]
    public float walkSpeedAfterTransition = 2f;
    [Tooltip("Direction the player walks after appearing (e.g., (1,0) for right)")]
    public Vector2 exitDirection = Vector2.right;
    [Tooltip("Time the player walks during fade-out (seconds)")]
    public float walkDuration = 0.5f;

    private GameObject player;
    private Rigidbody2D playerRb;
    private CameraController cameraController;
    private bool isTransitioning = false;

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null) Debug.LogError("CameraController not found on Main Camera.");
        if (destinationDoor == null) Debug.LogError("Destination Door not assigned.");
        if (fadePanel == null) Debug.LogError("Fade Panel not assigned.");
        else fadePanel.color = new Color(0, 0, 0, 0);

        if (zoneToActivate == null) Debug.LogWarning($"ZoneToActivate not assigned on {gameObject.name}.");
        if (zoneToDeactivate == null) Debug.LogWarning($"ZoneToDeactivate not assigned on {gameObject.name}.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isTransitioning)
        {
            player = collision.gameObject;
            playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb == null) Debug.LogError("Rigidbody2D not found on player.");
            else StartCoroutine(TransitionRoutine());
        }
    }

    private IEnumerator TransitionRoutine()
    {
        if (player == null || playerRb == null || cameraController == null || destinationDoor == null || fadePanel == null)
        {
            Debug.LogError("Required components missing for transition.");
            yield break;
        }

        isTransitioning = true;
        playerRb.velocity = Vector2.zero; // Detener al jugador al colisionar

        // Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 1f);

        // Teletransportar al jugador
        Vector3 destinationPos = destinationDoor.transform.position;
        destinationPos.y += 0.2f; // Ajuste de altura
        player.transform.position = new Vector3(destinationPos.x, destinationPos.y, player.transform.position.z);

        // Teletransportar cámara
        cameraController.TeleportCamera(new Vector2(destinationPos.x, destinationPos.y));

        // Gestionar zonas
        if (zoneToDeactivate != null)
        {
            zoneToDeactivate.SetActive(false);
            Debug.Log($"Zone deactivated: {zoneToDeactivate.name}");
        }
        if (zoneToActivate != null)
        {
            zoneToActivate.SetActive(true);
            Debug.Log($"Zone activated: {zoneToActivate.name}");
        }

        // Esperar pantalla en negro
        yield return new WaitForSeconds(blackScreenDuration);

        // Iniciar movimiento del jugador justo antes del Fade Out
        playerRb.velocity = exitDirection.normalized * walkSpeedAfterTransition;
        Debug.Log($"Player moving at velocity: {playerRb.velocity}");

        // Fade Out mientras el jugador se mueve
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0f);

        // Continuar movimiento hasta completar walkDuration
        float remainingWalkTime = Mathf.Max(0, walkDuration - fadeOutTime);
        if (remainingWalkTime > 0)
        {
            yield return new WaitForSeconds(remainingWalkTime);
        }

        // Detener al jugador
        playerRb.velocity = Vector2.zero;
        Debug.Log("Player stopped.");

        isTransitioning = false;
        Debug.Log("Transition completed.");
    }
}