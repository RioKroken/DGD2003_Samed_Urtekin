using UnityEngine;

public class PosterHangSpot : MonoBehaviour
{
    [Header("Görsel")]
    [SerializeField] private GameObject ghostVisual;
    [SerializeField] private GameObject hungPoster;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private float ghostAlpha = 0.35f;

    [Header("Kayıt")]
    [SerializeField] private string spotId = "";

    [Header("Collider")]
    [SerializeField] private Collider interactCollider;

    public bool IsOccupied { get; private set; }
    public string SpotId => string.IsNullOrEmpty(spotId) ? gameObject.name : spotId;

    private void Awake()
    {
        if (string.IsNullOrEmpty(spotId))
            spotId = gameObject.name;

        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();

        PrepareHungPoster();
        PrepareGhost();
        UpdateVisuals();
    }

    public bool TryHang()
    {
        if (IsOccupied) return false;
        if (!PosterInventory.TryUsePoster()) return false;

        SetOccupied(true);
        GameSaveManager.Instance?.RegisterHungSpot(SpotId);
        PosterWinChecker.CheckAfterHang();
        return true;
    }

    public void TryRestoreFromSave(string[] occupiedIds)
    {
        if (occupiedIds == null) return;

        foreach (string id in occupiedIds)
        {
            if (id == SpotId)
            {
                SetOccupied(true);
                return;
            }
        }
    }

    private void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
        UpdateVisuals();
    }

    private void PrepareHungPoster()
    {
        if (hungPoster == null) return;

        if (!hungPoster.scene.IsValid())
            hungPoster = Instantiate(hungPoster, transform);

        hungPoster.transform.localPosition = Vector3.zero;
        hungPoster.transform.localRotation = Quaternion.identity;
        hungPoster.SetActive(false);
    }

    private void PrepareGhost()
    {
        if (ghostVisual == null) return;

        foreach (Collider col in ghostVisual.GetComponentsInChildren<Collider>())
            col.enabled = false;

        ApplyGhostMaterial();
    }

    private void ApplyGhostMaterial()
    {
        foreach (Renderer renderer in ghostVisual.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (ghostMaterial != null)
                {
                    mats[i] = ghostMaterial;
                    continue;
                }

                Material mat = mats[i];
                if (!mat.HasProperty("_BaseColor")) continue;

                Color color = mat.GetColor("_BaseColor");
                color.a = ghostAlpha;
                mat.SetColor("_BaseColor", color);

                if (mat.HasProperty("_Surface"))
                    mat.SetFloat("_Surface", 1f);

                if (mat.HasProperty("_Blend"))
                    mat.SetFloat("_Blend", 0f);
            }
        }
    }

    private void UpdateVisuals()
    {
        if (ghostVisual != null)
            ghostVisual.SetActive(!IsOccupied);

        if (hungPoster != null)
            hungPoster.SetActive(IsOccupied);
    }
}
