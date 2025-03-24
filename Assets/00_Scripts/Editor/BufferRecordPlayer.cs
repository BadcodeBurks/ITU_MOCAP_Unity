using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class BufferRecordPlayer
    {
        BufferContainer _buffer;
        public BufferContainer Buffer => _buffer;
        BufferRecording _bufferRecording;
        bool _isRecordSet;

        ControlSet _controlToBind;
        bool _wasBoundToActiveBuffer;
        public bool CanPlay => _isRecordSet && _buffer.IsInitialized && _controlToBind != null && !IsPlaying;

        #region Playing
        bool _isPlaying = false;
        public bool IsPlaying => _isPlaying;
        private double recordPlayTime;
        public Action OnPlayFinished;
        private int _lastPlayedIndex;
        private double _nextClosestTime;
        #endregion

        public void Init()
        {
            _isRecordSet = false;
        }

        public void SetRecord(BufferRecording r)
        {
            if (_isRecordSet) UnsetRecord();
            if (r.GetDuration() <= .02d) return;
            _buffer = BufferContainer.Create(ControlsManager.ActiveBuffer.GetMetadata()); //TODO: use r.GetMetadata();
            _buffer.name = "BufferRecordPlayer";
            _bufferRecording = r;
            _isRecordSet = true;
        }

        public void UnsetRecord()
        {
            if (!_isRecordSet) return;
            _bufferRecording = null;
            _isRecordSet = false;
        }

        public void OnUpdate()
        {
            if (!_isRecordSet) return;
            if (!_isPlaying) return;

            recordPlayTime += EditorTime.DeltaTime;
            if (recordPlayTime >= _nextClosestTime)
            {
                _lastPlayedIndex++;
                _buffer.WriteFullBuffer(_bufferRecording.GetValues(_lastPlayedIndex));
                _nextClosestTime = _bufferRecording.GetNextClosestTime(_lastPlayedIndex);
                if (_nextClosestTime <= recordPlayTime)
                {
                    StopPlaying();
                }
            }
        }

        public void StartPlaying()
        {
            if (IsPlaying) return;
            if (_controlToBind.IsBound)
            {
                _wasBoundToActiveBuffer = _controlToBind.IsBound;
                _controlToBind.UnbindControls(true);
            }
            _controlToBind.BindControls(_buffer);
            _lastPlayedIndex = 0;
            _isPlaying = true;
            recordPlayTime = 0;
            _buffer.WriteFullBuffer(_bufferRecording.GetValues(_lastPlayedIndex));
            _nextClosestTime = _bufferRecording.GetNextClosestTime(0);
        }

        public void StopPlaying()
        {
            _isPlaying = false;
            recordPlayTime = 0d;
            _controlToBind.UnbindControls(true);
            if (_wasBoundToActiveBuffer) _controlToBind.BindControls(ControlsManager.ActiveBuffer);
            OnPlayFinished?.Invoke();
        }

        public double StretchSlider(float sliderValue)
        {
            return _bufferRecording.GetDuration() * sliderValue;
        }

        internal BufferRecording GetRecording()
        {
            if (!_isRecordSet) return null;
            return _bufferRecording;
        }

        internal void SetControl(ControlSet controlSet)
        {
            if (_isPlaying) return;
            _controlToBind = controlSet;
        }
    }
}