using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Oculus.Interaction.Input;
using UnityEditor;
using System.Collections.Generic;

namespace SpatialAnchorHelper
{
    public class SpatialAnchorsHelperUI : MonoBehaviour
    {
        public string layoutID;
        [SerializeField] Material hoverMaterial;

        private SpatialAnchorHelper _spatialAnchorHelper;
        private bool _isPlacing = false;
        private bool _wasPinchingLastFrame = false;
        private string _currentPrefabPath;
        private string _currentAnchorType;
        private GameObject _ghostModel;

        void Start()
        {
            if (SpatialAnchorHelper.Instance != null) _spatialAnchorHelper = SpatialAnchorHelper.Instance;
        }

        void Update()
        {

            if (_isPlacing)
            {
                SetupAndUpdateGhostModel();
                bool isPinching = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Hands);
                if (isPinching)
                {
                    PlaceSpatialAnchorAtController();
                    _isPlacing = false;
                    Destroy(_ghostModel);
                }
            }
        }

        public void PlaceAnchor(string prefabPath)
        {
            if (prefabPath != null)
            {
                _currentPrefabPath = prefabPath;
                _isPlacing = true;
                if (_ghostModel != null) Destroy(_ghostModel);
            }
        }

        private void PlaceSpatialAnchorAtController()
        {
            Vector3 position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion rawRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            _spatialAnchorHelper.CreateSpatialAnchor(position, rotation, _currentPrefabPath, _currentAnchorType);

        }
        private void SetupAndUpdateGhostModel()
        {
            if (_ghostModel == null)
            {
                _ghostModel = new GameObject("GhostModel");
                GameObject prefab = Resources.Load<GameObject>(_currentPrefabPath);

                var parentMesh = prefab.GetComponent<MeshFilter>();
                if (parentMesh != null)
                {
                    _ghostModel.AddComponent<MeshFilter>().mesh = parentMesh.sharedMesh;
                    _ghostModel.AddComponent<MeshRenderer>().material = hoverMaterial;
                }

                var childMesh = prefab.GetComponentsInChildren<MeshFilter>();
                if (childMesh != null)
                {
                    foreach (var item in childMesh)
                    {
                        var newGo = new GameObject(item.name);
                        newGo.transform.parent = _ghostModel.transform;
                        newGo.transform.localPosition = item.transform.localPosition;
                        newGo.transform.localRotation = item.transform.localRotation;
                        newGo.transform.localScale = item.transform.localScale;
                        newGo.AddComponent<MeshFilter>().mesh = item.sharedMesh;
                        newGo.AddComponent<MeshRenderer>().material = hoverMaterial;
                    }
                }

            }

            Vector3 position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion rawRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            _ghostModel.transform.position = position;
            _ghostModel.transform.rotation = rotation;
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

    [System.Serializable]
    public class ObjectData
    {
        public string Name;
        public string Path;
        public string IconPath;
    }

    [System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectData> Objects;
    }
}