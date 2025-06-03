using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public static class ControlsManager
    {
        private static Dictionary<int, ControlSet> _controlSets = new Dictionary<int, ControlSet>();

        public static Action OnControlSetListChanged;

        public static BufferContainer _activeBuffer;
        public static BufferContainer ActiveBuffer => _activeBuffer;

        private static BufferContainer[] _buffers;

        public static BufferContainer[] Buffers => _buffers;
        static bool _isInitialized = false;
        public static bool IsInitialized => _isInitialized;
        [InitializeOnLoadMethod]
        private static void Init()
        {
            _controlSets = new Dictionary<int, ControlSet>();
            ControlSet.OnControlSetValidated += OnControlSetValidated;
            EditorApplication.hierarchyChanged += OnHierarchyChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _isInitialized = true;

        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (KeyValuePair<int, ControlSet> controlSet in _controlSets)
                {
                    if (controlSet.Value != null)
                        controlSet.Value.UnbindControls(true);
                }
            }

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                foreach (KeyValuePair<int, ControlSet> controlSet in _controlSets)
                {
                    if (controlSet.Value != null)
                        controlSet.Value.UnbindControls(true);
                }
                foreach (BufferContainer buffer in _buffers)
                {
                    if (buffer.GetType() == typeof(PipeBufferContainer))
                    {
                        (buffer as PipeBufferContainer).StopClient();
                    }
                }
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ControlsManager.Init();
                ControlSet[] controlSets = GameObject.FindObjectsOfType<ControlSet>();
                _controlSets = new Dictionary<int, ControlSet>();
                foreach (ControlSet controlSet in controlSets)
                {
                    if (controlSet.IsPrefabDefinition()) continue;
                    AddControlSet(controlSet);
                }
                foreach (BufferContainer buffer in _buffers)
                {
                    if (buffer.GetType() == typeof(PipeBufferContainer))
                    {
                        (buffer as PipeBufferContainer).Init();
                    }
                }
            }
        }

        private static void OnControlSetValidated(ControlSet controlSet)
        {
            int instanceID = controlSet.GetInstanceID();

            if (_controlSets.ContainsKey(instanceID)) return;
            if (controlSet.IsPrefabDefinition()) return;

            AddControlSet(controlSet);
        }

        private static void OnHierarchyChange()
        {
            ValidateControlSets();
        }

        private static void ValidateControlSets()
        {
            int[] instanceIDs = new int[_controlSets.Count];
            _controlSets.Keys.CopyTo(instanceIDs, 0);
            for (int i = 0; i < instanceIDs.Length; i++)
            {
                if (EditorUtility.InstanceIDToObject(instanceIDs[i]) == null)
                {
                    RemoveControlSet(instanceIDs[i]);
                }
            }
        }
        private static void AddControlSet(ControlSet controlSet)
        {
            int instanceID = controlSet.GetInstanceID();
            _controlSets.Add(instanceID, controlSet);
            controlSet.Init(null);
            OnControlSetListChanged?.Invoke();
        }
        private static void RemoveControlSet(int instanceID)
        {
            _controlSets.Remove(instanceID);
            OnControlSetListChanged?.Invoke();
        }

        public static List<SerializedObject> GetControlSetProperties()
        {
            List<SerializedObject> properties = new List<SerializedObject>();
            foreach (KeyValuePair<int, ControlSet> controlSet in _controlSets)
            {
                if (controlSet.Value == null) continue;
                properties.Add(new SerializedObject(controlSet.Value));
            }
            return properties;
        }

        public static BufferContainer[] GetBuffers()
        {
            string[] containerGUIDs = AssetDatabase.FindAssets("t:BufferContainer");
            BufferContainer[] containers = new BufferContainer[containerGUIDs.Length];
            for (int i = 0; i < containerGUIDs.Length; i++)
            {
                containers[i] = AssetDatabase.LoadAssetAtPath<BufferContainer>(AssetDatabase.GUIDToAssetPath(containerGUIDs[i]));
                containers[i].Init();
            }
            _buffers = containers;
            return containers;
        }

        public static BufferContainer GetBufferByIndex(int index)
        {
            if (_buffers.Length <= index) return null;
            return _buffers[index];
        }

        public static string[] GetControlSetNames()
        {
            string[] names = new string[_controlSets.Count];
            int i = 0;
            foreach (KeyValuePair<int, ControlSet> controlSet in _controlSets)
            {
                names[i] = controlSet.Value.Name;
                i++;
            }
            return names;
        }

        public static ControlSet GetControlSetByNameOrder(int nameOrder)
        {
            if (_controlSets == null) Init();
            if (_controlSets.Count <= nameOrder) return null;
            return _controlSets.Values.ToList()[nameOrder];
        }

        public static void SetActiveBuffer(BufferContainer buffer)
        {
            _activeBuffer = buffer;
        }

        internal static string[] GetBufferNames()
        {
            if (_buffers == null) GetBuffers();
            string[] names = new string[_buffers.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = _buffers[i].name;
            }
            return names;

        }
    }
}