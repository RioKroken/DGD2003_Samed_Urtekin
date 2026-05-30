using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float     raycastRange     = 10f;
    [SerializeField] private LayerMask detectionMask    = ~0;

    [Header("Outline Materyali")]
    [SerializeField] private Material  outlineMaterial;

    [Header("Hariç Tutulacak Tag'ler")]
    [SerializeField] private string[]  ignoreTags = { "Player", "MainCamera" };

    private Renderer   _currentRenderer;
    private Material[] _originalMaterials;

    private void Update()
    {
        DetectAndOutline();
    }

    private void DetectAndOutline()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, detectionMask))
        {
            // Tag kontrolü — oyuncu ve kamera gibi olanları atla
            if (ShouldIgnore(hit.collider.gameObject))
            {
                RemoveOutline();
                return;
            }

            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) rend = hit.collider.GetComponentInChildren<Renderer>();

            if (rend != null && rend != _currentRenderer)
            {
                RemoveOutline();
                ApplyOutline(rend);
            }
        }
        else
        {
            RemoveOutline();
        }
    }

    private void ApplyOutline(Renderer rend)
    {
        if (outlineMaterial == null)
        {
            Debug.LogWarning("OutlineEffect: Outline Material atanmamış!");
            return;
        }

        _currentRenderer   = rend;
        _originalMaterials = rend.sharedMaterials;

        // Orijinal materyallerin sonuna outline materyalini ekle
        Material[] newMats = new Material[_originalMaterials.Length + 1];
        _originalMaterials.CopyTo(newMats, 0);
        newMats[newMats.Length - 1] = outlineMaterial;

        rend.materials = newMats;
    }

    private void RemoveOutline()
    {
        if (_currentRenderer == null) return;

        _currentRenderer.materials = _originalMaterials;
        _currentRenderer           = null;
        _originalMaterials         = null;
    }

    private bool ShouldIgnore(GameObject obj)
    {
        foreach (string tag in ignoreTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        RemoveOutline();
    }
}
