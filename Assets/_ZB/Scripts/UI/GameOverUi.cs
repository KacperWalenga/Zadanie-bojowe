using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer), typeof(StatisticsUi))]
    public class GameOverUi : MonoBehaviour
    {
        private PanelRenderer panelRenderer;
        private StatisticsUi statistics;
        private VisualElement hud;
        private VisualElement screen;
        private Button retryButton;
        private Button menuButton;
        private (int Floor, string Time, int Points)? results;
        private float previousTimeScale;

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
            statistics = GetComponent<StatisticsUi>();
        }

        private void OnEnable()
        {
            EventManager.AddListener(Consts.Events.Game.LOST, ShowGameOver);
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(Consts.Events.Game.LOST, ShowGameOver);
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            UnbindButtons();
        }

        private void OnDestroy()
        {
            if (results.HasValue)
                Time.timeScale = previousTimeScale;
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            UnbindButtons();

            hud = root.Q(className: "hud");
            screen = root.Q("game-over");
            
            retryButton = screen.Q<Button>("game-over-retry");
            menuButton = screen.Q<Button>("game-over-menu");
            
            retryButton.clicked += Retry;
            menuButton.clicked += ReturnToMenu;

            if (results is { } snapshot)
                ShowResults(snapshot);
        }

        private void ShowGameOver()
        {
            if (results.HasValue)
                return;

            var snapshot = statistics.GetResults();
            
            results = snapshot;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (screen != null)
                ShowResults(snapshot);
        }

        private void ShowResults((int Floor, string Time, int Points) snapshot)
        {
            hud.SetEnabled(false);
            
            screen.Q<Label>("game-over-floor").text = snapshot.Floor.ToString();
            screen.Q<Label>("game-over-time").text = snapshot.Time;
            screen.Q<Label>("game-over-points").text = snapshot.Points.ToString();
            screen.RemoveFromClassList("game-over--hidden");
            
            retryButton.Focus();
        }

        private void UnbindButtons()
        {
            if (retryButton != null)
                retryButton.clicked -= Retry;

            if (menuButton != null)
                menuButton.clicked -= ReturnToMenu;
        }

        private void Retry() => LoadScene("Game");
        private void ReturnToMenu() => LoadScene("MainMenu");

        private void LoadScene(string scene)
        {
            screen.SetEnabled(false);
            SceneManager.LoadScene(scene);
        }
    }
}
