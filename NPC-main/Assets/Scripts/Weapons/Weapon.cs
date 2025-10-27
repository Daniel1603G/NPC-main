using System;
using UnityEngine;

/// <summary>
/// Clase base abstracta para todas las armas.
/// Contiene la lógica común y define el contrato que deben cumplir las armas específicas.
/// Patrón Template Method: Define el esqueleto del algoritmo de disparo.
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] protected WeaponData weaponData;
    
    [Header("References")]
    [SerializeField] protected Transform muzzlePoint; // Punto desde donde sale el disparo
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected LayerMask hitLayers; // Capas que pueden ser impactadas
    
    // Estado interno
    protected int currentAmmo;
    protected float nextFireTime;
    protected GameObject currentWeaponModel;
    protected bool isEquipped = false;
    
    // Eventos
    public event Action<int> OnAmmoChanged;
    public event Action OnWeaponFired;
    public event Action OnAmmoEmpty;
    
    // Properties
    public WeaponData Data => weaponData;
    public int CurrentAmmo => currentAmmo;
    public bool HasAmmo => weaponData.ammoCapacity < 0 || currentAmmo > 0;
    public bool CanFire => Time.time >= nextFireTime && HasAmmo && isEquipped;
    public bool IsAutomatic => weaponData.isAutomatic;
    
    protected virtual void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // NO inicializar munición aquí - se hace después de asignar weaponData
    }
    
    /// <summary>
    /// Inicializa el arma después de que weaponData ha sido asignado.
    /// Llamado manualmente desde WeaponManager.
    /// </summary>
    public virtual void Initialize()
    {
        InitializeAmmo();
    }
    
    /// <summary>
    /// Inicializa la munición del arma.
    /// </summary>
    protected virtual void InitializeAmmo()
    {
        if (weaponData == null)
        {
            Debug.LogWarning("weaponData es null en InitializeAmmo");
            return;
        }
        
        currentAmmo = weaponData.ammoCapacity;
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    /// <summary>
    /// Equipa el arma (la hace visible y activa).
    /// </summary>
    public virtual void Equip()
    {
        isEquipped = true;
        
        // Instanciar modelo del arma si existe
        if (weaponData.weaponModelPrefab != null && currentWeaponModel == null)
        {
            currentWeaponModel = Instantiate(weaponData.weaponModelPrefab, transform);
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }
        
        if (currentWeaponModel != null)
            currentWeaponModel.SetActive(true);
    }
    
    /// <summary>
    /// Desequipa el arma (la oculta).
    /// </summary>
    public virtual void Unequip()
    {
        isEquipped = false;
        
        if (currentWeaponModel != null)
            currentWeaponModel.SetActive(false);
    }
    
    /// <summary>
    /// Intenta disparar el arma.
    /// Template Method: Define el flujo general del disparo.
    /// </summary>
    public virtual bool TryFire()
    {
        if (!CanFire)
        {
            // Si no tiene munición, reproducir sonido de vacío
            if (!HasAmmo)
            {
                PlayEmptySound();
                OnAmmoEmpty?.Invoke();
            }
            return false;
        }
        
        // Ejecutar disparo específico de cada arma
        PerformShot();
        
        // Consumir munición
        ConsumeAmmo();
        
        // Efectos comunes
        PlayFireEffects();
        
        // Actualizar cooldown
        nextFireTime = Time.time + weaponData.fireRate;
        
        // Disparar evento
        OnWeaponFired?.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// Método abstracto que cada arma debe implementar.
    /// Define el comportamiento específico del disparo.
    /// </summary>
    protected abstract void PerformShot();
    
    /// <summary>
    /// Consume munición si el arma no es infinita.
    /// </summary>
    protected virtual void ConsumeAmmo()
    {
        if (weaponData.ammoCapacity < 0) return; // Munición infinita
        
        currentAmmo--;
        currentAmmo = Mathf.Max(0, currentAmmo);
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    /// <summary>
    /// Reproduce los efectos visuales y de audio del disparo.
    /// </summary>
    protected virtual void PlayFireEffects()
    {
        // Efecto de muzzle flash
        if (weaponData.muzzleFlashEffect != null && muzzlePoint != null)
        {
            GameObject muzzle = Instantiate(weaponData.muzzleFlashEffect, muzzlePoint.position, muzzlePoint.rotation);
            Destroy(muzzle, 2f);
        }
        
        // Sonido del disparo
        if (weaponData.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.fireSound);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido cuando el arma está vacía.
    /// </summary>
    protected virtual void PlayEmptySound()
    {
        if (weaponData.emptySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.emptySound);
        }
    }
    
    /// <summary>
    /// Crea el efecto de impacto en una superficie.
    /// </summary>
    protected virtual void CreateImpactEffect(Vector3 position, Quaternion rotation)
    {
        if (weaponData.impactEffect != null)
        {
            GameObject impact = Instantiate(weaponData.impactEffect, position, rotation);
            Destroy(impact, 2f);
        }
    }
    
    /// <summary>
    /// Aplica daño a un objetivo.
    /// </summary>
    protected virtual void ApplyDamage(GameObject target, float damage)
    {
        // Intentar aplicar daño al PlayerHealth
        var playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Daño aplicado a jugador: {damage}");
            return;
        }
        
        // Aquí puedes agregar más interfaces de daño
        // Por ejemplo, si tus enemigos tienen un componente de salud
        var enemyHealth = target.GetComponent<GuardAI>();
        if (enemyHealth != null)
        {
            // Implementar daño a enemigos si lo necesitas
            Debug.Log($"Impacto en enemigo: {target.name} - Daño: {damage}");
        }
    }
    
    /// <summary>
    /// Dispara un raycast desde el muzzle point.
    /// Helper method para armas basadas en raycast.
    /// </summary>
    protected bool FireRaycast(out RaycastHit hit, Vector3 direction)
    {
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        return Physics.Raycast(origin, direction, out hit, weaponData.range, hitLayers);
    }
    
    /// <summary>
    /// Obtiene una dirección con dispersión aplicada.
    /// Útil para shotguns y armas con spread.
    /// </summary>
    protected Vector3 GetSpreadDirection(Vector3 baseDirection)
    {
        if (weaponData.spreadAngle <= 0)
            return baseDirection;
            
        float spreadX = UnityEngine.Random.Range(-weaponData.spreadAngle, weaponData.spreadAngle);
        float spreadY = UnityEngine.Random.Range(-weaponData.spreadAngle, weaponData.spreadAngle);
        
        Quaternion spread = Quaternion.Euler(spreadX, spreadY, 0);
        return spread * baseDirection;
    }
}