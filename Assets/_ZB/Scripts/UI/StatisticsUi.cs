using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class StatisticsUi : MonoBehaviour
    {
        private const int SecondsPerPointAward = 10;

        private readonly HudTabs tabs = new();

        private PanelRenderer panelRenderer;
        private Label levelLabel;
        private Label levelTimeLabel;
        private Label totalTimeLabel;
        private Label pointsLabel;

        private double gameStartedAt;
        private double levelStartedAt;
        private int level;
        private int pointsFromPreviousLevels;
        private int displayedLevelSeconds = -1;
        private int displayedGameSeconds = -1;
        private bool uiLoaded;

        private static string FormatTime(int seconds) => $"{seconds / 60:00}:{seconds % 60:00}";

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, StartLevel);
        }

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            tabs.Unbind();
            uiLoaded = false;
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.MAZE_GENERATED, StartLevel);
        }

        private void Update()
        {
            if (level == 0 || !uiLoaded)
                return;

            RefreshStatistics(Time.timeAsDouble);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            levelLabel = root.Q<Label>("level-label");
            levelTimeLabel = root.Q<Label>("level-time");
            totalTimeLabel = root.Q<Label>("total-time");
            pointsLabel = root.Q<Label>("points-value");
            tabs.Bind(root);
            uiLoaded = true;
            displayedGameSeconds = -1;

            if (level > 0)
                RefreshLevel(Time.timeAsDouble);
        }

        private void StartLevel()
        {
            var now = Time.timeAsDouble;

            if (level == 0)
                gameStartedAt = now;
            else
                pointsFromPreviousLevels += CalculateLevelPoints(GetLevelSeconds(now));

            level++;
            levelStartedAt = now;

            if (uiLoaded)
                RefreshLevel(now);
        }

        private void RefreshLevel(double now)
        {
            levelLabel.text = $"PIWNICA {level}";
            displayedLevelSeconds = -1;
            RefreshStatistics(now);
        }

        private void RefreshStatistics(double now)
        {
            var levelSeconds = GetLevelSeconds(now);

            if (levelSeconds != displayedLevelSeconds)
            {
                displayedLevelSeconds = levelSeconds;
                levelTimeLabel.text = FormatTime(levelSeconds);
                pointsLabel.text = (pointsFromPreviousLevels + CalculateLevelPoints(levelSeconds)).ToString();
            }

            var gameSeconds = (int)(now - gameStartedAt);

            if (gameSeconds == displayedGameSeconds)
                return;

            displayedGameSeconds = gameSeconds;
            totalTimeLabel.text = FormatTime(gameSeconds);
        }

        private int GetLevelSeconds(double now) => (int)(now - levelStartedAt);

        private int CalculateLevelPoints(int levelSeconds) => levelSeconds / SecondsPerPointAward * level;
    }
}
