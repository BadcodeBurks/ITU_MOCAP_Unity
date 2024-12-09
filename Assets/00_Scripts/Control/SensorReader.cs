using UnityEngine;

namespace Burk
{
    public class TensionSensorReader
    {
        private int _bufferIndex;
        private BufferContainer.BufferReader _bufferReader;
        public TensionSensorReader(BufferContainer.BufferReader reader, int bufferIndex)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
        }

        public float Read()
        {
            return _bufferReader.ReadFloat(_bufferIndex);
        }
    }

    public class IMUReader
    {
        private int _bufferIndex;
        private BufferContainer.BufferReader _bufferReader;
        public IMUReader(BufferContainer.BufferReader reader, int bufferIndex)
        {
            _bufferReader = reader;
            _bufferIndex = bufferIndex;
        }

        public Vector3 Read()
        {
            return _bufferReader.ReadVector3(_bufferIndex);
        }
    }
}
