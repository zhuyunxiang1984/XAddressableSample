#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace XGameKit.Core
{
    public class XDebugConfig : ScriptableObject
    {
        [Serializable]
        public class LoggerInfo
        {
            [TableColumnWidth(50, false)]
            public bool active = true;
            public string name;
            [TableColumnWidth(100, false)]
            public Color color = Color.white;
        }
        [TableList]
        public List<LoggerInfo> Datas = new List<LoggerInfo>();

        //检测重复命名
        public void CheckRepeatName()
        {
            var flags = new Dictionary<string, bool>();
            foreach (var info in Datas)
            {
                if (string.IsNullOrEmpty(info.name))
                    continue;
                if (flags.ContainsKey(info.name))
                {
                    Debug.LogError($"重复命名Logger {info.name}");
                    continue;
                }
                flags.Add(info.name,true);
            }
        }
    }

}


#endif

