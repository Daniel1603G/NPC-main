using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PowerUpEffect
{
    [SerializeField] private string effectName;
    [SerializeField] private float weight;
    [SerializeField] private float duration;
    [SerializeField] private Color effectColor = Color.white;
    [SerializeField] private string description;
    
    public string EffectName => effectName;
    public float Weight => weight;
    public float Duration => duration;
    public Color EffectColor => effectColor;
    public string Description => description;
    
    public PowerUpEffect(string name, float weight, float duration, Color color, string desc)
    {
        this.effectName = name;
        this.weight = weight;
        this.duration = duration;
        this.effectColor = color;
        this.description = desc;
    }
}
