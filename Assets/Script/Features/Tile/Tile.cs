using Tuleeeeee.GameSystem;
using Tuleeeeee.Enum;
using UnityEngine;


namespace Tuleeeeee.Gems
{
    public class Tile : MonoBehaviour
    {
        public GemType gemType;
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Id { get; private set; }

        public void Init(int row, int column, GemType gemType, int id)
        {
            Row = row;
            Column = column;
            this.gemType = gemType;
            Id = id;
        }

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnLevelClear(OnLevelClear);
        }

        private void OnDestroy()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.UnRegisterOnLevelClear(OnLevelClear);
        }

        private void OnMouseDown()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.TileCrusher(Row, Column, gemType, Id);
            if (levelSystem.crushCount <= 0) return;
            Destroy(gameObject);
        }

        private void OnLevelClear()
        {
            Destroy(gameObject);
        }
    }
}