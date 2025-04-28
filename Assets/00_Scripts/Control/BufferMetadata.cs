using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    public class BufferMetadata
    {
        public List<string> keys;
        public int tensionCount;
        public int imuCount;
        public bool useRaw;
        public List<Vector2> tensionCalibrations;

        public BufferMetadata Clone()
        {
            BufferMetadata clone = new BufferMetadata();
            clone.keys = new List<string>(keys);
            clone.tensionCount = tensionCount;
            clone.imuCount = imuCount;
            clone.useRaw = useRaw;
            clone.tensionCalibrations = new List<Vector2>(tensionCalibrations);
            Debug.Log("Clone: " + clone.tensionCalibrations.Count);
            return clone;
        }
    }
}