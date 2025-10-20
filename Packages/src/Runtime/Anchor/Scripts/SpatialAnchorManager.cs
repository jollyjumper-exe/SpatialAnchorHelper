using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Meta.XR.MRUtilityKit;


public class SpatialAnchorManager : MonoBehaviour
{
    public static SpatialAnchorManager Instance { get; private set; }
    public List<Anchor> anchors { get; private set; }
    private string _currentLayoutID = "Playground";
    private string _roomID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        anchors = new List<Anchor>();
        StartCoroutine(FetchRoomIdCoroutine());
    }

    public void SaveSpatialAnchors(string layoutID)
    {
        if (_roomID == null) return;
        SpatialAnchorUtils.SaveSpatialAnchors(anchors, _roomID, layoutID);
    }

    public async void LoadSpatialAnchors(string layoutID)
    {
        if (_roomID == null) return;
        ClearSpatialAnchors();
        anchors = await SpatialAnchorUtils.LoadSpatialAnchors(_roomID, layoutID);
    }

    public void CreateSpatialAnchor(Vector3 position, Quaternion rotation, string prefabPath, string type)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at Resources/{prefabPath}");
            return;
        }

        OVRSpatialAnchor OVRAnchor = SpatialAnchorUtils.PlaceSpatialAnchor(position, rotation, prefab);

        Anchor anchor = new Anchor
        {
            anchor = OVRAnchor,
            prefabPath = prefabPath,
            type = type
        };

        anchors.Add(anchor);
    }

    public void ClearSpatialAnchors()
    {
        SpatialAnchorUtils.ClearSpatialAnchors();
        anchors.Clear();
    }

    private IEnumerator FetchRoomIdCoroutine()
    {
        while (_roomID == null)
        {
            MRUKRoom room = FindObjectOfType<MRUKRoom>();
            if (room != null)
            {
                string name = room.gameObject.name;
                Debug.Log($"Found room object named: {name}");

                string[] parts = name.Split(' ');
                if (parts.Length >= 2)
                {
                    string extractedRoomId = parts[1];
                    Debug.Log($"Extracted Room ID: {extractedRoomId}");
                    _roomID = extractedRoomId;
                    yield break;
                }
            }
            else
            {
                Debug.Log("No room found...");
            }

            yield return new WaitForSeconds(0.1f); // Wait 100ms before trying again
        }
    }
}


