using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace XGameKit.Core
{
    public class XDebugEditorWindow : OdinEditorWindow
    {
        [MenuItem("XGameKit/XDebug/Setting")]
        public static void OpenWindow()
        {
            var window = GetWindow<XDebugEditorWindow>("XDebug Setting");
            window.minSize = new Vector2(300, 500);
            window.Show();
        }

        [BoxGroup("1", showLabel:false)]
        [LabelText("配置路径")]
        public string ConfigPath;
        [BoxGroup("1", showLabel: false)]
        [Button("修改路径", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        void ChangeConfigPath()
        {
            if (string.IsNullOrEmpty(ConfigPath))
            {
                EditorUtility.DisplayDialog("提醒", "不能设置为空路径", "OK");
                return;
            }
            var configPath = EditorPrefs.GetString(XDebugUtil.CONFIG_PATH_KEY, XDebugUtil.DEFAULT_CONFIG_PATH);
            if (configPath == ConfigPath)
                return;
            EditorPrefs.SetString(XDebugUtil.CONFIG_PATH_KEY, ConfigPath);
            _LoadConfig();
        }


        [TableList]
        public List<XDebugConfig.LoggerInfo> Datas = new List<XDebugConfig.LoggerInfo>();

        [Button("保存配置", ButtonSizes.Medium), GUIColor(0.0f, 1.0f, 0.0f)]
        void SaveConfig()
        {
            var assetPath = XDebugUtil.GetConfigAssetPath();
            var config = AssetDatabase.LoadAssetAtPath<XDebugConfig>(assetPath);
            if (config == null)
            {
                config = CreateInstance<XDebugConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }
            config.Datas.Clear();
            config.Datas.AddRange(Datas);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        protected override void Initialize()
        {
            base.Initialize();

            ConfigPath = EditorPrefs.GetString(XDebugUtil.CONFIG_PATH_KEY, XDebugUtil.DEFAULT_CONFIG_PATH);
            _LoadConfig();
        }

        protected void _LoadConfig()
        {
            var assetPath = XDebugUtil.GetConfigAssetPath();
            var config = AssetDatabase.LoadAssetAtPath<XDebugConfig>(assetPath);
            if (config == null)
                return;
            Datas.Clear();
            foreach (var data in config.Datas)
            {
                Datas.Add(data);
            }
        }
    }

}

