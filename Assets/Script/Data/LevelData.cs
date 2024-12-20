using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tuleeeeee.Enum;
using Tuleeeeee.Gems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tuleeeeee.Data
{
    [Serializable]
    public class GemMap
    {
        public GemType gemType;
        public int Group;
    }

    [CreateAssetMenu(fileName = "Level", menuName = "GameData/LevelData", order = 1)]
    public class LevelData : SerializedScriptableObject
    {
        public int Level;

        public int Rows;
        public int Columns;

        public int CrushCount = 10;

        public Tile tilePrefab;
        public List<Gem> gemPrefab;
        public List<GemMap> gemMaps = new List<GemMap>();

        //[TableMatrix(HorizontalTitle = "Row", VerticalTitle = "Column")]
        [TableMatrix(SquareCells = true, DrawElementMethod = "DrawElementMethod"), TabGroup("Table", "Table")]
        public GemMap[,] Table;

        public GemMap DrawElementMethod(Rect rect, GemMap value)
        {
            Color color = Color.clear;
            switch (value.gemType)
            {
                case GemType.None:
                    color = Color.grey;
                    break;
                case GemType.Red1x2:
                    color = Color.red;
                    break;
                case GemType.Blue1x3:
                    color = Color.blue;
                    break;
                case GemType.Green1x4:
                    color = Color.green;
                    break;
                case GemType.Black1x5:
                    color = Color.black;
                    break;
                case GemType.Cyan2x2:
                    color = Color.cyan;
                    break;
                case GemType.Pink3x3:
                    color = Color.magenta;
                    break;
                case GemType.Yellow2x3:
                    color = Color.yellow;
                    break;
                case GemType.Pink2x4:
                    color = Color.magenta;
                    break;
                case GemType.Red4x4:
                    color = Color.red;
                    break;
            }

            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), color);
            if (value.Group != 0)
                UnityEditor.EditorGUI.LabelField(rect.Padding(rect.width / 3), value.Group.ToString(), new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    }
                });
            return value;
        }


        [DictionaryDrawerSettings(KeyLabel = "GemType", ValueLabel = "GemLimited")]
        public Dictionary<GemType, int> gemLimits = new Dictionary<GemType, int>
        {
            { GemType.Red1x2, 0 },
        };

        private Dictionary<GemType, int> gemCount;

        [DictionaryDrawerSettings(KeyLabel = "GemType", ValueLabel = "GemSize")]
        public readonly Dictionary<GemType, GemSize> gemSize = new Dictionary<GemType, GemSize>
        {
            { GemType.Red1x2, new GemSize() },
        };

        [InlineProperty(LabelWidth = 90)]
        public struct GemSize
        {
            public int width;
            public int height;
        }


        [Button]
        public void CreateTable()
        {
            gemMaps.Clear();

            Table = new GemMap[Rows, Columns];
            // init
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Table[row, col] = new GemMap()
                    {
                        gemType = GemType.None,
                    };
                }
            }
        }

        [Button]
        public void RandomGems()
        {
            gemMaps.Clear();
            var sortedGem = gemLimits.Keys
                .OrderByDescending(gem => GetGemHeight(gem) * GetGemWidth(gem))
                .ToList();

            int groupId = 0;
            foreach (var gem in sortedGem)
            {
                {
                    int limit = gemLimits[gem];
                    for (int i = 0; i < limit; i++)
                    {
                        PlaceRandomGem(gem, groupId);
                        groupId++;
                    }
                }
            }
        }

        private void PlaceRandomGem(GemType gem, int groupId)
        {
            int height = GetGemHeight(gem);
            int width = GetGemWidth(gem);

            List<(int row, int col)> positions = new List<(int, int)>();

            for (int row = 0; row <= Rows - width; row++)
            {
                for (int col = 0; col <= Columns - height; col++)
                {
                    positions.Add((row, col));
                }
            }

            //Shuffe
            for (int i = 0; i < positions.Count; i++)
            {
                int j = Random.Range(i, positions.Count);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }


            foreach (var (row, col) in positions)
            {
                if (CanPlaceGem(row, col, width, height))
                {
                    PlaceGem(row, col, width, height, gem, groupId);
                    return;
                }
            }

            foreach (var (row, col) in positions)
            {
                if (CanPlaceGem(row, col, height, width))
                {
                    PlaceGem(row, col, height, width, gem, groupId);
                    return;
                }
            }

            Debug.LogWarning($"Failed to place {gem} in group {groupId}.");
        }

        private bool CanPlaceGem(int startRow, int startCol, int width, int height)
        {
            for (int row = 0; row < width; row++)
            {
                for (int col = 0; col < height; col++)
                {
                    if (Table[startRow + row, startCol + col].gemType != GemType.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PlaceGem(int startRow, int startCol, int width, int height, GemType gem, int groupId)
        {
            Debug.Log($"Placing {gem} in group {groupId}.");
            for (int row = 0; row < width; row++)
            {
                for (int col = 0; col < height; col++)
                {
                    Table[startRow + row, startCol + col] = new GemMap()
                    {
                        gemType = gem,
                        Group = groupId
                    };
                }
            }

            GemMap gemMap = new GemMap { gemType = gem, Group = groupId };
            gemMaps.Add(gemMap);
        }

        public int GetGemHeight(GemType gem)
        {
            return gemSize.ContainsKey(gem) ? gemSize[gem].height : 1;
        }

        public int GetGemWidth(GemType gem)
        {
            return gemSize.ContainsKey(gem) ? gemSize[gem].width : 1;
        }

        public List<GemMap> GetGemMaps()
        {
            return gemMaps;
        }
    }
}