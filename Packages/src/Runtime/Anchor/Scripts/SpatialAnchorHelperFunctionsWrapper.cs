using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Oculus.Interaction.Input;
using UnityEditor;
using System.Collections.Generic;

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

        public void LoadSpatialAnchors()
        {
            _spatialAnchorHelper.LoadSpatialAnchors(layoutID);
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