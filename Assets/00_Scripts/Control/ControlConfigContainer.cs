using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burk
{
    public enum ControlKeyType
    {
        Tension,
        IMU
    }

    public enum ConfigType
    {
        Hand,
        Head,
        Body,
        Leg,
    }

    [CreateAssetMenu(fileName = "ControlConfigContainer", menuName = "Burk/ControlConfigContainer")]
    public class ControlConfigContainer : ScriptableObject
    {
        private Dictionary<string, ControlConfig> _configs;
        [SerializeField] ControlConfig[] configs;

        public void Init()
        {
            _configs = new Dictionary<string, ControlConfig>();
            for (int i = 0; i < configs.Length; i++)
            {
                if (_configs.ContainsKey(configs[i].configName))
                {
                    Debug.LogWarning("Duplicate control config: " + configs[i].configName);
                    continue;
                }
                configs[i].Init();
                _configs.Add(configs[i].configName, configs[i]);
            }
        }

        public List<string> GetConfigKeys(string configName, ControlKeyType keyType)
        {
            if (!_configs.ContainsKey(configName)) return null;
            return _configs[configName].GetKeys(keyType);
        }

        public List<string> GetConfigNames()
        {
            return _configs.Keys.ToList();
        }

        public List<string> GetConfigNames(ConfigType configType)
        {
            return _configs.Where(x => x.Value.configType == configType).Select(x => x.Key).ToList();
        }

        internal bool CheckConfigExists(string value)
        {
            return _configs.ContainsKey(value);
        }
        private void OnValidate()
        {
            Init();
        }
    }

    [Serializable]
    public class ControlConfig
    {
        public string configName;
        public ConfigType configType;
        private Dictionary<ControlKeyType, int> _keyCounts;
        private Dictionary<string, ControlKeyType> _keyTypes;
        [SerializeField] KeyConfig[] keys;

        public void Init()
        {
            _keyCounts = new Dictionary<ControlKeyType, int>();
            _keyTypes = new Dictionary<string, ControlKeyType>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (_keyTypes.ContainsKey(keys[i].key))
                {
                    Debug.LogWarning("Duplicate control key: " + keys[i].key + " in config: " + configName);
                    continue;
                }
                if (!_keyCounts.ContainsKey(keys[i].keyType)) _keyCounts.Add(keys[i].keyType, 0);
                _keyCounts[keys[i].keyType] += 1;
                _keyTypes.Add(keys[i].key, keys[i].keyType);
            }
        }

        public List<string> GetKeys(ControlKeyType keyType)
        {
            return keys.Where(x => x.keyType == keyType).Select(x => x.key).ToList();
        }
    }

    [Serializable]
    public struct KeyConfig
    {
        public string key;
        public ControlKeyType keyType;
    }
}