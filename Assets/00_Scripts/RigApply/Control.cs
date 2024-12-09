using System;
using UnityEngine;

namespace Burk
{
    public enum ControlType
    {
        Transform,
        AnimationParam
    }
    [Serializable]
    public class Control
    {
        [SerializeField] protected string _key;
        public string Key => _key;
        [SerializeField] protected ControlType _controlType;
        public virtual ControlType ControlType => _controlType;

        public virtual SensorBinding CreateBinding(string readerKey) { return null; }
    }
}