using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Takip edilecek obje (Player)
    public Vector3 offset;        // Kamera ile oyuncu arasýndaki mesafe
    public float smoothSpeed = 5f; // Kamera yumuþatma

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
