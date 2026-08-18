using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using Newtonsoft.Json;
using UnityEngine;

namespace SAH
{
    public sealed class MetaSpatialAnchorBackend : ISpatialAnchorBackend
    {
        public IEnumerator FetchRoomIdCoroutine(
            Action<string> onRoomIdFound)
        {
            while (true)
            {
                var room = MRUK.Instance?.GetCurrentRoom();

                if (room != null)
                {
                    string roomId = room.Anchor.Uuid.ToString();
                    onRoomIdFound?.Invoke(roomId);
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public Task<SAHAnchor> PlaceSpatialAnchor(Vector3 position, Quaternion rotation, Vector3 scale, GameObject prefab)
        {
            GameObject parent = GameObject.Find("SpatialAnchors");
            if (parent == null)
            {
                parent = new GameObject("SpatialAnchors");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent.transform);
            instance.transform.localScale = scale;

            OVRSpatialAnchor nativeAnchor = instance.AddComponent<OVRSpatialAnchor>();

            return Task.FromResult(new SAHAnchor
            {
                Id = nativeAnchor.Uuid.ToString(),
                Content = instance,
                NativeHandle = nativeAnchor
            });
        }

        public void ClearSpatialAnchors()
        {
            GameObject parent = GameObject.Find("SpatialAnchors");
            if (parent == null)
            {
                Debug.LogWarning("No 'SpatialAnchors' GameObject found.");
                return;
            }

            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.transform.GetChild(i).gameObject;
                UnityEngine.Object.Destroy(child);
            }

            Debug.Log("Cleared all spatial anchors.");
        }

        public async Task SaveSpatialAnchors(List<SAHAnchor> anchors, string roomId, string scenarioId, string subfolder = "anchors")
        {
            // Construct the directory and file paths
            string folderPath = Path.Combine(Application.persistentDataPath, subfolder);
            string path = Path.Combine(folderPath, roomId);

            // Ensure the directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"Created directory: {folderPath}");
            }

            // Load existing JSON or create empty dictionary
            Dictionary<string, List<SAHAnchorSaveData>> allScenarios = File.Exists(path)
                ? JsonConvert.DeserializeObject<Dictionary<string, List<SAHAnchorSaveData>>>(File.ReadAllText(path))
                : new Dictionary<string, List<SAHAnchorSaveData>>();

            List<SAHAnchorSaveData> saveDataList = new List<SAHAnchorSaveData>();

            foreach (SAHAnchor anchor in anchors)
            {
                if (!(anchor.NativeHandle is OVRSpatialAnchor nativeAnchor))
                {
                    Debug.LogWarning($"Anchor '{anchor.Id}' is not backed by an OVRSpatialAnchor.");
                    continue;
                }

                var result = await nativeAnchor.SaveAsync();
                if (result)
                {
                    anchor.Id = nativeAnchor.Uuid.ToString();
                    saveDataList.Add(new SAHAnchorSaveData
                    {
                        anchorId = anchor.Id,
                        prefabPath = anchor.PrefabPath,
                        type = anchor.Type,
                        scale = ToSAHVector3(
                            anchor.Content != null
                                ? anchor.Content.transform.localScale
                                : Vector3.one)
                    });

                    Debug.Log($"Anchor {nativeAnchor.Uuid} saved successfully.");
                }
                else
                {
                    Debug.LogWarning("Failed to save anchor.");
                }
            }

            allScenarios[scenarioId] = saveDataList;

            string json = JsonConvert.SerializeObject(allScenarios, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"Saved {saveDataList.Count} anchors for scenario '{scenarioId}' to {path}");
        }

        public async Task<List<SAHAnchor>> LoadSpatialAnchors(string roomId, string scenarioId, string subfolder = "anchors")
        {
            string path = Path.Combine(Application.persistentDataPath, subfolder, roomId);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Anchor file not found at {path}");
                return null;
            }

            Dictionary<string, List<SAHAnchorSaveData>> allScenarios =
                JsonConvert.DeserializeObject<Dictionary<string, List<SAHAnchorSaveData>>>(File.ReadAllText(path));

            if (!allScenarios.TryGetValue(scenarioId, out var saveDataList))
            {
                Debug.LogWarning($"No anchors found for scenario '{scenarioId}'");
                return null;
            }

            List<Guid> anchorIds = new List<Guid>();
            Dictionary<Guid, SAHAnchorSaveData> idToData = new Dictionary<Guid, SAHAnchorSaveData>();

            foreach (var data in saveDataList)
            {
                if (Guid.TryParse(data.anchorId, out Guid guid))
                {
                    anchorIds.Add(guid);
                    idToData[guid] = data;
                }
                else
                {
                    Debug.LogError($"Invalid anchor UUID: {data.anchorId}");
                }
            }

            List<OVRSpatialAnchor.UnboundAnchor> unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(anchorIds, unboundAnchors);

            if (!result.Success)
            {
                Debug.LogError($"Failed to load anchors. Status: {result.Status}");
                return null;
            }

            List<SAHAnchor> anchors = new List<SAHAnchor>();

            foreach (var unbound in unboundAnchors)
            {
                bool success = await unbound.LocalizeAsync();
                if (!success)
                {
                    Debug.LogWarning($"Localization failed for anchor {unbound.Uuid}");
                    continue;
                }

                if (!idToData.TryGetValue(unbound.Uuid, out SAHAnchorSaveData anchorData))
                {
                    Debug.LogWarning($"No prefab path found for anchor {unbound.Uuid}");
                    continue;
                }

                GameObject prefab = Resources.Load<GameObject>(anchorData.prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab not found in Resources/{anchorData.prefabPath}");
                    continue;
                }

                GameObject parent = GameObject.Find("SpatialAnchors") ?? new GameObject("SpatialAnchors");
                Pose pose = unbound.Pose;
                GameObject spawned = UnityEngine.Object.Instantiate(prefab, pose.position, pose.rotation, parent.transform);

                // Older cache entries have no scale field. In that case, retain
                // the scale configured on the prefab.
                if (anchorData.scale.HasValue)
                {
                    spawned.transform.localScale = ToUnityVector3(anchorData.scale.Value);
                }

                var boundAnchor = spawned.AddComponent<OVRSpatialAnchor>();
                unbound.BindTo(boundAnchor);

                anchors.Add(new SAHAnchor
                {
                    Id = unbound.Uuid.ToString(),
                    PrefabPath = anchorData.prefabPath,
                    Type = anchorData.type,
                    Content = spawned,
                    NativeHandle = boundAnchor
                });

                Debug.Log($"Loaded and placed prefab for anchor {unbound.Uuid}");
            }

            return anchors;
        }

        private static SAHVector3 ToSAHVector3(Vector3 value)
        {
            return new SAHVector3
            {
                x = value.x,
                y = value.y,
                z = value.z
            };
        }

        private static Vector3 ToUnityVector3(SAHVector3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }

        public void ClearRoomCache(string roomId, string subfolder = "anchors")
        {
            string path = Path.Combine(Application.persistentDataPath, subfolder, roomId);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"Deleted anchor cache for room '{roomId}' at {path}");
                }
                else
                {
                    Debug.LogWarning($"No cache file found for room '{roomId}' at {path}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete cache for room '{roomId}': {ex.Message}");
            }
        }

        public void ClearAllCaches(string subfolder = "anchors")
        {
            string folderPath = Path.Combine(Application.persistentDataPath, subfolder);

            try
            {
                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    Debug.Log($"Cleared all anchor cache files in '{folderPath}'");
                }
                else
                {
                    Debug.LogWarning($"No anchor cache folder found at '{folderPath}'");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear all caches: {ex.Message}");
            }
        }

        public Dictionary<string, List<SAHAnchorSaveData>> LoadRoomCache(string roomId, string subfolder = "anchors")
        {
            string path = Path.Combine(Application.persistentDataPath, subfolder, roomId);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Anchor file not found at {path}");
                return null;
            }

            try
            {
                var allScenarios = JsonConvert.DeserializeObject<Dictionary<string, List<SAHAnchorSaveData>>>(File.ReadAllText(path));
                Debug.Log($"Loaded room cache '{roomId}' with {allScenarios?.Count ?? 0} scenarios.");
                return allScenarios ?? new Dictionary<string, List<SAHAnchorSaveData>>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse anchor file at {path}: {ex.Message}");
                return null;
            }
        }

        public List<SAHAnchorSaveData> LoadLayoutCache(string roomId, string scenarioId, string subfolder = "anchors")
        {
            var allScenarios = LoadRoomCache(roomId, subfolder);

            if (allScenarios == null)
            {
                return null;
            }

            if (!allScenarios.TryGetValue(scenarioId, out var saveDataList))
            {
                Debug.LogWarning($"No layout found for scenario '{scenarioId}' in room '{roomId}'");
                return null;
            }

            Debug.Log($"Loaded layout '{scenarioId}' for room '{roomId}' with {saveDataList.Count} anchors.");
            return saveDataList;
        }
    }
}
