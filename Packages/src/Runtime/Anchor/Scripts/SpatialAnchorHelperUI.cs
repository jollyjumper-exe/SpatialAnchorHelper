using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Oculus.Interaction.Input;
using UnityEditor;
using System.Collections.Generic;

namespace SAH
{
    public class SpatialAnchorHelperUI : SpatialAnchorHelperFunctionsWrapper
    {
        [SerializeField] Material hoverMaterial;
        [SerializeField] GameObject SetupObject;

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
                    PlaceSetupObjectAtController();
                    _isPlacing = false;
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

        public void PlaceAndSaveAnchor(Transform transform)
        {
            Vector3 position = transform.position;
            Quaternion rawRotation = transform.rotation;
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            _spatialAnchorHelper.CreateSpatialAnchor(position, rotation, _currentPrefabPath, _currentAnchorType);
        }

        private void PlaceSetupObjectAtController()
        {
            Vector3 position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

            if (true)
            {
                RaycastHit hit;
                float rayDistance = 2f;
                bool foundHit = false;
                Vector3 hitPoint = position;

                if (Physics.Raycast(position, Vector3.down, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }
                else if (Physics.Raycast(position, Vector3.up, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }

                if (foundHit)
                {
                    position = hitPoint;
                }
                else
                {
                    Debug.LogWarning("No surface found to snap anchor to.");
                }
            }

            Quaternion rawRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            SetupObject setupObject = Instantiate(SetupObject, position, rotation).GetComponent<SetupObject>();
            setupObject.SetGhostModel(_ghostModel);
            setupObject.SetPositionAndRotation(position, rotation);
            setupObject.SetSpatialAnchorHelperUI(this);

        }

        private void PlaceSpatialAnchorAtController()
        {
            Vector3 position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

            if (true)
            {
                RaycastHit hit;
                float rayDistance = 2f;
                bool foundHit = false;
                Vector3 hitPoint = position;

                if (Physics.Raycast(position, Vector3.down, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }
                else if (Physics.Raycast(position, Vector3.up, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }

                if (foundHit)
                {
                    position = hitPoint;
                }
                else
                {
                    Debug.LogWarning("No surface found to snap anchor to.");
                }
            }

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

            if (true)
            {
                RaycastHit hit;
                float rayDistance = 2f;
                bool foundHit = false;
                Vector3 hitPoint = position;

                if (Physics.Raycast(position, Vector3.down, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }
                else if (Physics.Raycast(position, Vector3.up, out hit, rayDistance))
                {
                    hitPoint = hit.point;
                    foundHit = true;
                }

                if (foundHit)
                {
                    position = hitPoint;
                }
                else
                {
                    Debug.LogWarning("No surface found to snap anchor to.");
                }
            }
            
            Quaternion rawRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            _ghostModel.transform.position = position;
            _ghostModel.transform.rotation = rotation;
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