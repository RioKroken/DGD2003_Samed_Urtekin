using UnityEngine;

public class ClickableCollectible : MonoBehaviour
{
    public void Collect()
    {
        ClickCollectSystem.RegisterCollected();
        Destroy(gameObject);
    }
}
