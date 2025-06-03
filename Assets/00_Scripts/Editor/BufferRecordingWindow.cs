using System;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList.Internal;
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
        int _selectedBufferIndex = 0;
        int _selectedControlIndex = 0;

        void OnEnable()
        {
            Initialize();
            EditorApplication.update += OnUpdate;
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
            _player.SetRecord(record.recording);
        }
        private string saveName = "";
        private bool _isNameValid = false;
        void OnGUI()
        {
            if (!ControlsManager.IsInitialized) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.BeginHorizontal();
                int temp = EditorGUILayout.Popup("Control Set", _selectedControlIndex, _controlSetNames);
                if (temp != _selectedControlIndex || _selectedControlIndex == -1)
                {
                    _selectedControlIndex = temp;
                    _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                int temp2 = EditorGUILayout.Popup("Buffer", _selectedBufferIndex, _bufferNames);
                if (temp2 != _selectedBufferIndex || _selectedBufferIndex == -1)
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
                        if (_player.IsRecordSet) _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                    }
                }
                Color ct = GUI.color;
                bool removeFlag = false;
                BufferRecordWrapper cacheRecord = null;
                bool saveFlag = false;
                foreach (BufferRecordWrapper record in _bufferRecords)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(record.name, EditorStyles.miniButton))
                    {
                        _player.SetRecord(record.recording);
                        if (_player.IsRecordSet) _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                    }

                    GUI.color = Color.red;
                    if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50f)))
                    {
                        removeFlag = true;
                        cacheRecord = record;
                    }
                    GUI.color = Color.green;
                    bool guiCache = GUI.enabled;
                    GUI.enabled = guiCache && _isNameValid;
                    if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(50f)))
                    {
                        saveFlag = true;
                        cacheRecord = record;
                    }
                    GUI.enabled = guiCache;
                    EditorGUILayout.EndHorizontal();
                }
                if (saveFlag)
                {
                    RecordsHandler.SaveRecordingAsCSV(cacheRecord.recording, saveName);
                }
                if (removeFlag)
                {
                    _bufferRecords.Remove(cacheRecord);
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

            //===== PLAYER =====

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(100f)))
            {
                GUILayout.Label("Record: " + _player.GetRecordName(), GUILayout.Height(20f));
                GUI.enabled = _player.IsRecordSet;
                float normPlayTime = _player.GetNormalizedPlayTime();
                double dur = _player.IsRecordSet ? _player.GetRecording().GetDuration() : 0;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label((dur * normPlayTime).ToString("00.00"), GUILayout.Width(40f));
                float temp = GUILayout.HorizontalSlider(normPlayTime, 0, 1, GUILayout.Height(20f));
                GUILayout.Label(dur.ToString("00.00"), GUILayout.Width(40f));
                EditorGUILayout.EndHorizontal();

                if (Mathf.Abs(temp - normPlayTime) > 0.01f)
                {
                    if (!_player.IsControlSet) _player.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                    if (_player.IsPlaying) _player.StopPlaying();
                    _player.SetNormalizedPlayTime(temp);
                }

                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(20f)))
                {
                    GUI.enabled = _player.IsRecordSet;
                    if (_player.IsPlaying)
                    {
                        if (GUILayout.Button("Stop", GUILayout.Width(50f))) _player.StopPlaying();
                    }
                    else
                    {
                        if (GUILayout.Button("Play", GUILayout.Width(50f))) _player.StartPlaying();
                    }
                }

                GUI.enabled = true;

                // ===== RECORDER =====

                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(20f)))
                {
                    if (_recorder.IsRecording)
                    {
                        GUI.color = Color.red;
                        if (GUILayout.Button("Stop Recording", GUILayout.Width(100f))) _recorder.StopRecording();
                    }
                    else
                    {
                        GUI.enabled = _recorder.CanRecord;
                        Color ct = GUI.color;
                        GUI.color = Color.green;
                        if (GUILayout.Button("Start Recording", GUILayout.Width(100f)))
                        {
                            if (!_recorder.IsControlSet) _recorder.SetControl(ControlsManager.GetControlSetByNameOrder(_selectedControlIndex));
                            _recorder.StartRecording();
                        }
                        GUI.color = ct;
                    }
                }
                GUI.enabled = true;
                using (new GUILayout.HorizontalScope(GUILayout.Height(20f)))
                {
                    Color ct = GUI.color;
                    GUI.color = _isNameValid ? Color.green : Color.red;
                    saveName = GUILayout.TextField(saveName).SanitizeFilename();
                    _isNameValid = !string.IsNullOrEmpty(saveName) && !RecordsHandler.CheckFileExists(saveName);
                    GUI.color = ct;
                }
            }
        }

        void OnUpdate()
        {
            _recorder?.OnFrameUpdate();
            _player?.OnFrameUpdate();
            SceneView.RepaintAll();
            Repaint();
        }
    }

}