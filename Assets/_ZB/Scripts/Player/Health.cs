using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using ZB;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int healPercent = 50;

    private Coroutine damageCoroutine;
    private int currentHealth;
    private int healthBeforeFight;
    private bool refilledFromFridge;

    private void Awake()
    {
        EventManager.AddListener<int>(Consts.Events.Maze.FLOOR_CHANGED, RefillFromFridge);
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int>(Consts.Events.Maze.FLOOR_CHANGED, RefillFromFridge);
    }

    private void RefillFromFridge(int floorNumber)
    {
        const int FloorAfterFridge = 2;

        if (floorNumber != FloorAfterFridge || refilledFromFridge)
            return;

        refilledFromFridge = true;
        currentHealth = maxHealth;
        
        UpdateHealthUI();
    }

    public void StartDealingDamage(int damagePerSecond)
    {
        if (currentHealth <= 0 || damageCoroutine != null)
            return;

        healthBeforeFight = currentHealth;
        damageCoroutine = StartCoroutine(DealingDamageCoroutine(damagePerSecond));
    }

    public void StopDealingDamage()
    {
        if (damageCoroutine == null)
            return;

        StopCoroutine(damageCoroutine);
        damageCoroutine = null;

        if (currentHealth <= 0)
            return;

        var heal = (healthBeforeFight - currentHealth) * math.clamp(healPercent, 0, 100) / 100;

        if (heal <= 0)
            return;

        currentHealth += heal;
        UpdateHealthUI();
    }

    public void DealDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth = math.max(currentHealth - damage, 0);
        UpdateHealthUI();

        if (currentHealth > 0)
            return;

        StopDealingDamage();
        LoseGame();
    }

    private IEnumerator DealingDamageCoroutine(int damagePerSecond)
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(1f);
            DealDamage(damagePerSecond);
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
