using System;
using Unity.VisualScripting;
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
        private Vector3 _value;
        public override void Bind(BufferContainer buffer)
        {
            _reader = buffer.GetIMUReader(readerKey);
            _transformControl.Init();
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
            _value = _reader.Read();
        }
        public override void Apply()
        {
            _transformControl.Update(_value, _reader.UseRaw);
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
        Quaternion forwardingRotation;
        public override ControlType ControlType => ControlType.Transform;

        public void Init()
        {
            FindRotationMatrix();
            _valueRange = new Vector2(0f, 360f);
            Update(Vector3.zero, true);
        }

        public override SensorBinding CreateBinding(string readerKey)
        {
            return new IMUTransformBinding(this, readerKey);
        }

        private void FindRotationMatrix()
        {
            rotation = Quaternion.Inverse(imuTransform.localRotation);
            SetForward(Vector3.right, Vector3.up);
        }

        public void SetForward(Vector3 forward, Vector3 up)
        {
            Vector3 desiredRight = Vector3.Cross(up, forward).normalized;
            Vector3 f = Quaternion.Inverse(forwardingRotation) * imuTransform.forward;
            Debug.DrawLine(imuTransform.position, imuTransform.position + f, Color.red);
            f.y = 0;
            f.Normalize();
            float angleY = Vector3.SignedAngle(f, forward, up);
            if (Vector3.Dot(forward, f) < 0) angleY += 180;
            forwardingRotation = Quaternion.AngleAxis(angleY, up);
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
            Quaternion tempRotation = forwardingRotation * Quaternion.Euler(val) * rotation;
            transform.localRotation = tempRotation;
            //transform.rotation = forwardingRotation * Quaternion.Euler(val) * Quaternion.Inverse(imuTransform.localRotation); //TODO: Figure this out.
        }
    }
}
