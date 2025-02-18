using System;
using System.Collections.Generic;
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

        [InitializeOnLoadMethod]
        private static void Init()
        {
            _controlSets = new Dictionary<int, ControlSet>();
            ControlSet.OnControlSetValidated += OnControlSetValidated;
            EditorApplication.hierarchyChanged += OnHierarchyChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
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

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ControlSet[] controlSets = GameObject.FindObjectsOfType<ControlSet>();
                _controlSets = new Dictionary<int, ControlSet>();
                foreach (ControlSet controlSet in controlSets)
                {
                    if (controlSet.IsPrefabDefinition()) continue;
                    AddControlSet(controlSet);
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
            Debug.Log("Added control set: " + controlSet.Name);
            OnControlSetListChanged?.Invoke();
        }
        private static void RemoveControlSet(int instanceID)
        {
            _controlSets.Remove(instanceID);
            Debug.Log("Removed control set: " + instanceID);
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

        public static void SetActiveBuffer(BufferContainer buffer)
        {
            _activeBuffer = buffer;
        }
    }
}