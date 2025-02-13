using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    [CustomEditor(typeof(SimulatedBufferContainer))]
    public class SimulatedBufferCustomInspector : Editor
    {
        private class BufferWriterEditor
        {
            public enum WriterControlType
            {
                single,
                singleSlider,
                vector,
                vectorSlider
            }

            List<Vector2> sliderMinMax = new List<Vector2>();

            string _key;
            int _index;
            BufferContainer.BufferReader _reader;
            SimulatedBufferContainer.BufferWriter _writer;
            Action draw;

            private WriterControlType _controlType;

            public BufferWriterEditor(string key, int index, BufferContainer.BufferReader reader, SimulatedBufferContainer.BufferWriter writer, Type keyType)
            {
                _key = key;
                _index = index;
                _reader = reader;
                _writer = writer;
                sliderMinMax = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 360) };
                if (keyType == typeof(Vector3))
                {
                    _controlType = WriterControlType.vectorSlider;
                    draw = DrawVectorSlider;
                }
                else if (keyType == typeof(float))
                {
                    _controlType = WriterControlType.singleSlider;
                    draw = DrawSingleSlider;
                }
            }

            public void DrawGUI()
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(_key);
                    draw.Invoke();
                }
            }

            private void DrawSingle()
            {
                float singleValue = _reader.ReadFloat(_index);
                EditorGUI.BeginChangeCheck();
                singleValue = EditorGUILayout.FloatField(singleValue);
                if (EditorGUI.EndChangeCheck())
                {
                    _writer.WriteFloat(_index, singleValue);
                }
            }

            private void DrawSingleSlider()
            {
                float singleValue = _reader.ReadFloat(_index);
                using (new EditorGUILayout.HorizontalScope())
                {
                    Vector2 minMax = sliderMinMax[0];
                    EditorGUI.BeginChangeCheck();
                    minMax.x = EditorGUILayout.FloatField(minMax.x);
                    singleValue = EditorGUILayout.Slider(singleValue, minMax.x, minMax.y);
                    minMax.y = EditorGUILayout.FloatField(minMax.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        sliderMinMax[0] = minMax;
                        _writer.WriteFloat(_index, singleValue);
                    }
                }
            }

            private void DrawVector()
            {
                Vector3 vectorValue = _reader.ReadVector3(_index);
                EditorGUI.BeginChangeCheck();
                vectorValue = EditorGUILayout.Vector3Field("", vectorValue);
                if (EditorGUI.EndChangeCheck())
                {
                    _writer.WriteVector3(_index, vectorValue);
                }
            }

            private void DrawVectorSlider()
            {
                Vector3 vectorValue = _reader.ReadVector3(_index);
                using (new EditorGUILayout.HorizontalScope())
                {
                    Vector2 minMax = sliderMinMax[1];
                    EditorGUI.BeginChangeCheck();
                    minMax.x = EditorGUILayout.FloatField(minMax.x);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        vectorValue.x = EditorGUILayout.Slider(vectorValue.x, minMax.x, minMax.y);
                        vectorValue.y = EditorGUILayout.Slider(vectorValue.y, minMax.x, minMax.y);
                        vectorValue.z = EditorGUILayout.Slider(vectorValue.z, minMax.x, minMax.y);
                    }
                    minMax.y = EditorGUILayout.FloatField(minMax.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        sliderMinMax[1] = minMax;
                        _writer.WriteVector3(_index, vectorValue);
                    }
                }
            }


        }
        SimulatedBufferContainer _container;
        List<BufferWriterEditor> _editors = new List<BufferWriterEditor>();
        // Called when the object is loaded.
        void OnEnable()
        {
            _container = (SimulatedBufferContainer)target;
            if (!_container.IsInitialized) _container.Init();
            GetControls();
        }

        void GetControls()
        {
            List<string> keys = _container.GetAllKeys();
            _editors = new List<BufferWriterEditor>();
            foreach (string key in keys)
            {
                Type keyType = _container.GetKeyType(key);
                BufferContainer.BufferReader reader = _container.GetBufferReader();
                SimulatedBufferContainer.BufferWriter writer = _container.GetBufferWriter();
                int index = _container.GetKeyIndex(key);
                BufferWriterEditor editor = new BufferWriterEditor(key, index, reader, writer, keyType);
                _editors.Add(editor);
            }
        }

        // Implement this function to make a custom inspector.
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            for (int i = 0; i < _editors.Count; i++)
            {
                _editors[i].DrawGUI();
            }

            if (_container.IsInitialized)
            {
                if (GUILayout.Button("SetActiveBuffer"))
                {
                    ControlsManager.SetActiveBuffer(_container);
                }
            }
        }
    }

}