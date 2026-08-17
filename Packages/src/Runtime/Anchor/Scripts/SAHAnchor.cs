using System;
using UnityEngine;

namespace SAH
{
    public sealed class SAHAnchor
    {
        public string Id { get; internal set; }

        public string PrefabPath { get; internal set; }

        public string Type { get; internal set; }

        public GameObject Content { get; internal set; }

        internal object NativeHandle { get; set; }
    }
}