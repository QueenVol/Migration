using UnityEngine;

public class CameraDragController : MonoBehaviour
{
    [Header("Drag")]
    public float dragSpeed = 1f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = 4f;
    public float maxZoom = 18f;

    [Header("Map Bounds")]
    public SpriteRenderer mapRenderer;

    private Camera cam;
    private Vector3 lastMouseWorldPosition;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
        ClampCamera();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            lastMouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 currentMouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = lastMouseWorldPosition - currentMouseWorldPosition;

            transform.position += difference * dragSpeed;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void ClampCamera()
    {
        if (mapRenderer == null)
            return;

        Bounds bounds = mapRenderer.bounds;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minX = bounds.min.x + camWidth;
        float maxX = bounds.max.x - camWidth;
        float minY = bounds.min.y + camHeight;
        float maxY = bounds.max.y - camHeight;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = -10f;

        transform.position = pos;
    }
}
