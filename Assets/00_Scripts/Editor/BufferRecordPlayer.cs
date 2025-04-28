using System;

using UnityEngine;

namespace Burk
{
    public class BufferRecordPlayer
    {
        BufferContainer _buffer;
        public BufferContainer Buffer => _buffer;
        BufferRecording _bufferRecording;
        bool _isRecordSet;
        public bool IsRecordSet => _isRecordSet;

        ControlSet _controlToBind;
        public bool IsControlSet => _controlToBind != null;
        bool _wasBoundToActiveBuffer;
        public bool CanPlay => _isRecordSet && _buffer.IsInitialized && _controlToBind != null && !IsPlaying;

        #region Playing
        bool _isPlaying = false;
        public bool IsPlaying => _isPlaying;
        private double recordPlayTime;
        public Action OnPlayFinished;
        private int _lastPlayedIndex;
        bool _settingPlayTime;
        public bool SettingPlayTime => _settingPlayTime;
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
            _buffer = BufferContainer.Create(r.BufferData); //TODO: use r.GetMetadata();
            _buffer.name = "BufferRecordPlayer";
            _buffer.Init();
            _bufferRecording = r;
            recordPlayTime = 0;
            _lastPlayedIndex = -1;
            _isRecordSet = true;
        }

        public string GetRecordName()
        {
            if (!_isRecordSet) return "No record is set";
            if (_bufferRecording.Name == null || _bufferRecording.Name == "") return "Latest Captured";
            return _bufferRecording.Name;
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

            if (_settingPlayTime) return;

            recordPlayTime += EditorTime.DeltaTime;
            if (recordPlayTime >= _nextClosestTime)
            {
                _lastPlayedIndex++;
                _buffer.WriteFullBuffer(_bufferRecording.GetValues(_lastPlayedIndex));
                _nextClosestTime = _bufferRecording.GetNextClosestTime(_lastPlayedIndex);
                if (recordPlayTime > _bufferRecording.GetDuration())
                {
                    StopPlaying();
                }
            }
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

        public void StartPlaying()
        {
            if (IsPlaying) return;
            BindToBuffer();
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
            UnbindFromBuffer();
            OnPlayFinished?.Invoke();
        }

        public float GetNormalizedPlayTime()
        {
            if (_bufferRecording == null) return 0;
            if (_bufferRecording.GetDuration() == 0) return 0;
            float t = (float)(recordPlayTime / _bufferRecording.GetDuration());
            return t;
        }

        public void StartSetPlayTime()
        {
            if (_settingPlayTime) return;
            _settingPlayTime = true;
            Debug.Log("StartSetPlayTime");
            if (!_isPlaying) BindToBuffer();
        }

        public void SetPlayTime(float nT)
        {
            recordPlayTime = _bufferRecording.GetDuration() * nT;
            _lastPlayedIndex = _bufferRecording.GetClosestTimeIndex(recordPlayTime);
            if (!_isPlaying) _buffer.WriteFullBuffer(_bufferRecording.GetValues(_lastPlayedIndex));
        }

        public void StopSetPlayTime()
        {
            if (!_settingPlayTime) return;
            Debug.Log("StopSetPlayTime");
            _settingPlayTime = false;
            if (!_isPlaying) UnbindFromBuffer();
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
            if (controlSet == null) return;
            Debug.Log("SetControl " + controlSet.Name);
            _controlToBind = controlSet;
        }

        internal void SetNormalizedPlayTime(float temp)
        {
            if (!_isRecordSet) return;
            if (!_settingPlayTime) StartSetPlayTime();
            SetPlayTime(temp);
        }

        internal void OnFrameUpdate()
        {
            if (!_isRecordSet) return;
            if (!_isPlaying) return;
            OnUpdate();
            _controlToBind.Update();
        }
    }
}