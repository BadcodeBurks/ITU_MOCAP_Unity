using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burk
{
    public class BufferContainer : ScriptableObject
    {
        public class BufferReader
        {
            BufferContainer _container;
            public BufferReader(BufferContainer container)
            {
                _container = container;
            }

            public float ReadFloat(int index)
            {
                float value = _container.Read(index);
                return value;
            }

            public Vector3 ReadVector3(int index)
            {
                Vector3 val = new Vector3(
                    _container.Read(index),
                    _container.Read(index + 1),
                    _container.Read(index + 2)
                );
                return val;
            }
        }
        protected bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        protected BufferReader _reader;
        protected Dictionary<string, TensionSensorReader> _tensionReaderCache;
        protected Dictionary<string, IMUReader> _imuReaderCache;
        protected float[] m_Buffer;

        public Action OnBufferInitialized;
        public Action OnBufferWrite;
        private BufferMetadata metadata;

        private void OnEnable()
        {
            _isInitialized = false;
        }
        public virtual void Init()
        {
            _isInitialized = true;
            OnBufferInitialized?.Invoke();
        }

        protected void CreateBuffer(int tensionSensorCount, int imuCount)
        {
            m_Buffer = new float[tensionSensorCount + imuCount * 3];
            _reader = new BufferReader(this);
        }

        protected void CreateReaders(BufferMetadata metadata, bool applyMappings = false)
        {
            this.metadata = metadata;
            _tensionReaderCache = new Dictionary<string, TensionSensorReader>();
            _imuReaderCache = new Dictionary<string, IMUReader>();
            for (int i = 0; i < metadata.keys.Count; i++)
            {
                string key = metadata.keys[i];
                int bufferIndex = GetBufferStartIndex(i);
                if (i < metadata.tensionCount)
                {
                    TensionSensorReader tr = new TensionSensorReader(_reader, bufferIndex, metadata.useRaw);
                    if (applyMappings) tr.ApplyMapping(metadata.tensionCalibrations[i]);
                    _tensionReaderCache.Add(key, tr);
                }
                else
                {
                    IMUReader ir = new IMUReader(_reader, bufferIndex, metadata.useRaw);
                    _imuReaderCache.Add(key, ir);
                }
            }
            int GetBufferStartIndex(int i)
            {
                if (i < metadata.tensionCount)
                {
                    return i;
                }
                else
                {
                    return metadata.tensionCount + (i - metadata.tensionCount) * 3;
                }
            }
        }
        private float Read(int index) => m_Buffer[index];
        protected void Write(int index, float value)
        {
            m_Buffer[index] = value;
            OnBufferWrite?.Invoke();
        }

        public TensionSensorReader GetTensionReader(string key)
        {
            if (_tensionReaderCache.ContainsKey(key)) return _tensionReaderCache[key];
            Debug.Log("No Reader With Key: " + key);
            return null;
        }

        public IMUReader GetIMUReader(string key)
        {
            if (_imuReaderCache.ContainsKey(key)) return _imuReaderCache[key];
            return null;
        }

        public BufferContainer.BufferReader GetBufferReader()
        {
            return _reader;
        }

        public int GetKeyIndex(string key)
        {
            if (_tensionReaderCache.ContainsKey(key))
            {
                return _tensionReaderCache[key].BufferIndex;
            }
            else if (_imuReaderCache.ContainsKey(key))
            {
                return _imuReaderCache[key].BufferIndex;
            }
            return -1;
        }

        public Type GetKeyType(string key)
        {
            if (_tensionReaderCache.ContainsKey(key))
            {
                return typeof(float);
            }
            else if (_imuReaderCache.ContainsKey(key))
            {
                return typeof(Vector3);
            }
            return null;
        }

        public virtual float[] ReadFullBuffer()
        {
            return m_Buffer;
        }

        public virtual void WriteFullBuffer(float[] buffer)
        {
            buffer.CopyTo(m_Buffer, 0);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            OnBufferWrite?.Invoke();
        }

        public virtual IEnumerator ReadFromPipe()
        {
            Debug.Log("Empty Coroutine");
            yield return null;
        }

        public BufferContainer Clone()
        {
            BufferContainer newContainer = ScriptableObject.CreateInstance<BufferContainer>();
            List<TensionSensorReader> tensionReaders = _tensionReaderCache.Values.ToList();
            metadata.tensionCalibrations = tensionReaders.Select(x => x.GetMapping()).ToList();
            newContainer.CreateBuffer(metadata.tensionCount, metadata.imuCount);
            newContainer.CreateReaders(metadata, true);
            newContainer.Init();
            return newContainer;
        }
    }
}

