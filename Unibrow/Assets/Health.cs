using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int hp;

    [Header("I-frames")]
    [SerializeField] private float invulnTime = 0.35f;
    private float invulnUntil;

    public int MaxHp => maxHp;
    public int Hp => hp;

    public event Action<int, int> OnHealthChanged; // (hp, maxHp)
    public event Action OnDied;

    private void Awake()
    {
        hp = Mathf.Clamp(hp, 1, maxHp);
        if (hp == 0) hp = maxHp;

        OnHealthChanged?.Invoke(hp, maxHp);
        
    }

    public bool CanTakeDamage() => Time.time >= invulnUntil && hp > 0;

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (!CanTakeDamage()) return;

        invulnUntil = Time.time + invulnTime;
        hp = Mathf.Max(0, hp - amount);

        OnHealthChanged?.Invoke(hp, maxHp);

        if (hp == 0)
            OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (hp <= 0) return; // optional: don’t heal if dead

        hp = Mathf.Min(maxHp, hp + amount);
        OnHealthChanged?.Invoke(hp, maxHp);
    }

    public void SetMaxHp(int newMax, bool fill = true)
    {
        maxHp = Mathf.Max(1, newMax);
        hp = fill ? maxHp : Mathf.Clamp(hp, 0, maxHp);
        OnHealthChanged?.Invoke(hp, maxHp);
    }
}