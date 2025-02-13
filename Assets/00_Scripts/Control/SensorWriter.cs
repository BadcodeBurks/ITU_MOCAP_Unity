using UnityEngine;

namespace Burk
{
    public class TensionSensorWriter
    {
        private int _bufferIndex;
        public int BufferIndex => _bufferIndex;
        private SimulatedBufferContainer.BufferWriter _bufferWriter;
        public TensionSensorWriter(SimulatedBufferContainer.BufferWriter reader, int bufferIndex)
        {
            _bufferWriter = reader;
            _bufferIndex = bufferIndex;
        }

        public void Write(float value)
        {
            _bufferWriter.WriteFloat(_bufferIndex, value);
        }
    }

    public class IMUWriter
    {
        private int _bufferIndex;
        public int BufferIndex => _bufferIndex;
        private SimulatedBufferContainer.BufferWriter _bufferWriter;
        public IMUWriter(SimulatedBufferContainer.BufferWriter reader, int bufferIndex)
        {
            _bufferWriter = reader;
            _bufferIndex = bufferIndex;
        }

        public void Write(Vector3 value)
        {
            _bufferWriter.WriteVector3(_bufferIndex, value);
        }
    }
}
