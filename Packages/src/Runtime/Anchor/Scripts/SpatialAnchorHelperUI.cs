using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SAH
{
    public class SpatialAnchorHelperUI : SpatialAnchorHelperFunctionsWrapper
    {
        [SerializeField] Material hoverMaterial;
        [SerializeField] GameObject SetupObject;
        [SerializeField] private MonoBehaviour inputProvider;

        private ISpatialAnchorPlacementInput Input =>
            inputProvider as ISpatialAnchorPlacementInput;

        private bool _isPlacing = false;
        private bool _wasPinchingLastFrame = false;
        private string _currentPrefabPath;
        private string _currentAnchorType;
        private GameObject _ghostModel;
        private List<GameObject> _pendingSpatialAnchors = new List<GameObject>();

        void Start()
        {
            if (SpatialAnchorHelper.Instance != null) _spatialAnchorHelper = SpatialAnchorHelper.Instance;
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

            if (!_isPlacing)
                return;

            if (!Input.TryGetPose(out Pose pose))
                return;

            SetupAndUpdateGhostModel(pose);

            if (Input.WasConfirmedThisFrame())
            {
                PlaceSetupObject(pose);
                _isPlacing = false;
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
            Quaternion rotation = Quaternion.Euler(euler);
            Vector3 scale = transform.localScale;


            _spatialAnchorHelper.CreateSpatialAnchor(position, rotation, scale, prefabPath, _currentAnchorType);
        }

        private void PlaceSetupObject(Pose pose)
        {
            Vector3 position = pose.position;

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

            Vector3 euler = pose.rotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            Quaternion rotation = Quaternion.Euler(euler);

            GameObject setupGameObject = Instantiate(SetupObject, position, rotation);
            _pendingSpatialAnchors.Add(setupGameObject);

            SetupObject setupObject = setupGameObject.GetComponent<SetupObject>();

            GameObject ghostCopy = Instantiate(_ghostModel);
            Destroy(_ghostModel);

            setupObject.prefabPath = _currentPrefabPath;
            setupObject.SetGhostModel(ghostCopy);
            setupObject.SetPositionAndRotation(position, rotation);
            setupObject.OnConfirmed += OnConfirmedSetupObject;

            _currentPrefabPath = null;
            _isPlacing = false;

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

        private void SetupAndUpdateGhostModel(Pose pose)
        {
            if (_ghostModel == null)
            {
                GameObject prefab = Resources.Load<GameObject>(_currentPrefabPath);

                _ghostModel = new GameObject("GhostModel");

                foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    var src = mf.transform;
                    var srcRenderer = mf.GetComponent<MeshRenderer>();
                    if (srcRenderer == null) continue;

                    GameObject go = new GameObject(mf.name);
                    go.transform.SetParent(_ghostModel.transform, false);

                    go.transform.position = src.position;
                    go.transform.rotation = src.rotation;
                    go.transform.localScale = src.lossyScale;

                    var newMF = go.AddComponent<MeshFilter>();
                    newMF.sharedMesh = mf.sharedMesh;

                    var newMR = go.AddComponent<MeshRenderer>();

                    var mats = srcRenderer.sharedMaterials;
                    var ghostMats = new Material[mats.Length];

                    for (int i = 0; i < mats.Length; i++)
                        ghostMats[i] = hoverMaterial;

                    newMR.sharedMaterials = ghostMats;

                    newMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    newMR.receiveShadows = false;
                }

                _ghostModel.transform.localScale = Vector3.one;
            }

            Vector3 position = pose.position;

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

            Vector3 euler = pose.rotation.eulerAngles;
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