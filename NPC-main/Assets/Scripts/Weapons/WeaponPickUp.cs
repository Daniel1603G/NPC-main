using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caja de arma que el jugador puede recoger.
/// Similar a PowerUpPickup pero para armas.
/// Usa Roulette Wheel Selection para elegir arma aleatoria.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Pool")]
    [Tooltip("Lista de armas disponibles con sus pesos para roulette wheel")]
    [SerializeField] private List<WeaponDropData> availableWeapons = new List<WeaponDropData>();
    
    [Header("Pickup Settings")]
    [Tooltip("¿Reaparece la caja después de recogerla?")]
    [SerializeField] private bool respawns = false;
    
    [Tooltip("Tiempo de reaparición en segundos")]
    [SerializeField] private float respawnTime = 60f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private AudioSource pickupSound;
    
    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private Renderer objectRenderer;
    
    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    
    private void Awake()
    {
        startPosition = transform.position;
        objectRenderer = GetComponentInChildren<Renderer>();
        
        if (availableWeapons.Count == 0)
        {
            SetupDefaultWeapons();
        }
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            AnimatePickup();
        }
    }
    
    /// <summary>
    /// Anima la caja (rotación y flotación).
    /// </summary>
    private void AnimatePickup()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    /// <summary>
    /// Configura armas por defecto si no hay ninguna asignada.
    /// </summary>
    private void SetupDefaultWeapons()
    {
        Debug.LogWarning($"No hay armas configuradas en {gameObject.name}. Por favor asigna WeaponData en el inspector.");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        
        var weaponManager = other.GetComponent<WeaponManager>();
        if (weaponManager == null) return;
        
        // Seleccionar arma usando Roulette Wheel Selection
        WeaponData selectedWeapon = SelectWeaponRouletteWheel();
        if (selectedWeapon != null)
        {
            GiveWeaponToPlayer(weaponManager, selectedWeapon);
        }
    }
    
    /// <summary>
    /// Selecciona un arma usando Roulette Wheel Selection.
    /// Las armas con mayor peso tienen más probabilidad de salir.
    /// </summary>
    private WeaponData SelectWeaponRouletteWheel()
    {
        if (availableWeapons.Count == 0) return null;
        
        // Calcular peso total
        float totalWeight = 0f;
        foreach (var weaponDrop in availableWeapons)
        {
            if (weaponDrop.weaponData != null)
                totalWeight += weaponDrop.weight;
        }
        
        if (totalWeight <= 0f) return null;
        
        // Generar número aleatorio en el rango del peso total
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // Iterar hasta encontrar el arma seleccionada
        foreach (var weaponDrop in availableWeapons)
        {
            if (weaponDrop.weaponData == null) continue;
            
            currentWeight += weaponDrop.weight;
            if (randomValue <= currentWeight)
            {
                Debug.Log($"Arma seleccionada: {weaponDrop.weaponData.weaponName} (Peso: {weaponDrop.weight})");
                return weaponDrop.weaponData;
            }
        }
        
        // Fallback: devolver la última arma
        return availableWeapons[availableWeapons.Count - 1].weaponData;
    }
    
    /// <summary>
    /// Da el arma al jugador.
    /// </summary>
    private void GiveWeaponToPlayer(WeaponManager weaponManager, WeaponData weaponData)
    {
        isCollected = true;
        
        // Crear instancia del arma
        Weapon newWeapon = weaponManager.CreateWeaponFromData(weaponData);
        
        // Dársela al jugador
        weaponManager.PickupWeapon(newWeapon);
        
        // Efectos
        PlayPickupEffects();
        
        // Respawn o destruir
        if (respawns)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }
    
    /// <summary>
    /// Reproduce efectos visuales y de audio.
    /// </summary>
    private void PlayPickupEffects()
    {
        if (pickupSound != null)
            pickupSound.Play();
            
        if (visualEffect != null)
            Instantiate(visualEffect, transform.position, Quaternion.identity);
    }
    
    /// <summary>
    /// Reaparece la caja después del tiempo configurado.
    /// </summary>
    private System.Collections.IEnumerator RespawnAfterDelay()
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

/// <summary>
/// Estructura que asocia un WeaponData con su peso de probabilidad.
/// Usada para Roulette Wheel Selection.
/// </summary>
[System.Serializable]
public class WeaponDropData
{
    [Tooltip("Datos del arma")]
    public WeaponData weaponData;
    
    [Tooltip("Peso de probabilidad (mayor = más común)")]
    [Range(0.1f, 100f)]
    public float weight = 10f;
}