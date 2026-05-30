using UnityEngine;

/// <summary>
/// FirstPersonCharacterController ile uyumluluk için minimal sınıf.
/// Tam pause menüsü için Gorkem projesindeki PauseMenuController ile değiştirin.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isPaused;

    private bool _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetPaused(bool paused)
    {
        _isPaused = paused;
    }
}
