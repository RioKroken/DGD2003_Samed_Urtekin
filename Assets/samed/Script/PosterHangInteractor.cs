using UnityEngine;
using UnityEngine.InputSystem;

public class PosterHangInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 10f;
    [SerializeField] private LayerMask hangSpotMask = ~0;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (_camera == null || Keyboard.current == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;
        if (PauseMenuController.IsOpen) return;

        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, hangSpotMask, QueryTriggerInteraction.Collide))
            return;

        PosterHangSpot spot = hit.collider.GetComponent<PosterHangSpot>();
        if (spot == null)
            spot = hit.collider.GetComponentInParent<PosterHangSpot>();

        spot?.TryHang();
    }
}
