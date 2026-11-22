namespace ZB
{
    public static partial class Consts
    {
        public static partial class Events
        {
            public static partial class Game
            {
                public const string WON = "WON";
                public const string LOST = "GAME_LOST";
            }

            public static partial class Maze
            {
                public const string GENERATED = "MAZE_GENERATED";
                public const string FLOOR_CHANGED = "FLOOR_CHANGED";
            }

            public static partial class Objectives
            {
                public const string BEER_COLLECTED = "BEER_COLLECTED";
                public const string FATHER_FIGHT_REQUESTED = "FATHER_FIGHT_REQUESTED";
            }

            public static partial class Player
            {
                public const string MOVED =  "PLAYER_MOVED";
                public const string ROTATED =  "PLAYER_ROTATED";
                public const string MOVE_STARTED = "PLAYER_MOVE_STARTED";
                public const string TURN_STARTED = "PLAYER_TURN_STARTED";
                public const string HEALTH_LOST = "PLAYER_HEALTH_LOST";
            }

            public static partial class Combat
            {
                public const string COMBAT_STARTED = "COMBAT_STARTED";
                public const string COMBAT_ENDED = "COMBAT_ENDED";
                public const string COMBAT_WIN = "COMBAT_WIN";
            }
        }
    }
}
