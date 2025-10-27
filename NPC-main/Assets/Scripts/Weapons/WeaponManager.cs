using System;
using UnityEngine;

/// <summary>
/// Gestiona el inventario de armas del jugador y el cambio entre ellas.
/// Patrón Singleton local: Solo hay un WeaponManager por jugador.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    [Tooltip("Pistola inicial (se crea automáticamente del WeaponData)")]
    [SerializeField] private Weapon primaryWeapon; // Pistola (siempre disponible)
    
    [Tooltip("Arma especial (se llena al recoger cajas)")]
    [SerializeField] private Weapon specialWeapon; // Arma especial (reemplazable)
    
    [Header("Initial Weapon")]
    [Tooltip("WeaponData de la pistola inicial")]
    [SerializeField] private WeaponData initialPistolData;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode primaryKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode specialKey = KeyCode.Alpha2;
    
    [Header("Weapon Mount Point")]
    [SerializeField] private Transform weaponHolder; // Punto donde se posicionan las armas
    
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera; // Para calcular dirección de disparo
    
    private Weapon currentWeapon;
    private bool isInputEnabled = true;
    
    // Eventos
    public event Action<Weapon> OnWeaponChanged;
    public event Action<Weapon> OnSpecialWeaponPickedUp;
    
    // Properties
    public Weapon CurrentWeapon => currentWeapon;
    public Weapon PrimaryWeapon => primaryWeapon;
    public Weapon SpecialWeapon => specialWeapon;
    public bool HasSpecialWeapon => specialWeapon != null;
    
    private void Awake()
    {
        Debug.Log("=== WEAPON MANAGER AWAKE ===");
        
        // Obtener cámara si no está asignada
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            Debug.Log($"Cámara auto-detectada: {playerCamera != null}");
        }
        else
        {
            Debug.Log($"Cámara asignada: {playerCamera.name}");
        }
            
        if (weaponHolder == null)
        {
            // Crear weapon holder si no existe
            GameObject holder = new GameObject("WeaponHolder");
            holder.transform.SetParent(playerCamera != null ? playerCamera.transform : transform);
            holder.transform.localPosition = new Vector3(0.3f, -0.3f, 0.7f);
            holder.transform.localRotation = Quaternion.identity;
            weaponHolder = holder.transform;
            Debug.Log($"WeaponHolder creado en: {weaponHolder.position}");
        }
        else
        {
            Debug.Log($"WeaponHolder asignado: {weaponHolder.name}");
        }
        
        // DEBUG: Verificar datos
        Debug.Log($"Primary Weapon existe: {primaryWeapon != null}");
        Debug.Log($"Initial Pistol Data existe: {initialPistolData != null}");
        
        // Crear pistola inicial si no está asignada
        if (primaryWeapon == null && initialPistolData != null)
        {
            Debug.Log($"Creando pistola desde: {initialPistolData.name}");
            Debug.Log($"Weapon Type del data: {initialPistolData.weaponType}");
            
            primaryWeapon = CreateWeaponFromData(initialPistolData);
            
            if (primaryWeapon != null)
            {
                Debug.Log($"✅ Pistola creada exitosamente: {primaryWeapon.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ ERROR: CreateWeaponFromData devolvió null!");
            }
        }
        else
        {
            if (primaryWeapon != null)
                Debug.Log("Primary weapon ya existe, no se crea");
            if (initialPistolData == null)
                Debug.LogError("❌ ERROR: initialPistolData NO está asignado!");
        }
        
        // Configurar armas iniciales
        SetupWeapons();
        
        Debug.Log("=== FIN WEAPON MANAGER AWAKE ===");
    }
    
    private void Start()
    {
        Debug.Log("=== WEAPON MANAGER START ===");
        Debug.Log($"Primary Weapon: {(primaryWeapon != null ? primaryWeapon.name : "NULL")}");
        Debug.Log($"Special Weapon: {(specialWeapon != null ? specialWeapon.name : "NULL")}");
        
        // Equipar pistola al inicio
        if (primaryWeapon != null)
        {
            Debug.Log($"Equipando: {primaryWeapon.name}");
            SwitchWeapon(primaryWeapon);
        }
        else
        {
            Debug.LogError("❌ No hay primary weapon para equipar!");
        }
        
        Debug.Log("=== FIN WEAPON MANAGER START ===");
    }
    
    private void Update()
    {
        if (!isInputEnabled) return;
        
        // Input de cambio de arma
        HandleWeaponSwitching();
        
        // Input de disparo
        HandleShooting();
    }
    
    /// <summary>
    /// Configura las armas iniciales.
    /// </summary>
    private void SetupWeapons()
    {
        if (primaryWeapon != null)
        {
            primaryWeapon.transform.SetParent(weaponHolder);
            primaryWeapon.transform.localPosition = Vector3.zero;
            primaryWeapon.transform.localRotation = Quaternion.identity;
            primaryWeapon.Unequip(); // Inicialmente desequipada
        }
        
        if (specialWeapon != null)
        {
            specialWeapon.transform.SetParent(weaponHolder);
            specialWeapon.transform.localPosition = Vector3.zero;
            specialWeapon.transform.localRotation = Quaternion.identity;
            specialWeapon.Unequip();
        }
    }
    
    /// <summary>
    /// Maneja el input de cambio de arma.
    /// </summary>
    private void HandleWeaponSwitching()
    {
        // Tecla 1: Pistola
        if (Input.GetKeyDown(primaryKey) && primaryWeapon != null && currentWeapon != primaryWeapon)
        {
            SwitchWeapon(primaryWeapon);
        }
        
        // Tecla 2: Arma especial
        if (Input.GetKeyDown(specialKey) && specialWeapon != null && currentWeapon != specialWeapon)
        {
            SwitchWeapon(specialWeapon);
        }
    }
    
    /// <summary>
    /// Maneja el input de disparo.
    /// </summary>
    private void HandleShooting()
    {
        if (currentWeapon == null) return;
        
        bool fireInput = currentWeapon.IsAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
        
        if (fireInput)
        {
            bool fired = currentWeapon.TryFire();
            
            // Si el arma especial se quedó sin munición, cambiar a pistola automáticamente
            if (fired && currentWeapon == specialWeapon && !currentWeapon.HasAmmo)
            {
                SwitchWeapon(primaryWeapon);
            }
        }
    }
    
    /// <summary>
    /// Cambia el arma actual.
    /// </summary>
    private void SwitchWeapon(Weapon newWeapon)
    {
        if (newWeapon == null || newWeapon == currentWeapon) return;
        
        // Desequipar arma actual
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
        }
        
        // Equipar nueva arma
        currentWeapon = newWeapon;
        currentWeapon.Equip();
        
        OnWeaponChanged?.Invoke(currentWeapon);
        
        Debug.Log($"Arma cambiada a: {currentWeapon.Data.weaponName}");
    }
    
    /// <summary>
    /// Recoge un arma nueva (reemplaza el slot especial).
    /// </summary>
    public void PickupWeapon(Weapon newWeapon)
    {
        if (newWeapon == null) return;
        
        // Destruir arma especial anterior si existe
        if (specialWeapon != null)
        {
            Destroy(specialWeapon.gameObject);
        }
        
        // Asignar nueva arma
        specialWeapon = newWeapon;
        specialWeapon.transform.SetParent(weaponHolder);
        specialWeapon.transform.localPosition = Vector3.zero;
        specialWeapon.transform.localRotation = Quaternion.identity;
        
        // Cambiar automáticamente a la nueva arma
        SwitchWeapon(specialWeapon);
        
        OnSpecialWeaponPickedUp?.Invoke(specialWeapon);
        
        Debug.Log($"¡Arma especial recogida! {specialWeapon.Data.weaponName}");
    }
    
    /// <summary>
    /// Crea una instancia de un arma desde un WeaponData.
    /// </summary>
    public Weapon CreateWeaponFromData(WeaponData weaponData)
    {
        Debug.Log($"--- CreateWeaponFromData START ---");
        
        if (weaponData == null)
        {
            Debug.LogError("weaponData es NULL!");
            return null;
        }
        
        Debug.Log($"Creando arma: {weaponData.weaponName} (Tipo: {weaponData.weaponType})");
        
        // Crear GameObject para el arma
        GameObject weaponObj = new GameObject(weaponData.weaponName);
        weaponObj.transform.SetParent(weaponHolder);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"GameObject creado: {weaponObj.name} en {weaponObj.transform.position}");
        
        // Agregar componente según el tipo
        Weapon weapon = null;
        switch (weaponData.weaponType)
        {
            case WeaponType.Pistol:
                Debug.Log("Agregando componente Pistol...");
                weapon = weaponObj.AddComponent<Pistol>();
                break;
            case WeaponType.Shotgun:
                Debug.Log("Agregando componente Shotgun...");
                weapon = weaponObj.AddComponent<Shotgun>();
                break;
            case WeaponType.Rifle:
                Debug.Log("Agregando componente Rifle...");
                weapon = weaponObj.AddComponent<Rifle>();
                break;
            case WeaponType.Sniper:
                Debug.Log("Agregando componente Sniper...");
                weapon = weaponObj.AddComponent<Sniper>();
                break;
            case WeaponType.Explosive:
                Debug.Log("Agregando componente Explosive...");
                weapon = weaponObj.AddComponent<Explosive>();
                break;
            default:
                Debug.LogError($"Tipo de arma desconocido: {weaponData.weaponType}");
                break;
        }
        
        if (weapon == null)
        {
            Debug.LogError("No se pudo crear el componente Weapon!");
            Destroy(weaponObj);
            return null;
        }
        
        Debug.Log($"Componente {weapon.GetType().Name} agregado exitosamente");
        
        // Asignar datos mediante reflection (ya que weaponData es protected)
        var field = typeof(Weapon).GetField("weaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(weapon, weaponData);
            Debug.Log("weaponData asignado por reflection");
        }
        else
        {
            Debug.LogError("No se pudo encontrar el campo weaponData!");
        }
        
        // Asignar muzzle point (puede ser la cámara por defecto)
        var muzzleField = typeof(Weapon).GetField("muzzlePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (playerCamera != null && muzzleField != null)
        {
            muzzleField.SetValue(weapon, playerCamera.transform);
            Debug.Log($"muzzlePoint asignado a: {playerCamera.transform.name}");
        }
        
        // Asignar hitLayers - usar LayerMask.GetMask para todo
        var hitLayersField = typeof(Weapon).GetField("hitLayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hitLayersField != null)
        {
            // Crear LayerMask que incluya todas las capas excepto la del jugador
            LayerMask mask = ~0; // Todo
            hitLayersField.SetValue(weapon, mask);
            Debug.Log("hitLayers configurado a Everything");
        }
        
        // IMPORTANTE: Inicializar el arma DESPUÉS de asignar weaponData
        weapon.Initialize();
        Debug.Log("Weapon.Initialize() llamado");
        
        Debug.Log($"--- CreateWeaponFromData END: {weapon != null} ---");
        
        return weapon;
    }
    
    /// <summary>
    /// Habilita/deshabilita el input de armas.
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
    }
}