using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class ControlsManagerEditorWindow : EditorWindow
    {
        const string WINDOW_TITLE = "Controls Manager";
        const string MENU_ITEM = "Window/" + WINDOW_TITLE;
        // Opens this editor window.
        [MenuItem(MENU_ITEM)]
        public static ControlsManagerEditorWindow Open()
        {
            var window = GetWindow<ControlsManagerEditorWindow>(false, WINDOW_TITLE, true);
            window.Show();
            return window;
        }


        List<SerializedObject> _controls;

        List<Editor> _cachedEditors;

        // Called when this window is open.
        void OnEnable()
        {
            _controls = ControlsManager.GetControlSetProperties();
            _cachedEditors = new List<Editor>();
            for (int i = 0; i < _controls.Count; i++)
            {
                _cachedEditors.Add(Editor.CreateEditor(_controls[i].targetObject));
            }
        }


        // Implement this function to make a custom window.
        void OnGUI()
        {
            if (_cachedEditors == null) return;
            foreach (Editor editor in _cachedEditors)
            {
                editor.OnInspectorGUI();
            }
        }
    }

}