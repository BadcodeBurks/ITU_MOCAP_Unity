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

        public override void Unbind() => _isBound = false;

        public override SensorType GetSensorType() => SensorType.IMU;

        public override void Update()
        {
            if (!_isBound) return;
            Vector3 pyrValue = _reader.Read();
            _transformControl.Update(pyrValue);
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
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new IMUTransformBinding(this, readerKey);
        }

        private void FindRotationMatrix()
        {
            rotation = Quaternion.Inverse(imuTransform.localRotation);
        }

        public void Update(Vector3 val)
        {
            Vector3 temp = val;
            //Debug.Log(val);

            val.z = (temp.x - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            val.x = (temp.y - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            val.y = (temp.z - valueRange.x) / (valueRange.y - valueRange.x) * (mapRange.y - mapRange.x) + mapRange.x;
            transform.rotation = Quaternion.Euler(val) * rotation;
        }

        public override void ResetCalibration()
        {
        }
    }
}
