using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SAH
{
    [System.Serializable]
    public struct AnchorSaveData
    {
        public string anchorId;
        public string prefabPath;
        public string type;
    }

    [System.Serializable]
    public class AnchorSaveDataList
    {
        public List<AnchorSaveData> anchors;
    }
    public struct Anchor
    {
        public OVRSpatialAnchor anchor;
        public string prefabPath;
        public string type;
    }

    public static class SpatialAnchorUtils
    {
        public static OVRSpatialAnchor PlaceSpatialAnchor(Vector3 position, Quaternion rotation, GameObject prefab)
        {
            GameObject parent = GameObject.Find("SpatialAnchors");
            if (parent == null)
            {
                parent = new GameObject("SpatialAnchors");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent.transform);

            OVRSpatialAnchor anchor = instance.AddComponent<OVRSpatialAnchor>();

            return anchor;
        }

        public static void ClearSpatialAnchors()
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

        public static async void SaveSpatialAnchors(List<Anchor> anchors, string roomId, string scenarioId, string subfolder = "anchors")
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
            Dictionary<string, List<AnchorSaveData>> allScenarios = File.Exists(path)
                ? JsonConvert.DeserializeObject<Dictionary<string, List<AnchorSaveData>>>(File.ReadAllText(path))
                : new Dictionary<string, List<AnchorSaveData>>();

            List<AnchorSaveData> saveDataList = new List<AnchorSaveData>();

            foreach (var pair in anchors)
            {
                var result = await pair.anchor.SaveAsync();
                if (result)
                {
                    saveDataList.Add(new AnchorSaveData
                    {
                        anchorId = pair.anchor.Uuid.ToString(),
                        prefabPath = pair.prefabPath,
                        type = pair.type
                    });

                    Debug.Log($"Anchor {pair.anchor.Uuid} saved successfully.");
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

        public static async Task<List<Anchor>> LoadSpatialAnchors(string roomId, string scenarioId, string subfolder = "anchors")
        {
            string path = Path.Combine(Application.persistentDataPath, subfolder, roomId);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Anchor file not found at {path}");
                return null;
            }

            Dictionary<string, List<AnchorSaveData>> allScenarios =
                JsonConvert.DeserializeObject<Dictionary<string, List<AnchorSaveData>>>(File.ReadAllText(path));

            if (!allScenarios.TryGetValue(scenarioId, out var saveDataList))
            {
                Debug.LogWarning($"No anchors found for scenario '{scenarioId}'");
                return null;
            }

            List<Guid> anchorIds = new List<Guid>();
            Dictionary<Guid, AnchorSaveData> idToData = new Dictionary<Guid, AnchorSaveData>();

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

            List<Anchor> anchors = new List<Anchor>();

            foreach (var unbound in unboundAnchors)
            {
                bool success = await unbound.LocalizeAsync();
                if (!success)
                {
                    Debug.LogWarning($"Localization failed for anchor {unbound.Uuid}");
                    continue;
                }

                if (!idToData.TryGetValue(unbound.Uuid, out var anchorData))
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
                var boundAnchor = spawned.AddComponent<OVRSpatialAnchor>();
                unbound.BindTo(boundAnchor);

                Anchor anchor = new Anchor();
                anchor.anchor = boundAnchor;
                anchor.prefabPath = anchorData.prefabPath;
                anchor.type = anchorData.type;

                anchors.Add(anchor);

                Debug.Log($"Loaded and placed prefab for anchor {unbound.Uuid}");
            }

            return anchors;
        }

        public static void ClearRoomCache(string roomId, string subfolder = "anchors")
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

        public static void ClearAllCaches(string subfolder = "anchors")
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
    }
}