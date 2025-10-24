using UnityEngine;
using TMPro;

namespace SAH
{
    public class DropdownToHelperBridge : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private SpatialAnchorHelperFunctionsWrapper _spatialAnchorHelperFunctionsWrapper;

        public bool sendValueOnStart = true;

        private void Start()
        {
            _dropdown.onValueChanged.AddListener(OnDropdownChanged);

            if (sendValueOnStart)
            {
                string selectedValue = _dropdown.options[_dropdown.value].text;
                _spatialAnchorHelperFunctionsWrapper.layoutID = selectedValue;
            }

        }

        private void OnDropdownChanged(int index)
        {
            string selectedValue = _dropdown.options[index].text;
            _spatialAnchorHelperFunctionsWrapper.layoutID = selectedValue;
        }

    }
}