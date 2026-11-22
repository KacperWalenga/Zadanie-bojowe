
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using ZB.Mobs;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class StatisticsUi : MonoBehaviour
    {
        private const int SecondsPerPointAward = 10;

        [SerializeField] private InputActionReference switchBookMinimap;

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

        private int FloorNumber => level == 0 ? 0 : 5 - level;

        private static string FormatTime(int seconds) => $"{seconds / 60:00}:{seconds % 60:00}";

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();

            EventManager.AddListener<int>(Consts.Events.Maze.FLOOR_CHANGED, StartLevel);
            EventManager.AddListener<MobData>(Consts.Events.Combat.COMBAT_WIN, FightEnded);
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
            EventManager.RemoveListener<int>(Consts.Events.Maze.FLOOR_CHANGED, StartLevel);
            EventManager.RemoveListener<MobData>(Consts.Events.Combat.COMBAT_WIN, FightEnded);
        }

        private void Update()
        {
            if (level == 0 || !uiLoaded)
                return;

            RefreshStatistics(Time.timeAsDouble);
        }

        public (int Floor, string Time, int Points) GetResults()
        {
            var now = Time.timeAsDouble;
            var gameSeconds = level == 0 ? 0 : (int)(now - gameStartedAt);
            return (FloorNumber, FormatTime(gameSeconds), GetTotalPoints(GetLevelSeconds(now)));
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            levelLabel = root.Q<Label>("level-label");
            levelTimeLabel = root.Q<Label>("level-time");
            totalTimeLabel = root.Q<Label>("total-time");
            pointsLabel = root.Q<Label>("points-value");

            tabs.Bind(root, switchBookMinimap.action);
            
            uiLoaded = true;
            displayedGameSeconds = -1;

            if (level > 0)
                RefreshLevel(Time.timeAsDouble);
        }

        private void StartLevel(int floor)
        {
            var now = Time.timeAsDouble;

            if (level == 0)
                gameStartedAt = now;
            else
                pointsFromPreviousLevels = Math.Max(0, pointsFromPreviousLevels - CalculateLevelPoints(GetLevelSeconds(now)));

            level = floor;
            levelStartedAt = now;

            if (uiLoaded)
                RefreshLevel(now);
        }

        private void FightEnded(MobData mob)
        {
            pointsFromPreviousLevels += mob.Points;

            if (uiLoaded && level > 0)
                RefreshPoints(GetLevelSeconds(Time.timeAsDouble));
        }

        private void RefreshLevel(double now)
        {
            levelLabel.text = $"PIWNICA {FloorNumber}";
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

                RefreshPoints(levelSeconds);
            }

            var gameSeconds = (int)(now - gameStartedAt);

            if (gameSeconds == displayedGameSeconds)
                return;

            displayedGameSeconds = gameSeconds;
            totalTimeLabel.text = FormatTime(gameSeconds);
        }

        private void RefreshPoints(int levelSeconds) =>
            pointsLabel.text = GetTotalPoints(levelSeconds).ToString();

        private int GetLevelSeconds(double now) => (int)(now - levelStartedAt);

        private int GetTotalPoints(int levelSeconds) =>
            Math.Max(0, pointsFromPreviousLevels - CalculateLevelPoints(levelSeconds));

        private int CalculateLevelPoints(int levelSeconds)
        {
            return levelSeconds / SecondsPerPointAward * GetLevelPointPenalty();
        }

        private int GetLevelPointPenalty() => level switch
        {
            0 or 1 => 0,
            2 => 2,
            _ => 1
        };
    }
}
