using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Joker.DynamicCodeDom.CodeBuilder.Dependency;
using Joker.DynamicCodeDom.CodeBuilder.Interfaces;

namespace Joker.DynamicCodeDom.CodeFactory
{
    /// <summary>
    /// 代码工厂
    /// </summary>
    public static class CodeFactory
    {
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="exportFileInfo"></param>
        /// <param name="exportDirectoryInfo"></param>
        public static void Export(string exportFileInfo,string exportDirectoryInfo)
        {
            if (exportFileInfo == null)
            {
                throw new ArgumentNullException(nameof(exportFileInfo));
            }

            if (exportDirectoryInfo == null)
            {
                throw new ArgumentNullException(nameof(exportDirectoryInfo));
            }

            if (!File.Exists(exportFileInfo))
            {
                throw new FileNotFoundException("动态编译文件未找到！", exportFileInfo);
            }

            if (!File.Exists(exportDirectoryInfo))
            {
                Common.CreateDirectory(exportDirectoryInfo);
            }

            var tempPath = Path.Combine(exportDirectoryInfo, Common.BuildTempPath);
            Common.CreateDirectory(tempPath);

            Logger.Log("解析动态编码生成规则！");
            //加载程序
            var assembly = Assembly.LoadFrom(exportFileInfo);
            var codeDomAttr = assembly.GetCustomAttribute<CodeDomAssemblyAttribute>();
            if (codeDomAttr == null)
            {
                throw new Exception($@"{exportFileInfo}缺少程序集特性{nameof(CodeDomAssemblyAttribute)}");
            }

            var codeBuilderTypes = assembly.GetExportedTypes().Where(q => typeof(ICodBuilder).IsAssignableFrom(q) && !q.IsAbstract && q.IsClass && !q.IsAutoClass).ToList();

            foreach (var builderType in codeBuilderTypes)
            {
                try
                {
                    Logger.Log($@"{builderType.FullName}--->开始编译");
                    var builderInstance = (ICodBuilder)Activator.CreateInstance(builderType);
                   
                    var exportName = Path.ChangeExtension(Path.Combine(exportDirectoryInfo,builderInstance.ExportName),".dll");
                    var exportXmlName = Path.ChangeExtension(builderInstance.ExportName, ".xml");

                    Logger.Log($"动态DLL编译临时路径:{tempPath}");

                    Logger.Log($"动态DLL文件路径:{exportName}");
                    Logger.Log($"动态DLL文档路径:{exportXmlName}");
                    CompilerHelper.CompileCode(builderInstance.Code(), exportName, tempPath, exportXmlName);
                    Logger.Log($"编译生成成功！{exportName}");
                }
                catch (Exception exception)
                {
                    throw new Exception($@"{builderType.FullName}生成动态插件失败！",exception);
                }
            }
        }
    }
}
