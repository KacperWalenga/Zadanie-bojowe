using UnityEngine.UIElements;

namespace ZB
{
    class HudTabs
    {
        private const string HiddenPanelClass = "hud__panel--hidden";
        private const string ActiveTabClass = "hud__tab--active";

        private Button journalTab;
        private Button minimapTab;
        private VisualElement journalPanel;
        private VisualElement minimapPanel;
        private bool minimapSelected;

        public void Bind(VisualElement root)
        {
            Unbind();

            journalTab = root.Q<Button>("journal-tab");
            minimapTab = root.Q<Button>("minimap-tab");
            
            journalPanel = root.Q("journal-panel");
            minimapPanel = root.Q("minimap-panel");

            journalTab.clicked += ShowJournal;
            minimapTab.clicked += ShowMinimap;
            UpdateTabs();
        }

        public void Unbind()
        {
            if (journalTab != null)
                journalTab.clicked -= ShowJournal;

            if (minimapTab != null)
                minimapTab.clicked -= ShowMinimap;
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
