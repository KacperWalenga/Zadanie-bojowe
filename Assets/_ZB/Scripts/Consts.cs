namespace ZB
{
    public static partial class Consts
    {
        public static partial class Events
        {
            public static partial class Game
            {
                public const string GAME_STARTED = "GAME_STARTED";
            }

            public static partial class Maze
            {
                public const string MAZE_GENERATED = "MAZE_GENERATED";
                public const string FLOOR_CHANGED = "FLOOR_CHANGED";
            }

            public static partial class Objectives
            {
                public const string EXIT_DOOR_LOCKED = "EXIT_DOOR_LOCKED";
                public const string BEER_COLLECTED = "BEER_COLLECTED";
                public const string FATHER_FIGHT_REQUESTED = "FATHER_FIGHT_REQUESTED";
            }

            public static partial class Player
            {
                public const string PLAYER_MOVED =  "PLAYER_MOVED";
                public const string PLAYER_ROTATED =  "PLAYER_ROTATED";
                public const string PLAYER_MOVE_STARTED = "PLAYER_MOVE_STARTED";
                public const string PLAYER_TURN_STARTED = "PLAYER_TURN_STARTED";
                public const string PLAYER_HEALTH_LOST = "PLAYER_HEALTH_LOST";
            }

            public static partial class Combat
            {
                public const string COMBAT_STARTED = "COMBAT_STARTED";
                public const string COMBAT_ENDED = "COMBAT_ENDED";
            }
        }
    }
}
