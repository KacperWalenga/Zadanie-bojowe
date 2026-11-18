using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class HUD : MonoBehaviour
    {
        private PanelRenderer panelRenderer;

        private Label levelTimeLabel;
        private Label totalTimeLabel;

        private double gameStartedAt;
        private double levelStartedAt;
        private bool gameStarted;
        private bool uiLoaded;
        
        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
        
        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            uiLoaded = false;
        }

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, StartLevelTimer);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            levelTimeLabel = rootElement.Q("level-timer")?.Q<Label>("timer-value");
            totalTimeLabel = rootElement.Q("total-timer")?.Q<Label>("timer-value");

            uiLoaded = true;
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.MAZE_GENERATED, StartLevelTimer);
        }

        private void Update()
        {
            if (!gameStarted || !uiLoaded)
                return;

            var now = Time.timeAsDouble;

            if (levelTimeLabel != null)
                levelTimeLabel.text = FormatTime(now - levelStartedAt);

            if (totalTimeLabel != null)
                totalTimeLabel.text = FormatTime(now - gameStartedAt);
        }

        private void StartLevelTimer()
        {
            levelStartedAt = Time.timeAsDouble;

            if (gameStarted)
                return;

            gameStartedAt = levelStartedAt;
            gameStarted = true;
        }

        private string FormatTime(double seconds)
        {
            var time = (int)seconds;
            
            return $"{time / 60:00}:{time % 60:00}";
        }
    }
}