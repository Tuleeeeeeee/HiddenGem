using DG.Tweening;
using Tuleeeeee.GameSystem;
using Sirenix.OdinInspector;
using Tuleeeeee.Enum;
using UnityEngine;

namespace Tuleeeeee.Gems
{
    public class Gem : MonoBehaviour
    {
        public GemType gemType;
        public int GemId { get; private set; }
        public Vector3 GemSize { get; private set; }

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnClearGem(OnGemClear);
        }

        private void OnDestroy()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.UnRegisterOnClearGem(OnGemClear);
        }

        private void OnGemClear(GemType gemType, int id)
        {
            if (this.gemType == gemType && GemId == id)
            {
                FlyToDestination();
            }
        }

        public void Init(GemType gemType, Vector3 size, int id)
        {
            this.gemType = gemType;
            GemSize = size;
            GemId = id;
        }


        [Button]
        public void FlyToDestination()
        {
            var levelSystem = LevelSystem.Instance;

            var level = levelSystem.GetDestination(gemType, GemId);
            var seq = DOTween.Sequence();

            seq.Append(transform.DOScale(transform.localScale * 2f, .3f));
            seq.Append(transform.DOMove(level.position, 0.3f)).Join(transform.DOScale(Vector3.one * 0.6f, .3f));
            seq.AppendCallback(() =>
            {
                transform.SetParent(levelSystem.currentMissionDestination.transform);
                levelSystem.CheckLevelClear();
            });
            seq.Play();
        }
    }
}