using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Burk
{
    public class TensionParameterControlBinding : SensorBinding
    {
        public TensionParameterControlBinding(string readerKey, ParameterControl parameterControl) : base(readerKey)
        {
            _paramControl = parameterControl;
            this.readerKey = readerKey;
        }
        private ParameterControl _paramControl;
        private TensionSensorReader _reader;
        private float _value;
        public override SensorType GetSensorType() => SensorType.Tension;
        public override void Bind(BufferContainer buffer)
        {
            //Debug.Log("binding " + _paramControl.Key + " to " + buffer.name);
            _reader = buffer.GetTensionReader(readerKey);
            if (_reader == null)
            {
                Debug.LogError("No Reader With Key: " + readerKey);
                return;
            }
            _paramControl.Reset();
            _isBound = true;
            _value = 0;
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
            _value = _reader.Read();
        }

        public float GetValue() => _value;
        public void OverrideValue(float value) => _value = value;

        public override void Apply()
        {
            _paramControl.Update(_value, _reader.UseRaw);
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
        [SerializeField][FormerlySerializedAs("_lutCurve")] private AnimationCurve _mapCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private int _parameterHash;

        private Animator _animator;

        public override ControlType ControlType => ControlType.AnimationParam;

        private float _value;
        public float Value => _value;

        public void SetAnimator(ref Animator animator)
        {
            if (!animator.TryGetNameHash(parameterName, out _parameterHash)) return;
            _animator = animator;
            _animator.SetFloat(_parameterHash, 0);
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new TensionParameterControlBinding(readerKey, this);
        }

        public void Update(float value, bool useRaw)
        {

            if (!useRaw) value = GetTemporalAverage(value);
            //value = _lutCurve.Evaluate(value);
            _value = Mathf.Clamp01(value);
            _animator.SetFloat(_parameterHash, _value);
        }
        float movingAverage = 0f;
        private float GetTemporalAverage(float value)
        {
            movingAverage = Mathf.Lerp(movingAverage, value, 0.12f);
            return movingAverage;
        }

        internal void Reset()
        {
            movingAverage = 0f;
        }
    }
}
