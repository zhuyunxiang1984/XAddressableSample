using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using XGameKit.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class XDebug
{
    public class MutiText
    {
        public StringBuilder StrBuilder { get; protected set; } = new StringBuilder(128);

        public void Reset()
        {
            StrBuilder.Clear();
        }
        public void AppendLine(string message)
        {
            StrBuilder.Append(message);
            StrBuilder.Append('\n');
        }
        public new string ToString()
        {
            return StrBuilder.ToString();
        }
    }

    private static XLogger _Logger;
    private static Stack<MutiText> _MutiTextPool = new Stack<MutiText>();
    

    //初始化
    static XDebug()
    {
        _Logger = new XLogger();

#if UNITY_EDITOR
        var assetPath = XDebugUtil.GetConfigAssetPath();
        var config = AssetDatabase.LoadAssetAtPath<XDebugConfig>(assetPath);
        if (config == null)
            return;
        config.CheckRepeatName();

        foreach (var data in config.Datas)
        {
            if (string.IsNullOrEmpty(data.name))
                continue;
            var logBuilder = new XLogBuilder();
            logBuilder.SetTag(data.name);
            logBuilder.SetCol(XDebugUtil.ColorToHex(data.color));
            _Logger.AddBuilder(data.name, logBuilder);
        }
#endif

    }


    public static bool GetEnable(string tag)
    {
        return _Logger.GetEnable(tag);
    }



#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void Log(string message)
    {
        _Logger.PrintLog(string.Empty, message);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void Log(Action<MutiText> callback)
    {
        if (callback == null)
            return;
        var mutiText = _GetMutiText();
        mutiText.Reset();
        callback.Invoke(mutiText);
        _Logger.PrintLog(string.Empty, mutiText.ToString());
        _RecycleMutiText(mutiText);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void LogError(string message)
    {
        _Logger.PrintError(string.Empty, message);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void LogWarning(string message)
    {
        _Logger.PrintWarning(string.Empty, message);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void Log(string tag, string message)
    {
        _Logger.PrintLog(tag, message);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void Log(string tag, Action<MutiText> callback)
    {
        if (callback == null)
            return;
        var mutiText = _GetMutiText();
        mutiText.Reset();
        callback.Invoke(mutiText);
        _Logger.PrintLog(tag, mutiText.ToString());
        _RecycleMutiText(mutiText);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void LogError(string tag, string message)
    {
        _Logger.PrintError(tag, message);
    }

#if !UNITY_EDITOR
        [Conditional(XDebugUtil.CONDITIONAL_NAME)]
#endif
    public static void LogWarning(string tag, string message)
    {
        _Logger.PrintWarning(tag, message);
    }


    private static MutiText _GetMutiText()
    {
        if (_MutiTextPool.Count > 1)
        {
            return _MutiTextPool.Pop();
        }
        return new MutiText();
    }
    private static void _RecycleMutiText(MutiText mutiText)
    {
        _MutiTextPool.Push(mutiText);
    }
}