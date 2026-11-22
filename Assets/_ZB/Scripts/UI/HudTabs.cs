using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ZB
{
    class HudTabs
    {
        private const string HiddenPanelClass = "hud__panel--hidden";
        private const string ActiveTabClass = "stone-button--selected";

        private Button journalTab;
        private Button minimapTab;
        private VisualElement journalPanel;
        private VisualElement minimapPanel;
        private bool minimapSelected;
        
        private InputAction toggleTabAction;

        public void Bind(VisualElement root, InputAction action)
        {
            Unbind();

            journalTab = root.Q<Button>("journal-tab");
            minimapTab = root.Q<Button>("minimap-tab");

            journalPanel = root.Q("journal-panel");
            minimapPanel = root.Q("minimap-panel");

            journalTab.clicked += ShowJournal;
            minimapTab.clicked += ShowMinimap;

            UpdateTabs();

            toggleTabAction = action;
            toggleTabAction.performed += ToggleTab;
            toggleTabAction.Enable();
        }

        public void Unbind()
        {
            if (journalTab != null)
                journalTab.clicked -= ShowJournal;

            if (minimapTab != null)
                minimapTab.clicked -= ShowMinimap;

            if (toggleTabAction == null)
                return;

            toggleTabAction.performed -= ToggleTab;
            toggleTabAction.Disable();
            toggleTabAction = null;
        }

        private void ToggleTab(InputAction.CallbackContext context)
        {
            if (!minimapTab.enabledInHierarchy)
                return;

            minimapSelected = !minimapSelected;

            UpdateTabs();
        }

        private void ShowJournal()
        {
            minimapSelected = false;

            UpdateTabs();
        }

        private void ShowMinimap()
        {
            minimapSelected = true;

            UpdateTabs();
        }

        private void UpdateTabs()
        {
            journalPanel.EnableInClassList(HiddenPanelClass, minimapSelected);
            minimapPanel.EnableInClassList(HiddenPanelClass, !minimapSelected);
            journalTab.EnableInClassList(ActiveTabClass, !minimapSelected);
            minimapTab.EnableInClassList(ActiveTabClass, minimapSelected);
        }
    }
}
