using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class LogHelper
{
    public enum LogGrade
    {
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warnning = 4,
        Error = 5
    }

    private static LogGrade m_LogGrade = LogGrade.Error;
    private static StringBuilder m_LogStringBuilder = new StringBuilder();

    private static StringBuilder m_Obj2StrStringBuilder = new StringBuilder();
    private static int m_Obj2StrInputIndex = 0;

    public class BaseLog
    {
        protected static LogGrade m_LogGrade;
        public void Log(params string[] message)
        {
            if (message.Length <= 0)
            {
                return;
            }

            LogHelper.Log(m_LogGrade,"[c#]", message);
        }
    }

    public class TraceLog : BaseLog
    {
        public TraceLog() { m_LogGrade = LogGrade.Trace; }
    }
    private static TraceLog m_Trace;
    public static TraceLog Trace
    {
        get
        {
            if (m_LogGrade >= LogGrade.Trace)
            {
                if (m_Trace == null)
                {
                    m_Trace = new TraceLog();
                }
                return m_Trace;
            }

            return null;
        }
    }

    public class DebugLog : BaseLog
    {
        public DebugLog() { m_LogGrade = LogGrade.Debug; }
    }
    private static DebugLog m_Debug;
    public static DebugLog Debug
    {
        get
        {
            if (m_LogGrade >= LogGrade.Debug)
            {
                if (m_Debug == null)
                {
                    m_Debug = new DebugLog();
                }
                return m_Debug;
            }

            return null;
        }
    }


    public class InfoLog : BaseLog
    {
        public InfoLog() { m_LogGrade = LogGrade.Info; }
    }
    private static InfoLog m_Info;
    public static InfoLog Info
    {
        get
        {
            if (m_LogGrade >= LogGrade.Info)
            {
                if (m_Info == null)
                {
                    m_Info = new InfoLog();
                }
                return m_Info;
            }

            return null;
        }
    }


    public class WarnningLog : BaseLog
    {
        public WarnningLog() { m_LogGrade = LogGrade.Warnning; }
    }
    private static WarnningLog m_Warnning;
    public static WarnningLog Warnning
    {
        get
        {
            if (m_LogGrade >= LogGrade.Warnning)
            {
                if (m_Warnning == null)
                {
                    m_Warnning = new WarnningLog();
                }
                return m_Warnning;
            }

            return null;
        }
    }

    public class ErrorLog : BaseLog
    {
        public ErrorLog() { m_LogGrade = LogGrade.Error; }
    }
    private static ErrorLog m_Error;
    public static ErrorLog Error
    {
        get
        {
            if (m_LogGrade >= LogGrade.Error)
            {
                if (m_Error == null)
                {
                    m_Error = new ErrorLog();
                }
                return m_Error;
            }

            return null;
        }
    }

    public static void LuaLog(int logGrade,string title,string message)
    {
        LogGrade cslogGrade = (LogGrade)logGrade;
        Log(cslogGrade, "[lua]", title, message);
    }

    private static void Log(LogGrade logGrade,string source, params string[] message)
    {
        int length = message.Length;
        if (length <= 0)
        {
            return;
        }

        m_LogStringBuilder.Length = 0;

        m_LogStringBuilder.AppendFormat("¡¾{0}¡¿", message[0]);
        for (int index = 1; index < length; index++)
        {
            m_LogStringBuilder.Append(message[index]);
            if (index != length - 1)
            {
                m_LogStringBuilder.Append(" ");
            }
        }

        if (logGrade == LogGrade.Error)
        {
            UnityEngine.Debug.LogError(m_LogStringBuilder.ToString());
        }
        else if (logGrade == LogGrade.Warnning)
        {
            UnityEngine.Debug.LogWarning(m_LogStringBuilder.ToString());
        }
        else
        {
            UnityEngine.Debug.Log(m_LogStringBuilder.ToString());
        }
    }

    public static string Object2String(object obj)
    {
        m_Obj2StrStringBuilder.Length = 0;
        m_Obj2StrInputIndex = 0;

        if (obj != null)
        {
            __DeepObject2String(obj);
        }

        return m_Obj2StrStringBuilder.ToString();
    }

    private static void __DeepObject2String(object obj)
    {
        for(int index = 0;index< m_Obj2StrInputIndex;index++)
        {
            m_Obj2StrStringBuilder.Append("     ");
        }
        m_Obj2StrStringBuilder.Append("{\n");

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];

            var fieldValue = field.GetValue(obj);
            if(fieldValue == null)
            {
                continue;
            }

            var fieldType = fieldValue.GetType();
            if (fieldType.IsValueType || fieldType.Equals(typeof(System.String)) || fieldType.IsEnum)
            {
                string fieldName = field.Name;
                for (int index = 0; index < m_Obj2StrInputIndex+2; index++)
                {
                    m_Obj2StrStringBuilder.Append("     ");
                }
                m_Obj2StrStringBuilder.AppendFormat("{0}£º{1}\n", fieldName, fieldValue);
            }
            else if(fieldType.IsArray)
            {
                string fieldName = field.Name;
                IEnumerable enumerable = (IEnumerable)fieldValue;
                var fieldEnumerable = enumerable.GetEnumerator();
                while(fieldEnumerable.MoveNext())
                {
                    var current = fieldEnumerable.Current;
                    for (int index = 0; index < m_Obj2StrInputIndex + 2; index++)
                    {
                        m_Obj2StrStringBuilder.Append("     ");
                    }
                    m_Obj2StrStringBuilder.AppendFormat("{0}£º{1}\n", fieldName, current);
                }
            }
            else
            {
                m_Obj2StrInputIndex++;
                __DeepObject2String(fieldValue);
                m_Obj2StrInputIndex--;
            }
        }
        for (int index = 0; index < m_Obj2StrInputIndex; index++)
        {
            m_Obj2StrStringBuilder.Append("     ");
        }
        m_Obj2StrStringBuilder.Append("}\n");
    }
}
