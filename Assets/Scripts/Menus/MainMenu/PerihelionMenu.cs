using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PerihelionMenu : MonoBehaviour
{
    private enum MenuOption
    {
        StartGame,
        LoadGame,
        Options,
        Exit
    }

    private enum SubMenuOption
    {
        Music,
        SoundEffects,
        AutoSave,
        Exit
    }

    private MenuOption currentOption = MenuOption.StartGame;
    private SubMenuOption currentSubOption = SubMenuOption.Exit;

    [Header("Main Menu Indicators")]
    [SerializeField] private GameObject[] startGameIndicators;
    [SerializeField] private GameObject[] loadGameIndicators;
    [SerializeField] private GameObject[] optionsIndicators;
    [SerializeField] private GameObject[] exitIndicators;
    [SerializeField] private TMP_Text loadGameText; // Referencia al texto de Load Game

    [Header("Sub Menu Indicators")]
    [SerializeField] private GameObject[] musicIndicators;
    [SerializeField] private GameObject[] soundEffectsIndicators;
    [SerializeField] private GameObject[] autoSaveIndicators;
    [SerializeField] private GameObject[] subExitIndicators;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip noActionSound;

    [Header("UI Elements")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject fadePanel;
    [Space]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Toggle autoSaveToggle;
    [SerializeField] private bool gameLoaded = false;

    private bool isProcessing = false;
    private bool inSubMenu = false;
    private float sliderAdjustSpeed = 0.5f;

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.value = 1f;
            UpdateVolumeText(musicSlider, musicVolumeText);
            musicSlider.onValueChanged.AddListener((value) => UpdateVolumeText(musicSlider, musicVolumeText));
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
            UpdateVolumeText(sfxSlider, sfxVolumeText);
            sfxSlider.onValueChanged.AddListener((value) => UpdateVolumeText(sfxSlider, sfxVolumeText));
        }

        UpdateMenuVisuals();
        UpdateLoadGameTextColor(); // Actualizar color al iniciar
        if (fadePanel != null)
        {
            fadePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {
        if (isProcessing) return;

        if (inSubMenu)
        {
            if (currentSubOption == SubMenuOption.Music && musicSlider != null)
            {
                AdjustSlider(musicSlider);
            }
            if (currentSubOption == SubMenuOption.SoundEffects && sfxSlider != null)
            {
                AdjustSlider(sfxSlider);
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.C))
            {
                CloseOptionsMenu();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveUp();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveDown();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                ProcessMainMenuSelection();
            }
        }
    }

    private void MoveUp()
    {
        switch (currentOption)
        {
            case MenuOption.StartGame:
                break;
            case MenuOption.LoadGame:
                currentOption = MenuOption.StartGame;
                break;
            case MenuOption.Options:
                currentOption = MenuOption.LoadGame;
                break;
            case MenuOption.Exit:
                currentOption = MenuOption.Options;
                break;
        }
        PlaySelectionSound();
        UpdateMenuVisuals();
    }

    private void MoveDown()
    {
        switch (currentOption)
        {
            case MenuOption.StartGame:
                currentOption = MenuOption.LoadGame;
                break;
            case MenuOption.LoadGame:
                currentOption = MenuOption.Options;
                break;
            case MenuOption.Options:
                currentOption = MenuOption.Exit;
                break;
            case MenuOption.Exit:
                break;
        }
        PlaySelectionSound();
        UpdateMenuVisuals();
    }

    private void UpdateMenuVisuals()
    {
        SetIndicatorsActive(startGameIndicators, false);
        SetIndicatorsActive(loadGameIndicators, false);
        SetIndicatorsActive(optionsIndicators, false);
        SetIndicatorsActive(exitIndicators, false);

        switch (currentOption)
        {
            case MenuOption.StartGame:
                SetIndicatorsActive(startGameIndicators, true);
                break;
            case MenuOption.LoadGame:
                SetIndicatorsActive(loadGameIndicators, true);
                break;
            case MenuOption.Options:
                SetIndicatorsActive(optionsIndicators, true);
                break;
            case MenuOption.Exit:
                SetIndicatorsActive(exitIndicators, true);
                break;
        }
        UpdateLoadGameTextColor(); // Actualizar color al mover entre opciones
    }

    private void UpdateSubMenuVisuals()
    {
        SetIndicatorsActive(musicIndicators, false);
        SetIndicatorsActive(soundEffectsIndicators, false);
        SetIndicatorsActive(autoSaveIndicators, false);
        SetIndicatorsActive(subExitIndicators, false);

        switch (currentSubOption)
        {
            case SubMenuOption.Music:
                SetIndicatorsActive(musicIndicators, true);
                break;
            case SubMenuOption.SoundEffects:
                SetIndicatorsActive(soundEffectsIndicators, true);
                break;
            case SubMenuOption.AutoSave:
                SetIndicatorsActive(autoSaveIndicators, true);
                break;
            case SubMenuOption.Exit:
                SetIndicatorsActive(subExitIndicators, true);
                break;
        }
    }

    private void SetIndicatorsActive(GameObject[] indicators, bool active)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(active);
            }
        }
    }

    private void PlaySelectionSound()
    {
        if (audioSource != null && selectionSound != null)
        {
            audioSource.PlayOneShot(selectionSound);
        }
    }

    private void PlayNoActionSound()
    {
        if (audioSource != null && noActionSound != null)
        {
            audioSource.PlayOneShot(noActionSound);
        }
    }

    private IEnumerator ProcessSelection()
    {
        isProcessing = true;

        if (currentOption == MenuOption.StartGame)
        {
            PlaySelectionSound();
            yield return StartCoroutine(FadeIn(0.80f));
            SceneManager.LoadScene("TestZone");
        }
        else if (currentOption == MenuOption.LoadGame)
        {
            if (gameLoaded)
            {
                PlaySelectionSound();
                yield return StartCoroutine(FadeIn(0.80f));
                SceneManager.LoadScene("TestZone");
            }
            else
            {
                PlayNoActionSound();
            }
        }
        else if (currentOption == MenuOption.Exit)
        {
            PlaySelectionSound();
            yield return StartCoroutine(FadeIn(0.80f));
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        isProcessing = false;
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadePanel != null)
        {
            Image fadeImage = fadePanel.GetComponent<Image>();
            if (fadeImage != null)
            {
                fadePanel.SetActive(true);
                float elapsedTime = 0f;
                Color startColor = new Color(0, 0, 0, 0);
                Color endColor = new Color(0, 0, 0, 1);

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
                    yield return null;
                }
                fadeImage.color = endColor;
            }
        }
        yield return null;
    }

    private void EnterOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            inSubMenu = true;
            currentSubOption = SubMenuOption.Exit;
            UpdateSubMenuVisuals();
        }
    }

    private void CloseOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
            inSubMenu = false;
            UpdateMenuVisuals();
        }
    }

    private void ProcessMainMenuSelection()
    {
        if (currentOption == MenuOption.Options)
        {
            EnterOptionsMenu();
        }
        else
        {
            StartCoroutine(ProcessSelection());
        }
    }

    private void AdjustSlider(Slider slider)
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            slider.value = Mathf.Clamp01(slider.value - sliderAdjustSpeed * Time.deltaTime);
            UpdateVolumeText(slider, slider == musicSlider ? musicVolumeText : sfxVolumeText);
            if (Mathf.Abs(sliderAdjustSpeed * Time.deltaTime) > 0.01f)
            {
                PlaySelectionSound();
            }
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            slider.value = Mathf.Clamp01(slider.value + sliderAdjustSpeed * Time.deltaTime);
            UpdateVolumeText(slider, slider == musicSlider ? musicVolumeText : sfxVolumeText);
            if (Mathf.Abs(sliderAdjustSpeed * Time.deltaTime) > 0.01f)
            {
                PlaySelectionSound();
            }
        }
    }

    private void UpdateVolumeText(Slider slider, TMP_Text volumeText)
    {
        if (slider != null && volumeText != null)
        {
            int percentage = Mathf.RoundToInt(slider.value * 100);
            volumeText.text = $"{percentage}%";
        }
    }

    private void UpdateLoadGameTextColor()
    {
        if (loadGameText != null)
        {
            loadGameText.color = gameLoaded ? Color.white : Color.gray; // Blanco si gameLoaded, gris si no
        }
    }
}