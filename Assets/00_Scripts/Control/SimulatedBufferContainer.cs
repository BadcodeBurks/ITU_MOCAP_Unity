using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// SimulatedBufferContainer
    /// </summary>
    [CreateAssetMenu(menuName = "Burk/SimulatedBufferContainer")]
    public class SimulatedBufferContainer : BufferContainer
    {
        public class BufferWriter
        {
            SimulatedBufferContainer _container;
            public BufferWriter(SimulatedBufferContainer container)
            {
                _container = container;
            }

            public void WriteFloat(int index, float value)
            {
                _container.Write(index, value);
            }

            public void WriteVector3(int index, Vector3 value)
            {
                _container.Write(index, value.x);
                _container.Write(index + 1, value.y);
                _container.Write(index + 2, value.z);
            }
        }
        BufferWriter _writer;
        public List<string> tensionReaderKeys;
        public List<string> imuReaderKeys;

        private Dictionary<string, TensionSensorWriter> _tensionWriterCache;
        public Dictionary<string, IMUWriter> _imuWriterCache;

        public override void Init()
        {
            _reader = new BufferReader(this);
            _writer = new BufferWriter(this);

            int tensionReaderCount = tensionReaderKeys.Count;
            int imuReaderCount = imuReaderKeys.Count;

            List<string> keys = new List<string>();
            keys.AddRange(tensionReaderKeys);
            keys.AddRange(imuReaderKeys);

            CreateBuffer(tensionReaderCount, imuReaderCount);
            CreateReaders(keys, tensionReaderCount, imuReaderCount, true);
            CreateWriters(keys, tensionReaderCount, imuReaderCount);

            _isInitialized = true;
            OnBufferInitialized?.Invoke();
        }

        public void CreateWriters(List<string> keys, int tensionSensorCount, int imuCount)
        {
            _tensionWriterCache = new Dictionary<string, TensionSensorWriter>();
            _imuWriterCache = new Dictionary<string, IMUWriter>();
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int bufferIndex = GetBufferStartIndex(i);
                if (i < tensionSensorCount)
                {
                    TensionSensorWriter tr = new TensionSensorWriter(_writer, bufferIndex);
                    _tensionWriterCache.Add(key, tr);
                }
                else
                {
                    IMUWriter ir = new IMUWriter(_writer, bufferIndex);
                    _imuWriterCache.Add(key, ir);
                }
            }
            int GetBufferStartIndex(int i)
            {
                if (i < tensionSensorCount)
                {
                    return i;
                }
                else
                {
                    return tensionSensorCount + (i - tensionSensorCount) * 3;
                }
            }
        }

        public TensionSensorWriter GetTensionWriter(string key)
        {
            if (_tensionWriterCache.ContainsKey(key)) return _tensionWriterCache[key];
            return null;
        }

        public IMUWriter GetIMUWriter(string key)
        {
            if (_imuWriterCache.ContainsKey(key)) return _imuWriterCache[key];
            return null;
        }

        public string[] GetTensionSensorKeys()
        {
            return _tensionWriterCache.Keys.ToArray();
        }
        public string[] GetIMUKeys()
        {
            return _imuWriterCache.Keys.ToArray();
        }

        public BufferWriter GetBufferWriter()
        {
            return _writer;
        }

        public List<string> GetAllKeys()
        {
            if (!_isInitialized) return null;
            return _tensionWriterCache.Keys.Union(_imuWriterCache.Keys).ToList();
        }
    }
}
