using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    public class DemoBufferManager : MonoBehaviour
    {
        [SerializeField] SensorUIController sensorUIController;
        // [SerializeField] HandRigController rigController;
        [SerializeField] ControlSet controlSet;
        [SerializeField] SimulatedBufferContainer simulatedBufferContainer;
        [SerializeField] PipeBufferContainer pipeBufferContainer;
        [SerializeField] PipeBufferUIController pipeBufferUIController;
        [SerializeField] bool usePipeBuffer = false;
        [Header("Pipe Buffer Settings")][SerializeField] float calibrationDuration = 5f;

        private void Start()
        {

            if (usePipeBuffer)
            {
                pipeBufferUIController.OnCalibrate += CalibrateControls;
                Debug.Log("Initializing pipe buffer");
                pipeBufferContainer.SetMono(this);
                pipeBufferContainer.Init();
                StartPipeBuffer();
            }
            else
            {
                Debug.Log("Initializing simulated buffer");
                simulatedBufferContainer.OnBufferInitialized += () =>
                {
                    sensorUIController.Init(simulatedBufferContainer);
                    controlSet.Init(simulatedBufferContainer);
                };
                simulatedBufferContainer.Init();
            }
        }

        private void StartPipeBuffer()
        {
            pipeBufferContainer.OnBufferInitialized += () =>
            {
                controlSet.Init(pipeBufferContainer);
                controlSet.CalibrateControls(calibrationDuration);
            };
            pipeBufferContainer.CreateClient();
        }

        private void StopPipeBuffer()
        {
            pipeBufferContainer.StopClient();
        }

        public void ReconnectPipeBuffer()
        {
            StopPipeBuffer();
            controlSet.UnbindControls();
            StartPipeBuffer();
            controlSet.BindControls(pipeBufferContainer);
        }

        public void CalibrateControls(float calibrationDuration)
        {
            this.calibrationDuration = calibrationDuration;
            controlSet.CalibrateControls(calibrationDuration);
        }
    }
}
