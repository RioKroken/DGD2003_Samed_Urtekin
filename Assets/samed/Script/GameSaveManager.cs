using System;
using System.IO;
using UnityEngine;

/// <summary>
/// PlayerPrefs: ses, hassasiyet gibi ayarlar.
/// JSON: poster sayısı, asılı posterler, skor, süre, ilerleme.
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    const string KeyMusicOn = "MusicOn";
    const string KeyMasterVolume = "MasterVolume";
    const string KeyMouseSens = "MouseSensitivity";

    const string SaveFileName = "gamesave.json";

    public GameSaveData Data { get; private set; } = new GameSaveData();

    public bool MusicOn
    {
        get => PlayerPrefs.GetInt(KeyMusicOn, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(KeyMusicOn, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KeyMasterVolume, 1f);
        set
        {
            PlayerPrefs.SetFloat(KeyMasterVolume, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(KeyMouseSens, 0.5f);
        set
        {
            PlayerPrefs.SetFloat(KeyMouseSens, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadJson();
        ApplySettings();
    }

    private void Start()
    {
        ApplyProgressToScene();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveProgress();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    // --- PlayerPrefs (ayarlar) ---

    public void ApplySettings()
    {
        AudioListener.volume = MasterVolume;
    }

    // --- JSON ---

    public void LoadJson()
    {
        if (!File.Exists(SavePath))
        {
            Data = new GameSaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<GameSaveData>(json);
            if (Data == null)
                Data = new GameSaveData();
            if (Data.occupiedHangSpots == null)
                Data.occupiedHangSpots = Array.Empty<string>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameSaveManager] JSON okunamadı: {e.Message}");
            Data = new GameSaveData();
        }
    }

    public void SaveJson()
    {
        Data.lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string json = JsonUtility.ToJson(Data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    public void SyncFromGame()
    {
        Data.posterCount = PosterInventory.Count;
        Data.totalPostersHung = Mathf.Max(Data.totalPostersHung, CountOccupiedSpotsInScene());

        GameTimer timer = FindFirstObjectByType<GameTimer>();
        if (timer != null)
            Data.timerSecondsLeft = timer.TimeLeft;

        int runScore = Data.posterCount + Data.totalPostersHung;
        if (runScore > Data.bestScore)
            Data.bestScore = runScore;
    }

    public void SaveProgress()
    {
        SyncFromGame();
        SaveJson();
    }

    public void ApplyProgressToScene()
    {
        PosterInventory.SetCount(Data.posterCount);

        PosterHangSpot[] spots = FindObjectsByType<PosterHangSpot>(FindObjectsSortMode.None);
        foreach (PosterHangSpot spot in spots)
            spot.TryRestoreFromSave(Data.occupiedHangSpots);

        GameTimer timer = FindFirstObjectByType<GameTimer>();
        if (timer != null && Data.timerSecondsLeft >= 0f)
            timer.LoadTimeLeft(Data.timerSecondsLeft);
    }

    public void RegisterHungSpot(string spotId)
    {
        if (string.IsNullOrEmpty(spotId)) return;

        if (ContainsSpot(spotId)) return;

        int len = Data.occupiedHangSpots.Length;
        Array.Resize(ref Data.occupiedHangSpots, len + 1);
        Data.occupiedHangSpots[len] = spotId;

        Data.totalPostersHung = Data.occupiedHangSpots.Length;
        SaveProgress();
    }

    public void OnPosterCountChanged()
    {
        Data.posterCount = PosterInventory.Count;
        SaveJson();
    }

    public void RecordGameOver()
    {
        Data.gamesPlayed++;
        Data.timerSecondsLeft = -1f;
        SyncFromGame();
        SaveJson();
    }

    public void RecordGameWin()
    {
        Data.gamesPlayed++;
        Data.totalPostersHung = CountOccupiedSpotsInScene();
        if (Data.totalPostersHung > Data.bestScore)
            Data.bestScore = Data.totalPostersHung;

        Data.posterCount = 0;
        Data.occupiedHangSpots = Array.Empty<string>();
        Data.timerSecondsLeft = -1f;
        SaveJson();
    }

    public void DeleteAllSaves()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        Data = new GameSaveData();
        ApplyProgressToScene();
        ApplySettings();
    }

    private bool ContainsSpot(string spotId)
    {
        foreach (string id in Data.occupiedHangSpots)
        {
            if (id == spotId) return true;
        }

        return false;
    }

    private static int CountOccupiedSpotsInScene()
    {
        int count = 0;
        foreach (PosterHangSpot spot in FindObjectsByType<PosterHangSpot>(FindObjectsSortMode.None))
        {
            if (spot.IsOccupied) count++;
        }

        return count;
    }
}
