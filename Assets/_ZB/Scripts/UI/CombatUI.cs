using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class CombatUI : MonoBehaviour
    {
        private PanelRenderer panelRenderer;
        private VisualElement combatFieldsRoot;

        private List<TextElement> combatFields = new();
        
        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            combatFieldsRoot = rootElement.Q("combat-fields");
        }
        
        public void StartCombat(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var combatField = new TextElement
                {
                    text = "_",
                    name = $"combat-field-{i}"
                };
            
                combatFieldsRoot.Add(combatField);
                combatFields.Add(combatField);
            }
        }

        public void SetCombatNumber(int index, int number)
        {
            combatFields[index].text = number.ToString();
        }

        public async Awaitable ResetCombatNumber(bool applyColor = true)
        {
            if(applyColor)
                await AnimateIncorrectNumbers();
            
            foreach (var combatField in combatFields)
                combatField.text = "_";
        }

        public async Awaitable StopCombat(bool applyColor = true)
        {
            if (applyColor)
                await AnimateCorrectNumbers();
            
            foreach (var combatField in combatFields)
                combatField.RemoveFromHierarchy();
            
            combatFields.Clear();
        }

        private async Awaitable AnimateCorrectNumbers()
        {
            for (var i = 0; i < 2; i++)
            {
                SetCombatFieldsColor(Color.green);
                await Awaitable.WaitForSecondsAsync(0.1f);
                SetCombatFieldsColor(Color.white);
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
        }

        private async Awaitable AnimateIncorrectNumbers()
        {
            for (var i = 0; i < 2; i++)
            {
                SetCombatFieldsColor(Color.red);
                await Awaitable.WaitForSecondsAsync(0.1f);
                SetCombatFieldsColor(Color.white);
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
        }

        private void SetCombatFieldsColor(Color color)
        {
            foreach (var combatField in combatFields)
            {
                combatField.style.color = color;
            }
        }
    }
}
