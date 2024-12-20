using System.Collections.Generic;
using DG.Tweening;
using Tuleeeeee.Data;
using Tuleeeeee.Enum;
using Tuleeeeee.GameSystem;
using UnityEngine;

namespace Tuleeeeee.Features.Mission
{
    public class MissionDestination : MonoBehaviour
    {
        public int level = 1;
        [SerializeField] private Destination destination;

        public List<Destination> Destination;
        [SerializeField] private List<Transform> targetsPosition;

        public Transform GetDestination(GemType gemType, int id)
        {
            return Destination.Find(x => x.gemType == gemType && x.id == id).transform;
        }

        private void InstantiateDestination(Destination des, GemType gemType, Vector3 position, Quaternion rotation,
            int id, Transform parent)
        {
            Destination newDestination = Instantiate(des, position, rotation);
            newDestination.Init(gemType, id);
            newDestination.transform.SetParent(parent);
            Destination.Add(newDestination);
        }

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnLevelClear(OnLevelClear);

            List<GemMap> gemMaps = levelSystem.GetGemMaps();

            for (int i = 0; i < gemMaps.Count; i++)
            {
                var gem = gemMaps[i];
                Transform targetTransform = targetsPosition[i];
                InstantiateDestination(destination, gem.gemType, targetTransform.position, targetTransform.rotation,
                    gem.Group, targetTransform);
            }
        }

        private void OnDestroy()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.UnRegisterOnLevelClear(OnLevelClear);
        }

        private void OnLevelClear()
        {
            var seq = DOTween.Sequence();

            seq.Append(transform.DOScale(Vector3.one * 0.9f, 0.5f));
            seq.Append(transform.DORotate(new Vector3(0, 0, 360), 4f));
            seq.Join(transform.DOMoveX(50, 2f));
            Destroy(gameObject, 1f);
        }
    }
}