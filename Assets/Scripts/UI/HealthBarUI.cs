using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform barPoint;
    [SerializeField] private bool alwaysVisiable;
    [SerializeField] private float visiableTime;
    Image healthSlider;
    Transform UIBar, camera;
    CharacterStats characterStats;
    private float timeLeft;
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        characterStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }
    private void OnEnable()
    {
        camera = Camera.main.transform;
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                UIBar = Instantiate(healthBarPrefab, canvas.transform).transform;
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisiable);
            }
        }
    }
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            Destroy(UIBar.gameObject);
        }

        UIBar.gameObject.SetActive(true);
        healthSlider.fillAmount = (float)currentHealth / (float)maxHealth;
        timeLeft = visiableTime;
    }
    private void LateUpdate()
    {
        if(UIBar != null)
        {
            UIBar.position = barPoint.position;
            UIBar.forward = -camera.forward;
            if (timeLeft <= 0 && !alwaysVisiable)
            {
                UIBar.gameObject.SetActive(false);
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
