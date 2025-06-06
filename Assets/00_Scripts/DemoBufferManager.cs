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
        [SerializeField] bool useSimulated;

        private void Start()
        {
            if (!Application.isPlaying) return;
            pipeBufferContainer.SetMono(this);
            pipeBufferContainer.Init();
            simulatedBufferContainer.Init();
            controlSet.Init(null);
            if (!useSimulated)
            {
                pipeBufferUIController.SetReconnectButtonActive(true);
                pipeBufferUIController.OnReconnect += ReconnectPipeBuffer;
            }
            else
            {
                controlSet.BindControls(simulatedBufferContainer);
            }
        }

        private void StartPipeBuffer()
        {
            if (!Application.isPlaying) return;
            pipeBufferContainer.OnBufferInitialized += () =>
            {
                controlSet.Init(pipeBufferContainer);
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
            //Do something to reset calibration
        }
    }
}
