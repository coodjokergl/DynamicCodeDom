using System;

namespace Joker.DynamicCodeDom
{
    internal static class Logger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(object msg)
        {
            Console.WriteLine(msg);
        }
    }
}
