using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    public class ParameterFeatureExtractor
    {
        List<ParameterControl> _controls;
        FeatureVector _featureVector;
        public ParameterFeatureExtractor(List<ParameterControl> controls)
        {
            _controls = controls;
            _featureVector = new FeatureVector(controls.Count);
        }

        public void Update()
        {
            for (int i = 0; i < _controls.Count; i++)
            {
                _featureVector[i] = _controls[i].Value;
            }

            Debug.Log(_featureVector.ToString());
        }
    }
}
