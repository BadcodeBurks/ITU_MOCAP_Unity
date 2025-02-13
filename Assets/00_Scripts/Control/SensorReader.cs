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
        public TensionSensorReader(BufferContainer.BufferReader reader, int bufferIndex, bool useRaw = false)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
            _useRaw = useRaw;
        }

        public float Read()
        {
            return _bufferReader.ReadFloat(_bufferIndex);
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
