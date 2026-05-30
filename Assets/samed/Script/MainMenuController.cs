using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ayarlar")]
    [SerializeField] private Slider volumeSlider;

    [Header("Sahne")]
    [SerializeField] private string gameSceneName = "MainScene";

    private bool _sliderBound;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        TryAutoAssignPanels();
        TryAutoAssignVolumeSlider();
        BindVolumeSlider();
        ShowMainPanel();
        ApplySavedVolume();
    }

    // --- Butonlar (OnClick) ---

    public void OnStartClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenu] Game Scene Name boş!", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"[MainMenu] '{gameSceneName}' Build Settings'te yok. File > Build Settings'e ekle.", this);
            return;
        }

        GameSaveManager.ResetRunData();

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    public void OnBackClicked()
    {
        ShowMainPanel();
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnVolumeChanged(float value) => ApplyVolume(value);

    // --- Paneller ---

    private void ShowMainPanel()
    {
        SetPanelActive(settingsPanel, false);
        SetPanelActive(mainPanel, true);
    }

    private void ShowSettingsPanel()
    {
        SetPanelActive(mainPanel, false);
        SetPanelActive(settingsPanel, true);
        ApplySavedVolume();
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

    // --- Ses ---

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
        if (volumeSlider != null) return;

        if (settingsPanel != null)
        {
            foreach (Slider slider in settingsPanel.GetComponentsInChildren<Slider>(true))
            {
                string n = slider.name.ToLowerInvariant();
                if (n.Contains("volume") || n.Contains("ses"))
                {
                    volumeSlider = slider;
                    return;
                }
            }

            Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
            if (sliders.Length > 0)
                volumeSlider = sliders[0];
        }
    }

    private void TryAutoAssignPanels()
    {
        if (mainPanel == null)
            mainPanel = FindPanel("MainPanel", "FirstPanel", "MenuPanel", "Start");

        if (settingsPanel == null)
            settingsPanel = FindPanel("SettingsPanel", "Settings Panel", "Settings");
    }

    private GameObject FindPanel(params string[] names)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            foreach (string name in names)
            {
                if (t.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return t.gameObject;
            }
        }

        return null;
    }

    private static void EnsureSaveManager()
    {
        if (GameSaveManager.Instance != null) return;

        GameSaveManager existing = FindFirstObjectByType<GameSaveManager>();
        if (existing != null) return;

        new GameObject("GameSaveManager (Auto)").AddComponent<GameSaveManager>();
    }
}
