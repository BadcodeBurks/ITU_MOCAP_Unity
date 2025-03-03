using System;
using System.Collections.Generic;
using UnityEngine;
namespace Burk
{
    public class BufferRecording
    {
        List<double> _timeStamps = new List<double>();
        List<float[]> _bufferValues;

        public BufferRecording()
        {
            _bufferValues = new List<float[]>();
            _timeStamps = new List<double>();
        }
        public void AddRecordFrame(float[] bufferValues, double timeStamp)
        {
            _timeStamps.Add(timeStamp);
            _bufferValues.Add(bufferValues.Clone() as float[]);
        }

        public double GetDuration() => _timeStamps[_timeStamps.Count - 1] - _timeStamps[0];
        public int GetFrameCount() => _bufferValues.Count;

        public int GetClosestTimeIndex(double time)
        {
            if (time <= _timeStamps[0]) return 0;
            if (time >= _timeStamps[_timeStamps.Count - 1]) return _timeStamps.Count - 1;
            int index = _bufferValues.Count / 2;
            int lastIndex = index;
            int direction = -1;
            int step = _bufferValues.Count / 2;
            while (true)
            {
                int temp = index;
                if (_timeStamps[index] > time)
                {
                    if (direction == 1)
                    {
                        direction = -1;
                        step = step / 2 + 1;
                    }
                    index += step;
                }
                else if (_timeStamps[index] < time)
                {
                    if (direction == -1)
                    {
                        direction = 1;
                        step = step / 2 + 1;
                    }
                    index -= step;
                }

                if (lastIndex == index) return index;
                lastIndex = temp;
            }
        }

        public double GetNextClosestTime(int timeStampIndex)
        {
            if (timeStampIndex >= _timeStamps.Count - 1) return _timeStamps[_timeStamps.Count - 1];
            return _timeStamps[timeStampIndex + 1];
        }

        public float[] GetValues(int index) => _bufferValues[index];

    }
}