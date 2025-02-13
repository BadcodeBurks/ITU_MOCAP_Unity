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

        private class ControlWrapper
        {
            public bool isExpanded;
            public Vector2 scrollPosition;
            public Editor cachedEditor;
        }

        List<SerializedObject> _controls;

        List<ControlWrapper> _cachedEditors;
        private bool _isEditable;


        void OnEnable()
        {
            Debug.Log("ControlsManagerEditorWindow OnEnable");
            ControlsManager.OnControlSetListChanged += GetControls;
            AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            GetControls();
        }

        void OnAssemblyReload()
        {
            UnbindAll();
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                UnbindAll();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                GetControls();
            }
        }

        void UnbindAll()
        {
            foreach (ControlWrapper control in _cachedEditors)
            {
                ControlSet c = control.cachedEditor.target as ControlSet;
                if (c != null && c.IsBound) c.UnbindControls(true);
            }
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
            ControlsManager.OnControlSetListChanged -= GetControls;
        }

        void GetControls()
        {
            _controls = ControlsManager.GetControlSetProperties();
            _cachedEditors = new List<ControlWrapper>();
            for (int i = 0; i < _controls.Count; i++)
            {
                _cachedEditors.Add(
                    new ControlWrapper
                    {
                        scrollPosition = Vector2.zero,
                        cachedEditor = Editor.CreateEditor(_controls[i].targetObject)
                    });
            }
            Repaint();
        }

        void OnGUI()
        {
            _isEditable = !Application.isPlaying;
            GUI.enabled = _isEditable;
            if (_cachedEditors == null) return;
            foreach (ControlWrapper control in _cachedEditors)
            {
                DrawControl(control);
            }

            GUI.enabled = true;
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh"))
            {
                GetControls();
            }
        }

        void DrawControl(ControlWrapper control)
        {
            using (new GUILayout.VerticalScope())
            {
                DrawTitle(control);
                if (control.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    using (var scope = new EditorGUILayout.ScrollViewScope(control.scrollPosition, GUILayout.Width(position.width - 10)))
                    {
                        control.scrollPosition = scope.scrollPosition;
                        control.cachedEditor.OnInspectorGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void DrawTitle(ControlWrapper control)
        {
            ControlSet controlSet = control.cachedEditor.target as ControlSet;
            using (var titleScope = new GUILayout.HorizontalScope())
            {
                control.isExpanded = EditorGUILayout.Foldout(control.isExpanded, controlSet.Name);
                EditorGUILayout.Space();
                Color guiBGColor = GUI.backgroundColor;
                if (controlSet.IsBound)
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Unbind"))
                    {
                        controlSet.UnbindControls(true);
                    }
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                    GUI.enabled = ControlsManager.ActiveBuffer != null;
                    if (GUILayout.Button("Bind"))
                    {
                        controlSet.BindControls(ControlsManager.ActiveBuffer);
                        SceneView.lastActiveSceneView.sceneViewState.SetAllEnabled(true);
                    }
                    GUI.enabled = _isEditable;
                }
                GUI.backgroundColor = guiBGColor;
            }
        }
    }
}