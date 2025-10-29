using UnityEngine;

/// <summary>
/// Controls an Eraser sprite and triggers PaintEraser.EraseAtPixel.
/// Supports Mouse dragging and Keyboard movement (arrow keys / WASD).
/// Attach to the Eraser GameObject (the visible eraser cursor).
/// </summary>
public class EraserController : MonoBehaviour
{
    public Camera cam;
    public PaintEraser paintEraser;
    public SpriteRenderer eraserRenderer;

    [Header("Eraser settings")]
    public float eraserWorldRadius = 0.5f; // size in world units
    public int eraserPixelRadius = 20; // size in texture pixels (will be computed too)
    public float keyboardSpeed = 3f;
    public float mouseFollowSpeed = 5f; // controls smoothness of mouse following
    public SpriteRenderer background; 

    bool isMouseDown = false;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (eraserRenderer == null) eraserRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    void HandleMouseInput()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        
        transform.position = Vector3.Lerp(transform.position, mouseWorld, mouseFollowSpeed * Time.deltaTime);

        // Clamp inside background borders
        if (background != null)
        {
            Bounds b = background.bounds;
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
            pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);
            transform.position = pos;
        }

        if (Input.GetMouseButtonDown(0)) isMouseDown = true;
        if (Input.GetMouseButtonUp(0)) isMouseDown = false;

        if (isMouseDown)
        {
            TryEraseAt(transform.position);
        }
    }

    void HandleKeyboardInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            Vector3 delta = new Vector3(h, v, 0) * keyboardSpeed * Time.deltaTime;
            transform.position += delta;

            // Optionally erase while arrow keys pressed:
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                TryEraseAt(transform.position);
            }
        }
    }

    void TryEraseAt(Vector3 worldPos)
    {
        if (paintEraser == null) return;

        // Convert world->pixel coordinates on texture
        Vector2 pixel = paintEraser.WorldToPixelPoint(worldPos);

        // compute pixel radius roughly from eraserWorldRadius and sprite pixelsPerUnit
        Sprite overlaySprite = paintEraser.overlayRenderer.sprite;
        if (overlaySprite != null)
        {
            float ppu = overlaySprite.pixelsPerUnit;
            int pixelRadius = Mathf.Max(2, Mathf.RoundToInt(eraserWorldRadius * ppu));
            paintEraser.EraseAtPixel(pixel, pixelRadius);
        }
        else
        {
            paintEraser.EraseAtPixel(pixel, eraserPixelRadius);
        }
    }
}
