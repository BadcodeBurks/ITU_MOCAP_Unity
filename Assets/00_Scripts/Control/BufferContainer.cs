using System;
using System.Collections;
using System.Collections.Generic;
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

        protected void CreateReaders(List<string> keys, int tensionSensorCount, int imuCount, bool useRaw = false)
        {
            _tensionReaderCache = new Dictionary<string, TensionSensorReader>();
            _imuReaderCache = new Dictionary<string, IMUReader>();
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int bufferIndex = GetBufferStartIndex(i);
                if (i < tensionSensorCount)
                {
                    TensionSensorReader tr = new TensionSensorReader(_reader, bufferIndex, useRaw);
                    _tensionReaderCache.Add(key, tr);
                }
                else
                {
                    IMUReader ir = new IMUReader(_reader, bufferIndex, useRaw);
                    _imuReaderCache.Add(key, ir);
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
        private float Read(int index) => m_Buffer[index];
        protected void Write(int index, float value) => m_Buffer[index] = value;

        public TensionSensorReader GetTensionReader(string key)
        {
            if (_tensionReaderCache.ContainsKey(key)) return _tensionReaderCache[key];
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

        public virtual IEnumerator ReadFromPipe()
        {
            Debug.Log("Empty Coroutine");
            yield return null;
        }
    }
}

