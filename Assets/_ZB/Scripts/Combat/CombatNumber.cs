using TMPro;
using UnityEngine;

public class CombatNumber : MonoBehaviour
{
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        
        if(!text)
        {
            enabled = false;
            Debug.LogError("[CombatNumber] TMP_Text not found");
        }
    }

    public void SetNumber(int number)
    {
        if (number is < 0 or > 9)
            return;

        text.text = number.ToString();
    }
}
