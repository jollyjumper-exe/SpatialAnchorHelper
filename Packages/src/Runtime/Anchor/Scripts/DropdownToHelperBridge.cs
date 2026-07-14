using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace SAH
{
    public class DropdownToHelperBridge : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private SpatialAnchorHelperFunctionsWrapper _spatialAnchorHelperFunctionsWrapper;
        [SerializeField] private TMP_Text _summaryTextField;

        public bool sendValueOnStart = true;

        private void Start()
        {
            _dropdown.onValueChanged.AddListener(OnDropdownChanged);
            
            SpatialAnchorHelper.OnSaving += UpdateSummary;
            SpatialAnchorHelper.OnClearingRoomCache += UpdateSummary;
            SpatialAnchorHelper.OnClearingAllCaches += UpdateSummary;

            if (sendValueOnStart)
            {
                string selectedValue = _dropdown.options[_dropdown.value].text;
                _spatialAnchorHelperFunctionsWrapper.layoutID = selectedValue;

                UpdateSummary();
            }

        }

        private void OnDropdownChanged(int index)
        {
            string selectedValue = _dropdown.options[index].text;
            _spatialAnchorHelperFunctionsWrapper.layoutID = selectedValue;

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            List<AnchorSaveData> savedAnchors = null;

            if(SpatialAnchorHelper.Instance != null) 
                savedAnchors = SpatialAnchorHelper.Instance.LoadLayoutCache(_spatialAnchorHelperFunctionsWrapper.layoutID);

            if(savedAnchors != null)
            {
                string summary = "";
                foreach(AnchorSaveData anchor in savedAnchors)
                {
                    string entry = $"\n{anchor.anchorId}, {anchor.type}";
                    summary += entry;
                }

                if(summary != "")
                    _summaryTextField.text = summary;
                else
                    _summaryTextField.text = "Nothing found...";
            }
            else 
                _summaryTextField.text = "Nothing found...";
        }

    }
}