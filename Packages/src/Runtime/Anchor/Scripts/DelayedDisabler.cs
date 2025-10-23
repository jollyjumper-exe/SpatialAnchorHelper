using UnityEngine;
using System.Collections;

namespace SpatialAnchorHelper
{
    public class DelayedDisabler : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour component;

        public void DelayedDisable()
        {
            StartCoroutine(DisableAfterFrame());
        }

        private IEnumerator DisableAfterFrame()
        {
            yield return null;
            component.enabled = false;
        }
    }
}