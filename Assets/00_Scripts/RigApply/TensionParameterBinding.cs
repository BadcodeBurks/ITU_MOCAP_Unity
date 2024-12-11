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
        private Vector2Int valueRangeCounts = new Vector2Int(0, 0);

        private Animator _animator;

        public override ControlType ControlType => ControlType.AnimationParam;

        public void SetAnimator(ref Animator animator)
        {
            if (!animator.TryGetNameHash(parameterName, out _parameterHash)) return;
            if (autoMap)
            {
                valueRange.x = -1f;
                valueRange.y = -1f;
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
            _animator.SetFloat(_parameterHash, value);
        }

        private void ConfigureMapping(float value)
        {
            if (value < valueRange.x || valueRange.x < 1) valueRange.x = value;
            if (value > valueRange.y || valueRange.y > 1024) valueRange.y = value;
        }
    }
}
