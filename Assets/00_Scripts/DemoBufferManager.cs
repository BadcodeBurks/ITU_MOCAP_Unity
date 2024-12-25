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

        [SerializeField] bool usePipeBuffer = false;

        private void Start()
        {

            if (usePipeBuffer)
            {
                Debug.Log("Initializing pipe buffer");
                pipeBufferContainer.OnBufferInitialized += () =>
                {
                    StartCoroutine(pipeBufferContainer.ReadFromPipe());
                    controlSet.Init(pipeBufferContainer);
                };
                pipeBufferContainer.Init();
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
    }
}
