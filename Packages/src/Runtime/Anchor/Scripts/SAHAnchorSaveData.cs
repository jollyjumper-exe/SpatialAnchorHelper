using System;

namespace SAH
{
    [Serializable]
    public struct SAHVector3
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public struct SAHAnchorSaveData
    {
        public string anchorId;
        public string prefabPath;
        public string type;
        public SAHVector3? scale;
    }
}
