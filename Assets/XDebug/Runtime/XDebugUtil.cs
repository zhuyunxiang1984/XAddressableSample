using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XGameKit.Core
{
    public static class XDebugUtil
    {
        //预编译宏
        public const string CONDITIONAL_NAME = "XDEBUG";
        //配置路径键值名
        public const string CONFIG_PATH_KEY = "XDebug_Config_Path";
        //默认配置路径
        public const string DEFAULT_CONFIG_PATH = "Assets/XGameKitSetting/";
        //默认Tag名
        public const string DEFAULT_TAG = "Default";

        //颜色值转换16进制
        public static string ColorToHex(Color color)
        {
            return string.Format("{0}{1}{2}{3}",
                ((int)(color.r * 255)).ToString("X2"),
                ((int)(color.g * 255)).ToString("X2"),
                ((int)(color.b * 255)).ToString("X2"),
                ((int)(color.a * 255)).ToString("X2"));
        }

#if UNITY_EDITOR
        //获取配置的资源路径
        public static string GetConfigAssetPath()
        {
            var path = EditorPrefs.GetString(XDebugUtil.CONFIG_PATH_KEY, XDebugUtil.DEFAULT_CONFIG_PATH);
            if(!path.EndsWith("/"))
            {
                path += "/";
            }
            return $"{path}XDebugConfig.asset";
        }
#endif
    }

}