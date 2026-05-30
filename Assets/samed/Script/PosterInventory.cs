using System;
using UnityEngine;

public class PosterInventory : MonoBehaviour
{
    public static PosterInventory Instance { get; private set; }

    public static event Action<int> OnCountChanged;

    public static int Count { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void AddPoster()
    {
        Count++;
        OnCountChanged?.Invoke(Count);
    }

    public static bool TryUsePoster()
    {
        if (Count <= 0) return false;

        Count--;
        OnCountChanged?.Invoke(Count);
        return true;
    }
}
