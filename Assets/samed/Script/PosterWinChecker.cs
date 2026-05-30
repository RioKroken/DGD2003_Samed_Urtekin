using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kazanma: duvardaki tüm poster noktalarına E ile basılıp poster değişince (PosterHangSpot).
/// </summary>
public class PosterWinChecker : MonoBehaviour
{
    public static PosterWinChecker Instance { get; private set; }
    public static event Action OnAllPostersHung;

    [Header("Kazanma")]
    [SerializeField] private bool loadSceneOnWin = true;
    [Tooltip("Boş = FirstMenuScene")]
    [SerializeField] private string winSceneName = "FirstMenuScene";

    [Header("Debug")]
    [SerializeField] private bool logProgress;

    private static bool _gameEnded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _gameEnded = false;

    private void Start() => StartCoroutine(CheckAfterSceneReady());

    private IEnumerator CheckAfterSceneReady()
    {
        yield return null;
        CheckAfterHang();
    }

    public static void CheckAfterHang()
    {
        if (_gameEnded) return;

        PosterHangSpot[] spots = FindObjectsByType<PosterHangSpot>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (spots.Length == 0) return;

        int changed = 0;
        foreach (PosterHangSpot spot in spots)
        {
            if (spot.IsOccupied)
                changed++;
            else
                return;
        }

        if (Instance != null && Instance.logProgress)
            Debug.Log($"[PosterWin] Duvar posterleri tamam: {changed}/{spots.Length}", Instance);

        if (Instance != null)
            Instance.HandleWin();
        else
            HandleWinStatic();
    }

    public static int GetWallSpotTotal()
    {
        return FindObjectsByType<PosterHangSpot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
    }

    public static int GetWallSpotChanged()
    {
        int count = 0;
        foreach (PosterHangSpot spot in FindObjectsByType<PosterHangSpot>(
                     FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (spot.IsOccupied)
                count++;
        }

        return count;
    }

    private void HandleWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        Debug.Log("[PosterWin] Tüm duvar posterleri değişti — oyun bitti!", this);

        OnAllPostersHung?.Invoke();

        GameTimer timer = FindFirstObjectByType<GameTimer>();
        if (timer != null)
            timer.StopTimer();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameSaveManager.Instance?.RecordGameWin();
        LoadWinScene();
    }

    private static void HandleWinStatic()
    {
        _gameEnded = true;
        OnAllPostersHung?.Invoke();
        Time.timeScale = 1f;
        GameSaveManager.Instance?.RecordGameWin();
        LoadWinSceneStatic();
    }

    private void LoadWinScene()
    {
        if (!loadSceneOnWin) return;
        LoadWinSceneStatic();
    }

    private static void LoadWinSceneStatic()
    {
        string scene = string.IsNullOrEmpty(Instance?.winSceneName)
            ? "FirstMenuScene"
            : Instance.winSceneName;

        if (Application.CanStreamedLevelBeLoaded(scene))
        {
            SceneManager.LoadScene(scene);
            return;
        }

        Debug.LogError($"[PosterWin] '{scene}' Build Settings'te yok.");
    }
}
