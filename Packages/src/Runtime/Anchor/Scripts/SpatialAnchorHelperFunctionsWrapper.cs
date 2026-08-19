using System.Collections.Generic;
using Oculus.Interaction.Input;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SAH
{
    public class SpatialAnchorHelperFunctionsWrapper : MonoBehaviour
    {
        public string layoutID;
        protected SpatialAnchorHelper _spatialAnchorHelper;

        void Start()
        {
            if (SpatialAnchorHelper.Instance != null) _spatialAnchorHelper = SpatialAnchorHelper.Instance;
        }

        public async void LoadSpatialAnchors()
        {
            await _spatialAnchorHelper.LoadSpatialAnchors(layoutID);
        }

        public void SaveSpatialAnchors()
        {
            _spatialAnchorHelper.SaveSpatialAnchors(layoutID);
        }

        public void ClearSpatialAnchors()
        {
            _spatialAnchorHelper.ClearSpatialAnchors();
        }

        public void ClearRoomCache()
        {
            _spatialAnchorHelper.ClearRoomCache();
        }

        public void ClearAllCaches()
        {
            _spatialAnchorHelper.ClearAllCaches();
        }
    }
}