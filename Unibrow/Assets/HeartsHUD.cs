using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image heartPrefab; // a simple UI Image prefab

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private readonly List<Image> hearts = new();

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += Refresh;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= Refresh;
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("HeartsHUD: playerHealth is not assigned.");
            return;
        }

        BuildHearts(playerHealth.MaxHp);
        Refresh(playerHealth.Hp, playerHealth.MaxHp);
    }

    private void BuildHearts(int maxHp)
    {
        // Clear old
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        hearts.Clear();

        for (int i = 0; i < maxHp; i++)
        {
            var img = Instantiate(heartPrefab, transform);
            img.sprite = fullHeart;
            hearts.Add(img);
        }
    }

    private void Refresh(int hp, int maxHp)
    {
        // If max HP changes at runtime, rebuild
        if (hearts.Count != maxHp)
            BuildHearts(maxHp);

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].sprite = (i < hp) ? fullHeart : emptyHeart;
    }
}