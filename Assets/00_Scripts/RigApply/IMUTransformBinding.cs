using System;
using UnityEngine;

namespace Burk
{
    public class IMUTransformBinding : SensorBinding
    {
        public IMUTransformBinding(TransformControl control, string readerKey) : base(readerKey)
        {
            _transformControl = control;
        }

        private TransformControl _transformControl;
        private IMUReader _reader;
        public override void Bind(BufferContainer buffer)
        {
            _reader = buffer.GetIMUReader(readerKey);
            _isBound = true;
        }

        public override void Unbind(bool reset = false)
        {
            if (reset) _transformControl.Update(Vector3.zero, true);
            _isBound = false;
        }

        public override SensorType GetSensorType() => SensorType.IMU;

        public override void Update()
        {
            if (!_isBound) return;
            Vector3 pyrValue = _reader.Read();
            _transformControl.Update(pyrValue, _reader.UseRaw);
        }
    }

    public class TransformControl : Control
    {
        public TransformControl()
        {
            _controlType = ControlType.Transform;
        }
        [SerializeField] Transform transform;
        [SerializeField] Transform imuTransform;
        Quaternion rotation;

        public override ControlType ControlType => ControlType.Transform;

        public void Init()
        {
            FindRotationMatrix();
            _valueRange = new Vector2(0f, 360f);
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new IMUTransformBinding(this, readerKey);
        }

        private void FindRotationMatrix()
        {
            rotation = Quaternion.Inverse(imuTransform.localRotation);
        }

        public void Update(Vector3 val, bool useRaw = false)
        {
            Vector3 temp = val;
            if (!useRaw)
            {
                val.z = (temp.x - _valueRange.x) / (_valueRange.y - _valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
                val.x = (temp.y - _valueRange.x) / (_valueRange.y - _valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
                val.y = (temp.z - _valueRange.x) / (_valueRange.y - _valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            }
            transform.rotation = Quaternion.Euler(val) * rotation;
        }

        public override void ResetCalibration()
        {
        }
    }
}
