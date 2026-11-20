using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using ZB;
using ZB.Mobs;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private InputActionReference numberAction;
    [SerializeField] public int playerMaxHP = 100;

    private int currentNumberIndex = -1;
    private string combatNumber;
    private bool isCombatInProgress => combatNumber != null;

    private Action CombatFinished;

    [field: SerializeField] public int PlayerHP { get; private set; }

    private void Awake()
    {
        numberAction.action.performed += OnNumberPressed;
        PlayerHP = playerMaxHP;
    }

    private void OnDestroy()
    {
        numberAction.action.performed -= OnNumberPressed;
    }
    
    private int? GetExpectedNumber()
    {
        if (currentNumberIndex < 0)
            return null;

        if (combatNumber.Length < currentNumberIndex + 1)
            return null;

        return int.TryParse(combatNumber[currentNumberIndex].ToString(), out var number)
            ? number
            : null;
    }

    private void OnNumberPressed(InputAction.CallbackContext context)
    {
        if (!isCombatInProgress)
            return;
        
        if (!context.ReadValueAsButton())
            return;

        if (context.control is not KeyControl key)
            return;

        var number = key.keyCode switch
        {
            Key.Digit0 => 0,
            >= Key.Digit1 and <= Key.Digit9 => (int)key.keyCode - (int)Key.Digit1 + 1,
            >= Key.Numpad0 and <= Key.Numpad9 => (int)key.keyCode - (int)Key.Numpad0,
            _ => -1
        };

        if (number == -1)
            return;

        var expectedNumber = GetExpectedNumber();

        if (expectedNumber == null)
            return;
        
        if(expectedNumber == number)
            CorrectNumberPressed();
        else
            IncorrectNumberPressed();
    }

    public void StartCombat(MobData mobData, Action combatFinished)
    {
        EventManager.Trigger(Consts.Events.Combat.COMBAT_STARTED);
        combatNumber = mobData.CombatNumber;
        currentNumberIndex = 0;

        CombatFinished = combatFinished;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || PlayerHP <= 0)
            return;

        PlayerHP = Mathf.Max(0, PlayerHP - damage);
        EventManager.Trigger(Consts.Events.PLAYER_HEALTH_LOST, (Health: PlayerHP, MaxHealth: playerMaxHP));
    }

    private void CorrectNumberPressed()
    {
        currentNumberIndex++;
        
        if(combatNumber.Length < currentNumberIndex + 1)
            FinishCombat();
    }

    private void IncorrectNumberPressed()
    {
        currentNumberIndex = 0;
    }

    private void FinishCombat()
    {
        EventManager.Trigger(Consts.Events.Combat.COMBAT_ENDED);
        combatNumber = null;
        
        CombatFinished?.Invoke();
        CombatFinished = null;
    }
}
