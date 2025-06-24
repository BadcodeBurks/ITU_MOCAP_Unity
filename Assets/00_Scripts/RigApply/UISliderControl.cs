using System;
using UnityEngine;
using UnityEngine.UI;

namespace Burk
{
    public class UISliderControl : Control
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Gradient gradient;
        private Image _fillRect;
        public override SensorBinding CreateBinding(string readerKey)
        {
            _fillRect = slider.fillRect.GetComponent<Image>();
            return new TensionSliderBinding(readerKey, this);
        }

        public void Update(float value)
        {
            value = GetTemporalAverage(value);
            slider.value = value;
            _fillRect.color = gradient.Evaluate(value);
        }

        float movingAverage = 0f;
        private float GetTemporalAverage(float value)
        {
            movingAverage = Mathf.Lerp(movingAverage, value, 0.12f);
            return movingAverage;
        }
    }

    public class TensionSliderBinding : SensorBinding
    {
        UISliderControl _sliderControl;
        TensionSensorReader _reader;
        public TensionSliderBinding(string readerKey, UISliderControl sliderControl) : base(readerKey)
        {
            _sliderControl = sliderControl;
        }

        public override void Apply()
        {
            _sliderControl.Update(_reader.Read());
        }

        public override void Bind(BufferContainer buffer)
        {
            _reader = buffer.GetTensionReader(readerKey);
        }

        public override SensorType GetSensorType()
        {
            return SensorType.Tension;
        }

        public override void Unbind(bool reset = false)
        {
            _sliderControl.Update(0);
        }

        public override void Update()
        {

        }
    }

}