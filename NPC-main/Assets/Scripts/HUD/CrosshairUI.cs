using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Crosshair Parts")]
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine;
    [SerializeField] private RectTransform leftLine;
    [SerializeField] private RectTransform rightLine;
    [SerializeField] private Image centerDot;
    
    [Header("Settings")]
    [SerializeField] private float baseSpread = 15f; 
    [SerializeField] private float movingSpreadMultiplier = 1.5f;
    [SerializeField] private float sprintSpreadMultiplier = 2f;
    [SerializeField] private float shootSpreadIncrease = 10f;
    [SerializeField] private float spreadSmoothSpeed = 8f;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color friendlyColor = Color.green;
    
    [Header("Hit Detection")]
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private float rayDistance = 100f;
    
    private float currentSpread;
    private float targetSpread;
    private Camera playerCamera;
    
    private Image[] crosshairImages;
    
    private void Awake()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();
            
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
            
        playerCamera = Camera.main;
        

        crosshairImages = new Image[]
        {
            topLine?.GetComponent<Image>(),
            bottomLine?.GetComponent<Image>(),
            leftLine?.GetComponent<Image>(),
            rightLine?.GetComponent<Image>()
        };
        
        currentSpread = baseSpread;
        targetSpread = baseSpread;
    }
    
    private void OnEnable()
    {
        if (weaponManager != null && weaponManager.CurrentWeapon != null)
        {
            weaponManager.CurrentWeapon.OnWeaponFired += OnWeaponFired;
        }
        
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += OnWeaponChanged;
        }
    }
    
    private void OnDisable()
    {
        if (weaponManager != null && weaponManager.CurrentWeapon != null)
        {
            weaponManager.CurrentWeapon.OnWeaponFired -= OnWeaponFired;
        }
        
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
        }
    }
    
    private void Update()
    {
        UpdateCrosshairSpread();
        UpdateCrosshairPosition();
        UpdateCrosshairColor();
    }
    

    private void UpdateCrosshairSpread()
    {
    
        targetSpread = baseSpread;
        
        if (playerMovement != null)
        {
        
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (input.sqrMagnitude > 0.01f)
            {
                targetSpread *= movingSpreadMultiplier;
            }
            
   
            if (playerMovement.IsSprinting)
            {
                targetSpread *= sprintSpreadMultiplier;
            }
        }
        
 
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * spreadSmoothSpeed);
    }
    
    
    private void UpdateCrosshairPosition()
    {
        if (topLine != null)
            topLine.anchoredPosition = new Vector2(0, currentSpread);
            
        if (bottomLine != null)
            bottomLine.anchoredPosition = new Vector2(0, -currentSpread);
            
        if (leftLine != null)
            leftLine.anchoredPosition = new Vector2(-currentSpread, 0);
            
        if (rightLine != null)
            rightLine.anchoredPosition = new Vector2(currentSpread, 0);
    }
    
 
    private void UpdateCrosshairColor()
    {
        Color targetColor = normalColor;
        
        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, hitLayers))
            {
           
                if (hit.collider.GetComponent<EnemyHealth>() != null)
                {
                    targetColor = enemyColor;
                }
           
                else if (hit.collider.GetComponent<PlayerHealth>() != null)
                {
                    targetColor = friendlyColor;
                }
            }
        }
        

        foreach (var img in crosshairImages)
        {
            if (img != null)
                img.color = targetColor;
        }
        
        if (centerDot != null)
            centerDot.color = targetColor;
    }
    
 
    private void OnWeaponFired()
    {
      
        currentSpread += shootSpreadIncrease;
    }
    
 
    private void OnWeaponChanged(Weapon newWeapon)
    {

        newWeapon.OnWeaponFired += OnWeaponFired;
    }
}