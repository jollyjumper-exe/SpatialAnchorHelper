using System;
using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace SAH
{
    public class SpatialAnchorSpawnEmitter : MonoBehaviour
    {
        public string optionalSpatialAnchorType;
        public OVRSpatialAnchor spatialAnchor;
        public static event Action<SpatialAnchorSpawnEmitter> OnSpawned;
        public static event Action<SpatialAnchorSpawnEmitter> OnDespawned;

        private void Awake()
        {
            spatialAnchor = GetComponent<OVRSpatialAnchor>();
            OnSpawned?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnDespawned?.Invoke(this);
        }
    }
}