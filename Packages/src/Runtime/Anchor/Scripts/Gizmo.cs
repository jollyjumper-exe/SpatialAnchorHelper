using UnityEngine;
using System.Collections;

namespace SAH
{
    public class Gizmo: MonoBehaviour
    {
        [SerializeField] public Transform target; 

        private GizmoHandle _activeHandle = null;

        void Awake()
        {
            
        }

        void Update()
        {
            if(target == null) return;

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
    }
}