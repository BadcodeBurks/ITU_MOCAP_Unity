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
            //Debug.Log("binding " + _paramControl.Key + " to " + buffer.name);
            _reader = buffer.GetTensionReader(readerKey);
            _isBound = true;
        }

        public override void Unbind(bool reset = false)
        {
            //Debug.Log("Unbinding " + _paramControl.Key);
            if (reset) _paramControl.Update(0, true);
            _isBound = false;
        }

        public override void Update()
        {
            if (!_isBound) return;
            _paramControl.Update(_reader.Read(), _reader.UseRaw);
            //IDEA: Separate Reading and updating, for value processing.
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
        [SerializeField] private float inputDeadzone = 1.2f;
        [SerializeField] private AnimationCurve _lutCurve = AnimationCurve.Linear(0, 0, 1, 1);
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

        public void Update(float value, bool useRaw)
        {

            if (!useRaw) value = GetTemporalAverage(value);
            value = _lutCurve.Evaluate(value);
            _animator.SetFloat(_parameterHash, value);
        }
        int movingAverageLength = 0;
        float movingAverage = 0f;
        private float GetTemporalAverage(float value)
        {
            movingAverage += value / Mathf.Max(movingAverageLength, 1f);
            if (movingAverageLength < 10)
            {
                movingAverageLength++;
            }
            else
            {
                movingAverage -= movingAverage / movingAverageLength;
            }
            return movingAverage;
        }
    }
}
