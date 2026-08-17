using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SAH
{
    public interface ISpatialAnchorBackend
    {
        IEnumerator FetchRoomIdCoroutine(Action<string> onRoomIdFound);

        Task<SAHAnchor> PlaceSpatialAnchor(
            Vector3 position,
            Quaternion rotation,
            GameObject prefab);

        void ClearSpatialAnchors();

        Task SaveSpatialAnchors(
            List<SAHAnchor> anchors,
            string roomId,
            string scenarioId,
            string subfolder = "anchors");

        Task<List<SAHAnchor>> LoadSpatialAnchors(
            string roomId,
            string scenarioId,
            string subfolder = "anchors");

        void ClearRoomCache(string roomId, string subfolder = "anchors");

        void ClearAllCaches(string subfolder = "anchors");

        Dictionary<string, List<SAHAnchorSaveData>> LoadRoomCache(
            string roomId,
            string subfolder = "anchors");

        List<SAHAnchorSaveData> LoadLayoutCache(
            string roomId,
            string scenarioId,
            string subfolder = "anchors");
    }
}
