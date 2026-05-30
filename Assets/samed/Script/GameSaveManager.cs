using System;
using System.IO;
using UnityEngine;

/// <summary>
/// PlayerPrefs: basit ayarlar (ses, müzik açık/kapalı).
/// JSON dosyası: daha zengin kayıt (skor, toplam para, oyun sayısı).
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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadJson();
    }

    string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

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
        Debug.Log($"[GameSaveManager] Kaydedildi: {SavePath}");
    }

    public void RecordGameResult(int moneyEarnedThisRun, int scoreThisRun)
    {
        Data.gamesPlayed++;
        Data.totalMoneyEarned += moneyEarnedThisRun;
        if (scoreThisRun > Data.bestScore)
            Data.bestScore = scoreThisRun;
        SaveJson();
    }

    public void DeleteAllSaves()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        Data = new GameSaveData();
    }
}
