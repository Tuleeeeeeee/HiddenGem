using System.Collections.Generic;
using Tuleeeeee.Enum;
using Tuleeeeee.GameSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tuleeeeee.Features.Mission
{
    public class Destination : MonoBehaviour
    {
        [FormerlySerializedAs("runeType")] public GemType gemType;
        public int id;

        public void Init(GemType gemType, int id)
        {
            this.gemType = gemType;
            this.id = id;
        }
    }
}