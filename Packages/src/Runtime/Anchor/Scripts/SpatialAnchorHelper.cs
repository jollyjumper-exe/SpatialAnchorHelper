using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SAH
{
    public class SpatialAnchorHelper : MonoBehaviour
    {
        public static SpatialAnchorHelper Instance { get; private set; }

        public static Action OnRoomIdFound;
        public static Action OnSaving;
        public static Action OnLoading;
        public static Action OnCreating;
        public static Action OnClearingScene;
        public static Action OnClearingRoomCache;
        public static Action OnClearingAllCaches;

        public List<SAHAnchor> Anchors { get; private set; } = new List<SAHAnchor>();


        [SerializeField] private string _persistentDataLocation = "anchors";
        [SerializeField] private SpatialAnchorBackendType _backendType = SpatialAnchorBackendType.Meta;

        private ISpatialAnchorBackend _backend;
        private string _roomID;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _backend = SpatialAnchorBackendFactory.Create(_backendType);
        }

        private void OnEnable()
        {
            SpatialAnchorSpawnEmitter.OnDespawned += RemoveSpatialAnchor;
        }

        private void OnDisable()
        {
            SpatialAnchorSpawnEmitter.OnDespawned -= RemoveSpatialAnchor;
        }

        void Start()
        {
            StartCoroutine(
                _backend.FetchRoomIdCoroutine(HandleRoomIdFound));
        }

        private void HandleRoomIdFound(string roomId)
        {
            _roomID = roomId;

            Debug.Log($"Found Room {_roomID}");

            OnRoomIdFound?.Invoke();
        }

        public async void SaveSpatialAnchors(string layoutID)
        {
            if (_roomID == null) return;
            await _backend.SaveSpatialAnchors(Anchors, _roomID, layoutID, _persistentDataLocation);

            OnSaving?.Invoke();
        }

        public async Task<List<SAHAnchor>> LoadSpatialAnchors(string layoutID)
        {
            if (_roomID == null)
                return null;

            ClearSpatialAnchors();

            Anchors = await _backend.LoadSpatialAnchors(_roomID, layoutID, _persistentDataLocation);

            if (Anchors == null)
                Anchors = new List<SAHAnchor>();

            OnLoading?.Invoke();

            return Anchors;
        }

        public async void CreateSpatialAnchor(Vector3 position, Quaternion rotation, Vector3 scale, string prefabPath, string type = null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at Resources/{prefabPath}");
                return;
            }

            if (type == null)
            {
                SpatialAnchorSpawnEmitter spawnEmitter = prefab.GetComponent<SpatialAnchorSpawnEmitter>();

                if (spawnEmitter != null && spawnEmitter.optionalSpatialAnchorType != null)
                {
                    type = spawnEmitter.optionalSpatialAnchorType;
                }
            }

            SAHAnchor anchor = await _backend.PlaceSpatialAnchor(position, rotation, scale, prefab);
            if (anchor == null)
            {
                return;
            }

            anchor.PrefabPath = prefabPath;
            anchor.Type = type;

            Anchors.Add(anchor);

            OnCreating?.Invoke();
        }

        public void ClearSpatialAnchors()
        {
            _backend.ClearSpatialAnchors();
            Anchors.Clear();

            OnClearingScene?.Invoke();
        }

        public void ClearRoomCache()
        {
            _backend.ClearRoomCache(_roomID, _persistentDataLocation);

            OnClearingRoomCache?.Invoke();
        }

        public void ClearAllCaches()
        {
            _backend.ClearAllCaches(_persistentDataLocation);

            OnClearingAllCaches?.Invoke();
        }

        public Dictionary<string, List<SAHAnchorSaveData>> LoadRoomCache()
        {
            return _backend.LoadRoomCache(_roomID, _persistentDataLocation);
        }

        public List<SAHAnchorSaveData> LoadLayoutCache(string layoutID)
        {
            return _backend.LoadLayoutCache(_roomID, layoutID, _persistentDataLocation);
        }

        private void RemoveSpatialAnchor(SpatialAnchorSpawnEmitter emitter)
        {
            int index = Anchors.FindIndex(a => a.Content == emitter.gameObject);

            if (index == -1)
            {
                Debug.LogWarning($"Anchor not found for emitter {emitter.name}");
                return;
            }

            Anchors.RemoveAt(index);
        }
    }
}
