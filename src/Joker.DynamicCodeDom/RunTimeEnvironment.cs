using System;
using System.Collections.Generic;
using System.Reflection;

namespace Joker.DynamicCodeDom
{
    /// <summary>
    /// 当前应用程序的运行时环境
    /// </summary>
    public static class RunTimeEnvironment
    {
        private static Assembly[] GetLoadAssembliesImpl()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// 获取当前程序加载的所有程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetLoadAssemblies()
        {
            return GetLoadAssemblies(false);
        }

        /// <summary>
        /// 获取当前程序加载的所有程序集
        /// </summary>
        /// <param name="ignoreSystemAssembly">是否忽略以System开头和动态程序集，通常用于反射时不搜索它们。</param>
        /// <returns></returns>
        public static Assembly[] GetLoadAssemblies(bool ignoreSystemAssembly)
        {
            Assembly[] assemblies = GetLoadAssembliesImpl();

            // 过滤一些反射中几乎用不到的程序集
            List<Assembly> list = new List<Assembly>(128);

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic)    // 动态程序通常是不需要参考的
                    continue;

                if (ignoreSystemAssembly)
                {
                    // 过滤以【System】开头的程序集，加快速度
                    if (assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                list.Add(assembly);
            }
            return list.ToArray();
        }
    }
}
