using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    private SubMenuOption currentSubOption = SubMenuOption.Music;

    [SerializeField] private GameObject[] startGameIndicators;
    [SerializeField] private GameObject[] loadGameIndicators;
    [SerializeField] private GameObject[] optionsIndicators;
    [SerializeField] private GameObject[] exitIndicators;

    [SerializeField] private GameObject[] musicIndicators;
    [SerializeField] private GameObject[] soundEffectsIndicators;
    [SerializeField] private GameObject[] autoSaveIndicators;
    [SerializeField] private GameObject[] subExitIndicators;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip noActionSound;

    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject fadePanel;

    [SerializeField] private Slider musicSlider; // Slider para Music
    [SerializeField] private Slider sfxSlider;   // Slider para Sound Effects
    [SerializeField] private Toggle autoSaveToggle; // Checkbox para AutoSave

    private bool isProcessing = false;
    private bool inSubMenu = false;
    private bool controllingSlider = false; // Controla si estás ajustando un slider
    private Slider activeSlider = null;     // Slider actualmente controlado

    void Start()
    {
        UpdateMenuVisuals();
        if (fadePanel != null)
        {
            fadePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {
        if (isProcessing) return;

        if (controllingSlider)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AdjustSlider(-0.1f);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AdjustSlider(0.1f);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                controllingSlider = false;
                activeSlider = null;
                UpdateSubMenuVisuals();
            }
        }
        else if (inSubMenu)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSubMenuUp();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSubMenuDown();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                ProcessSubMenuSelection();
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
                if (currentOption == MenuOption.Options)
                {
                    EnterOptionsMenu();
                }
                else
                {
                    StartCoroutine(ProcessSelection());
                }
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

    private void MoveSubMenuUp()
    {
        switch (currentSubOption)
        {
            case SubMenuOption.Music:
                break;
            case SubMenuOption.SoundEffects:
                currentSubOption = SubMenuOption.Music;
                break;
            case SubMenuOption.AutoSave:
                currentSubOption = SubMenuOption.SoundEffects;
                break;
            case SubMenuOption.Exit:
                currentSubOption = SubMenuOption.AutoSave;
                break;
        }
        PlaySelectionSound();
        UpdateSubMenuVisuals();
    }

    private void MoveSubMenuDown()
    {
        switch (currentSubOption)
        {
            case SubMenuOption.Music:
                currentSubOption = SubMenuOption.SoundEffects;
                break;
            case SubMenuOption.SoundEffects:
                currentSubOption = SubMenuOption.AutoSave;
                break;
            case SubMenuOption.AutoSave:
                currentSubOption = SubMenuOption.Exit;
                break;
            case SubMenuOption.Exit:
                break;
        }
        PlaySelectionSound();
        UpdateSubMenuVisuals();
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

    private IEnumerator ProcessSelection()
    {
        isProcessing = true;
        yield return StartCoroutine(FadeIn(0.80f));

        switch (currentOption)
        {
            case MenuOption.StartGame:
                SceneManager.LoadScene("TestZone");
                break;
            case MenuOption.LoadGame:
                if (audioSource != null && noActionSound != null)
                {
                    audioSource.PlayOneShot(noActionSound);
                }
                break;
            case MenuOption.Exit:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
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
    }

    private void EnterOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            inSubMenu = true;
            currentSubOption = SubMenuOption.Music;
            UpdateSubMenuVisuals();
        }
    }

    private void ProcessSubMenuSelection()
    {
        switch (currentSubOption)
        {
            case SubMenuOption.Music:
                if (musicSlider != null)
                {
                    controllingSlider = true;
                    activeSlider = musicSlider;
                }
                break;
            case SubMenuOption.SoundEffects:
                if (sfxSlider != null)
                {
                    controllingSlider = true;
                    activeSlider = sfxSlider;
                }
                break;
            case SubMenuOption.AutoSave:
                if (autoSaveToggle != null)
                {
                    autoSaveToggle.isOn = !autoSaveToggle.isOn;
                }
                break;
            case SubMenuOption.Exit:
                if (optionsPanel != null)
                {
                    optionsPanel.SetActive(false);
                    inSubMenu = false;
                    controllingSlider = false;
                    activeSlider = null;
                    UpdateMenuVisuals();
                }
                break;
        }
    }

    private void AdjustSlider(float change)
    {
        if (activeSlider != null)
        {
            activeSlider.value = Mathf.Clamp01(activeSlider.value + change);
            PlaySelectionSound();
        }
    }
}