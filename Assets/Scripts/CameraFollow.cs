using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // the eraser
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10); // keep camera back on Z

    void LateUpdate()
    {
        if (target == null) return;

        // Smooth follow
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }
}
