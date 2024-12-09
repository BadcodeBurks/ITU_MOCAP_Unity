using System;
using UnityEngine;

namespace Burk
{
    public enum SensorType
    {
        IMU,
        Tension
    }

    [Serializable]
    public abstract class SensorBinding
    {
        public SensorBinding(string readerKey)
        {
            this.readerKey = readerKey;
        }
        [SerializeField] protected string readerKey;
        [SerializeField] protected SensorType sensorType;
        protected bool _isBound;
        public abstract SensorType GetSensorType();
        public abstract void Bind(BufferContainer buffer);
        public abstract void Update();
    }
}