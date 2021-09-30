using System.Text;

namespace XGameKit.Core
{
    /*
     * ILogBuilder
     * XLogBuilder
     * 
     * ILogger
     * XLogger->XLogerBuilder
     * 
     * XDebug->XLogger
     * */

    public interface ILogBuilder
    {
        void AppendTag(StringBuilder sb);
        void AppendCol(StringBuilder sb);
    }
    public interface ILogger
    {
        void SetEnable(string tag, bool flag);
        bool GetEnable(string tag);
        void PrintLog(string tag, string message);
        void PrintError(string tag, string message);
        void PrintWarning(string tag, string message);
    }
}