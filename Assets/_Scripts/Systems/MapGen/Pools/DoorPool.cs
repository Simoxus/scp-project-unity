using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "DoorPool", menuName = "Custom/Map Gen/Door Pool")]
    public class DoorPool : ScriptableObject
    {
        [SerializeField] private List<AssetReferenceGameObject> doorPrefabReferences = new List<AssetReferenceGameObject>();

        public AssetReferenceGameObject GetRandomDoorReference(int seed)
        {
            if (doorPrefabReferences.Count == 0)
            {
                return null;
            }

            Random.InitState(seed);
            return doorPrefabReferences[Random.Range(0, doorPrefabReferences.Count)];
        }

        public int GetDoorCount() => doorPrefabReferences.Count;
    }
}