using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    [CustomPropertyDrawer(typeof(ConfigNamePopupAttribute))]
    public class ConfigNamePopupDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);
            string current = property.stringValue;
            List<string> configNames = ConfigNamePopupAttribute.ConfigNames;
            int currentIndex = configNames.IndexOf(current);
            if (currentIndex == -1) currentIndex = 0;
            {
                property.stringValue = configNames[currentIndex];
                property.serializedObject.ApplyModifiedProperties();
            }
            int newIndex = EditorGUI.Popup(position, currentIndex, configNames.ToArray());
            if (currentIndex != newIndex)
            {
                property.stringValue = configNames[newIndex];
                property.serializedObject.ApplyModifiedProperties();
            }
            ConfigNamePopupAttribute.LastConfigName = property.stringValue;
            EditorGUI.EndProperty();
        }
    }
}
