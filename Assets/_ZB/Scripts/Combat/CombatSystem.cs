using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using ZB;
using ZB.Mobs;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private InputActionReference numberAction;

    private int currentNumberIndex = -1;
    private string combatNumber;
    private bool isCombatInProgress => combatNumber != null;
    private bool isInputPaused;
    private CombatUI combatUI;
    private int currentHearts;
    private Mob mob;
    
    
    private Action CombatFinished;
    
    private void Awake()
    {
        numberAction.action.performed += OnNumberPressed;
        combatUI = FindAnyObjectByType<CombatUI>();
        
        EventManager.AddListener(Consts.Events.Game.LOST, StopCombatOnLoss);
    }

    private void OnDestroy()
    {
        numberAction.action.performed -= OnNumberPressed;
        EventManager.RemoveListener(Consts.Events.Game.LOST, StopCombatOnLoss);
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

        if (isInputPaused)
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
        
        combatUI.SetCombatNumber(currentNumberIndex, number);
        
        if(expectedNumber == number)
            CorrectNumberPressed();
        else
            IncorrectNumberPressed();
    }

    public void StartCombat(Mob mobData, Action combatFinished)
    {
        EventManager.Trigger(Consts.Events.Combat.COMBAT_STARTED);

        mob = mobData;
        
        combatNumber = mobData.Data.CombatNumber;
        currentNumberIndex = 0;
        
        combatUI.StartCombat(combatNumber.Length, mobData.Data.Health);

        CombatFinished = combatFinished;
        currentHearts = mobData.Data.Health;
    }

    private void CorrectNumberPressed()
    {
        currentNumberIndex++;
        
        if (combatNumber.Length >= currentNumberIndex + 1) 
            return;

        currentHearts--;
        
        if (currentHearts > 0)
        {
            combatUI.DamageMob(currentHearts);
            currentNumberIndex = 0;
            
            return;
        }
        
        EventManager.Trigger(Consts.Events.Combat.COMBAT_WIN, mob);
        mob = null;
        
        FinishCombat();
    }

    private async void IncorrectNumberPressed()
    {
        currentNumberIndex = 0;
        isInputPaused = true;

        mob.HitPlayer();
        await combatUI.ResetCombatNumber();
        
        isInputPaused = false;
    }

    private void StopCombatOnLoss()
    {
        combatNumber = null;
        CombatFinished = null;
    }

    private async void FinishCombat()
    {
        await combatUI.StopCombat();

        if (!isCombatInProgress)
            return;

        EventManager.Trigger(Consts.Events.Combat.COMBAT_ENDED);
        combatNumber = null;
        
        CombatFinished?.Invoke();
        CombatFinished = null;
    }
}
