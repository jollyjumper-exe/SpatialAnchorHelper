using SAH;
using UnityEngine;
using UnityEngine.UI; 

namespace SAH{
    public class SetupObject : MonoBehaviour
    {
        [SerializeField] private Button _button;
        private GameObject _ghostModel;
        
        
        private void Start()
        {

        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        public void SetGhostModel(GameObject ghostModel)
        {
            _ghostModel = ghostModel;
            _ghostModel.transform.parent = transform;
            _ghostModel.transform.localPosition = Vector3.zero;
            _ghostModel.transform.localRotation = Quaternion.identity;

            Renderer[] renderers = _ghostModel.GetComponentsInChildren<Renderer>();

            Bounds bounds = renderers[0].bounds;

            if (renderers.Length == 0) bounds = new Bounds(_ghostModel.transform.position, Vector3.zero);

            else{
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            Debug.Log("Center: " + bounds.center);
            Debug.Log("Size: " + bounds.size);

            Vector3 pos = bounds.center;
            pos.y += bounds.size.y/2;

            _button.GetComponentInParent<Canvas>().transform.position = pos;

        }

        public void SetSpatialAnchorHelperUI(SpatialAnchorHelperUI spatialAnchorHelperUI)
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() =>
                {
                    spatialAnchorHelperUI.PlaceAndSaveAnchor(transform);
                    Destroy(gameObject);
                });
            }
        }
    }
}