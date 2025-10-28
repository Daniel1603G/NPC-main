using UnityEngine;

/// <summary>
/// Crea una línea visual mejorada que representa la trayectoria de una bala.
/// Ahora con glow, grosor animado y efectos de partículas.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 4f;
    [SerializeField] private float width = 0.05f;
    [SerializeField] private bool useGlow = true;
    [SerializeField] private AnimationCurve widthCurve = AnimationCurve.Linear(0, 1, 1, 0.3f);
    
    private LineRenderer lineRenderer;
    private float alpha = 1f;
    private float lifeTime = 0f;
    private Color startColor;
    private Color endColor;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        

        if (useGlow)
        {
         
            Material glowMat = new Material(Shader.Find("Particles/Standard Unlit"));
            glowMat.EnableKeyword("_EMISSION");
            lineRenderer.material = glowMat;
        }
        else
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
      
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width * 0.3f;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.numCapVertices = 2;
        

        startColor = lineRenderer.startColor;
        endColor = lineRenderer.endColor;
    }
    
    private void Update()
    {
        lifeTime += Time.deltaTime;
        

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
        

        float widthMultiplier = widthCurve.Evaluate(lifeTime * fadeSpeed);
        lineRenderer.startWidth = width * widthMultiplier;
        lineRenderer.endWidth = width * 0.3f * widthMultiplier;
    }
    
 
    public void Initialize(Vector3 start, Vector3 end, Color color)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
     
        Color brightColor = color * 2f; 
        brightColor.a = 1f;
        
        lineRenderer.startColor = brightColor;
        lineRenderer.endColor = brightColor * 0.5f;
        
        startColor = brightColor;
        endColor = brightColor * 0.5f;
        
     
        if (useGlow)
        {
            lineRenderer.material.SetColor("_EmissionColor", color * 3f);
        }
    }
    

    public static void Create(Vector3 start, Vector3 end, Color color, float width = 0.08f)
    {
        GameObject tracerObj = new GameObject("BulletTracer");
        tracerObj.transform.position = start;
        
        LineRenderer lr = tracerObj.AddComponent<LineRenderer>();
        
        // Usar shader de partículas para glow
        Material glowMat = new Material(Shader.Find("Particles/Standard Unlit"));
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", color * 3f);
        lr.material = glowMat;
        
        // Color más brillante
        Color brightColor = color * 2f;
        brightColor.a = 1f;
        
        lr.startColor = brightColor;
        lr.endColor = brightColor * 0.5f;
        lr.startWidth = width;
        lr.endWidth = width * 0.3f;
        lr.positionCount = 2;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        BulletTracer tracer = tracerObj.AddComponent<BulletTracer>();
        tracer.width = width;
        tracer.useGlow = true;
        tracer.Initialize(start, end, color);
        
        // Agregar una pequeña luz en el punto de origen (opcional)
        CreateMuzzleLight(start, color);
        
        Destroy(tracerObj, 0.5f);
    }
    

    private static void CreateMuzzleLight(Vector3 position, Color color)
    {
        GameObject lightObj = new GameObject("MuzzleLight");
        lightObj.transform.position = position;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 2f;
        light.range = 3f;
        
        Destroy(lightObj, 0.1f);
    }
}