using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace XGameKit.Core
{
    public static class XDebugEditor
    {
        //[MenuItem("XGameKit/XDebug/Setting")]
        //static void XDebugSetting()
        //{
        //    string assetPath = XDebug.CONFIG_PATH;
        //    var config = AssetDatabase.LoadAssetAtPath<XDebugConfig>(assetPath);
        //    if (config == null)
        //    {
        //        config = ScriptableObject.CreateInstance<XDebugConfig>();
        //        var dir = Path.GetDirectoryName(assetPath);
        //        if (!Directory.Exists(dir))
        //        {
        //            Directory.CreateDirectory(dir);
        //        }
        //        AssetDatabase.CreateAsset(config, assetPath);
        //    }
        //    Selection.activeObject = config;
        //}

        
        static List<string> _IngoreScriptNames = new List<string>() { "XDebug.cs", "XDebugLogger.cs", "XDebugMutiLogger.cs" };

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            //Debug.Log("OnOpenAsset " + instanceId + " " + line);
            var stackTrace = GetStackTrace();
            if (string.IsNullOrEmpty(stackTrace))
                return false;
            //Debug.Log(stackTrace);

            //正则表达式查找匹配
            //eg:XDataSampleEntrance:Update() (at Assets/XToolsSample/XDataSample/XDataSampleEntrance.cs:29)
            Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            var matchText = string.Empty;

            while (matches.Success)
            {
                matchText = matches.Groups[1].Value;

                //解析脚本路径和行号
                int index = matchText.LastIndexOf(":");
                var temp1 = matchText.Substring(0, index);
                //无视我们自己封装的脚本
                if (!_IngoreScriptNames.Contains(Path.GetFileName(temp1)))
                {
                    var temp2 = int.Parse(matchText.Substring(index + 1));

                    var filePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + temp1;
                    filePath = filePath.Replace('/', '\\');
                    if (InternalEditorUtility.OpenFileAtLineExternal(filePath, temp2))
                    {
                        //打开成功处理完成
                        return true;
                    }
                }
                matches = matches.NextMatch();
            }
            //如果没有成功打开，那么交给unity处理
            return false;
        }

        //获取控制台输出的文本
        static string GetStackTrace()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInst = fieldInfo.GetValue(null);
            if (consoleWindowInst == null)
                return string.Empty;
            if ((object)EditorWindow.focusedWindow != consoleWindowInst)
                return string.Empty;
            var listViewStateType = assembly.GetType("UnityEditor.ListViewState");
            fieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
            var listView = fieldInfo.GetValue(consoleWindowInst);

            fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            return fieldInfo.GetValue(consoleWindowInst).ToString();
        }
        
    }
    
}