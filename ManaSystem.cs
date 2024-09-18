using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class ManaSystem : MonoBehaviour
{
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 5f;

    [Header("UI")]
    public TextMeshProUGUI manaText;
    public Image manaBar;            

    void Start()
    {
        currentMana = maxMana;
        UpdateManaUI();
    }

    void Update()
    {
        RegenerateMana();
        UpdateManaUI(); 
    }

    public void UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            UpdateManaUI(); 
        }
    }

    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        }
    }

    void UpdateManaUI()
    {
        
        int roundedMana = Mathf.RoundToInt(currentMana);
        manaText.text = "Mana: " + roundedMana.ToString();

        
        if (manaBar != null)
        {
            manaBar.fillAmount = currentMana / maxMana; 
        }
    }

    public bool HasEnoughMana()
    {
        return currentMana > 0;
    }
}
