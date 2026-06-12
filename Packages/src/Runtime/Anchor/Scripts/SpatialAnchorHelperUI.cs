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
        private List<GameObject> _pendingSpatialAnchors = new List<GameObject>();

        void Start()
        {
            if(SpatialAnchorHelper.Instance != null) _spatialAnchorHelper = SpatialAnchorHelper.Instance;
        }

        void OnEnable()
        {
            SpatialAnchorHelper.OnClearingScene += RemoveAllPendingSpatialAnchors;
        }

        void OnDisable()
        {
            SpatialAnchorHelper.OnClearingScene -= RemoveAllPendingSpatialAnchors;
            
            RemoveAllPendingSpatialAnchors();
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

        public void PlaceSpatialAnchor(Transform transform, string prefabPath)
        {
            Vector3 position = transform.position;
            Quaternion rawRotation = transform.rotation;
            Vector3 euler = rawRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            _spatialAnchorHelper.CreateSpatialAnchor(position, rotation, prefabPath, _currentAnchorType);
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

            GameObject setupGameObject = Instantiate(SetupObject, position, rotation);
            _pendingSpatialAnchors.Add(setupGameObject);

            SetupObject setupObject = setupGameObject.GetComponent<SetupObject>();
            
            GameObject ghostCopy = Instantiate(_ghostModel);
            Destroy(_ghostModel);

            setupObject.prefabPath =_currentPrefabPath;
            setupObject.SetGhostModel(ghostCopy);
            setupObject.SetPositionAndRotation(position, rotation);
            setupObject.SetSpatialAnchorHelperUI(this);
            setupObject.OnConfirmed += OnConfirmedSetupObject;

        }

        private void OnConfirmedSetupObject(SetupObject setupObject)
        {
            PlaceSpatialAnchor(setupObject.transform, setupObject.prefabPath);
            RemovePendingSpatialAnchor(setupObject.gameObject);
        }

        private void RemovePendingSpatialAnchor(GameObject objectToRemove)
        {
            if (objectToRemove == null)
                return;

            _pendingSpatialAnchors.Remove(objectToRemove);

            SetupObject setupObject = objectToRemove.GetComponent<SetupObject>();
            setupObject.OnConfirmed -= OnConfirmedSetupObject;

            Destroy(objectToRemove);
        }

        private void RemoveAllPendingSpatialAnchors()
        {
            foreach (GameObject pending in _pendingSpatialAnchors)
            {
                if (pending != null)
                {
                    Destroy(pending);

                    SetupObject setupObject = pending.GetComponent<SetupObject>();
                    setupObject.OnConfirmed -= OnConfirmedSetupObject;
                }
            }

            _pendingSpatialAnchors.Clear();
        }

        private void SetupAndUpdateGhostModel()
        {
            if (_ghostModel == null)
            {
                GameObject prefab = Resources.Load<GameObject>(_currentPrefabPath);
                _ghostModel = Object.Instantiate(prefab);
                _ghostModel.name = "GhostModel";

                foreach (var renderer in _ghostModel.GetComponentsInChildren<MeshRenderer>())
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                        materials[i] = hoverMaterial;
                    renderer.sharedMaterials = materials;
                }

                foreach (var component in _ghostModel.GetComponentsInChildren<Component>())
                {
                    if (component is Transform || component is MeshFilter || component is MeshRenderer)
                        continue;

                    Object.Destroy(component);
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