using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class HUD : MonoBehaviour
    {
        [SerializeField] private RenderTexture gameRenderTexture;

        private Label levelTimeLabel;
        private Label totalTimeLabel;

        private double gameStartedAt;
        private double levelStartedAt;
        private bool gameStarted;
        private bool uiLoaded;
        private int uiVersion = -1;
        
        private void OnEnable()
        {
            GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
        }
        
        private void OnDisable()
        {
            GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        }

        private void Awake()
        {
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, StartLevelTimer);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {    
            if (uiVersion == version)
                return;

            uiVersion = version;
            
            rootElement.Q<Image>("game-view").image = gameRenderTexture;
            
            levelTimeLabel = rootElement.Q<Label>("level-time-label");
            totalTimeLabel = rootElement.Q<Label>("total-time-label");

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

            levelTimeLabel.text = FormatTime(now - levelStartedAt);
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