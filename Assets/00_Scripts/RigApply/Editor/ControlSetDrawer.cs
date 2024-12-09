using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    [CustomPropertyDrawer(typeof(ControlSet.ControlWrapper))]
    public class ControlSetDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty controlProperty = property.FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(controlProperty, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty controlProperty = property.FindPropertyRelative("value");
            SerializedProperty keyProperty = property.FindPropertyRelative("key");
            Rect keyRect = position;
            keyRect.width = position.width * .4f;
            keyRect.x = position.x + position.width * .6f;
            Rect clippedPosRect = position;
            clippedPosRect.width = position.width * .6f;
            EditorGUI.BeginProperty(clippedPosRect, label, property);
            if (!controlProperty.isExpanded) EditorGUI.PropertyField(clippedPosRect, controlProperty, label, true);
            else EditorGUI.PropertyField(position, controlProperty, label, true);
            if (ConfigNamePopupAttribute.LastConfigValid)
            {
                List<string> keyList = ConfigNamePopupAttribute.Container.GetConfigKeys(ConfigNamePopupAttribute.LastConfigName, GetControlType((ControlType)controlProperty.FindPropertyRelative("_controlType").enumValueIndex));
                int index = keyList.IndexOf(keyProperty.stringValue);
                int newIndex = EditorGUI.Popup(keyRect, index, keyList.ToArray());
                if (index != newIndex)
                {
                    keyProperty.stringValue = keyList[newIndex];
                    keyProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            else EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none, true);

            EditorGUI.EndProperty();
        }

        public static ControlKeyType GetControlType(ControlType type)
        {
            switch (type)
            {
                case ControlType.Transform:
                    return ControlKeyType.IMU;
                case ControlType.AnimationParam:
                    return ControlKeyType.Tension;
                default:
                    return ControlKeyType.Tension;
            }
        }
    }
}
