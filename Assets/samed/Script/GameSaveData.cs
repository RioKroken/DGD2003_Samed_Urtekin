using System;

/// <summary>
/// JSON dosyasına yazılan oyun ilerlemesi (JsonUtility ile).
/// </summary>
[Serializable]
public class GameSaveData
{
    public int posterCount;
    public int totalPostersHung;
    public int bestScore;
    public int gamesPlayed;
    public float timerSecondsLeft = -1f;
    public string lastPlayedDate;
    public string[] occupiedHangSpots = Array.Empty<string>();
}
