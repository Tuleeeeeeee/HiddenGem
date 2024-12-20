using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tuleeeeee.Delegate;
using Sirenix.OdinInspector;
using TMPro;
using Tuleeeeee.Data;
using Tuleeeeee.Enum;
using Tuleeeeee.Features.Mission;
using Tuleeeeee.Model;
using Tuleeeeee.Gems;
using UnityEngine;

namespace Tuleeeeee.GameSystem
{
    public class LevelSystem : SerializedMonoBehaviour
    {
        public static LevelSystem Instance;

        [Header("Data")] [SerializeField] private ManagerGameLevel managerGameLevel;

        [SerializeField] private MissionDestinationData missionDestinationData;
        private LevelData currentLevelData;

        private LevelModel levelModel;

        [TableMatrix, SerializeField] private GemType[,] cloneTable;

        public MissionDestination currentMissionDestination;

        private List<Tile> tiles = new List<Tile>();
        [SerializeField] private TextMeshProUGUI crushCountText;
        [SerializeField] private GameObject addPickaxePopup;

        private int stage = 1;

        public int crushCount { get; private set; }

        private OnGemClear onGemClear;
        private OnLevelClear onLevelClear;

        private GameObject tileContainer;
        private GameObject gemContainer;

        public void RegisterOnClearGem(OnGemClear onGemClear)
        {
            this.onGemClear += onGemClear;
        }

        public void UnRegisterOnClearGem(OnGemClear onGemClear)
        {
            this.onGemClear -= onGemClear;
        }

        public void RegisterOnLevelClear(OnLevelClear onLevelClear)
        {
            this.onLevelClear += onLevelClear;
        }

        public void UnRegisterOnLevelClear(OnLevelClear onLevelClear)
        {
            this.onLevelClear -= onLevelClear;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            tileContainer = new GameObject("TileContainer");
            gemContainer = new GameObject("GemContainer");
            gemContainer.transform.SetParent(transform);

            levelModel = new LevelModel();
            GenerateLevel(stage);
            GenerateMission(stage);
        }

        private void Update()
        {
            if (crushCount <= 0)
            {
                addPickaxePopup.SetActive(true);
            }

            crushCountText.SetText(crushCount.ToString());
        }

        private void GenerateMission(int stage)
        {
            var prefab = missionDestinationData.GetMissionDestination(stage);
            currentMissionDestination = Instantiate(prefab, new Vector3(0, 7, 0), Quaternion.identity);
        }

        private void GenerateLevel(int stage)
        {
            currentLevelData = managerGameLevel.GetLevel(stage);

            currentLevelData.CreateTable();
            currentLevelData.RandomGems();


            levelModel.CloneTable = (GemMap[,])currentLevelData.Table.Clone();

            levelModel.CrushCount = currentLevelData.CrushCount;
            crushCount += levelModel.CrushCount;

            crushCountText.SetText(crushCount.ToString());

            GenerateTile();

            GenerateGem();

            CenterGrid(tileContainer.transform, currentLevelData.Rows, 1);
            CenterGrid(gemContainer.transform, currentLevelData.Rows, 1);
        }

        private void GenerateGem()
        {
            List<GemMap> gemMaps = GetGemMaps();
            foreach (var gem in currentLevelData.gemPrefab)
            {
                Debug.Log($"Checking Gem {gem.gemType}:");
                foreach (var gemMap in gemMaps)
                {
                    List<Tile> listTile = GetTileHideGem(gem.gemType, gemMap.Group);
                    if (listTile.Count == 0)
                        continue;
                    Debug.Log($"Checking Gem {gem.gemType} in list:");
                    Debug.Log($"Caculate Gem {gem.gemType} bound:");
                    BoundsInt bounds = CalculateBounds(listTile);
                    Debug.Log($"Gem bounds: {bounds.size}");
                    Vector3 centerPosition = GetCenterPosition(bounds);
                    Debug.Log($"Center Position: {centerPosition}");
                    HandleGemPlacement(gem, bounds, centerPosition, gemMap.Group);
                }
            }
        }

        private BoundsInt CalculateBounds(List<Tile> listTile)
        {
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;

            int baseRow = listTile[0].Row;
            int baseColumn = listTile[0].Column;

            foreach (var tile in listTile)
            {
                minRow = Mathf.Min(minRow, tile.Row);
                maxRow = Mathf.Max(maxRow, tile.Row);

                minColumn = Mathf.Min(minColumn, tile.Column);
                maxColumn = Mathf.Max(maxColumn, tile.Column);

                Debug.Log($"Tile Position: Row {tile.Row}, Column {tile.Column}, id {tile.Id}");
            }

            return new BoundsInt(new Vector3Int(minRow, minColumn, 0),
                new Vector3Int(maxRow - minRow + 1, maxColumn - minColumn + 1, 0));
        }

        private void HandleGemPlacement(Gem gem, BoundsInt bounds, Vector3 centerPosition, int id)
        {
            int width = bounds.size.x;
            int height = bounds.size.y;

            if (width == height)
            {
                Debug.Log($"Gem {gem.gemType} is a square {bounds.size.x}x{bounds.size.y}.");
                InstantiateGem(gem, centerPosition, Quaternion.identity, id);
            }
            else if (width > height)
            {
                Debug.Log($"Gem {gem.gemType} is horizontally.");
                InstantiateGem(gem, centerPosition, Quaternion.Euler(0, 0, 90), id);
            }
            else if (width < height)
            {
                Debug.Log($"Gem {gem.gemType} is vertically.");
                InstantiateGem(gem, centerPosition, Quaternion.identity, id);
            }
            else
            {
                Debug.Log($"Gem {gem.gemType} is neither horizontal nor vertical.");
            }
        }

        private void InstantiateGem(Gem gem, Vector3 position, Quaternion rotation, int id)
        {
            Gem newGem = Instantiate(gem, position, rotation);
            newGem.Init(gem.gemType, Vector3.one, id);
            newGem.transform.SetParent(gemContainer.transform, false);
        }

        private Vector3 GetCenterPosition(BoundsInt bounds)
        {
            float centerRow = bounds.min.x + (bounds.size.x - 1) / 2f;
            float centerColumn = bounds.min.y + (bounds.size.y - 1) / 2f;

            return new Vector3(centerRow, -centerColumn, 0);
        }

        private List<Tile> GetTileHideGem(GemType gemType, int groupId)
        {
            return tiles.Where(x => x.gemType == gemType && x.Id == groupId).ToList();
        }

        private void GenerateTile()
        {
            tileContainer.transform.SetParent(transform);
            for (int row = 0; row < currentLevelData.Rows; row++)
            {
                for (int col = 0; col < currentLevelData.Columns; col++)
                {
                    var tile = Instantiate(currentLevelData.tilePrefab, new Vector3(row, -col, 0),
                        Quaternion.identity);
                    tile.Init(row, col, currentLevelData.Table[row, col].gemType,
                        currentLevelData.Table[row, col].Group);
                    tile.transform.SetParent(tileContainer.transform, false);
                    tiles.Add(tile);
                }
            }
        }

        private void CenterGrid(Transform parent, int rows, float cellSize)
        {
            parent.position = Vector3.zero;
            float gridWidth = (rows - 1) * cellSize;
            parent.position = new Vector3(-gridWidth / 2f, parent.position.y, parent.position.z);
        }

        public void TileCrusher(int row, int column, GemType type, int id)
        {
            if (crushCount <= 0)
            {
                return;
            }

            crushCount--;


            if (row < 0 || row >= currentLevelData.Rows || column < 0 || column >= currentLevelData.Columns)
            {
                return;
            }

            if (currentLevelData.Table[row, column].gemType == GemType.None)
            {
                return;
            }

            levelModel.CloneTable[row, column].gemType = GemType.None;

            if (IsClearType(type, id))
            {
                onGemClear?.Invoke(type, id);
                return;
            }
        }

        private bool IsClearType(GemType type, int id)
        {
            foreach (var cell in levelModel.CloneTable)
            {
                if (cell.gemType == type && cell.Group == id)
                    return false;
            }

            return true;
        }

        public List<GemMap> GetGemMaps()
        {
            return currentLevelData.GetGemMaps();
        }

        public Transform GetDestination(GemType gemType, int id)
        {
            return currentMissionDestination.GetDestination(gemType, id);
        }

        public void AddPickaxe()
        {
            crushCount += 100;
        }

        public void CheckLevelClear()
        {
            if (!levelModel.HasClear()) return;
            Debug.Log("Level clear");
            onLevelClear?.Invoke();
            tiles.Clear();
            stage++;
            StartCoroutine(DelayedMissionGeneration(stage, 1f));
        }

        IEnumerator DelayedMissionGeneration(int stage, float delay)
        {
            yield return new WaitForSeconds(delay);
            GenerateMission(stage);
            GenerateLevel(stage);
        }
    }
}