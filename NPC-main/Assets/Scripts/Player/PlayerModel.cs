using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerModel : MonoBehaviour, ISpin
{
    [Header("Detection Settings")]
    [SerializeField] public bool _isDetectable = true;
    [SerializeField] private Transform[] _detectablePositions;
    
    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private float invisibilityAlpha = 0.3f;
    [SerializeField] private Color invisibilityTint = Color.cyan;
    
    [Header("Audio")]
    [SerializeField] private AudioSource spinAudioSource;
    [SerializeField] private AudioClip spinSound;
    
    // Eventos y delegates
    private Action _onSpin = delegate { };
    
    // Referencias a otros componentes
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PowerUpManager powerUpManager;
    
    // Materiales originales para el efecto de invisibilidad
    private Material[] originalMaterials;
    private Material[] invisibilityMaterials;
    
    // Propiedades públicas requeridas por ISpin
    public bool IsDetectable => _isDetectable;
    public Action OnSpin { get => _onSpin; set => _onSpin = value; }
    
    // Propiedades adicionales útiles
    public Transform[] DetectablePositions => _detectablePositions;
    public Transform Transform => transform;
    public PlayerMovement Movement => playerMovement;
    public PlayerHealth Health => playerHealth;
    public PowerUpManager PowerUps => powerUpManager;
    
    private void Awake()
    {
        // Obtener referencias a componentes
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        powerUpManager = GetComponent<PowerUpManager>();
        
        // Auto-configurar detectablePositions si no están asignadas
        if (_detectablePositions == null || _detectablePositions.Length == 0)
        {
            SetupDefaultDetectablePositions();
        }
        
        // Auto-configurar renderers si no están asignados
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            SetupDefaultRenderers();
        }
        
        // Preparar materiales para efectos visuales
        SetupInvisibilityMaterials();
        
        // Configurar audio si no está asignado
        if (spinAudioSource == null)
        {
            spinAudioSource = GetComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // Actualizar visual inicial
        UpdateVisualState();
        
        // Suscribirse a eventos si es necesario
        _onSpin += OnSpinStateChanged;
    }
    
    private void OnDestroy()
    {
        // Limpiar eventos
        _onSpin -= OnSpinStateChanged;
    }
    
    #region ISpin Implementation
    
    public void Spin()
    {
        _isDetectable = !_isDetectable;
        
        // Reproducir sonido
        PlaySpinSound();
        
        // Actualizar visual
        UpdateVisualState();
        
        // Invocar evento
        _onSpin?.Invoke();
        
        // Log para debug
        Debug.Log($"Jugador {(IsDetectable ? "ahora es DETECTABLE" : "ahora es INVISIBLE")}");
    }
    
    #endregion
    
    #region Setup Methods
    
    private void SetupDefaultDetectablePositions()
    {
        // Crear posiciones detectables por defecto
        // Cabeza, pecho, pies
        _detectablePositions = new Transform[3];
        
        // Posición de la cabeza (cámara si existe)
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            _detectablePositions[0] = playerCamera.transform;
        }
        else
        {
            // Crear un punto en la cabeza
            GameObject headPoint = new GameObject("HeadDetectionPoint");
            headPoint.transform.SetParent(transform);
            headPoint.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            _detectablePositions[0] = headPoint.transform;
        }
        
        // Posición del pecho
        GameObject chestPoint = new GameObject("ChestDetectionPoint");
        chestPoint.transform.SetParent(transform);
        chestPoint.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        _detectablePositions[1] = chestPoint.transform;
        
        // Posición de los pies (transform principal)
        _detectablePositions[2] = transform;
    }
    
    private void SetupDefaultRenderers()
    {
        // Obtener todos los renderers del jugador y sus hijos
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        
        // Filtrar renderers válidos (excluir UI, partículas, etc.)
        System.Collections.Generic.List<Renderer> validRenderers = new System.Collections.Generic.List<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            // Excluir renderers de UI y efectos especiales
            if (renderer.gameObject.layer != LayerMask.NameToLayer("UI") && 
                !(renderer is ParticleSystemRenderer))
            {
                validRenderers.Add(renderer);
            }
        }
        
        playerRenderers = validRenderers.ToArray();
    }
    
    private void SetupInvisibilityMaterials()
    {
        if (playerRenderers == null) return;
        
        originalMaterials = new Material[playerRenderers.Length];
        invisibilityMaterials = new Material[playerRenderers.Length];
        
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null && playerRenderers[i].material != null)
            {
                // Guardar material original
                originalMaterials[i] = playerRenderers[i].material;
                
                // Crear material de invisibilidad
                invisibilityMaterials[i] = new Material(originalMaterials[i]);
                
                // Configurar propiedades para transparencia
                if (invisibilityMaterials[i].HasProperty("_Color"))
                {
                    Color color = invisibilityTint;
                    color.a = invisibilityAlpha;
                    invisibilityMaterials[i].SetColor("_Color", color);
                }
                
                // Configurar modo de renderizado para transparencia
                SetMaterialTransparent(invisibilityMaterials[i]);
            }
        }
    }
    
    private void SetMaterialTransparent(Material material)
    {
        // Configurar material para transparencia
        material.SetFloat("_Mode", 2); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    #endregion
    
    #region Visual and Audio Effects
    
    private void UpdateVisualState()
    {
        if (playerRenderers == null) return;
        
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                if (_isDetectable)
                {
                    // Restaurar material original
                    if (originalMaterials[i] != null)
                        playerRenderers[i].material = originalMaterials[i];
                }
                else
                {
                    // Aplicar material de invisibilidad
                    if (invisibilityMaterials[i] != null)
                        playerRenderers[i].material = invisibilityMaterials[i];
                }
            }
        }
    }
    
    private void PlaySpinSound()
    {
        if (spinAudioSource != null && spinSound != null)
        {
            spinAudioSource.PlayOneShot(spinSound);
        }
    }
    
    private void OnSpinStateChanged()
    {
        // Aquí puedes agregar efectos adicionales cuando cambia el estado
        // Por ejemplo: partículas, screen shake, etc.
        
        // Ejemplo: efecto de partículas simple
        Debug.Log($"Estado de detección cambiado: {(IsDetectable ? "Visible" : "Invisible")}");
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Fuerza el estado de detectabilidad sin activar efectos
    /// </summary>
    public void SetDetectable(bool detectable, bool playEffects = true)
    {
        bool changed = _isDetectable != detectable;
        _isDetectable = detectable;
        
        if (changed)
        {
            if (playEffects)
            {
                PlaySpinSound();
                _onSpin?.Invoke();
            }
            
            UpdateVisualState();
            Debug.Log($"Detectabilidad forzada a: {(IsDetectable ? "Visible" : "Invisible")}");
        }
    }
    
    /// <summary>
    /// Obtiene información completa del estado del jugador
    /// </summary>
    public string GetPlayerStatus()
    {
        string status = $"=== ESTADO DEL JUGADOR ===\n";
        status += $"Detectable: {IsDetectable}\n";
        status += $"Salud: {Health?.CurrentHealth:F1}/{Health?.MaxHealth:F1}\n";
        status += $"Velocidad: {Movement?.moveSpeed:F1}\n";
        status += $"Corriendo: {Movement?.IsSprinting}\n";
        
        if (PowerUps != null)
        {
            status += $"Power-ups activos:\n";
            if (PowerUps.IsInvisible) status += "  - Invisibilidad\n";
            if (PowerUps.IsSpeedBoostActive) status += "  - Velocidad\n";
            if (PowerUps.HasInfiniteSprint) status += "  - Sprint Infinito\n";
            if (PowerUps.HasSuperJump) status += "  - Súper Salto\n";
        }
        
        return status;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Dibujar posiciones detectables
        if (_detectablePositions != null)
        {
            Gizmos.color = IsDetectable ? Color.red : Color.green;
            
            foreach (Transform detectablePos in _detectablePositions)
            {
                if (detectablePos != null)
                {
                    Gizmos.DrawWireSphere(detectablePos.position, 0.2f);
                }
            }
        }
    }
    
    #endregion
}