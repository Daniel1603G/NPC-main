using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private List<PowerUpEffect> availableEffects = new List<PowerUpEffect>();
    [SerializeField] private bool respawns = false;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private AudioSource pickupSound;
    
    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private Renderer objectRenderer;
    
    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        startPosition = transform.position;
        objectRenderer = GetComponentInChildren<Renderer>();
        
        if (availableEffects.Count == 0)
        {
            SetupDefaultEffects();
        }
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            AnimatePowerUp();
        }
    }
    
    private void AnimatePowerUp()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    private void SetupDefaultEffects()
    {
        availableEffects.Add(new PowerUpEffect("Speed       ", 25f, 8f, Color.yellow, "Aumenta la velocidad de movimiento"));
        availableEffects.Add(new PowerUpEffect("Invisibility", 15f, 5f, Color.cyan, "Te vuelve invisible a los enemigos"));
        availableEffects.Add(new PowerUpEffect("Infinite Sprint", 20f, 10f, Color.green, "Sprint ilimitado sin cooldown"));
        availableEffects.Add(new PowerUpEffect("Healing", 25f, 0f, Color.red, "Restaura salud completamente"));
        availableEffects.Add(new PowerUpEffect("Super Jump", 15f, 12f, Color.magenta, "Aumenta la altura de salto"));
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        
        var powerUpManager = other.GetComponent<PowerUpManager>();
        if (powerUpManager == null) return;
        
        PowerUpEffect selectedEffect = SelectRandomEffect();
        if (selectedEffect != null)
        {
            powerUpManager.ApplyPowerUp(selectedEffect);
            CollectPowerUp(selectedEffect);
        }
    }
    
    private PowerUpEffect SelectRandomEffect()
    {
        if (availableEffects.Count == 0) return null;
        
        float totalWeight = 0f;
        foreach (var effect in availableEffects)
        {
            totalWeight += effect.Weight;
        }
        
        if (totalWeight <= 0f) return null;
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var effect in availableEffects)
        {
            currentWeight += effect.Weight;
            if (randomValue <= currentWeight)
            {
                Debug.Log($"Power-up seleccionado: {effect.EffectName}");
                return effect;
            }
        }
        
        return availableEffects[availableEffects.Count - 1];
    }
    
    private void CollectPowerUp(PowerUpEffect effect)
    {
        isCollected = true;
        
        if (pickupSound != null)
            pickupSound.Play();
            
        if (visualEffect != null)
            Instantiate(visualEffect, transform.position, Quaternion.identity);
        
        if (objectRenderer != null)
            objectRenderer.material.color = effect.EffectColor;
        
        if (respawns)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }
    
    private IEnumerator RespawnAfterDelay()
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