using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    public class PoseFeatureRecorder : MonoBehaviour
    {
        [SerializeField] PoseDataFeatureContainer containerToRecordAt;
        [SerializeField] ControlSet controlSetToRecord;

        private bool _isRecording = false;

        public void Start()
        {
            _isRecording = false;
            containerToRecordAt.Init();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _isRecording = !_isRecording;
                if (!_isRecording)
                {
                    containerToRecordAt.CalculateFeatureBaseAndToleranceVector();
                }
            }
            if (!_isRecording) return;
            containerToRecordAt.AddExampleFrame(controlSetToRecord.ParamFeatureExtractor.FeatureVector);
        }
    }
}
