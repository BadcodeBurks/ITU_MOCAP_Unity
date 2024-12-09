using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burk
{
    public class ConfigNamePopupAttribute : PropertyAttribute
    {
        private static ControlConfigContainer _container;
        public static ControlConfigContainer Container { get { return _container; } }

        public static List<string> ConfigNames
        {
            get
            {
                if (_container == null)
                {
                    _container = Resources.Load<ControlConfigContainer>("ControlConfigContainer");
                    _container.Init();

                }

                return _container.GetConfigNames();
            }
        }
        public static string LastConfigName
        {
            get { return lastConfigName; }
            set
            {
                if (_container.CheckConfigExists(value))
                {
                    _lastConfigValid = true;
                    lastConfigName = value;
                }
                else
                {
                    _lastConfigValid = false;
                    lastConfigName = "";
                }
            }
        }
        public static bool LastConfigValid { get { return _lastConfigValid; } }
        private static bool _lastConfigValid = false;
        private static string lastConfigName = "";
    }
}