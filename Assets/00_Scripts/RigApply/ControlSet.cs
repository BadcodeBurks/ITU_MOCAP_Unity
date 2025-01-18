using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// ControlSet
    /// </summary>
    public class ControlSet : MonoBehaviour
    {
        [Serializable]
        public class ControlWrapper
        {
            [SerializeReference] public Control value;
            [SerializeReference] public string key;
        }
        [SerializeField][ConfigNamePopup] private string _configName;
        [SerializeField] string _name = "ControlSet";
        public string Name => _name;

        [SerializeField] private List<ControlWrapper> _controlList;

        private List<SensorBinding> _bindings;
        [SerializeField] private Animator animator;

        private bool _isBound = false;
        private bool _isCalibrating;


        public void Init(BufferContainer buffer)
        {
            // _isBound = false;
            InitControls();
            BindControls(buffer);
        }

        private void InitControls()
        {
            _bindings = new List<SensorBinding>();

            for (int i = 0; i < _controlList.Count; i++)
            {
                _bindings.Add(CreateBinding(_controlList[i].value, _controlList[i].key));
            }
        }

        public void BindControls(BufferContainer buffer)
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                _controlList[i].value.ResetCalibration();
                _bindings[i].Bind(buffer);
            }

            _isBound = true;
        }

        public void UnbindControls()
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Unbind();
            }
            _isBound = false;
        }

        private SensorBinding CreateBinding(Control control, string readerKey)
        {
            if (control.ControlType == ControlType.AnimationParam)
            {
                ParameterControl controlP = control as ParameterControl;
                controlP.SetAnimator(ref animator);
            }
            else if (control.ControlType == ControlType.Transform)
            {
                TransformControl controlT = control as TransformControl;
                controlT.Init();
            }
            return control.CreateBinding(readerKey);
        }

        public void Update()
        {
            if (!_isBound) return;
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Update();
            }
        }

        public void CalibrateControls(float calibrationDuration)
        {
            if (_isCalibrating) return;
            StartCoroutine(Calibrate(calibrationDuration));
        }

        private IEnumerator Calibrate(float calibrationDuration)
        {
            Debug.Log("Calibrating");
            _isCalibrating = true;
            for (int i = 0; i < _bindings.Count; i++)
            {
                _controlList[i].value.StartCalibration();
            }
            yield return new WaitForSeconds(calibrationDuration);
            for (int i = 0; i < _bindings.Count; i++)
            {
                _controlList[i].value.EndCalibration();
            }
            _isCalibrating = false;
            Debug.Log("Stopped calibration");
        }
    }
}
