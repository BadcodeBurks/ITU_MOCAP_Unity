using System.Collections;
using UnityEngine;

namespace Burk
{
    public class UIShowHideController : MonoBehaviour
    {
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private AnimationCurve moveCurve;
        [SerializeField] private float moveDuration = 0.3f;

        private bool _isVisible = false;
        private Vector3 startPosition;
        private Vector3 hidePosition;

        private void Start()
        {
            startPosition = panelTransform.position;
            hidePosition = panelTransform.position + panelTransform.right * panelTransform.rect.width;
            panelTransform.position = hidePosition;
        }

        public void TogglePanel()
        {
            if (_isVisible)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
            _isVisible = !_isVisible;
        }

        public void ShowPanel()
        {
            StartCoroutine(SlideRoutine(moveDuration, panelTransform.position, startPosition));
        }

        public void HidePanel()
        {
            StartCoroutine(SlideRoutine(moveDuration, panelTransform.position, hidePosition));
        }

        private IEnumerator SlideRoutine(float d, Vector3 start, Vector3 end)
        {
            float t = 0f;
            while (t < d)
            {
                t += Time.deltaTime;
                float tD = t / d;
                panelTransform.position = Vector3.Lerp(start, end, moveCurve.Evaluate(tD));
                yield return null;
            }
            panelTransform.position = end;

        }

    }
}
