using System;
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

        private class BufferWrapper
        {
            public bool isInitializing = false;
            private BufferContainer container;
            public BufferContainer Container => container;
            public Action<BufferWrapper> OnBufferWrapperInitialized;
            public void SetBuffer(BufferContainer buffer)
            {
                isInitializing = true;
                container = buffer;
                container.OnBufferInitialized += OnBufferInitialized;
                container.Init();
            }

            private void OnBufferInitialized()
            {
                isInitializing = false;
                OnBufferWrapperInitialized?.Invoke(this);
            }
        }

        List<SerializedObject> _controls;

        List<ControlWrapper> _cachedEditors;
        private bool _isEditable;

        List<BufferWrapper> _buffers;

        void OnEnable()
        {
            Debug.Log("ControlsManagerEditorWindow OnEnable");
            ControlsManager.OnControlSetListChanged += GetControls;
            AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
            GetControls();
            GetBuffers();
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
                ResetBuffers();

            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                GetControls();
            }
        }
        void ResetBuffers()
        {
            if (_buffers == null) return;
            foreach (BufferWrapper buffer in _buffers)
            {
                if (buffer.Container.GetType() == typeof(PipeBufferContainer))
                {
                    (buffer.Container as PipeBufferContainer).StopClient();
                }
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
            EditorApplication.update -= OnEditorUpdate;
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

        void GetBuffers()
        {
            ResetBuffers();
            _buffers = new List<BufferWrapper>();
            string[] containerGUIDs = AssetDatabase.FindAssets("t:BufferContainer");
            for (int i = 0; i < containerGUIDs.Length; i++)
            {
                BufferWrapper wrapper = new BufferWrapper();
                _buffers.Add(wrapper);
                wrapper.OnBufferWrapperInitialized += OnBufferInitialized;
                wrapper.SetBuffer(AssetDatabase.LoadAssetAtPath<BufferContainer>(AssetDatabase.GUIDToAssetPath(containerGUIDs[i])));
            }
            Repaint();
        }

        void OnBufferInitialized(BufferWrapper buffer)
        {
            buffer.OnBufferWrapperInitialized -= OnBufferInitialized;
            Repaint();
        }

        void OnGUI()
        {
            _isEditable = !Application.isPlaying;
            GUI.enabled = _isEditable;
            if (_buffers == null) return;
            if (_cachedEditors == null) return;

            foreach (BufferWrapper buffer in _buffers)
            {
                DrawBuffer(buffer);
            }

            foreach (ControlWrapper control in _cachedEditors)
            {
                DrawControl(control);
            }
            GUI.enabled = true;
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh"))
            {
                UnbindAll();
                GetControls();
                GetBuffers();
            }
        }

        void DrawBuffer(BufferWrapper buffer)
        {
            bool isActiveBuffer = buffer.Container == ControlsManager.ActiveBuffer;
            using (new GUILayout.HorizontalScope())
            {
                Color bg = GUI.backgroundColor;
                GUI.backgroundColor = isActiveBuffer ? Color.green : Color.white;
                if (buffer.isInitializing)
                {
                    GUI.backgroundColor = Color.yellow;
                    GUI.enabled = false;
                }
                if (GUILayout.Button(buffer.Container.name, EditorStyles.miniButton))
                {
                    if (!isActiveBuffer) ControlsManager.SetActiveBuffer(buffer.Container);
                }
                GUI.backgroundColor = bg;
                GUI.enabled = true;
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
                        SceneView.lastActiveSceneView.sceneViewState.alwaysRefresh = true;

                    }
                    GUI.enabled = _isEditable;
                }
                GUI.backgroundColor = guiBGColor;
            }
        }

        private void OnEditorUpdate()
        {
            bool update = false;
            for (int i = 0; i < _cachedEditors.Count; i++)
            {
                ControlSet control = _cachedEditors[i].cachedEditor.target as ControlSet;
                if (control != null) control.Update();
            }
            if (!update) return;
            SceneView.RepaintAll();
        }
    }
}