using System;
using UnityEngine;

namespace Burk
{
    public class FeatureVector
    {
        float[] _features;
        private int _dimension;
        public int Dimension => _dimension;
        private float c_length;
        private bool _lengthDirtyFlag;
        public float Length => _lengthDirtyFlag ? CalculateLength() : c_length;
        public FeatureVector(int featureCount)
        {
            _dimension = featureCount;
            _features = new float[featureCount];
            c_length = 0f;
            _lengthDirtyFlag = false;
        }

        public FeatureVector(float[] features) : this(features.Length)
        {
            Array.Copy(features, _features, features.Length);
            _lengthDirtyFlag = true;
        }

        public float[] GetFeatures()
        {
            float[] result = new float[_features.Length];
            Array.Copy(_features, result, _features.Length);
            return result;
        }

        public void SetFeature(int index, float value)
        {
            if (index < 0 || index >= _features.Length) return;
            _features[index] = value;
            _lengthDirtyFlag = true;
        }

        public void SetFeatureVectorClip(FeatureVector vector)
        {
            int minLength = Mathf.Min(Dimension, vector.Dimension);
            Array.Copy(vector._features, _features, minLength);
            _lengthDirtyFlag = true;
        }

        public void SetFeatureVectorZero(FeatureVector vector)
        {
            SetFeatureVectorClip(vector);
            if (vector.Dimension < Dimension)
            {
                for (int i = vector.Dimension; i < Dimension; i++)
                {
                    _features[i] = 0f;
                }
            }
        }

        public void SetFeatureVectorEqual(FeatureVector vector)
        {
            if (vector.Dimension != Dimension) return;
            SetFeatureVectorClip(vector);
        }

        public float CalculateLength()
        {
            c_length = 0f;
            for (int i = 0; i < _features.Length; i++)
            {
                c_length += _features[i] * _features[i];
            }
            c_length = Mathf.Sqrt(c_length);
            _lengthDirtyFlag = false;
            return c_length;
        }

        public override string ToString()
        {
            string result = "[";
            for (int i = 0; i < _features.Length; i++)
            {
                result += ", " + _features[i].ToString("0.000");
            }
            return result + "]";
        }

        public float this[int index]
        {
            get => _features[index];
            set => SetFeature(index, value);
        }

        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            if (a.Dimension != b.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(b));
            FeatureVector result = new FeatureVector(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                result[i] = a[i] - b[i];
            }
            return result;
        }

        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            if (a.Dimension != b.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(b));
            FeatureVector result = new FeatureVector(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                result[i] = a[i] + b[i];
            }
            return result;
        }

        public static FeatureVector operator *(FeatureVector a, float b)
        {
            FeatureVector result = new FeatureVector(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                result[i] = a[i] * b;
            }
            return result;
        }

        public static FeatureVector operator /(FeatureVector a, float b)
        {
            if (b == 0f) throw new DivideByZeroException("FeatureVector division by zero");
            FeatureVector result = new FeatureVector(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                result[i] = a[i] / b;
            }
            return result;
        }

        public static void AddTo(FeatureVector to, FeatureVector from) => AddTo(to, from, 1f);

        public static void AddTo(FeatureVector to, FeatureVector from, float weight)
        {
            if (to.Dimension != from.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(from));
            for (int i = 0; i < to.Dimension; i++)
            {
                to[i] += from[i] * weight;
            }
        }
    }

    public static class FeatureVectorExtensions
    {
        public static float Dot(this FeatureVector a, FeatureVector b)
        {
            if (a.Dimension != b.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(b));
            float result = 0f;
            for (int i = 0; i < a.Dimension; i++)
            {
                result += a[i] * b[i];
            }
            return result;
        }

        public static float Distance(this FeatureVector a, FeatureVector b)
        {
            return (a - b).CalculateLength();
        }

        public static float DistanceNonAlloc(this FeatureVector a, FeatureVector b)
        {
            if (a.Dimension != b.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(b));
            float result = 0f;
            for (int i = 0; i < a.Dimension; i++)
            {
                result += (a[i] - b[i]) * (a[i] - b[i]);
            }
            return Mathf.Sqrt(result);
        }

        public static void Combine(this FeatureVector a, FeatureVector b)
        {
            if (a.Dimension != b.Dimension) throw new ArgumentException("FeatureVector dimension mismatch", nameof(b));
            for (int i = 0; i < a.Dimension; i++)
            {
                a[i] += b[i];
            }
        }
    }
}