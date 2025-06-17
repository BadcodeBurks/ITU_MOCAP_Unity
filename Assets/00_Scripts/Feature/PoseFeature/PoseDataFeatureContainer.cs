using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    [CreateAssetMenu(fileName = "NewPoseDataFeatureContainer", menuName = "Burk/PoseDataFeatureContainer")]
    public class PoseDataFeatureContainer : ScriptableObject
    {
        [SerializeField] private string poseName;
        private FeatureVector _featureBase;
        private FeatureVector _toleranceVector;
        private FeatureVector _maskVector;

        public string PoseName => poseName;
        public FeatureVector FeatureBase => _featureBase;
        public FeatureVector ToleranceVector => _toleranceVector;
        public FeatureVector MaskVector => _maskVector;

        private List<float[]> _featureExampleList;

        [SerializeField] private float[] featureBaseValues;
        [SerializeField] private float[] toleranceVectorValues;
        [SerializeField] private float[] maskVectorValues;
        [SerializeField, HideInInspector] bool useMask;
        [SerializeField, HideInInspector] bool isValid;
        public bool IsValid => isValid;

        void OnValidate()
        {
            if (featureBaseValues.Length > 0 && toleranceVectorValues.Length > 0)
            {
                isValid = true;
                if (featureBaseValues.Length != toleranceVectorValues.Length)
                {
                    isValid = false;
                    Debug.LogError("FeatureBase and ToleranceVector must have the same length!");
                }
                useMask = maskVectorValues.Length == featureBaseValues.Length;
            }
        }

        public void Init()
        {
            OnValidate();
            _featureExampleList = new List<float[]>();
            if (isValid)
            {
                _featureBase = new FeatureVector(featureBaseValues);
                _toleranceVector = new FeatureVector(toleranceVectorValues);
                if (useMask)
                {
                    _maskVector = new FeatureVector(maskVectorValues);
                }
            }
        }

        public void AddExampleFrame(FeatureVector featureVector)
        {
            Debug.Log(featureVector.Dimension);
            _featureExampleList.Add(featureVector.GetFeatures());
        }

        public void CalculateFeatureBaseAndToleranceVector()
        {
            featureBaseValues = new float[_featureExampleList[0].Length];
            toleranceVectorValues = new float[_featureExampleList[0].Length];
            Debug.Log(_featureExampleList[0].Length);
            float[] largestValues = new float[_featureExampleList[0].Length];
            float[] smallestValues = new float[_featureExampleList[0].Length];
            for (int i = 0; i < _featureExampleList[0].Length; i++)
            {
                for (int j = 0; j < _featureExampleList.Count; j++)
                {
                    if (j == 0)
                    {
                        largestValues[i] = _featureExampleList[j][i];
                        smallestValues[i] = _featureExampleList[j][i];
                    }
                    featureBaseValues[i] += _featureExampleList[j][i];
                    toleranceVectorValues[i] += _featureExampleList[j][i] * _featureExampleList[j][i];
                    if (_featureExampleList[j][i] > largestValues[i])
                    {
                        largestValues[i] = _featureExampleList[j][i];
                    }
                    else if (_featureExampleList[j][i] < smallestValues[i])
                    {
                        smallestValues[i] = _featureExampleList[j][i];
                    }
                }
                featureBaseValues[i] /= _featureExampleList.Count;
                toleranceVectorValues[i] = Mathf.Lerp(largestValues[i] - smallestValues[i], Mathf.Sqrt(toleranceVectorValues[i] / _featureExampleList.Count - featureBaseValues[i] * featureBaseValues[i]), 0.15f);
            }
        }
    }
}
