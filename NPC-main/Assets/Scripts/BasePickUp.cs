using UnityEngine;

/// <summary>
/// Clase base para todos los pickups del juego.
/// Contiene la lógica común de animación, respawn, etc.
/// Patrón Template Method.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class BasePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("¿Reaparece después de recogerlo?")]
    [SerializeField] protected bool respawns = false;
    
    [Tooltip("Tiempo de reaparición en segundos")]
    [SerializeField] protected float respawnTime = 30f;
    
    [Header("Visual Feedback")]
    [SerializeField] protected GameObject visualEffect;
    [SerializeField] protected AudioSource pickupSound;
    
    [Header("Animation")]
    [SerializeField] protected float rotationSpeed = 90f;
    [SerializeField] protected float bobSpeed = 2f;
    [SerializeField] protected float bobHeight = 0.3f;
    
    protected Vector3 startPosition;
    protected bool isCollected = false;
    protected Renderer objectRenderer;
    
    protected virtual void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    
    protected virtual void Awake()
    {
        startPosition = transform.position;
        objectRenderer = GetComponentInChildren<Renderer>();
    }
    
    protected virtual void Update()
    {
        if (!isCollected)
        {
            AnimatePickup();
        }
    }
    
    /// <summary>
    /// Animación común: rotación y flotación.
    /// </summary>
    protected virtual void AnimatePickup()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        
        // Template Method: las subclases implementan la lógica específica
        if (OnPickup(other))
        {
            isCollected = true;
            PlayPickupEffects();
            
            if (respawns)
            {
                StartCoroutine(RespawnAfterDelay());
            }
            else
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }
    
    /// <summary>
    /// Método abstracto: cada pickup implementa su lógica.
    /// Retorna true si el pickup fue exitoso.
    /// </summary>
    protected abstract bool OnPickup(Collider player);
    
    /// <summary>
    /// Reproduce efectos visuales y de audio.
    /// </summary>
    protected virtual void PlayPickupEffects()
    {
        if (pickupSound != null)
            pickupSound.Play();
            
        if (visualEffect != null)
            Instantiate(visualEffect, transform.position, Quaternion.identity);
    }
    
    /// <summary>
    /// Reaparece después del tiempo configurado.
    /// </summary>
    protected virtual System.Collections.IEnumerator RespawnAfterDelay()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        
        if (gameObject != null)
        {
            isCollected = false;
            gameObject.SetActive(true);
            
            if (objectRenderer != null)
                objectRenderer.material.color = Color.white;
        }
    }
}