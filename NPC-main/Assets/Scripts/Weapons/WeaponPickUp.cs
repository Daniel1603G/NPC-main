using System.Collections.Generic;
using UnityEngine;


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
    
   
    private void AnimatePickup()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
   
    private void SetupDefaultWeapons()
    {
        Debug.LogWarning($"No hay armas configuradas en {gameObject.name}. Por favor asigna WeaponData en el inspector.");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        
        var weaponManager = other.GetComponent<WeaponManager>();
        if (weaponManager == null) return;
        
 
        WeaponData selectedWeapon = SelectWeaponRouletteWheel();
        if (selectedWeapon != null)
        {
            GiveWeaponToPlayer(weaponManager, selectedWeapon);
        }
    }
    
  
    private WeaponData SelectWeaponRouletteWheel()
    {
        if (availableWeapons.Count == 0) return null;
        
       
        float totalWeight = 0f;
        foreach (var weaponDrop in availableWeapons)
        {
            if (weaponDrop.weaponData != null)
                totalWeight += weaponDrop.weight;
        }
        
        if (totalWeight <= 0f) return null;
        
     
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
       
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
        
       
        return availableWeapons[availableWeapons.Count - 1].weaponData;
    }
    
  
    private void GiveWeaponToPlayer(WeaponManager weaponManager, WeaponData weaponData)
    {
        isCollected = true;
        
  
        Weapon newWeapon = weaponManager.CreateWeaponFromData(weaponData);
        
     
        weaponManager.PickupWeapon(newWeapon);
        
   
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
    
 
    private void PlayPickupEffects()
    {
        if (pickupSound != null)
            pickupSound.Play();
            
        if (visualEffect != null)
            Instantiate(visualEffect, transform.position, Quaternion.identity);
    }
    

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


[System.Serializable]
public class WeaponDropData
{
    [Tooltip("Datos del arma")]
    public WeaponData weaponData;
    
    [Tooltip("Peso de probabilidad (mayor = más común)")]
    [Range(0.1f, 100f)]
    public float weight = 10f;
}