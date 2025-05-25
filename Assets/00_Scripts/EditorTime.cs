using UnityEngine;
using UnityEditor;

namespace Burk
{
#if UNITY_EDITOR
    public static class EditorTime
    {

        private static double _startTime;
        public static float StartTime => (float)_startTime;

        public static void SetStartTime() => _startTime = EditorApplication.timeSinceStartup;
        private static float _deltaTime;
        public static float DeltaTime => _deltaTime;

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update += Update;
            }
        }

        static void Update()
        {
            SetEditorDeltaTime();
        }

        private static void SetEditorDeltaTime()
        {
            if (_startTime == 0f)
            {
                _startTime = EditorApplication.timeSinceStartup;
            }
            _deltaTime = (float)(EditorApplication.timeSinceStartup - _startTime);
            _startTime = EditorApplication.timeSinceStartup;
        }

    }
#endif
}