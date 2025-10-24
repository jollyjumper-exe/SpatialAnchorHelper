using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace SAH
{
    public class SpatialAnchorHelper : MonoBehaviour
    {
        public static SpatialAnchorHelper Instance { get; private set; }

        [SerializeField] private string _persistentDataLocation = "anchors";

        private List<Anchor> _anchors;
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
            _anchors = new List<Anchor>();
            StartCoroutine(FetchRoomIdCoroutine());
        }

        public void SaveSpatialAnchors(string layoutID)
        {
            if (_roomID == null) return;
            SpatialAnchorUtils.SaveSpatialAnchors(_anchors, _roomID, layoutID, _persistentDataLocation);
        }

        public async void LoadSpatialAnchors(string layoutID)
        {
            if (_roomID == null) return;
            ClearSpatialAnchors();
            _anchors = await SpatialAnchorUtils.LoadSpatialAnchors(_roomID, layoutID, _persistentDataLocation);
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

            _anchors.Add(anchor);
        }

        public void ClearSpatialAnchors()
        {
            SpatialAnchorUtils.ClearSpatialAnchors();
            _anchors.Clear();
        }

        public void ClearRoomCache()
        {
            SpatialAnchorUtils.ClearRoomCache(_roomID, _persistentDataLocation);
        }

        public void ClearAllCaches()
        {
            SpatialAnchorUtils.ClearAllCaches(_persistentDataLocation);
        }

        private IEnumerator FetchRoomIdCoroutine()
        {
            while (_roomID == null)
            {
                if (MRUK.Instance != null && MRUK.Instance.GetCurrentRoom() != null)
                {
                    _roomID = MRUK.Instance.GetCurrentRoom().Anchor.Uuid.ToString();
                    Debug.Log($"Found Room {_roomID}");
                    yield break;
                }
                else
                {
                    Debug.Log("No room found...");
                }

                yield return new WaitForSeconds(0.1f); // Wait 100ms before trying again
            }
        }
    }
}