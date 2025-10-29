using UnityEngine;

/// <summary>
/// Attach to the Overlay GameObject (with SpriteRenderer).
/// The script makes a runtime copy of the overlay sprite texture and allows erasing circular areas.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PaintEraser : MonoBehaviour
{
    public SpriteRenderer overlayRenderer;
    Texture2D writableTexture;
    Color[] clearColorsBuffer; // reused buffer for performance

    void Awake()
    {
        if (overlayRenderer == null) overlayRenderer = GetComponent<SpriteRenderer>();
        PrepareWritableTexture();
    }

    // Make an independent, writable copy of the sprite's texture
    void PrepareWritableTexture()
    {
        Sprite s = overlayRenderer.sprite;
        if (s == null)
        {
            Debug.LogError("Overlay SpriteRenderer has no sprite assigned.");
            return;
        }

        Texture2D source = s.texture;

        // Create a new texture with same dimensions and copy pixels
        writableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        writableTexture.SetPixels(source.GetPixels());
        writableTexture.filterMode = source.filterMode;
        writableTexture.wrapMode = source.wrapMode;
        writableTexture.Apply();

        // Create a new sprite that uses the writableTexture and keep same pivot/rect
        Rect rect = new Rect(0, 0, writableTexture.width, writableTexture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        if (overlayRenderer.sprite != null)
        {
            rect = overlayRenderer.sprite.rect;
            pivot = new Vector2(
                overlayRenderer.sprite.pivot.x / overlayRenderer.sprite.rect.width,
                overlayRenderer.sprite.pivot.y / overlayRenderer.sprite.rect.height
            );
        }

        Sprite newSprite = Sprite.Create(writableTexture, rect, pivot, overlayRenderer.sprite.pixelsPerUnit);
        overlayRenderer.sprite = newSprite;

        // Prepare buffer max size (we allocate when erasing)
        clearColorsBuffer = null;
    }

    // Call this to erase circle at a pixel position on the texture
    public void EraseAtPixel(Vector2 pixelPos, int radius)
    {
        if (writableTexture == null) return;

        int texW = writableTexture.width;
        int texH = writableTexture.height;

        int startX = Mathf.Clamp(Mathf.FloorToInt(pixelPos.x - radius), 0, texW - 1);
        int startY = Mathf.Clamp(Mathf.FloorToInt(pixelPos.y - radius), 0, texH - 1);
        int endX = Mathf.Clamp(Mathf.CeilToInt(pixelPos.x + radius), 0, texW - 1);
        int endY = Mathf.Clamp(Mathf.CeilToInt(pixelPos.y + radius), 0, texH - 1);

        int w = endX - startX + 1;
        int h = endY - startY + 1;

        // Buffer to reduce Get/Set overhead
        Color[] pixels = writableTexture.GetPixels(startX, startY, w, h);

        int ri = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++, ri++)
            {
                // compute distance from center
                float dx = (startX + x) - pixelPos.x;
                float dy = (startY + y) - pixelPos.y;
                float distSq = dx * dx + dy * dy;

                if (distSq <= radius * radius)
                {
                    // set alpha to 0 (transparent)
                    Color c = pixels[ri];
                    c.a = 0f;
                    pixels[ri] = c;
                }
            }
        }

        writableTexture.SetPixels(startX, startY, w, h, pixels);
        writableTexture.Apply(false); // no mipmaps
    }

    // Helper: convert world position to texture pixel coordinates relative to overlay sprite
    public Vector2 WorldToPixelPoint(Vector3 worldPos)
    {
        Sprite sprite = overlayRenderer.sprite;
        if (sprite == null) return Vector2.zero;

        Vector2 localPos = transform.InverseTransformPoint(worldPos);

        // sprite.rect is in pixels and sprite.pixelsPerUnit defines conversion
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 spriteSize = sprite.rect.size / pixelsPerUnit; // in world units

        // transform local position to [0..1] UV by shifting by sprite center and dividing by size
        Vector2 pivot = sprite.pivot / sprite.rect.size; // normalized pivot
        Vector2 uv = new Vector2(
            (localPos.x / spriteSize.x) + 0.5f,
            (localPos.y / spriteSize.y) + 0.5f
        );

        // If sprite uses rect not full texture, offset it
        Rect rect = sprite.rect;
        float pixelX = rect.x + uv.x * rect.width;
        float pixelY = rect.y + uv.y * rect.height;

        return new Vector2(pixelX, pixelY);
    }
}
