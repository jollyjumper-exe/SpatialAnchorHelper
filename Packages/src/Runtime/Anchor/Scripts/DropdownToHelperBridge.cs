using UnityEngine;
using TMPro;

namespace SpatialAnchorHelper
{
    public class DropdownToHelperBridge : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private SpatialAnchorsHelperUI _spatialAnchorsHelperUI;

        public bool sendValueOnStart = true;

        private void Start()
        {
            _dropdown.onValueChanged.AddListener(OnDropdownChanged);

            if (sendValueOnStart)
            {
                string selectedValue = _dropdown.options[_dropdown.value].text;
                _spatialAnchorsHelperUI.layoutID = selectedValue;
            }

        }

        private void OnDropdownChanged(int index)
        {
            string selectedValue = _dropdown.options[index].text;
            _spatialAnchorsHelperUI.layoutID = selectedValue;
        }

    }
}