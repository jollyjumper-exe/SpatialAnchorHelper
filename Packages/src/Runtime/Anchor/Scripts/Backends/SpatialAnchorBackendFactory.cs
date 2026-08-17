using System;
using UnityEngine.XR.ARFoundation;

namespace SAH
{
    public enum SpatialAnchorBackendType
    {
        Meta,
        ARFoundation
    }

    public static class SpatialAnchorBackendFactory
    {
        public static ISpatialAnchorBackend Create(
            SpatialAnchorBackendType backendType)
        {
            switch (backendType)
            {
                case SpatialAnchorBackendType.Meta:
                    return new MetaSpatialAnchorBackend();

                case SpatialAnchorBackendType.ARFoundation:
                    return new ARFoundationSpatialAnchorBackend();

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(backendType),
                        backendType,
                        "Unsupported spatial-anchor backend.");
            }
        }
    }
}
