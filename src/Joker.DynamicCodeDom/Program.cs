using System;
using System.Linq;

namespace Joker.DynamicCodeDom
{
    class Program
    {
        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            try
            {
                if (args.FirstOrDefault() != Common.BuildCliKey)
                {
                    throw new Exception($@"非法调用！");
                }

                if (args.Length != 3)
                {
                    throw new Exception($@"参数数目不正确！");
                }

                if (args.ToList().Exists(string.IsNullOrEmpty))
                {
                    throw new Exception($@"参数不能为空字符串！");
                }
                Logger.Log("命令参数");
                Logger.Log(string.Join(Environment.NewLine,args));

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                //导出Excel
                CodeFactory.CodeFactory.Export(Common.FormatPath(args[1]), Common.FormatPath(args[2]));
                return 0;
            }
            catch (Exception e)
            {
               Logger.Log(e);
               return 1;
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log(e.ExceptionObject);
            System.Environment.Exit(1);
        }
    }
}
