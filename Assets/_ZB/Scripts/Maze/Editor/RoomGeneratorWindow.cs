
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZB.Maze.Editor
{
    public class RoomGeneratorWindow : EditorWindow
    {
        private const float CellSize = 1f;

        private const string FloorPath = "Assets/_ZB/Prefabs/Floor.prefab";
        private const string RoofPath = "Assets/_ZB/Prefabs/Top C.prefab";
        private const string WallPath = "Assets/_ZB/Prefabs/Room Wall.prefab";
        private const string DoorPath = "Assets/_ZB/Prefabs/Room Door.prefab";
        private const string PrefabFolder = "Assets/_ZB/Prefabs/Rooms";
        private const string DataFolder = "Assets/_ZB/Data/Rooms";
        
        private TextField roomNameField;
        private IntegerField sizeField;

        [MenuItem("Tools/Room Generator")]
        public static void Open()
        {
            GetWindow<RoomGeneratorWindow>("Room Generator");
        }

        public void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_ZB/Scripts/Maze/Editor/RoomGenerator.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_ZB/Scripts/Maze/Editor/RoomGenerator.uss");

            visualTree.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(styleSheet);
            
            roomNameField = rootVisualElement.Q<TextField>("room-name");
            sizeField = rootVisualElement.Q<IntegerField>("size");

            rootVisualElement.Q<Button>("generate-button").clicked += GenerateRoom;
        }
        
        private void GenerateRoom()
        {
            if (!ValidateInput())
                return;
            
            var roomObject = CreateRoomObject();
            var prefab = SaveRoomPrefab(roomObject);
            
            DestroyImmediate(roomObject);
            SaveRoomData(prefab);

            Debug.Log($"Room generated: {prefab.name}");
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(roomNameField.value))
            {
                Debug.LogError("name is empty");
                return false;
            }

            if (sizeField.value < 2)
            {
                Debug.LogError("size must be greater than two");
                return false;
            }

            return true;
        }

        private GameObject CreateRoomObject()
        {
            var roomObject = new GameObject(roomNameField.value.Trim());
            var floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FloorPath);
            var roofPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoofPath);
            var doors = GetDefaultDoors(sizeField.value);

            BuildFloorAndRoof(roomObject.transform, floorPrefab, roofPrefab, sizeField.value);
            BuildWalls(roomObject.transform, sizeField.value, doors);

            return roomObject;
        }
        
        private List<Vector2Int> GetDefaultDoors(int size)
        {
            var middle = size / 2;

            return new List<Vector2Int>
            {
                new(middle, size), // N
                new(size, middle), // E
                new(middle, -1),   // S
                new(-1, middle)    // W
            };
        }

        private GameObject SaveRoomPrefab(GameObject roomObject)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{PrefabFolder}/{roomObject.name}.prefab");

            return PrefabUtility.SaveAsPrefabAsset(roomObject, path);
        }

        private void SaveRoomData(GameObject prefab)
        {
            var roomData = CreateInstance<MazeRoom>();

            roomData.Size = sizeField.value;
            roomData.Prefab = prefab;
            roomData.Doors = GetDefaultDoors(sizeField.value).ToArray();

            var path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{roomData.name}.asset");

            roomData.name = prefab.name;
            
            path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{roomData.name}.asset");

            AssetDatabase.CreateAsset(roomData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void BuildFloorAndRoof(Transform parent, GameObject floorPrefab, GameObject roofPrefab, int size)
        {
            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    var position = new Vector3((x + 0.5f) * CellSize, 0f, (z + 0.5f) * CellSize);
                    
                    CreatePart(floorPrefab, parent, position, Quaternion.identity);
                    CreatePart(roofPrefab, parent, position, Quaternion.identity);
                }
            }
        }
        
        private void BuildWalls(Transform parent, int size, List<Vector2Int> doors)
        {
            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallPath);
            var doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorPath);
            var farEdge = size * CellSize;

            var southRotation = Quaternion.identity;
            var northRotation = Quaternion.Euler(0f, 180f, 0f);
            var westRotation = Quaternion.Euler(0f, 90f, 0f);
            var eastRotation = Quaternion.Euler(0f, -90f, 0f);

            for (var index = 0; index < size; index++)
            {
                var center = (index + 0.5f) * CellSize;

                PlaceSection(new Vector2Int(index, -1), new Vector3(center, 0f, 0f), southRotation);
                PlaceSection(new Vector2Int(index, size), new Vector3(center, 0f, farEdge), northRotation);
                PlaceSection(new Vector2Int(-1, index), new Vector3(0f, 0f, center), westRotation);
                PlaceSection(new Vector2Int(size, index), new Vector3(farEdge, 0f, center), eastRotation);
            }

            void PlaceSection(Vector2Int outsideCell, Vector3 position, Quaternion rotation)
            {
                var prefab = doors.Contains(outsideCell) ? doorPrefab : wallPrefab;
                CreatePart(prefab, parent, position, rotation);
            }
        }

        private void CreatePart(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
        }
    }
}
