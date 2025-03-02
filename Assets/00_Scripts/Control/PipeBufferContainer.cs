using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace Burk
{
    [CreateAssetMenu(fileName = "PipeBufferContainer", menuName = "Burk/PipeBufferContainer")]
    public class PipeBufferContainer : BufferContainer
    {
        [SerializeField] private string pipeName;
        NamedPipeClientStream pipeClient;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken token;
        Coroutine readRoutine;
#if UNITY_EDITOR
        EditorCoroutine editorReadRoutine;
#endif
        MonoBehaviour _routineMono;

        public void SetMono(MonoBehaviour mono)
        {
            _routineMono = mono;
        }

        public override void Init()
        {
            _reader = new BufferReader(this);
            List<string> keys = new List<string> { "T", "I", "M", "R", "P", "B" };
            CreateBuffer(5, 1);
            CreateReaders(keys, 5, 1);
        }

        public void CreateClient()
        {
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;
            Task.Run(CreateClientAsync, token).GetAwaiter().OnCompleted(CompleteInit);
        }

        private void CompleteInit()
        {
            _isInitialized = true;
            cancellationTokenSource = null;
            if (Application.isPlaying) readRoutine = _routineMono.StartCoroutine(ReadFromPipe());
#if UNITY_EDITOR
            else editorReadRoutine = EditorCoroutineUtility.StartCoroutine(ReadFromPipe(), this);
#endif
            OnBufferInitialized?.Invoke();
        }

        private async void CreateClientAsync()
        {
            pipeClient = new NamedPipeClientStream("localhost", pipeName, PipeDirection.In, PipeOptions.Asynchronous);
            await pipeClient.ConnectAsync(token);
        }

        private void OnApplicationQuit()
        {
            StopClient();
        }

        public void StopClient()
        {
            if (!_isInitialized)
            {
                if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
                return;
            }
            _routineMono.StopCoroutine(readRoutine);
            if (pipeClient.IsConnected) pipeClient.Close();
        }


        public override IEnumerator ReadFromPipe()
        {
            while (!pipeClient.IsConnected)
            {

                Debug.Log("connecting");
                yield return null;
            }
            Debug.Log("Pipe connected");
            while (true)
            {
                byte[] buffer = new byte[m_Buffer.Length * sizeof(float)];
                //Debug.Log("Reading from pipe");
                int bytesRead = 0;
                TaskAwaiter aw = Task.Run(() => { bytesRead = pipeClient.Read(buffer, 0, buffer.Length); }).GetAwaiter();
                while (!aw.IsCompleted) yield return null;
                //Debug.Log("Read " + bytesRead);
                yield return new WaitForEndOfFrame();
                if (bytesRead != buffer.Length) continue;
                Buffer.BlockCopy(buffer, 0, m_Buffer, 0, bytesRead);
            }
        }
    }
}