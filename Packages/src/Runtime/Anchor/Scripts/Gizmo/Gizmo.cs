using System.Collections;
using UnityEngine;

namespace SAH
{
    public class Gizmo : MonoBehaviour
    {
        public Transform target;

        private GizmoHandle _activeHandle = null;

        void Awake()
        {

        }

        void Update()
        {
            if (target == null) return;

            transform.position = target.position;
            transform.rotation = target.rotation;
        }

        public bool TryActivateHandle(GizmoHandle handle)
        {
            return true;
        }

        public void TranslateTarget(Vector3 translation)
        {
            target.position += translation;
        }

        public void RotateTarget(Quaternion rotation)
        {
            target.rotation = rotation * target.rotation;
        }

        public void ScaleTarget(Vector3 scaleDelta, float minScale = 0.01f)
        {
            Vector3 scale = target.localScale + scaleDelta;

            scale.x = Mathf.Max(minScale, scale.x);
            scale.y = Mathf.Max(minScale, scale.y);
            scale.z = Mathf.Max(minScale, scale.z);

            target.localScale = scale;
        }
    }
}