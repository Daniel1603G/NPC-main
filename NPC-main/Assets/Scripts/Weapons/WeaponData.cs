using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("Nombre del arma")]
    public string weaponName = "Weapon";
    
    [Tooltip("Tipo de arma")]
    public WeaponType weaponType;
    
    [Header("Estadísticas de Combate")]
    [Tooltip("Daño por disparo")]
    [Range(1f, 200f)]
    public float damage = 10f;
    
    [Tooltip("Alcance máximo del disparo en metros")]
    [Range(5f, 500f)]
    public float range = 100f;
    
    [Tooltip("Tiempo entre disparos en segundos")]
    [Range(0.05f, 5f)]
    public float fireRate = 0.5f;
    
    [Header("Munición")]
    [Tooltip("Cantidad de munición inicial (-1 = infinita)")]
    public int ammoCapacity = -1;
    
    [Tooltip("¿Es un arma automática? (mantener presionado para disparar)")]
    public bool isAutomatic = false;
    
    [Header("Efectos de Disparo Múltiple (Shotgun)")]
    [Tooltip("Cantidad de proyectiles por disparo")]
    [Range(1, 20)]
    public int projectilesPerShot = 1;
    
    [Tooltip("Dispersión del disparo en grados")]
    [Range(0f, 30f)]
    public float spreadAngle = 0f;
    
    [Header("Proyectil (Explosive)")]
    [Tooltip("Prefab del proyectil (solo para armas explosivas)")]
    public GameObject projectilePrefab;
    
    [Tooltip("Velocidad del proyectil")]
    [Range(5f, 100f)]
    public float projectileSpeed = 20f;
    
    [Tooltip("Radio de explosión")]
    [Range(0f, 20f)]
    public float explosionRadius = 5f;
    
    [Header("Visuales y Audio")]
    [Tooltip("Prefab del modelo del arma")]
    public GameObject weaponModelPrefab;
    
    [Tooltip("Efecto de partículas del disparo")]
    public GameObject muzzleFlashEffect;
    
    [Tooltip("Efecto de impacto en superficies")]
    public GameObject impactEffect;
    
    [Tooltip("Sonido del disparo")]
    public AudioClip fireSound;
    
    [Tooltip("Sonido al recargar/agotar munición")]
    public AudioClip emptySound;
    
    [Header("UI")]
    [Tooltip("Icono del arma para la UI")]
    public Sprite weaponIcon;
    
    [Tooltip("Color representativo del arma")]
    public Color weaponColor = Color.white;
}

/// <summary>
/// Enumeración de tipos de armas.
/// Facilita la identificación y permite filtros.
/// </summary>
public enum WeaponType
{
    Pistol,
    Shotgun,
    Rifle,
    Sniper,
    Explosive
}