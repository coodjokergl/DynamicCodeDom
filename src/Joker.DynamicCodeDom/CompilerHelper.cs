using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using Joker.DynamicCodeDom.Exceptions;
using Microsoft.CSharp;

namespace Joker.DynamicCodeDom
{
	/// <summary>
	/// 编译辅助类
	/// </summary>
    public class CompilerHelper
    {
        /// <summary>
        /// 代码编译
        /// </summary>
        /// <param name="code"></param>
        /// <param name="outputName"></param>
        /// <param name="tempOutPath"></param>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static Assembly CompileCode(string code, string outputName = null, string tempOutPath = null,string xmlDoc = null)
		{
			SaveLastTempCode(code, tempOutPath);

			// 准备编译器参数
			CompilerParameters cp = new CompilerParameters();
			cp.GenerateExecutable = false;
            if (!string.IsNullOrEmpty(xmlDoc))
            {
                cp.CompilerOptions = $"/doc:{xmlDoc}";
            }
			if (outputName == null)
				cp.GenerateInMemory = true;
			else
				cp.OutputAssembly = outputName;

            // 添加编译必须的程序集引用
			Assembly[] assemblies = RunTimeEnvironment.GetLoadAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.IsDynamic)
					continue;
				cp.ReferencedAssemblies.Add(assembly.Location);
			}


			// 创建编译器实例并编译代码
			CSharpCodeProvider csProvider = new CSharpCodeProvider();
			CompilerResults cr = csProvider.CompileAssemblyFromSource(cp, code);

			// 检查编译结果
			if (cr.Errors != null && cr.Errors.HasErrors)
			{
				SaveLastCompileError(cr, tempOutPath);
				throw new CompileException { Code = code, CompilerResult = cr };
			}
			
			// 获取编译结果，它是编译后的程序集
			return cr.CompiledAssembly;
		}

		/// <summary>
		/// 获取编译错误信息
		/// </summary>
		/// <param name="compilerResults"></param>
		/// <returns></returns>
		public static string GetCompileErrorMessage(CompilerResults compilerResults)
		{
			if (compilerResults.Errors == null || compilerResults.Errors.HasErrors == false)
				return string.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (CompilerError error in compilerResults.Errors)
				sb.AppendLine(error.ErrorText);

			return sb.ToString();
		}


		internal static void SaveLastTempCode(string code, string tempOutPath)
		{
			if (string.IsNullOrEmpty(tempOutPath))
				return;

			try
			{
				string filePath = Path.Combine(tempOutPath, "__last_temp_code.cs");
				RetryFile.WriteAllText(filePath, code);
			}
			catch
			{  /* 输出最近一次运行时运行的代码，方便调试程序，忽略写文件出现的异常。 */
			}
		}


		internal static void SaveLastCompileError(CompilerResults cr, string tempOutPath)
		{
			if (string.IsNullOrEmpty(tempOutPath))
				return;

			try
			{
				string errorText = GetCompileErrorMessage(cr);
				string filePath = Path.Combine(tempOutPath, "__last_CompilerError.txt");
				RetryFile.WriteAllText(filePath, errorText);
			}
			catch
			{  /* 输出最近一次运行时运行的异常，方便调试程序，忽略写文件出现的异常。 */
			}
		}
	}
}
