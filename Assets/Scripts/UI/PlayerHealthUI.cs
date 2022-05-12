using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    Text levelText;
    Image healthSlider, expSlider;
    private void Awake()
    {
        levelText = transform.GetChild(0).GetComponent<Text>();
        healthSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(2).GetChild(0).GetComponent<Image>();
    }
    void Update()
    {
        levelText.text = "Level " + GameManager.Instance.playerStats.characterData.currentLevel.ToString("00");
        UpdateHealth();
        UpdateExp();
    }
    void UpdateHealth()
    {
        healthSlider.fillAmount = (float)GameManager.Instance.playerStats.CurrentHealth / GameManager.Instance.playerStats.MaxHealth;
    }
    void UpdateExp()
    {
        expSlider.fillAmount = (float)GameManager.Instance.playerStats.characterData.currentExp / GameManager.Instance.playerStats.characterData.baseExp;
    }
}
