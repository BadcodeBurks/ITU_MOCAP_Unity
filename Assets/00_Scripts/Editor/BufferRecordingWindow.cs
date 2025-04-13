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

        string[] _bufferNames;
        string[] _controlSetNames;
        int _selectedBufferIndex = -1;
        int _selectedControlIndex = -1;

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            _bufferRecords = new List<BufferRecordWrapper>();
            _bufferNames = ControlsManager.GetBufferNames();

            _recorder = new BufferRecorder();
            _recorder.Init();
            _recorder.OnRecorded += OnNewRecordingCaptured;
            _player = new BufferRecordPlayer();
            _player.Init();
            _player.SetControl(ControlsManager.GetControlSetByNameOrder(0));
            _controlSetNames = ControlsManager.GetControlSetNames();
            ControlsManager.OnControlSetListChanged += OnControlSetListChanged;
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
            //TODO: Draw Settings:
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.BeginHorizontal();
                int temp = EditorGUILayout.Popup("Control Set", _selectedControlIndex, _controlSetNames);
                if (temp != _selectedControlIndex)
                {
                    _selectedControlIndex = temp;
                    _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                int temp2 = EditorGUILayout.Popup("Buffer", _selectedBufferIndex, _bufferNames);
                if (temp2 != _selectedBufferIndex)
                {
                    _selectedBufferIndex = temp2;
                    _recorder.SetBuffer(ControlsManager.GetBufferByIndex(_selectedBufferIndex));
                }
                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;
            GUILayout.Label("Recordings", EditorStyles.centeredGreyMiniLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (_player.IsPlaying || _recorder.IsRecording) GUI.enabled = false;
                foreach (string recordName in RecordsHandler.RecordNames)
                {
                    if (GUILayout.Button(recordName, EditorStyles.miniButton))
                    {
                        _player.SetRecord(RecordsHandler.GetRecording(recordName));
                    }
                }
                GUILayout.FlexibleSpace();
                Color guiTemp = GUI.color;
                GUI.color = Color.green;
                GUI.enabled = true;
                if (GUILayout.Button("Extract All As CSV"))
                {
                    RecordsHandler.ExtractAllRecords();
                }
                GUI.color = guiTemp;
            }

            //===== RECORDER =====

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(100f)))
            {
                GUILayout.Label("Record: " + _player.GetRecordName(), GUILayout.Height(20f));
                float temp = GUILayout.HorizontalSlider(_player.GetNormalizedPlayTime(), 0, 1);
                if (Mathf.Abs(temp - _player.GetNormalizedPlayTime()) > 0.01f) _player.SetNormalizedPlayTime(temp);
                else if (_player.SettingPlayTime) _player.StopSetPlayTime();
            }

            //TODO: Draw Recorder:
            //specify which buffer is being recorded (Active buffer name)
            //disable while playing

            //TODO: Add buffer metadata on recording
        }
    }

}