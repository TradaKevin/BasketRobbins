using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu button wiring for BasketRobbins.
/// Assign scene names and button references in the Inspector.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string playSceneName = "Main GamePlay";
    public string levelSelectSceneName = "LevelSelect";

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject shopPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button levelSelectButton;
    public Button shopButton;
    public Button optionsButton;
    public Button quitButton;
    public Button settingsButton;
    public Button soundButton;

    [Header("Coins")]
    public TMP_Text coinText;
    public string coinsPlayerPrefsKey = "Coins";
    public int defaultCoinAmount = 250;

    [Header("Audio Hooks")]
    public AudioSource uiAudioSource;
    public AudioClip clickSound;

    private bool soundEnabled = true;

    private void Awake()
    {
        soundEnabled = PlayerPrefs.GetInt("MenuSoundEnabled", 1) == 1;
        AudioListener.volume = soundEnabled ? 1f : 0f;

        WireButton(playButton, PlayGame);
        WireButton(levelSelectButton, OpenLevelSelect);
        WireButton(shopButton, OpenShop);
        WireButton(optionsButton, OpenOptions);
        WireButton(quitButton, QuitGame);
        WireButton(settingsButton, OpenOptions);
        WireButton(soundButton, ToggleSound);

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        UpdateCoinText();
    }

    private void WireButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void PlayClickSound()
    {
        if (!soundEnabled || uiAudioSource == null || clickSound == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clickSound);
    }

    private void UpdateCoinText()
    {
        if (coinText == null)
        {
            return;
        }

        int coins = PlayerPrefs.GetInt(coinsPlayerPrefsKey, defaultCoinAmount);
        coinText.text = coins.ToString();
    }

    public void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene(playSceneName);
    }

    public void OpenLevelSelect()
    {
        PlayClickSound();
        SceneManager.LoadScene(levelSelectSceneName);
    }

    public void OpenShop()
    {
        PlayClickSound();

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Shop button pressed. Assign a shop panel or shop scene when it is ready.");
        }
    }

    public void OpenOptions()
    {
        PlayClickSound();

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        AudioListener.volume = soundEnabled ? 1f : 0f;
        PlayerPrefs.SetInt("MenuSoundEnabled", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        PlayClickSound();
    }

    public void QuitGame()
    {
        PlayClickSound();

#if UNITY_EDITOR
        Debug.Log("Quit requested. This only closes the app in a build.");
#else
        Application.Quit();
#endif
    }
}
