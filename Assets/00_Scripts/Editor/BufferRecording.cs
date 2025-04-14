using System.Collections.Generic;
using UnityEngine;
namespace Burk
{
    public class BufferRecording
    {
        private string _name = "no_name";
        public string Name => _name;

        private BufferMetadata _bufferData;
        public BufferMetadata BufferData => _bufferData;
        public bool HasBufferData => _bufferData != null;

        List<double> _timeStamps = new List<double>();
        List<float[]> _bufferValues;

        public BufferRecording()
        {
            _bufferValues = new List<float[]>();
            _timeStamps = new List<double>();
        }

        public BufferRecording(string name) : this() => _name = name;

        public void AddBufferData(BufferMetadata data) => _bufferData = data;
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
            int step = _timeStamps.Count / 2;
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
                    index -= step;
                }
                else if (_timeStamps[index] < time)
                {
                    if (direction == -1)
                    {
                        direction = 1;
                        step = step / 2 + 1;
                    }
                    index += step;
                }
                index = Mathf.Clamp(index, 0, _timeStamps.Count - 1);

                if (lastIndex == index)
                {
                    index = _timeStamps[index] < time ? index : index - 1;
                    return index - 1;
                }
                lastIndex = temp;
            }
        }

        public double GetNextClosestTime(int timeStampIndex)
        {
            if (timeStampIndex >= _timeStamps.Count - 1) return _timeStamps[_timeStamps.Count - 1];
            return _timeStamps[timeStampIndex + 1];
        }

        public double GetTimeStamp(int index) => _timeStamps[index];

        public float[] GetValues(int index) => _bufferValues[index];

    }
}