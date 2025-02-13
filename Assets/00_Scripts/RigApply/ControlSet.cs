using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// ControlSet
    /// </summary>
    [ExecuteAlways]
    public class ControlSet : MonoBehaviour
    {
        public static Action<ControlSet> OnControlSetValidated;
        private void OnValidate()
        {
            Debug.Log("OnValidate");
            OnControlSetValidated?.Invoke(this);
        }

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
        public bool IsBound => _isBound;
        private bool _isCalibrating;


        public void Init(BufferContainer buffer)
        {
            InitControls();
            if (buffer != null) BindControls(buffer);
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

        public void UnbindControls(bool resetPositions = false)
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Unbind(resetPositions);
            }
            if (resetPositions) animator.Update(0);
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
#if UNITY_EDITOR
            animator.Update(EditorTime.DeltaTime);
#endif
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
