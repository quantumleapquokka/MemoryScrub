using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalPosition;
    private float originalSize;

    [Header("Zoom settings")]
    public float zoomedOutSize = 8f; // how far to zoom out
    public float moveSpeed = 2f;     // how quickly the camera moves
    public float zoomSpeed = 2f;     // how quickly it zooms
    public Transform backgroundCenter;
    void Start()
    {
        cam = GetComponent<Camera>();
        originalPosition = transform.position;
        originalSize = cam.orthographicSize;
    }

    public void ZoomToBackground()
    {
        if (backgroundCenter == null) return;
        StopAllCoroutines();
        StartCoroutine(SmoothZoom(backgroundCenter.position));
    }

    System.Collections.IEnumerator SmoothZoom(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float startSize = cam.orthographicSize;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * zoomSpeed;
            transform.position = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y, startPos.z), t);
            cam.orthographicSize = Mathf.Lerp(startSize, zoomedOutSize, t);
            yield return null;
        }
    }


}
