using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ZoneTransition : MonoBehaviour
{
    [Header("Teleport Destinations")]
    public GameObject normalGravityDestination;
    public GameObject invertedGravityDestination;

    [Header("Zone Management")]
    public GameObject zoneToActivate;
    public GameObject zoneToDeactivate;

    [Header("Fade Settings")]
    public Image fadePanel;
    public float fadeTime = 0.65f;
    public float blackScreenDuration = 0.2f;

    [Header("Head Offset Settings")]
    public MoveDirection headOffsetDirection = MoveDirection.Right;
    public float headOffsetDistance = 0.25f;

    [Header("Gravity Settings")]
    public GravityMode gravityAfterTeleport = GravityMode.KeepCurrent;

    public enum MoveDirection { Left, Right }
    public enum GravityMode { KeepCurrent, Normal, Inverted }

    private GameObject player;
    private PlayerMovement playerMovement;
    private CameraController cameraController;
    private bool isTransitioning = false;

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        if (fadePanel != null) fadePanel.color = Color.clear;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTransitioning)
        {
            if (collision.CompareTag("PlayerHead"))
            {
                player = collision.gameObject;
                StartCoroutine(TransitionHeadAndBody());
                return;
            }

            if (collision.CompareTag("Player") && collision.gameObject.GetComponent<Dismember>() == null)
            {
                player = collision.gameObject;
                playerMovement = player.GetComponent<PlayerMovement>();

                if (playerMovement != null && !player.GetComponent<PlayerDeath>().IsDead())
                {
                    StartCoroutine(TransitionToNewZone());
                }
            }
        }
    }

    private IEnumerator TransitionHeadAndBody()
    {
        isTransitioning = true;
        Rigidbody2D headRb = player.GetComponent<Rigidbody2D>();
        headRb.velocity = Vector2.zero;

        yield return StartCoroutine(FadeScreen(0f, 1f, fadeTime));
        yield return new WaitForSeconds(blackScreenDuration);

        // Revisamos si la gravedad está invertida o no y asignamos la posición adecuada
        Vector3 targetPosition = playerMovement.IsGravityNormal() ? 
            (normalGravityDestination != null ? normalGravityDestination.transform.position : transform.position) :
            (invertedGravityDestination != null ? invertedGravityDestination.transform.position : transform.position);

        // Aplicamos el cambio de gravedad si corresponde
        ApplyGravityChange();

        PlayerMovement fullPlayerMovement = FindFullPlayer();
        if (fullPlayerMovement != null)
        {
            playerMovement = fullPlayerMovement;
            GameObject bodyObject = playerMovement.bodyObject;
            if (bodyObject != null)
            {
                bodyObject.transform.position = targetPosition;
            }
            else
            {
                Debug.LogError("bodyObject no está asignado en PlayerMovement.");
            }

            // Aquí gestionamos la dirección de la cabeza, según la gravedad y la dirección del movimiento
            float offsetDirection = headOffsetDirection == MoveDirection.Right ? 1f : -1f;
            Vector3 headPosition = targetPosition + new Vector3(offsetDirection * headOffsetDistance, 0f, 0f);
            player.transform.position = headPosition;
        }
        else
        {
            Debug.LogError("No se encontró el PlayerMovement del jugador completo.");
        }

        if (cameraController != null && playerMovement != null && playerMovement.bodyObject != null)
        {
            cameraController.TeleportCamera(targetPosition);
        }

        if (zoneToDeactivate != null) zoneToDeactivate.SetActive(false);
        if (zoneToActivate != null) zoneToActivate.SetActive(true);

        yield return StartCoroutine(FadeScreen(1f, 0f, fadeTime));

        isTransitioning = false;
    }

    private IEnumerator TransitionToNewZone()
    {
        isTransitioning = true;
        playerMovement.SetMovementLocked(true);
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return StartCoroutine(FadeScreen(0f, 1f, fadeTime));
        yield return new WaitForSeconds(blackScreenDuration);

        // Comprobamos si la gravedad está normal o invertida y asignamos la posición adecuada
        Vector3 targetPosition = playerMovement.IsGravityNormal() ?
            (normalGravityDestination != null ? normalGravityDestination.transform.position : transform.position) :
            (invertedGravityDestination != null ? invertedGravityDestination.transform.position : transform.position);

        player.transform.position = targetPosition;
        ApplyGravityChange();

        if (cameraController != null)
            cameraController.TeleportCamera(targetPosition);

        if (zoneToDeactivate != null) zoneToDeactivate.SetActive(false);
        if (zoneToActivate != null) zoneToActivate.SetActive(true);

        yield return StartCoroutine(FadeScreen(1f, 0f, fadeTime));

        playerMovement.SetMovementLocked(false);
        isTransitioning = false;
    }

    private void ApplyGravityChange()
    {
        switch (gravityAfterTeleport)
        {
            case GravityMode.Normal:
                // Si la gravedad no es normal, la cambiamos
                if (!playerMovement.IsGravityNormal())
                    playerMovement.ChangeGravity(); // Cambia la gravedad
                break;
            case GravityMode.Inverted:
                // Si la gravedad es normal, la cambiamos a invertida
                if (playerMovement.IsGravityNormal())
                    playerMovement.ChangeGravity(); // Cambia la gravedad
                break;
        }
    }

    private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, endAlpha);
    }

    private PlayerMovement FindFullPlayer()
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in players)
        {
            if (pm.isDismembered)
            {
                return pm;
            }
        }
        return null;
    }
}
