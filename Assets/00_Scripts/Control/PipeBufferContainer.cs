using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
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
        [NonSerialized] private bool _pipeInitialized = false;
        [NonSerialized] private bool _waitingPipeConnection = false;
        public Action OnBufferFailedToInitialize;
        public void SetMono(MonoBehaviour mono)
        {
            _routineMono = mono;
        }

        public override void Init()
        {
            if (_pipeInitialized)
            {

                Debug.Log("Already initialized pipe");
                if (!_pipeInitialized && !_waitingPipeConnection) CreateClient();
                return;
            }
            _reader = new BufferReader(this);
            List<string> keys = new List<string> { "T", "I", "M", "R", "P", "B" };
            CreateBuffer(5, 1);
            BufferMetadata meta = new BufferMetadata()
            {
                keys = keys,
                tensionCount = 5,
                imuCount = 1,
                useRaw = false,
            };
            CreateReaders(meta);
#if UNITY_EDITOR
            if (!Application.isPlaying) CreateClient();
#endif
        }

        public void CreateClient()
        {
            if (_waitingPipeConnection)
            {
                Debug.Log("Already waiting for pipe connection");
                return;
            }
            _waitingPipeConnection = true;
            if (pipeClient != null)
            {
                if (pipeClient.IsConnected) pipeClient.Close();
                pipeClient.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;
            Debug.Log("Connecting: " + this.name);
            if (Application.isPlaying) _routineMono.StartCoroutine(WaitUntilTaskFinished(Task.Run(CreateClientAsync, token), CompleteInit));
#if UNITY_EDITOR
            else EditorCoroutineUtility.StartCoroutine(WaitUntilTaskFinished(Task.Run(CreateClientAsync, token), CompleteInit), this);
#endif
        }

        private IEnumerator WaitUntilTaskFinished(Task task, Action onComplete)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            onComplete?.Invoke();
        }

        private void CompleteInit()
        {
            _waitingPipeConnection = false;
            if (!pipeClient.IsConnected)
            {
                _isInitialized = false;
                _pipeInitialized = false;
                Debug.Log("Connection Failed: " + this.name);
                OnBufferFailedToInitialize?.Invoke();
                return;
            }
            _isInitialized = true;
            _pipeInitialized = true;
            cancellationTokenSource = null;
            if (Application.isPlaying) readRoutine = _routineMono.StartCoroutine(ReadFromPipe());
#if UNITY_EDITOR
            else editorReadRoutine = EditorCoroutineUtility.StartCoroutine(ReadFromPipe(), this);
#endif
            OnBufferInitialized?.Invoke();
        }

        private async Task CreateClientAsync()
        {
            pipeClient = new NamedPipeClientStream("localhost", pipeName, PipeDirection.In, PipeOptions.Asynchronous);
            await pipeClient.ConnectAsync(10000, token);
            Debug.Log("Connection Result: " + pipeClient.IsConnected);
        }

        private void OnApplicationQuit()
        {
            StopClient();
        }

        public void StopClient()
        {
            if (!_pipeInitialized)
            {
                if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
                return;
            }
            if (Application.isPlaying) _routineMono.StopCoroutine(readRoutine);
            if (pipeClient.IsConnected) pipeClient.Close();
            _pipeInitialized = false;
        }


        public override IEnumerator ReadFromPipe()
        {
#if UNITY_EDITOR
            float t = 0f;
#endif
            Debug.Log("Pipe Read Start: " + pipeClient.IsConnected);
            if (!pipeClient.IsConnected) yield break;
            while (true)
            {
                byte[] buffer = new byte[m_Buffer.Length * sizeof(float)];
                int bytesRead = 0;
                TaskAwaiter aw = Task.Run(() => { bytesRead = pipeClient.Read(buffer, 0, buffer.Length); }).GetAwaiter();
                while (!aw.IsCompleted)
                {
                    Debug.Log("Waiting");
                    if (pipeClient.NumberOfServerInstances == 0)
                    {
                        pipeClient.Close();
                        Debug.Log("Pipe Disconnected");
                        _pipeInitialized = false;
                        yield break;
                    }
                    yield return Wait();
                }
                if (bytesRead != buffer.Length)
                {
                    Debug.Log("Bytes Read: " + bytesRead);
                    continue;
                }
                Buffer.BlockCopy(buffer, 0, m_Buffer, 0, bytesRead);
                Debug.Log("Read: " + bytesRead);
                OnBufferWrite?.Invoke();
            }
            IEnumerator Wait()
            {
                if (Application.isPlaying) yield return new WaitForEndOfFrame();
#if UNITY_EDITOR

                else yield return new EditorWaitForSeconds(0.02f);
#endif
            }
        }
    }
}