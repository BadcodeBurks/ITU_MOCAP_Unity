using System.Collections;
using System.IO.Pipes;
using System.Threading.Tasks;
using UnityEngine;

namespace Burk
{
    public static class PipeHandler
    {
        public static NamedPipeServerStream pipeServer;
        static bool _createFlag;
        static float[] _randomFloats;

        public static void CreateMockServer()
        {
            pipeServer = new NamedPipeServerStream("MockPipe", PipeDirection.Out, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Task.Run(StartServer);
        }

        public static async void StartServer()
        {
            Debug.Log("Server starting");
            await pipeServer.WaitForConnectionAsync();
            Debug.Log("Connected: " + pipeServer.IsConnected);
            while (true)
            {
                byte[] buffer = GetRandomFloatBuffer();
                await pipeServer.WriteAsync(buffer, 0, buffer.Length);
                Debug.Log("Written");
                _createFlag = true;
                await Task.Delay(100);
            }
        }

        private static byte[] GetRandomFloatBuffer()
        {
            byte[] buffer = new byte[_randomFloats.Length * sizeof(float)];
            System.Buffer.BlockCopy(_randomFloats, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static IEnumerator GenerateRandomFloats()
        {
            _randomFloats = new float[8];
            float t = Time.time;
            float dt;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!_createFlag) continue;
                _createFlag = false;
                dt = Time.time - t;
                t = Time.time;
                for (int i = 0; i < _randomFloats.Length; i++)
                {
                    _randomFloats[i] += Random.Range(-1f, 1f) * dt;
                    _randomFloats[i] = Mathf.Clamp01(_randomFloats[i]);
                }
            }
        }
    }
}
