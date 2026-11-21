using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using ZB;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private Coroutine damageCoroutine;
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    public void StartDealingDamage(int damagePerSecond)
    {
        damageCoroutine = StartCoroutine(DealingDamageCoroutine(damagePerSecond));
    }

    public void StopDealingDamage()
    {
        if (damageCoroutine == null)
            return;
        
        StopCoroutine(damageCoroutine);
    }

    private IEnumerator DealingDamageCoroutine(int damagePerSecond)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            currentHealth = math.clamp(currentHealth - damagePerSecond, 0, maxHealth);
            
            UpdateHealthUI();

            if (currentHealth <= 0)
                LoseGame();
        }
    }

    private void LoseGame()
    {
        EventManager.Trigger(Consts.Events.Game.LOST);
    }

    private void UpdateHealthUI()
    {
        EventManager.Trigger<(int, int)>(Consts.Events.Player.HEALTH_LOST, (currentHealth, maxHealth));
    }
}
