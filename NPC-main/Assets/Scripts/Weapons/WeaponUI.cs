using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI para mostrar información del arma actual.
/// Opcional pero recomendado para feedback visual.
/// </summary>
public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image crosshair;
    
    [Header("Colors")]
    [SerializeField] private Color normalAmmoColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color emptyAmmoColor = Color.red;
    [SerializeField] private float lowAmmoThreshold = 0.25f; // 25%
    
    private Weapon currentWeapon;
    
    private void Awake()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();
    }
    
    private void OnEnable()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += UpdateWeaponUI;
        }
    }
    
    private void OnDisable()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= UpdateWeaponUI;
        }
        
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
        }
    }
    
    private void Update()
    {
        // Actualizar munición en tiempo real
        if (currentWeapon != null)
        {
            UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
        }
    }
    
    /// <summary>
    /// Actualiza toda la UI cuando cambia el arma.
    /// </summary>
    private void UpdateWeaponUI(Weapon newWeapon)
    {
        // Desuscribirse del arma anterior
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
        }
        
        currentWeapon = newWeapon;
        
        if (currentWeapon == null) return;
        
        // Suscribirse al evento de munición del arma nueva
        currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
        
        // Actualizar nombre
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.Data.weaponName.ToUpper();
            weaponNameText.color = currentWeapon.Data.weaponColor;
        }
        
        // Actualizar icono
        if (weaponIcon != null && currentWeapon.Data.weaponIcon != null)
        {
            weaponIcon.sprite = currentWeapon.Data.weaponIcon;
            weaponIcon.color = Color.white;
        }
        
        // Actualizar munición
        UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
    }
    
    /// <summary>
    /// Actualiza la visualización de munición.
    /// </summary>
    private void UpdateAmmoDisplay(int ammo)
    {
        if (ammoText == null || currentWeapon == null) return;
        
        // Texto de munición
        if (currentWeapon.Data.ammoCapacity < 0)
        {
            ammoText.text = "∞"; // Infinito
            ammoText.color = normalAmmoColor;
        }
        else
        {
            ammoText.text = $"{ammo} / {currentWeapon.Data.ammoCapacity}";
            
            // Cambiar color según munición restante
            float ammoPercentage = (float)ammo / currentWeapon.Data.ammoCapacity;
            
            if (ammo <= 0)
                ammoText.color = emptyAmmoColor;
            else if (ammoPercentage <= lowAmmoThreshold)
                ammoText.color = lowAmmoColor;
            else
                ammoText.color = normalAmmoColor;
        }
    }
}