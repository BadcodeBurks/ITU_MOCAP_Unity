using System;
using System.Collections.Generic;
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
        [SerializeField] private bool autoMap;
        [SerializeField] private float inputDeadzone = 1.2f;
        [SerializeField] private AnimationCurve _lutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private int _parameterHash;
        private float[] _values;
        private float _latestInput;

        private Animator _animator;

        public override ControlType ControlType => ControlType.AnimationParam;

        public void SetAnimator(ref Animator animator)
        {
            if (!animator.TryGetNameHash(parameterName, out _parameterHash)) return;
            if (autoMap)
            {
                valueRange.x = 0f;
                valueRange.y = 1f;
            }
            _latestInput = 0f;
            _animator = animator;
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new TensionParameterControlBinding(readerKey, this);
        }

        public void Update(float value)
        {
            if (autoMap) ConfigureMapping(value);
            value = ApplyDeadzone(value);
            value = ApplyMapping(value);
            value = GetTemporalAverage(value);
            value = _lutCurve.Evaluate(value);
            _animator.SetFloat(_parameterHash, value);
        }

        private float ApplyDeadzone(float inputValue)
        {
            if (Mathf.Abs(_latestInput - inputValue) > inputDeadzone)
            {
                _latestInput = inputValue;
            }

            return _latestInput;
        }

        private void ConfigureMapping(float value)
        {
            if (value < 0.02f || value > 1024) return;
            if (value < valueRange.x || valueRange.x <= 0.02f) valueRange.x = value;
            if (value > valueRange.y || valueRange.y > 1024) valueRange.y = value;
        }

        private float ApplyMapping(float value) => (value - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;

        private float GetTemporalAverage(float value)
        {
            if (_values == null) _values = new float[20];
            Array.Copy(_values, 1, _values, 0, _values.Length - 1);
            _values[_values.Length - 1] = value;
            float sum = 0f;
            foreach (float val in _values) sum += val;
            return sum / _values.Length;
        }
    }
}
