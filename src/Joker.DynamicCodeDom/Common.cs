using System;
using System.IO;

namespace Joker.DynamicCodeDom
{
    /// <summary>
    /// 公共模块
    /// </summary>
    internal class Common
    {
        /// <summary>
        /// 动态编译临时目录
        /// </summary>
        public const string BuildTempPath = "TempDynamicCodeBuild";

        /// <summary>
        /// 编译命令参数 标识。放置误操作
        /// </summary>
        public const string BuildCliKey = "MTIzNDU2Z2w=";

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception($@"路径不能为空！");
            }

            if (Directory.Exists(path))
            {
                return path;
            }

            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir))
            {
                return dir;
            }

            if (string.IsNullOrEmpty(dir))
            {
                throw new Exception($@"{path}数据错误，解析文件夹名称失败！");
            }

            Logger.Log($"创建路径：{dir}");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string FormatPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return path.Replace("\\\\", "\\").Trim().Trim('\"').Trim();
        }
    }
}
