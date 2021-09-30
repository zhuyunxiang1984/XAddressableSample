using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace XGameKit.Core
{
    public class XLogger : ILogger
    {
        private Dictionary<string, ILogBuilder> _LogBuilders = new Dictionary<string, ILogBuilder>();
        private Dictionary<string, bool> _LogFlags = new Dictionary<string, bool>();
        private StringBuilder _StrBuilder = new StringBuilder(128);
        public void Reset()
        {
            _LogBuilders.Clear();
        }
        public void AddBuilder(string tag, ILogBuilder builder)
        {
            if (_LogBuilders.ContainsKey(tag))
            {
                _LogBuilders[tag] = builder;
            }
            else
            {
                _LogBuilders.Add(tag, builder);
            }
        }
        public void SetEnable(string tag, bool flag)
        {
            _LogFlags[tag] = flag;
        }

        public bool GetEnable(string tag)
        {
            return !_LogFlags.ContainsKey(tag) || _LogFlags[tag];
        }

        public void PrintLog(string tag, string message)
        {
            if (!GetEnable(tag))
                return;
            _StrBuilder.Clear();
            _StrBuilder.Append(message);

            var logBuilder = _GetOrCreateBuilder(tag);
            logBuilder.AppendTag(_StrBuilder);
            logBuilder.AppendCol(_StrBuilder);

            Debug.Log(_StrBuilder.ToString());
        }
        public void PrintError(string tag, string message)
        {
            _StrBuilder.Clear();
            _StrBuilder.Append(message);

            var logBuilder = _GetOrCreateBuilder(tag);
            logBuilder.AppendTag(_StrBuilder);
            logBuilder.AppendCol(_StrBuilder);

            Debug.LogError(_StrBuilder.ToString());
        }
        public void PrintWarning(string tag, string message)
        {
            if (!GetEnable(tag))
                return;
            _StrBuilder.Clear();
            _StrBuilder.Append(message);

            var logBuilder = _GetOrCreateBuilder(tag);
            logBuilder.AppendTag(_StrBuilder);
            logBuilder.AppendCol(_StrBuilder);

            Debug.LogWarning(_StrBuilder.ToString());
        }

        private ILogBuilder _GetOrCreateBuilder(string tag)
        {
            if (_LogBuilders.ContainsKey(tag))
                return _LogBuilders[tag];
            var logBuilder = new XLogBuilder();
            logBuilder.SetTag(tag);
            _LogBuilders.Add(tag, logBuilder);
            return logBuilder;
        }
    }

}
