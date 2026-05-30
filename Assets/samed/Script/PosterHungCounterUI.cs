using TMPro;
using UnityEngine;

/// <summary>
/// Duvarda değişen poster / toplam duvar noktası.
/// </summary>
public class PosterHungCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private string format = "Poster: {0}/{1}";

    private void OnEnable()
    {
        PosterHangSpot.OnOccupancyChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        PosterHangSpot.OnOccupancyChanged -= Refresh;
    }

    private void Refresh()
    {
        if (countText == null) return;

        int changed = PosterWinChecker.GetWallSpotChanged();
        int total = PosterWinChecker.GetWallSpotTotal();
        countText.text = string.Format(format, changed, total);
    }
}
