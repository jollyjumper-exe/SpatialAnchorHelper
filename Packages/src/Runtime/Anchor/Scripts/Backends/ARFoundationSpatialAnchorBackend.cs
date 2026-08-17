using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SAH
{
    public sealed class ARFoundationSpatialAnchorBackend : ISpatialAnchorBackend
    {
        private const string CacheScopeId = "arfoundation";

        private readonly ARAnchorManager _anchorManager;
        private readonly List<ARAnchor> _activeAnchors = new List<ARAnchor>();

        public IEnumerator FetchRoomIdCoroutine(Action<string> onRoomIdFound)
        {
            // AR Foundation has no provider-independent room identifier.
            onRoomIdFound?.Invoke(CacheScopeId);
            yield break;
        }

        public async Task<SAHAnchor> PlaceSpatialAnchor(
            Vector3 position,
            Quaternion rotation,
            GameObject prefab)
        {
            EnsureManagerReady();

            var result = await _anchorManager.TryAddAnchorAsync(
                new Pose(position, rotation));
            if (!result.status.IsSuccess())
            {
                Debug.LogError($"AR Foundation failed to create an anchor: {result.status}");
                return null;
            }

            ARAnchor nativeAnchor = result.value;
            _activeAnchors.Add(nativeAnchor);
            GameObject content = InstantiateContent(prefab, nativeAnchor, null);

            return new SAHAnchor
            {
                Content = content,
                NativeHandle = nativeAnchor
            };
        }

        public void ClearSpatialAnchors()
        {
            foreach (ARAnchor anchor in _activeAnchors)
            {
                if (anchor != null)
                {
                    UnityEngine.Object.Destroy(anchor.gameObject);
                }
            }

            _activeAnchors.Clear();
            Debug.Log("Cleared all AR Foundation spatial anchors.");
        }

        public async Task SaveSpatialAnchors(
            List<SAHAnchor> anchors,
            string roomId,
            string scenarioId,
            string subfolder = "anchors")
        {
            EnsureManagerReady();
            EnsurePersistenceSupported(save: true);

            var saveData = new List<SAHAnchorSaveData>();
            foreach (SAHAnchor anchor in anchors)
            {
                if (!(anchor.NativeHandle is ARAnchor nativeAnchor))
                {
                    Debug.LogWarning(
                        $"Anchor '{anchor.Id}' is not backed by an AR Foundation anchor.");
                    continue;
                }

                var result = await _anchorManager.TrySaveAnchorAsync(nativeAnchor);
                if (!result.status.IsSuccess())
                {
                    Debug.LogWarning(
                        $"AR Foundation failed to save an anchor: {result.status}");
                    continue;
                }

                anchor.Id = result.value.guid.ToString();
                saveData.Add(new SAHAnchorSaveData
                {
                    anchorId = anchor.Id,
                    prefabPath = anchor.PrefabPath,
                    type = anchor.Type,
                    scale = ToSAHVector3(
                        anchor.Content != null
                            ? anchor.Content.transform.localScale
                            : Vector3.one)
                });
            }

            SaveLayoutCache(roomId, scenarioId, saveData, subfolder);
        }

        public async Task<List<SAHAnchor>> LoadSpatialAnchors(
            string roomId,
            string scenarioId,
            string subfolder = "anchors")
        {
            EnsureManagerReady();
            EnsurePersistenceSupported(save: false);

            List<SAHAnchorSaveData> saveData =
                LoadLayoutCache(roomId, scenarioId, subfolder);
            if (saveData == null)
            {
                return null;
            }

            var anchors = new List<SAHAnchor>();
            foreach (SAHAnchorSaveData data in saveData)
            {
                if (!Guid.TryParse(data.anchorId, out Guid guid))
                {
                    Debug.LogError($"Invalid persistent anchor UUID: {data.anchorId}");
                    continue;
                }

                var result = await _anchorManager.TryLoadAnchorAsync(
                    new SerializableGuid(guid));
                if (!result.status.IsSuccess())
                {
                    Debug.LogWarning(
                        $"AR Foundation failed to load anchor '{guid}': {result.status}");
                    continue;
                }

                GameObject prefab = Resources.Load<GameObject>(data.prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab not found in Resources/{data.prefabPath}");
                    continue;
                }

                ARAnchor nativeAnchor = result.value;
                _activeAnchors.Add(nativeAnchor);
                GameObject content = InstantiateContent(prefab, nativeAnchor, data.scale);

                anchors.Add(new SAHAnchor
                {
                    Id = data.anchorId,
                    PrefabPath = data.prefabPath,
                    Type = data.type,
                    Content = content,
                    NativeHandle = nativeAnchor
                });
            }

            return anchors;
        }

        public void ClearRoomCache(string roomId, string subfolder = "anchors")
        {
            string path = GetCachePath(roomId, subfolder);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted AR Foundation anchor cache at '{path}'.");
            }
        }

        public void ClearAllCaches(string subfolder = "anchors")
        {
            string folderPath = Path.Combine(Application.persistentDataPath, subfolder);
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
            }
        }

        public Dictionary<string, List<SAHAnchorSaveData>> LoadRoomCache(
            string roomId,
            string subfolder = "anchors")
        {
            string path = GetCachePath(roomId, subfolder);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<
                           Dictionary<string, List<SAHAnchorSaveData>>>(
                           File.ReadAllText(path))
                       ?? new Dictionary<string, List<SAHAnchorSaveData>>();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load anchor cache '{path}': {exception.Message}");
                return null;
            }
        }

        public List<SAHAnchorSaveData> LoadLayoutCache(
            string roomId,
            string scenarioId,
            string subfolder = "anchors")
        {
            Dictionary<string, List<SAHAnchorSaveData>> roomCache =
                LoadRoomCache(roomId, subfolder);

            return roomCache != null
                   && roomCache.TryGetValue(scenarioId, out var layout)
                ? layout
                : null;
        }

        private void EnsureManagerReady()
        {
            if (!_anchorManager.isActiveAndEnabled || _anchorManager.subsystem == null)
            {
                throw new InvalidOperationException(
                    "ARAnchorManager must be enabled with an active anchor subsystem.");
            }
        }

        private void EnsurePersistenceSupported(bool save)
        {
            XRAnchorSubsystemDescriptor descriptor = _anchorManager.descriptor;
            bool supported = save
                ? descriptor.supportsSaveAnchor
                : descriptor.supportsLoadAnchor;

            if (!supported)
            {
                throw new NotSupportedException(
                    save
                        ? "The active AR Foundation provider cannot save persistent anchors."
                        : "The active AR Foundation provider cannot load persistent anchors.");
            }
        }

        private static GameObject InstantiateContent(
            GameObject prefab,
            ARAnchor nativeAnchor,
            SAHVector3? scale)
        {
            GameObject content = UnityEngine.Object.Instantiate(
                prefab,
                nativeAnchor.transform);
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
            if (scale.HasValue)
            {
                content.transform.localScale = ToUnityVector3(scale.Value);
            }

            return content;
        }

        private static void SaveLayoutCache(
            string roomId,
            string scenarioId,
            List<SAHAnchorSaveData> saveData,
            string subfolder)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, subfolder);
            Directory.CreateDirectory(folderPath);
            string path = GetCachePath(roomId, subfolder);

            Dictionary<string, List<SAHAnchorSaveData>> roomCache = File.Exists(path)
                ? JsonConvert.DeserializeObject<
                      Dictionary<string, List<SAHAnchorSaveData>>>(
                      File.ReadAllText(path))
                  ?? new Dictionary<string, List<SAHAnchorSaveData>>()
                : new Dictionary<string, List<SAHAnchorSaveData>>();

            roomCache[scenarioId] = saveData;
            File.WriteAllText(
                path,
                JsonConvert.SerializeObject(roomCache, Formatting.Indented));
        }

        private static string GetCachePath(string roomId, string subfolder)
        {
            return Path.Combine(Application.persistentDataPath, subfolder, roomId);
        }

        private static SAHVector3 ToSAHVector3(Vector3 value)
        {
            return new SAHVector3 { x = value.x, y = value.y, z = value.z };
        }

        private static Vector3 ToUnityVector3(SAHVector3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
    }
}
