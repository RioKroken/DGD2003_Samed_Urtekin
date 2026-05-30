using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PosterWinChecker : MonoBehaviour
{
    public static PosterWinChecker Instance { get; private set; }
    public static event Action OnAllPostersHung;

    [Header("Kazanma")]
    [SerializeField] private bool reloadSceneOnWin = true;
    [Tooltip("Boş = aynı sahne yeniden yüklenir")]
    [SerializeField] private string reloadSceneName = "";

    private static bool _gameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CheckAllPostersHung();
    }

    public static void CheckAfterHang()
    {
        if (_gameEnded) return;

        PosterHangSpot[] spots = FindObjectsByType<PosterHangSpot>(FindObjectsSortMode.None);
        if (spots.Length == 0) return;

        foreach (PosterHangSpot spot in spots)
        {
            if (!spot.IsOccupied) return;
        }

        if (Instance != null)
            Instance.HandleWin();
        else
            HandleWinStatic();
    }

    private void CheckAllPostersHung()
    {
        if (_gameEnded) return;

        PosterHangSpot[] spots = FindObjectsByType<PosterHangSpot>(FindObjectsSortMode.None);
        if (spots.Length == 0) return;

        foreach (PosterHangSpot spot in spots)
        {
            if (!spot.IsOccupied) return;
        }

        HandleWin();
    }

    private void HandleWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        OnAllPostersHung?.Invoke();

        GameTimer timer = FindFirstObjectByType<GameTimer>();
        if (timer != null)
            timer.StopTimer();

        GameSaveManager.Instance?.RecordGameWin();
        ReloadSceneIfNeeded();
    }

    private static void HandleWinStatic()
    {
        _gameEnded = true;
        OnAllPostersHung?.Invoke();
        GameSaveManager.Instance?.RecordGameWin();

        if (string.IsNullOrEmpty(SceneManager.GetActiveScene().name))
            return;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReloadSceneIfNeeded()
    {
        if (!reloadSceneOnWin) return;

        if (string.IsNullOrEmpty(reloadSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(reloadSceneName);
    }
}
