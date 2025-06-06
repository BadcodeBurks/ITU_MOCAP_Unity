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
        [SerializeField] private bool useMask;

        public void Init()
        {
            _featureExampleList = new List<float[]>();
        }

        public void AddExampleFrame(FeatureVector featureVector)
        {
            _featureExampleList.Add(featureVector.GetFeatures());
        }

        public void CalculateFeatureBaseAndToleranceVector()
        {
            featureBaseValues = new float[_featureExampleList[0].Length];
            toleranceVectorValues = new float[_featureExampleList[0].Length];
            for (int i = 0; i < _featureExampleList[0].Length; i++)
            {
                for (int j = 0; j < _featureExampleList.Count; j++)
                {
                    featureBaseValues[i] += _featureExampleList[j][i];
                    toleranceVectorValues[i] += _featureExampleList[j][i] * _featureExampleList[j][i];
                }
                featureBaseValues[i] /= _featureExampleList.Count;
                toleranceVectorValues[i] = Mathf.Sqrt(toleranceVectorValues[i] / _featureExampleList.Count - featureBaseValues[i] * featureBaseValues[i]);
            }
        }
    }
}
