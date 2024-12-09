using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Burk
{
    public class IMUSliderController : MonoBehaviour
    {
        [SerializeField] Slider xSlider;
        [SerializeField] Slider ySlider;
        [SerializeField] Slider zSlider;
        [SerializeField] TMP_Text header;

        IMUWriter _writer;
        Vector3 _vectorValue;
        public void Init(string headerText, IMUWriter writer)
        {
            _writer = writer;
            header.text = headerText;
            xSlider.onValueChanged.AddListener(OnXValueChanged);
            ySlider.onValueChanged.AddListener(OnYValueChanged);
            zSlider.onValueChanged.AddListener(OnZValueChanged);
        }

        void OnXValueChanged(float value)
        {
            _vectorValue.x = value;
            UpdateValue();
        }

        void OnYValueChanged(float value)
        {
            _vectorValue.y = value;
            UpdateValue();
        }

        void OnZValueChanged(float value)
        {
            _vectorValue.z = value;
            UpdateValue();
        }

        void UpdateValue()
        {
            _writer.Write(_vectorValue);
        }
    }
}
