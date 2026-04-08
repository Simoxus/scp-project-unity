using System;
using TriInspector;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Space]
    [SerializeField, ReadOnly] private HealthLevel healthLevel = HealthLevel.Healthy;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Injury Settings")]
    [SerializeField] private float injuryFactor = 0f;
    [SerializeField, ReadOnly] private float maxInjuryFactor = 3f;

    public event Action<float, float> OnHealthChanged;
    public event Action<HealthLevel> OnHealthLevelChanged;
    public event Action<float> OnInjuryChanged;

    public enum HealthLevel
    {
        Healthy,
        Injured,
        Critical,
        NearDeath,
        Dead
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        BroadcastHealth();
    }

    public void Take(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        BroadcastHealth();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        BroadcastHealth();
    }

    public void Set(float amount)
    {
        currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
        BroadcastHealth();
    }

    private void BroadcastHealth()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        HealthLevel newLevel = GetHealthLevel();
        if (newLevel != healthLevel)
        {
            healthLevel = newLevel;
            OnHealthLevelChanged?.Invoke(newLevel);
        }

        float newInjuryFactor = CalculateInjuryFactor();
        if (!Mathf.Approximately(newInjuryFactor, injuryFactor))
        {
            injuryFactor = newInjuryFactor;
            OnInjuryChanged?.Invoke(injuryFactor);
        }
    }

    private float CalculateInjuryFactor()
    {
        float healthPercent = currentHealth / maxHealth;
        float damage = 1f - healthPercent;

        return Mathf.Clamp(damage * maxInjuryFactor, 0f, maxInjuryFactor);
    }

    private HealthLevel GetHealthLevel()
    {
        float ratio = currentHealth / maxHealth;

        if (currentHealth <= 0f)
            return HealthLevel.Dead;
        if (ratio <= 0.25f)
            return HealthLevel.NearDeath;
        if (ratio <= 0.5f)
            return HealthLevel.Critical;
        if (ratio <= 0.75f)
            return HealthLevel.Injured;
        return HealthLevel.Healthy;
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetInjuryFactor() => injuryFactor;
}
