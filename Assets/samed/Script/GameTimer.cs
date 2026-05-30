using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Süre")]
    [SerializeField] private float totalSeconds = 300f;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private bool pauseWhenGamePaused = true;

    [Header("Dijital saat formatı")]
    [Tooltip("Açıksa 01:05:30 (1 saatten fazlaysa), kapalıysa 05:30")]
    [SerializeField] private bool showHoursWhenNeeded = true;

    [Header("Süre bitince")]
    [SerializeField] private bool reloadSceneOnTimeUp = true;
    [Tooltip("Boş bırak = aynı sahne yeniden yüklenir")]
    [SerializeField] private string reloadSceneName = "";

    [Header("Son saniye uyarısı")]
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    private float _timeLeft;
    private bool _isRunning;

    public static event Action OnTimeUp;
    public float TimeLeft => _timeLeft;
    public bool IsRunning => _isRunning;

    private void Start()
    {
        _timeLeft = totalSeconds;
        _isRunning = startOnPlay;
        UpdateUI();
    }

    private void Update()
    {
        if (!_isRunning) return;
        if (pauseWhenGamePaused && PauseMenuController.IsOpen) return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            _isRunning = false;
            UpdateUI();
            OnTimeUp?.Invoke();
            ReloadSceneIfNeeded();
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        timerText.text = FormatDigitalTime(_timeLeft);
        timerText.color = _timeLeft <= warningThreshold && _timeLeft > 0f ? warningColor : normalColor;
    }

    private string FormatDigitalTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;

        if (showHoursWhenNeeded && (h > 0 || totalSeconds >= 3600f))
            return $"{h:00}:{m:00}:{s:00}";

        return $"{m:00}:{s:00}";
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }

    public void ResetTimer()
    {
        _timeLeft = totalSeconds;
        _isRunning = startOnPlay;
        UpdateUI();
    }

    public void AddTime(float seconds)
    {
        _timeLeft += seconds;
        UpdateUI();
    }

    private void ReloadSceneIfNeeded()
    {
        if (!reloadSceneOnTimeUp) return;

        if (string.IsNullOrEmpty(reloadSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(reloadSceneName);
    }
}
