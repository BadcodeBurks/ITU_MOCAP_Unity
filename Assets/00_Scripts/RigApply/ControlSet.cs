using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// ControlSet
    /// </summary>
    public class ControlSet : MonoBehaviour
    {
        public static Action<ControlSet> OnControlSetValidated;
        private void OnValidate()
        {
            OnControlSetValidated?.Invoke(this);
        }

        [Serializable]
        public class ControlWrapper
        {
            [SerializeReference] public Control value;
            [SerializeReference] public string key;
        }
        //[SerializeField][ConfigNamePopup] private string _configName;
        [SerializeField] string _name = "ControlSet";
        public string Name => _name;

        [SerializeField] private List<ControlWrapper> _controlList;

        private List<SensorBinding> _bindings;
        private List<TransformControl> _transformControls = new List<TransformControl>();
        private List<ParameterControl> _parameterControls = new List<ParameterControl>();
        [SerializeField] private Animator animator;

        private bool _isBound = false;
        public bool IsBound => _isBound;
        private bool _usesAnimator;
        ParameterFeatureExtractor _paramFeatureExtractor;
        public ParameterFeatureExtractor ParamFeatureExtractor => _paramFeatureExtractor;

        public void Init(BufferContainer buffer)
        {
            InitControls();
            if (buffer != null) BindControls(buffer);
        }

        private void InitControls()
        {
            _bindings = new List<SensorBinding>();
            _parameterControls = new List<ParameterControl>();
            _transformControls = new List<TransformControl>();

            for (int i = 0; i < _controlList.Count; i++)
            {
                _usesAnimator = _usesAnimator || _controlList[i].value.GetType() == typeof(ParameterControl);
                _bindings.Add(CreateBinding(_controlList[i].value, _controlList[i].key));
            }

            _paramFeatureExtractor = new ParameterFeatureExtractor(_parameterControls);
        }

        public void BindControls(BufferContainer buffer)
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                //_controlList[i].value.ResetCalibration();
                _bindings[i].Bind(buffer);
            }
            Debug.Log($"{name} is Bound to " + buffer.name);
            _isBound = true;
            if (_usesAnimator) animator.StartPlayback();
        }

        public void UnbindControls(bool resetPositions = false)
        {
            if (!_isBound) return;
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Unbind(resetPositions);
            }
            if (resetPositions)
            {
                if (_usesAnimator) animator.Rebind();
            }
            _isBound = false;
        }

        private SensorBinding CreateBinding(Control control, string readerKey)
        {
            if (control.ControlType == ControlType.AnimationParam)
            {
                ParameterControl controlP = control as ParameterControl;
                controlP.SetAnimator(ref animator);
                _parameterControls.Add(controlP);
            }
            else if (control.ControlType == ControlType.Transform)
            {
                TransformControl controlT = control as TransformControl;
                controlT.Init();
                _transformControls.Add(controlT);
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
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Apply();
            }

            _paramFeatureExtractor.Update();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (_usesAnimator) animator.Update(EditorTime.DeltaTime);
            }
#endif
        }

        public void SetForward(Vector3 forward)
        {
            if (!_isBound) return;
            for (int i = 0; i < _transformControls.Count; i++)
            {
                _transformControls[i].SetForward(forward, Vector3.up);
            }
        }
    }
}
