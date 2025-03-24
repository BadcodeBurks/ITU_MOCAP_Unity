using System;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class BufferRecorder
    {
        BufferContainer _buffer;
        bool _isBufferSelected;
        bool _isBufferInitialized;
        bool _isBufferUpdated;
        public bool CanRecord => _isBufferSelected && _isBufferInitialized;

        #region Recording
        bool _isRecording = false;
        public bool IsRecording => _isRecording;
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
            _isBufferInitialized = _buffer.IsInitialized;
            if (!_isBufferInitialized) _buffer.OnBufferInitialized += OnBufferInitialized;
            else _buffer.OnBufferWrite += OnBufferWrite;
        }

        public void UnsetBuffer()
        {
            if (!_isBufferSelected) return;
            if (!_isBufferInitialized) _buffer.OnBufferInitialized -= OnBufferInitialized;
            if (_isRecording)
            {
                StopRecording();
            }
            _isBufferSelected = false;
            _buffer = null;
            _isBufferInitialized = false;
        }

        private void OnBufferInitialized()
        {
            _buffer.OnBufferInitialized -= OnBufferInitialized;
            _isBufferInitialized = true;
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
            Debug.Log("StartRecording: " + _buffer.name);

            currentRecording = new BufferRecording();
            currentRecording.AddBufferData(_buffer.GetMetadata());
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
            Debug.Log("StopRecording: " + _buffer.name);
            OnRecorded?.Invoke(currentRecording);
        }
    }
}