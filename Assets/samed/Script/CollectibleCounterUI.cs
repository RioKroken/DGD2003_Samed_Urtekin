using TMPro;
using UnityEngine;

public class CollectibleCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private string format = "Poster: {0}";

    private void OnEnable()
    {
        PosterInventory.OnCountChanged += UpdateText;
        UpdateText(PosterInventory.Count);
    }

    private void OnDisable()
    {
        PosterInventory.OnCountChanged -= UpdateText;
    }

    private void UpdateText(int count)
    {
        if (countText == null) return;
        countText.text = string.Format(format, count);
    }
}
