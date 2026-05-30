using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC: oyun durur, FirstPanel açılır.
/// Settings → SettingsPanel. Back → FirstPanel.
/// Resume / ESC: menü kapanır, oyun devam eder.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isPaused;

    [Header("Paneller (EscMenu altı)")]
    [Tooltip("Arka plan karartması / panel (Image). ESC ile birlikte açılır kapanır.")]
    [SerializeField] private GameObject menuBackground;
    [SerializeField] private GameObject firstPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ayarlar")]
    [SerializeField] private Slider volumeSlider;

    [Header("Quit")]
    [SerializeField] private string mainMenuSceneName = "";

    [Header("Opsiyonel")]
    [SerializeField] private FirstPersonCharacterController firstPersonController;

    private bool _isPaused;
    private bool _sliderBound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        TryAutoAssignPanels();
        TryAutoAssignBackground();
        TryAutoAssignVolumeSlider();
        BindVolumeSlider();
        CloseAllPanels();
        ApplySavedVolume();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // --- UI butonları (OnClick) ---

    public void OnResumeClicked() => ResumeGame();

    public void OnContinueClicked() => ResumeGame();

    public void OnSettingsClicked()
    {
        if (!_isPaused)
        {
            _isPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ShowMenu(settingsPanel);
    }

    public void OnBackClicked()
    {
        ShowMenu(firstPanel);
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnVolumeChanged(float value) => ApplyVolume(value);

    // --- Pause / Resume ---

    private void PauseGame()
    {
        if (firstPanel == null)
        {
            Debug.LogError("[PauseMenu] FirstPanel atanmamış!", this);
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameSaveManager.Instance?.SaveProgress();
        ShowMenu(firstPanel);
    }

    private void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        HideMenu();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetPaused(bool paused)
    {
        if (paused) PauseGame();
        else ResumeGame();
    }

    private void ShowMenu(GameObject activePanel)
    {
        SetPanelActive(menuBackground, true);
        SetPanelActive(firstPanel, activePanel == firstPanel);
        SetPanelActive(settingsPanel, activePanel == settingsPanel);

        if (activePanel == settingsPanel)
            ApplySavedVolume();
    }

    private void HideMenu()
    {
        SetPanelActive(menuBackground, false);
        SetPanelActive(firstPanel, false);
        SetPanelActive(settingsPanel, false);
    }

    private void CloseAllPanels() => HideMenu();

    private void TryAutoAssignBackground()
    {
        if (menuBackground != null) return;
        menuBackground = FindChildPanel("Image", "Background", "Panel", "EscPanel");
    }

    // --- Volume ---

    private void BindVolumeSlider()
    {
        if (_sliderBound || volumeSlider == null) return;

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        _sliderBound = true;
    }

    private void OnVolumeSliderChanged(float value) => ApplyVolume(value);

    private void ApplyVolume(float value)
    {
        float v = Mathf.Clamp01(value);
        EnsureSaveManager();

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.MasterVolume = v;
        else
            PlayerPrefs.SetFloat("MasterVolume", v);

        AudioListener.volume = v;
    }

    private void ApplySavedVolume()
    {
        EnsureSaveManager();
        float v = GameSaveManager.Instance != null ? GameSaveManager.Instance.MasterVolume : 1f;

        ApplyVolume(v);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(v);
    }

    private void TryAutoAssignVolumeSlider()
    {
        if (volumeSlider != null || settingsPanel == null) return;

        foreach (Slider slider in settingsPanel.GetComponentsInChildren<Slider>(true))
        {
            string n = slider.name.ToLowerInvariant();
            if (n.Contains("volume") || n.Contains("ses") || n.Contains("sound"))
            {
                volumeSlider = slider;
                return;
            }
        }

        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
        if (sliders.Length > 0)
            volumeSlider = sliders[0];
    }

    private void TryAutoAssignPanels()
    {
        if (firstPanel == null)
            firstPanel = FindChildPanel("FirstPanel", "PausePanel", "Pause Menu");

        if (settingsPanel == null)
            settingsPanel = FindChildPanel("SettingsPanel", "Settings Panel", "Settings");
    }

    private GameObject FindChildPanel(params string[] names)
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
        {
            foreach (string name in names)
            {
                if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return child.gameObject;
            }
        }

        return null;
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        if (panel == gameObject) return;

        if (active)
        {
            Transform t = panel.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                    t.gameObject.SetActive(true);
                t = t.parent;
            }
        }

        panel.SetActive(active);
    }

    private static void EnsureSaveManager()
    {
        if (GameSaveManager.Instance != null) return;
        new GameObject("GameSaveManager (Auto)").AddComponent<GameSaveManager>();
    }
}
