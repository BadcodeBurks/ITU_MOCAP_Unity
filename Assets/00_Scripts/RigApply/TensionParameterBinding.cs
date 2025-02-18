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

        public override void Unbind(bool reset = false)
        {
            if (reset) _paramControl.Update(0, true);
            _isBound = false;
        }

        public override void Update()
        {
            if (!_isBound) return;
            _paramControl.Update(_reader.Read(), _reader.UseRaw);
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
                _valueRange.x = 0f;
                _valueRange.y = 1f;
            }
            _latestInput = 0f;
            _animator = animator;
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new TensionParameterControlBinding(readerKey, this);
        }

        public void Update(float value, bool useRaw)
        {
            if (autoMap && _isCalibrating && !useRaw) ConfigureMapping(value);
            if (!useRaw)
            {
                value = ApplyDeadzone(value);
                value = ApplyMapping(value);
                value = GetTemporalAverage(value);
            }
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
            if (value < _valueRange.x || _valueRange.x <= 0.02f) _valueRange.x = value;
            if (value > _valueRange.y || _valueRange.y > 1024) _valueRange.y = value;
        }

        private float ApplyMapping(float value) => (value - _valueRange.x) / (_valueRange.y - _valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;

        private float GetTemporalAverage(float value)
        {
            if (_values == null) _values = new float[10];
            int sourceIndex = _values.Length == 0 ? 0 : 1;
            float[] newValues = new float[_values.Length + 1];
            Array.Copy(_values, sourceIndex, newValues, 0, Mathf.Clamp(_values.Length - 1, 0, 9));
            newValues[newValues.Length - 1] = value;
            _values = newValues;
            float sum = 0f;
            foreach (float val in _values) sum += val;
            return sum / _values.Length;
        }
    }
}
