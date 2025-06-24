using System;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    [CustomPropertyDrawer(typeof(Control))]
    public class ControlDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!PropertyTypeCheck<Control>(property))
            {
                Debug.Log("Creating control");
                property.managedReferenceValue = GenerateControl(ControlType.AnimationParam);
                property.FindPropertyRelative("_controlType").enumValueIndex = 1;
                property.serializedObject.ApplyModifiedProperties();

            }
            label.text = property.FindPropertyRelative("_key").stringValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (!PropertyTypeCheck(property)) property.managedReferenceValue = GenerateControl((ControlType)property.FindPropertyRelative("_controlType").enumValueIndex);
            }
        }

        private bool PropertyTypeCheck<T>(SerializedProperty property)
        {
            return property.managedReferenceValue != null && property.managedReferenceValue is T;
        }

        private bool PropertyTypeCheck(SerializedProperty property)
        {
            Type type = property.managedReferenceValue.GetType();
            Type requiredType = GetControlReferenceType((ControlType)property.FindPropertyRelative("_controlType").enumValueIndex);
            return property.managedReferenceValue != null && type == requiredType;
        }

        private Type GetControlReferenceType(ControlType type)
        {
            switch (type)
            {
                case ControlType.Transform:
                    return typeof(TransformControl);
                case ControlType.AnimationParam:
                    return typeof(ParameterControl);
                case ControlType.UISliderValue:
                    return typeof(UISliderControl);
                default:
                    return null;
            }
        }

        private Control GenerateControl(ControlType type)
        {
            switch (type)
            {
                case ControlType.Transform:
                    return new TransformControl();
                case ControlType.AnimationParam:
                    return new ParameterControl();
                case ControlType.UISliderValue:
                    return new UISliderControl();
                default:
                    return null;
            }
        }
    }
}
