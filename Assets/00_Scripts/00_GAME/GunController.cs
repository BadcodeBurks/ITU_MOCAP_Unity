using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    public class GunInputController : MonoBehaviour
    {
        [SerializeField] FeatureChecker holdChecker;
        [SerializeField] FeatureChecker triggerChecker;
        [SerializeField] MeshRenderer gunRenderer;

        private bool _gunEnabled = false;
        private bool _canShoot = false;
        private Coroutine _countDownRoutine;
        [SerializeField]
        [Range(0f, 1f)]
        float gunEnableDuration = .2f;
        [SerializeField]
        [Range(0f, 1f)]
        float gunDisableDuration = .2f;

        private bool _isInitialized = false;

        private void Start()
        {
            _isInitialized = false;
            InitFeatures();
        }

        public void OnDestroy()
        {
            DeInitFeatures();
        }

        public void InitFeatures()
        {
            if (_isInitialized) return;
            holdChecker = PoseFeatureChecker.Instance.AddFeatureChecker(holdChecker);
            triggerChecker = PoseFeatureChecker.Instance.AddFeatureChecker(triggerChecker);
            holdChecker.OnMatched += OnGunHold;
            holdChecker.OnUnmatched += OnGunRelease;
            triggerChecker.OnMatched += OnTriggerPress;
            triggerChecker.OnUnmatched += OnTriggerRelease;
            gunRenderer.enabled = false;
            _isInitialized = true;
        }

        public void DeInitFeatures()
        {
            if (!_isInitialized) return;
            holdChecker.OnMatched -= OnGunHold;
            holdChecker.OnUnmatched -= OnGunRelease;
            triggerChecker.OnMatched -= OnTriggerPress;
            triggerChecker.OnUnmatched -= OnTriggerRelease;
            _isInitialized = false;
        }

        public void OnTriggerPress()
        {
            if (!_gunEnabled) return;
            if (!_canShoot) return;
            Debug.Log("Shoot");
            _canShoot = false;
        }

        public void OnTriggerRelease()
        {
            if (!_gunEnabled) return;
            Debug.Log("Trigger Release");
            _canShoot = true;
        }

        public void OnGunHold()
        {
            if (_gunEnabled)
            {
                if (_countDownRoutine != null) StopCoroutine(_countDownRoutine);
                return;
            }
            _countDownRoutine = StartCoroutine(GunEnableChangeRoutine(gunEnableDuration, true));
        }

        public void OnGunRelease()
        {
            if (!_gunEnabled)
            {
                if (_countDownRoutine != null) StopCoroutine(_countDownRoutine);
                return;
            }
            _countDownRoutine = StartCoroutine(GunEnableChangeRoutine(gunDisableDuration, false));
        }

        public void SetGunEnabled(bool value)
        {
            Debug.Log(value ? "Gun Hold" : "Gun Release");
            _gunEnabled = value;
            _canShoot = value && !triggerChecker.IsMatching;
            gunRenderer.enabled = value;
        }

        private IEnumerator GunEnableChangeRoutine(float duration, bool value)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            SetGunEnabled(value);
        }
    }
}
