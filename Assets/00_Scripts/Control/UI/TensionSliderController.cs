using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Burk
{
    public class TensionSliderController : MonoBehaviour
    {
        [SerializeField] Slider slider;
        [SerializeField] TMP_Text header;

        TensionSensorWriter _writer;

        public void Init(string headerText, TensionSensorWriter writer)
        {
            _writer = writer;
            header.text = headerText;
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        void OnValueChanged(float value)
        {
            slider.value = value;
            _writer.Write(value);
        }
    }
}
