using System;

/// <summary>
/// JSON dosyasına yazılacak oyun verisi. Unity'nin JsonUtility'si
/// sadece bu tür basit, [Serializable] sınıfları destekler.
/// </summary>
[Serializable]
public class GameSaveData
{
    public int totalMoneyEarned;
    public int bestScore;
    public int gamesPlayed;
    public string lastPlayedDate;
}
