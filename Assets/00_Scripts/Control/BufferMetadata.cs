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
    }
}