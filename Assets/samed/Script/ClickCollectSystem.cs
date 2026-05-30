using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickCollectSystem : MonoBehaviour
{
    public static event Action<int> OnCollected;
    public static int CollectedCount => PosterInventory.Count;

    [SerializeField] private float interactRange = 10f;
    [SerializeField] private LayerMask interactMask = ~0;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (_camera == null) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;
        if (PauseMenuController.IsOpen) return;

        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
            return;

        TryCollectHit(hit.collider);
    }

    public static void TryCollectHit(Collider col)
    {
        if (col == null) return;

        PosterHangSpot hangSpot = col.GetComponent<PosterHangSpot>();
        if (hangSpot == null)
            hangSpot = col.GetComponentInParent<PosterHangSpot>();

        if (hangSpot != null && hangSpot.TryHang())
            return;

        ClickableCollectible collectible = col.GetComponent<ClickableCollectible>();
        if (collectible == null)
            collectible = col.GetComponentInParent<ClickableCollectible>();

        if (collectible != null)
        {
            collectible.Collect();
            return;
        }

        ClickCollectSystem marker = col.GetComponent<ClickCollectSystem>();
        if (marker == null)
            marker = col.GetComponentInParent<ClickCollectSystem>();

        if (marker != null && marker._camera == null)
            marker.CollectObject();
    }

    public static void RegisterCollected()
    {
        PosterInventory.AddPoster();
        OnCollected?.Invoke(PosterInventory.Count);
    }

    /// <summary>Kamera üzerindeki sistem değil — yerdeki poster prefab'ı.</summary>
    public bool IsFloorCollectible =>
        _camera == null && GetComponentInParent<Camera>() == null &&
        GetComponentInParent<PosterHangSpot>() == null;

    public void CollectObject()
    {
        if (_camera != null) return;

        RegisterCollected();
        Destroy(gameObject);
    }
}
