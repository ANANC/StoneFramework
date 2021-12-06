using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class CSharpUtility
{
    /// <summary>
    /// 添加一些指定的程序集名称，这样可以用于优先查询.
    /// </summary>
    /// <param name="assemblyName">程序集名称</param>
    public static void AddSpecifyAssembly(string assemblyName)
    {
        var assembly = FindAssemblyInAll(assemblyName);

        if (null == assembly) // 一般不会发生，只有assembly是在后续手动加载的情况
        {
            assembly = FindAssemblyInAll(assemblyName, true); // 找不到强制重新查找以便
        }

        if (null != assembly)
        {
            if (null == s_AssembliesCache)
            {
                s_AssembliesCache = new Dictionary<string, Assembly>();
            }

            // 加入到缓存里面
            s_AssembliesCache.Add(assemblyName, assembly);
        }
        else
        {
            Debug.LogError(s_className + " Can't find assembly:" + assemblyName);
        }
    }

    public static Type FindType(string qualifiedTypeName, bool log = true)
    {
        if (null == qualifiedTypeName || qualifiedTypeName.Length <= 0) { return null; }

        Type result = null;

        s_TypeCache?.TryGetValue(qualifiedTypeName, out result);

        if (null == result)
        {
            result = Type.GetType(qualifiedTypeName); // 当前程序集

            if (null == result)
            {
                result = __GetTypeFromAssemblyInCache(qualifiedTypeName); // 快速程序集
            }

            if (null == result)
            {
                result = __GetTypeFromAssemblyInAll(qualifiedTypeName); // 所有的程序集
            }

            if (null == result)
            {
                result = __GetTypeFromAssemblyInAll(qualifiedTypeName, true); // 实时所有的程序集
            }

            if (null != result)
            {
                s_TypeCache.Add(qualifiedTypeName, result);
            }
            else
            {
                 Debug.LogError(s_className+ " Can't find "+ qualifiedTypeName);
            }
        }

        return result;
    }

    public static object CreateInstance(string qualifiedTypeName, bool log = true)
    {

        Type t = FindType(qualifiedTypeName);

        if (null != t)
        {
            // Convert.ChangeType
            var obj = Activator.CreateInstance(t);

            if (null == obj && log)
            {
                Debug.LogError(s_className + " CSharpUtility.CreateInstance fail!" + qualifiedTypeName);
            }

            return obj;

        }
        else if (log)
        {
            Debug.LogError(s_className + "CSharpUtility.CreateInstance fail!The class is not in the AppDomain " + qualifiedTypeName);
        }

        return null;
    }

    /// <summary>
    /// 保存类的Atrribute.
    /// </summary>
    private static Dictionary<Type, Dictionary<Type, object>> s_TypeAttribute = new Dictionary<Type, Dictionary<Type, object>>();

    /// <summary>
    /// 获取指定类的Atrribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="res"></param>
    /// <returns></returns>
    public static bool GetAttribute<T>(this Type type, ref T res)
    {
        Dictionary<Type, object> subDict = null;
        if (s_TypeAttribute.TryGetValue(type, out subDict) == true)
        {
            object obj;
            if (subDict.TryGetValue(typeof(T), out obj) == true)
            {
                res = (T)obj;

                return true;
            }
        }

        var ttype = typeof(T);

        var attr = type.GetCustomAttributes(ttype, false);
        if (attr == null || attr.Length == 0)
        {
            return false;
        }

        res = (T)attr[0];

        if (null == subDict)
        {
            subDict = new Dictionary<Type, object>();
            s_TypeAttribute[type] = subDict;
        }

        subDict.Add(ttype, attr[0]);

        return true;
    }

    public static List<Type> FindDerivedTypesFromAssembly(this Type baseType, Assembly assembly, bool classOnly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException("assembly", "Assembly must be defined");
        }

        if (baseType == null)
        {
            throw new ArgumentNullException("baseType", "Parent Type must be defined");
        }

        List<Type> res = new List<Type>();

        // get all the types
        var types = assembly.GetTypes();

        // works out the derived types
        foreach (var type in types)
        {
            // if classOnly, it must be a class
            // useful when you want to create instance
            if (classOnly && !type.IsClass)
            {
                continue;
            }

            if (type.IsAbstract)
            {
                continue;
            }

            if (baseType.IsInterface)
            {
                var it = type.GetInterface(baseType.FullName);

                if (it != null)
                {
                    // add it to result list
                    res.Add(type);
                }
            }
            else if (baseType.IsAssignableFrom(type))
            {
                // add it to result list
                res.Add(type);
            }
        }

        return res;
    }

    #region private

    private static string s_className = typeof(CSharpUtility).Name;

    /// <summary>
    /// 缓存常用的用于优先查询.
    /// </summary>
    private static Dictionary<string, Assembly> s_AssembliesCache = null;

    /// <summary>
    /// 缓存所以的程序集减少经常获取全部程序集.
    /// </summary>
    private static Dictionary<string, Assembly> s_AllAssemblies = null;

    /// <summary>
    /// 类型缓存.
    /// </summary>
    private static Dictionary<string, Type> s_TypeCache = null;

    /// <summary>
    /// 在所有的代码集中查找.
    /// </summary>
    /// <param name="assemblyName">代码集的名称.</param>
    /// <param name="update">是否重新或者当前所以的代码集.</param>
    /// <returns>对应名称的代码集.</returns>
    public static Assembly FindAssemblyInAll(string assemblyName, bool update = false)
    {
        var allAssemblies = __GetAllAssemblies(update);

        allAssemblies.TryGetValue(assemblyName, out var result);

        return result;
    }

    /// <summary>
    /// 加快速度，不让每次去查所有的Assembly.
    /// </summary>
    /// <returns>返回所有字符集.</returns>
    private static Dictionary<string, Assembly> __GetAllAssemblies(bool update = false)
    {
        if (null == s_AllAssemblies || update)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            s_AllAssemblies = new Dictionary<string, Assembly>();

            foreach (var ass in assemblies)
            {
                s_AllAssemblies.Add(ass.GetName().Name, ass);
            }
        }

        return s_AllAssemblies;
    }

    private static Type __GetTypeFromAssemblyInCache(string qualifiedTypeName)
    {
        Type result = null;

        foreach (var assembly in s_AssembliesCache)
        {
            result = assembly.Value.GetType(qualifiedTypeName);
            if (null != result)
            {
                break;
            }
        }

        return result;
    }

    private static Type __GetTypeFromAssemblyInAll(string qualifiedTypeName, bool update = false)
    {
        var allAssemblies = __GetAllAssemblies(update);

        Type result = null;

        foreach (var assembly in allAssemblies)
        {
            result = assembly.Value.GetType(qualifiedTypeName);
            if (null != result)
            {
                // 如果有这个类型，那么就把程序集缓存起来，加快下一次的查询
                if (null == s_AssembliesCache) s_AssembliesCache = new Dictionary<string, Assembly>();

                string assemblyName = assembly.Value.GetName().Name;
                if (s_AssembliesCache.ContainsKey(assemblyName) == false)
                {
                    s_AssembliesCache.Add(assemblyName, assembly.Value);
                }

                break;
            }
        }

        return result;
    }
    #endregion
}

