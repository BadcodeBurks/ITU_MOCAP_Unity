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
        private int _parameterHash;
        private float[] values;

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
            _animator = animator;
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new TensionParameterControlBinding(readerKey, this);
        }

        public void Update(float value)
        {
            if (autoMap) ConfigureMapping(value);
            value = (value - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            _animator.SetFloat(_parameterHash, GetTemporalAverage(value));
        }

        private void ConfigureMapping(float value)
        {
            if (value < 0.02f || value > 1024) return;
            if (value < valueRange.x || valueRange.x <= 0.02f) valueRange.x = value;
            if (value > valueRange.y || valueRange.y > 1024) valueRange.y = value;
        }

        private float GetTemporalAverage(float value)
        {
            if (values == null) values = new float[20];
            Array.Copy(values, 1, values, 0, values.Length - 1);
            values[values.Length - 1] = value;
            float sum = 0f;
            foreach (float val in values) sum += val;
            return sum / values.Length;
        }
    }
}
