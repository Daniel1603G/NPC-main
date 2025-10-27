using UnityEngine;

/// <summary>
/// Crea una línea visual que representa la trayectoria de una bala.
/// Se desvanece rápidamente después de aparecer.
/// Usado por armas basadas en raycast para feedback visual.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 4f;
    [SerializeField] private float width = 0.05f;
    
    private LineRenderer lineRenderer;
    private float alpha = 1f;
    private Color startColor;
    private Color endColor;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width * 0.5f;

      
        startColor = lineRenderer.startColor;
        endColor = lineRenderer.endColor;
    }
    
    private void Update()
    {

        alpha -= fadeSpeed * Time.deltaTime;
        
        if (alpha <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        
       
        startColor.a = alpha;
        endColor.a = alpha * 0.5f;
        
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
    }
    
 
    public void Initialize(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
 
    public static void Create(Vector3 start, Vector3 end, Color color, float width = 0.05f)
    {
        GameObject tracerObj = new GameObject("BulletTracer");
        
        LineRenderer lr = tracerObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color * 0.5f;
        lr.startWidth = width;
        lr.endWidth = width * 0.5f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        BulletTracer tracer = tracerObj.AddComponent<BulletTracer>();
        tracer.width = width;
        
        Destroy(tracerObj, 0.5f);
    }
}