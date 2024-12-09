#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// SensorUIController
    /// </summary>
    public class SensorUIController : MonoBehaviour
    {
        [SerializeField] IMUSliderController imuSliderPrefab;
        [SerializeField] TensionSliderController tensionSliderPrefab;

        [SerializeField] RectTransform panelParent;

        public void Init(SimulatedBufferContainer buffer)
        {
            string[] tensionKeys = buffer.GetTensionSensorKeys();
            string[] imuKeys = buffer.GetIMUKeys();
            for (int i = 0; i < tensionKeys.Length; i++)
            {
                TensionSensorWriter writer = buffer.GetTensionWriter(tensionKeys[i]);
                TensionSliderController slider = Instantiate(tensionSliderPrefab, panelParent);
                slider.Init(tensionKeys[i], writer);
            }
            for (int i = 0; i < imuKeys.Length; i++)
            {
                IMUWriter writer = buffer.GetIMUWriter(imuKeys[i]);
                IMUSliderController slider = Instantiate(imuSliderPrefab, panelParent);
                slider.Init(imuKeys[i], writer);
            }
        }
        public void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
