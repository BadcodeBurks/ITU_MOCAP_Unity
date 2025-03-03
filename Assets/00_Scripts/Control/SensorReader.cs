using UnityEngine;

namespace Burk
{
    public class TensionSensorReader
    {
        private bool _useRaw;
        public bool UseRaw => _useRaw;
        private int _bufferIndex;
        public int BufferIndex => _bufferIndex;
        private BufferContainer.BufferReader _bufferReader;
        private Vector2 _valueRange;
        private const float inputDeadzone = 1.2f;

        public TensionSensorReader(BufferContainer.BufferReader reader, int bufferIndex, bool useRaw = false)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
            _useRaw = useRaw;
        }

        public float Read()
        {
            float value = _bufferReader.ReadFloat(_bufferIndex);
            if (_useRaw) return value;
            //value = ApplyDeadzone(value);
            ConfigureMapping(value);
            value = (value - _valueRange.x) / (_valueRange.y - _valueRange.x);
            return value;
        }

        private void ConfigureMapping(float value)
        {
            if (value < 0.02f || value > 1024) return;
            if (value < _valueRange.x || _valueRange.x <= 0.02f)
            {
                _valueRange.x = value;
            }
            if (value > _valueRange.y || _valueRange.y > 1024)
            {
                _valueRange.y = value;
            }
        }

        float _latestInput = 0f;
        private float ApplyDeadzone(float inputValue)
        {
            if (Mathf.Abs(_latestInput - inputValue) > inputDeadzone)
            {
                _latestInput = inputValue;
            }

            return _latestInput;
        }
    }

    public class IMUReader
    {
        private bool _useRaw;
        public bool UseRaw => _useRaw;
        private int _bufferIndex;
        public int BufferIndex => _bufferIndex;
        private BufferContainer.BufferReader _bufferReader;

        public IMUReader(BufferContainer.BufferReader reader, int bufferIndex, bool useRaw = false)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
            _useRaw = useRaw;
        }

        public Vector3 Read()
        {
            return _bufferReader.ReadVector3(_bufferIndex);
        }
    }
}
