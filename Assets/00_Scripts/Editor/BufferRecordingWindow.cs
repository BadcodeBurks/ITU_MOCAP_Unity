using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class BufferRecordingWindow : EditorWindow
    {
        const string WINDOW_TITLE = "Buffer Recording Window";
        const string MENU_ITEM = "Window/" + WINDOW_TITLE;

        [MenuItem(MENU_ITEM)]
        public static BufferRecordingWindow Open()
        {
            var window = GetWindow<BufferRecordingWindow>(false, WINDOW_TITLE, true);
            window.Show();
            return window;
        }

        private class BufferRecordWrapper
        {
            public BufferRecordWrapper(BufferRecording recording, string name)
            {
                this.recording = recording;
                this.name = name;
                duration = recording.GetDuration();
            }
            public readonly BufferRecording recording;
            public string name;
            public readonly double duration;
        }

        List<BufferRecordWrapper> _bufferRecords;

        BufferRecorder _recorder;
        BufferRecordPlayer _player;

        string[] _controlSetNames;
        int _selectedControlIndex = 0;

        void OnEnable()
        {
            _bufferRecords = new List<BufferRecordWrapper>();
            _recorder = new BufferRecorder();
            _recorder.Init();
            _recorder.OnRecorded += OnNewRecordingCaptured;
            _player = new BufferRecordPlayer();
            _player.Init();
            _controlSetNames = ControlsManager.GetControlSetNames();
        }

        void OnControlSetListChanged()
        {
            _controlSetNames = ControlsManager.GetControlSetNames();
        }

        void OnNewRecordingCaptured(BufferRecording recording)
        {
            Debug.Log("OnNewRecordingCaptured: " + recording.GetDuration() + " " + recording.GetFrameCount());
            BufferRecordWrapper record = new BufferRecordWrapper(recording, "Record_" + _bufferRecords.Count);
            _bufferRecords.Add(record);
        }

        void OnGUI()
        {
            GUI.enabled = ControlsManager.ActiveBuffer != null;
            //TODO: Draw Settings:
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                int temp = EditorGUILayout.Popup("Control Set", _selectedControlIndex, _controlSetNames);
                if (temp != _selectedControlIndex)
                {
                    _selectedControlIndex = temp;
                    _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                }
            }

            //TODO: Draw Record List
            //if there are no recordings draw accordingly
            //if there are recordings:
            // Draw name, duration and play button
            //disable while recording

            //TODO: Draw Recorder:
            //specify which buffer is being recorded (Active buffer name)
            //disable while playing

            //TODO: Add buffer metadata on recording
        }
    }

}