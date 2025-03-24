using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Burk
{
    public class PipeBufferUIController : MonoBehaviour
    {
        public Action<float> OnCalibrate;
        [SerializeField] Slider durationSlider;
        [SerializeField] TMP_Text durationText;
        [SerializeField] Button calibrateButton;
        [SerializeField] AnimationCurve calibrateDurationSliderCurve;
        private float _duration = 1f;

        private void Start()
        {
            _duration = 3f;
            durationSlider.value = 0.713f;
        }

        public void OnCalibrateButtonPressed()
        {
            OnCalibrate?.Invoke(_duration);
            StartCoroutine(CalibrateButtonDisableRoutine());
        }

        public void OnDurationSliderChanged()
        {
            _duration = Mathf.Clamp(calibrateDurationSliderCurve.Evaluate(durationSlider.value), 1f, 10f);
            durationText.text = _duration.ToString("F1") + "s";
        }

        public void OnReconnectPressed()
        {
            StartCoroutine(CalibrateButtonDisableRoutine());
        }

        private IEnumerator CalibrateButtonDisableRoutine()
        {
            calibrateButton.interactable = false;
            yield return new WaitForSeconds(_duration);
            calibrateButton.interactable = true;
        }
    }
}
