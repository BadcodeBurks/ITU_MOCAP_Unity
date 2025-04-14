using System;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class BufferRecorder
    {
        BufferContainer _buffer;
        bool _isBufferSelected;
        bool _isBufferInitialized => _isBufferSelected && _buffer.IsInitialized;
        bool _isBufferUpdated;
        public bool CanRecord => _isBufferSelected && _isBufferInitialized;

        #region Recording
        bool _isRecording = false;
        public bool IsRecording => _isRecording;

        private ControlSet _controlToBind;
        public bool IsControlSet => _controlToBind != null;
        private bool _wasBoundToActiveBuffer;
        BufferRecording currentRecording;
        private double recordStartTime;
        public double RecordTime => _isRecording ? EditorApplication.timeSinceStartup - recordStartTime : 0d;
        public Action<BufferRecording> OnRecorded;
        #endregion

        public void Init()
        {
            _isBufferSelected = false;
            _buffer = null;
        }

        public void SetBuffer(BufferContainer buffer)
        {
            if (_isBufferSelected) UnsetBuffer();
            _buffer = buffer;
            _isBufferSelected = true;
        }

        public void SetControl(ControlSet controlSet)
        {
            _controlToBind = controlSet;
        }

        public void UnsetBuffer()
        {
            if (!_isBufferSelected) return;
            if (_isRecording)
            {
                StopRecording();
            }
            _isBufferSelected = false;
            _buffer = null;
        }

        private void OnBufferWrite()
        {
            _isBufferUpdated = true;
        }

        public void UpdateRecord()
        {
            if (!_isRecording) return;
            if (!_isBufferUpdated) return;
            double timeStamp = EditorApplication.timeSinceStartup - recordStartTime;
            currentRecording.AddRecordFrame(_buffer.ReadFullBuffer(), timeStamp);
            _isBufferUpdated = false;
        }

        public void StartRecording()
        {
            if (!_isBufferInitialized) return;
            Debug.Log("StartRecording: " + _buffer.name);

            currentRecording = new BufferRecording();
            currentRecording.AddBufferData(_buffer.GetMetadata());
            BindToBuffer();
            _isRecording = true;
            _isBufferUpdated = true;
            recordStartTime = EditorApplication.timeSinceStartup;
            UpdateRecord();
            _buffer.OnBufferWrite += OnBufferWrite;
        }

        public void StopRecording()
        {
            _buffer.OnBufferWrite -= OnBufferWrite;
            _isBufferUpdated = true;
            UpdateRecord();
            _isRecording = false;
            UnbindFromBuffer();
            OnRecorded?.Invoke(currentRecording);
        }

        private void BindToBuffer()
        {
            if (_controlToBind.IsBound)
            {
                _wasBoundToActiveBuffer = _controlToBind.IsBound;
                _controlToBind.UnbindControls(true);
            }
            _controlToBind.BindControls(_buffer);
        }

        private void UnbindFromBuffer()
        {
            _controlToBind.UnbindControls(true);
            if (_wasBoundToActiveBuffer) _controlToBind.BindControls(ControlsManager.ActiveBuffer);
        }

        internal void OnFrameUpdate()
        {
            if (!_isRecording) return;
            UpdateRecord();
            _controlToBind.Update();
        }
    }
}