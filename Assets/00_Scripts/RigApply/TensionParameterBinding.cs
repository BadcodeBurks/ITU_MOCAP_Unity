using System;
using UnityEngine;

namespace Burk
{
    public class TensionParameterControlBinding : SensorBinding
    {
        public TensionParameterControlBinding(string readerKey, ParameterControl parameterControl) : base(readerKey)
        {
            _paramControl = parameterControl;
        }
        private ParameterControl _paramControl;
        private TensionSensorReader _reader;
        public override SensorType GetSensorType() => SensorType.Tension;
        public override void Bind(BufferContainer buffer)
        {
            _reader = buffer.GetTensionReader(readerKey);
            _isBound = true;
        }

        public override void Update()
        {
            if (!_isBound) return;
            _paramControl.Update(_reader.Read());
            //TODO: Separate Reading and updating, for value processing.
        }
    }

    [Serializable]
    public class ParameterControl : Control
    {
        public ParameterControl()
        {
            _controlType = ControlType.AnimationParam;
        }
        [SerializeField] private string parameterName;
        private int _parameterHash;
        private Animator _animator;

        public override ControlType ControlType => ControlType.AnimationParam;

        public void SetAnimator(ref Animator animator)
        {
            if (!animator.TryGetNameHash(parameterName, out _parameterHash)) return;
            _animator = animator;
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new TensionParameterControlBinding(readerKey, this);
        }

        public void Update(float value)
        {
            value = (value - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            _animator.SetFloat(_parameterHash, value);
        }
    }
}
