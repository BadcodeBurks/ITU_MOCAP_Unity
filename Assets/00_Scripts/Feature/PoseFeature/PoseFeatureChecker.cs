using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burk
{
    /// <summary>
    /// PoseFeatureChecker
    /// </summary>
    public class PoseFeatureChecker : Singleton<PoseFeatureChecker>
    {
        [SerializeField]
        List<FeatureChecker> featureCheckers;
        [SerializeField] ControlSet controlSet;
        Dictionary<string, FeatureChecker> _featureCheckerLookup;
        private bool _checking = false;
        private bool _isInitialized = false;
        void Start()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _featureCheckerLookup = new Dictionary<string, FeatureChecker>();
            foreach (var checker in featureCheckers)
            {
                if (_featureCheckerLookup.ContainsKey(checker.Container.PoseName)) continue;
                _featureCheckerLookup.Add(checker.Container.PoseName, checker);
                checker.Init();
            }
        }

        public FeatureChecker GetChecker(string poseName)
        {
            if (_featureCheckerLookup.ContainsKey(poseName)) return _featureCheckerLookup[poseName];
            return null;
        }

        public FeatureChecker AddFeatureChecker(FeatureChecker checker)
        {
            if (!_isInitialized) Start();
            if (_featureCheckerLookup.ContainsKey(checker.Container.PoseName)) return GetChecker(checker.Container.PoseName);
            _featureCheckerLookup.Add(checker.Container.PoseName, checker);
            featureCheckers.Add(checker);
            checker.Init();
            return checker;
        }
        void Update()
        {
            if (!_checking)
            {
                if (!controlSet.IsBound) return;
                _checking = true;
            }
            foreach (var checker in featureCheckers)
            {
                if (!checker.IsInitialized) continue;
                checker.CheckMatch(controlSet.ParamFeatureExtractor.FeatureVector);
            }
        }
    }

    [Serializable]
    public class FeatureChecker
    {
        [SerializeField] PoseDataFeatureContainer container;
        public PoseDataFeatureContainer Container => container;
        private bool _isMatching = false;
        public bool IsMatching => _isMatching;
        private bool _isActive = false;

        FeatureVector _differenceVector;
        bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        public Action OnMatched;
        public Action OnUnmatched;
        public void Init()
        {
            _isInitialized = false;
            if (!container.IsValid) return;
            Debug.Log(container.name + " Init");
            _isMatching = false;
            _isActive = false;
            container.Init();
            _differenceVector = new FeatureVector(container.FeatureBase.Dimension);
            _isInitialized = true;
        }

        public void CheckMatch(FeatureVector vector)
        {
            _differenceVector.SetFeatureVectorEqual(vector);
            FeatureVector.AddTo(_differenceVector, container.FeatureBase, -1f);
            bool match = true;
            if (_differenceVector.Length > container.ToleranceVector.Length)
            {
                match = false;
            }
            else
            {
                for (int i = 0; i < _differenceVector.Dimension; i++)
                {
                    if (Mathf.Abs(_differenceVector[i]) > container.ToleranceVector[i])
                    {
                        match = false;
                        break;
                    }
                }
            }
            //Debug.Log(container.name + " Match: " + match + " " + _differenceVector.ToString());
            if (match == _isMatching) return;
            _isMatching = match;
            if (_isMatching && !_isActive)
            {
                //TODO: add event
                // Debug.Log(container.name + " Matched");
                _isActive = true;
                OnMatched?.Invoke();
            }
            else if (!_isMatching && _isActive)
            {
                //TODO: add event
                // Debug.Log(container.name + " Unmatched");
                _isActive = false;
                OnUnmatched?.Invoke();
            }
        }



    }
}
