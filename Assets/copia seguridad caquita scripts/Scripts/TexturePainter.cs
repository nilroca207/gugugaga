using UnityEngine;

public class TexturePainter : MonoBehaviour
{
    public RenderTexture canvasTexture;
    public Color brushColor = Color.red;
    public int brushSize = 8;
    private Material paintMat;

    void Start()
    {
        // Use a built-in shader that works even in URP/HDRP
        paintMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        
        if (canvasTexture != null)
        {
            RenderTexture.active = canvasTexture;
            paintMat.SetPass(0);
            GL.Clear(true, true, Color.white);
            RenderTexture.active = null;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) HandlePainting();
    }

    void HandlePainting()
    {
        if (Camera.main == null || canvasTexture == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // --- DEBUG SECTION ---
            // If you don't see this Red Ray in the Scene view, the click isn't hitting!
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            
            Vector2 uv = hit.textureCoord;
            
            // If UV is (0,0), it usually means it's a Box Collider, not a Mesh Collider
            if (uv == Vector2.zero)
            {
                Debug.LogWarning("Hit " + hit.collider.name + " but UV is (0,0). Check Collider Type!");
                return;
            }

            PaintPixel(uv);
        }
    }

    void PaintPixel(Vector2 uv)
    {
        RenderTexture.active = canvasTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, canvasTexture.width, canvasTexture.height, 0);

        paintMat.SetPass(0);

        float x = uv.x * canvasTexture.width;
        float y = (1 - uv.y) * canvasTexture.height; // Flipped for GL space

        GL.Begin(GL.QUADS);
        GL.Color(brushColor);
        GL.Vertex3(x - brushSize, y - brushSize, 0);
        GL.Vertex3(x + brushSize, y - brushSize, 0);
        GL.Vertex3(x + brushSize, y + brushSize, 0);
        GL.Vertex3(x - brushSize, y + brushSize, 0);
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = null;
    }
}