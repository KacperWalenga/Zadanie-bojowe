using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZB.Maze
{
    public struct PlacedRoom
    {
        public Vector2Int Position;
        public MazeRoom Room;
    }
    
    public enum WallType
    {
        AIR = 0,
        CORRIDOR = 1,
        ROOM = 2
    }
    
    public class MazeData
    {
        private readonly HashSet<Vector2Int> reservedEntrances = new();

        private WallType[] cells;
        public int Size { get; private set; }
        public int RoomPadding { get; private set; }

        private List<PlacedRoom> placedRooms;
        public IReadOnlyList<PlacedRoom> Rooms => placedRooms.AsReadOnly();

        public MazeData(int size = 51, int roomPadding = 1)
        {
            Size = size;
            RoomPadding = roomPadding;
            cells = new WallType[size * size];
            
            for (var i = 0; i < cells.Length; i++)
                cells[i] = WallType.AIR;

            placedRooms = new();
        }
        
        public WallType GetCellType(Vector2Int position)
        {
            if (!IsInsideGrid(position))
                return WallType.AIR;

            var index = position.y * Size + position.x;
            
            return cells[index];
        }
        
        public bool HasCorridors() => Array.IndexOf(cells, WallType.CORRIDOR) >= 0;

        public bool IsInsideGrid(Vector2Int position)
        {
            return position.x >= 0 &&
                   position.y >= 0 &&
                   position.x < Size &&
                   position.y < Size;
        }
        
        public bool IsCellFree(Vector2Int position)
        {
            return GetCellType(position) == WallType.AIR && !reservedEntrances.Contains(position);
        }
        
        public bool CanGenerateCorridor(Vector2Int position)
        {
            return IsInsideGrid(position) && IsCellFree(position);
        }
        
        public void SetCellType(Vector2Int position, WallType type)
        {
            if (!IsInsideGrid(position))
                return;

            var index = position.y * Size + position.x;
            
            cells[index] = type;
        }
        
        public bool IsZoneFree(Vector2Int bottomLeftCorner, int size)
        {
            var min = bottomLeftCorner - Vector2Int.one * RoomPadding;
            var max = bottomLeftCorner + Vector2Int.one * (size + RoomPadding - 1);

            if (!IsInsideGrid(min) || !IsInsideGrid(max))
                return false;

            for (var y = min.y; y <= max.y; y++)
                for (var x = min.x; x <= max.x; x++)
                    if (!IsCellFree(new Vector2Int(x, y)))
                        return false;

            return true;
        }
        
        public void PlaceRoom(Vector2Int position, MazeRoom room)
        {
            for (var y = 0; y < room.Size; y++)
                for (var x = 0; x < room.Size; x++)
                    SetCellType(position + new Vector2Int(x, y), WallType.ROOM);

            placedRooms.Add(new PlacedRoom { Position = position, Room = room });

            foreach (var door in room.Doors)
            {
                var doorPosition = position + door;

                if (!IsInsideGrid(doorPosition))
                    continue;

                reservedEntrances.Add(doorPosition);
            }
        }
    }
}