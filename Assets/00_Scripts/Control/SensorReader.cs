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
        private const float inputDeadzone = 1f;
        private bool _calibrate = false;

        public TensionSensorReader(BufferContainer.BufferReader reader, int bufferIndex, bool useRaw = false)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
            _useRaw = useRaw;
            ResetCalibration();
        }

        public float Read()
        {
            float value = _bufferReader.ReadFloat(_bufferIndex);
            if (_useRaw) return value;
            value = ApplyDeadzone(value);
            if (_calibrate) ConfigureMapping(value);
            value = (value - _valueRange.x) / Mathf.Max(_valueRange.y - _valueRange.x, 1f);
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

        public void ResetCalibration()
        {
            _calibrate = true;
            _valueRange.x = float.MaxValue;
            _valueRange.y = float.MinValue;
        }

        public void StopCalibration()
        {
            _calibrate = false;
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

        public void ApplyMapping(Vector2 valueRange)
        {
            _valueRange = valueRange;
        }

        public Vector2 GetMapping()
        {
            if (UseRaw) return Vector2.up;
            return _valueRange;
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
