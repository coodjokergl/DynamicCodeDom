﻿using System;
using System.IO;
using System.Text;

namespace Joker.DynamicCodeDom
{
    /// <summary>
    /// 提供一些与 System.IO.File 相同签名且功能相同的工具方法，
    /// 差别在于：当出现IOException时，这个类中的方法支持重试功能。
    /// </summary>
    public static class RetryFile
    {
        /// <summary>
        /// 等同于：System.IO.File.Exists()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Exists(string filePath)
        {
            // 由于 File.Exists 内部已经吃掉了很多异常，包含IOException，所以这里就不再对它重试。
            // 只是为了在代码中消灭 File.xxxxxxx 就提供了这个方法。
            return File.Exists(filePath);
        }

        // 本地磁盘 I/O 的重试参数
        private static readonly int s_tryCount = 5;
        private static readonly int s_WaitMillisecond = 3 * 1000;

        internal static Retry CreateRetry()
        {
            // 重试策略：当发生 IOException 时，最大重试 5 次，每次间隔 500 毫秒
            return Retry.Create(s_tryCount, s_WaitMillisecond).Filter<IOException>();
        }

        /// <summary>
        /// 等同于：System.IO.File.Delete()，
        /// 但是会在删除文件前检查文件是否存在。
        /// </summary>
        /// <param name="filePath"></param>
        public static void Delete(string filePath)
        {
            if (File.Exists(filePath) == false)
                return;

            CreateRetry()
                .Filter<UnauthorizedAccessException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is UnauthorizedAccessException)
                        try
                        {
                            File.SetAttributes(filePath, FileAttributes.Normal);
                        }
                        catch
                        { // 这里就是一个尝试机制，所以如果出错就忽略这个异常
                        }
                })
                .Run(() => {
                    File.Delete(filePath);
                    return 1;
                });
        }

        /// <summary>
        /// 等同于：System.IO.File.ReadAllText()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadAllText(string filePath)
        {
            return CreateRetry().Run(() => File.ReadAllText(filePath, Encoding.UTF8));
        }


        /// <summary>
        /// 等同于：System.IO.File.ReadAllLines()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string[] ReadAllLines(string filePath)
        {
            return CreateRetry().Run(() => File.ReadAllLines(filePath, Encoding.UTF8));
        }


        /// <summary>
        /// 等同于：System.IO.File.ReadAllBytes()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(string filePath)
        {
            return CreateRetry().Run(() => File.ReadAllBytes(filePath));
        }


        private static void SafeCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch
            {// 这里就是一个尝试机制，所以如果出错就忽略这个异常
            }
        }

        /// <summary>
        /// 等同于：System.IO.File.WriteAllText()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public static void WriteAllText(string filePath, string text)
        {
            CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => {
                    File.WriteAllText(filePath, text, Encoding.UTF8);
                    return 1;
                });
        }


        /// <summary>
        /// 等同于：System.IO.File.WriteAllBytes()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="buffer"></param>
        public static void WriteAllBytes(string filePath, byte[] buffer)
        {
            CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => {
                    File.WriteAllBytes(filePath, buffer);
                    return 1;
                });
        }


        /// <summary>
        /// 等同于：System.IO.File.AppendAllText()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public static void AppendAllText(string filePath, string text)
        {
            CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => {
                    File.AppendAllText(filePath, text, Encoding.UTF8);
                    return 1;
                });
        }


        /// <summary>
        /// 等同于：System.IO.File.GetLastWriteTime()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DateTime GetLastWriteTime(string filePath)
        {
            return CreateRetry().Run(() => File.GetLastWriteTime(filePath));
        }


        /// <summary>
        /// 等同于：System.IO.File.GetLastWriteTimeUtc()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DateTime GetLastWriteTimeUtc(string filePath)
        {
            return CreateRetry().Run(() => File.GetLastWriteTimeUtc(filePath));
        }


        /// <summary>
        /// 等同于：System.IO.File.GetAttributes()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileAttributes GetAttributes(string filePath)
        {
            return CreateRetry().Run(() => File.GetAttributes(filePath));
        }

        /// <summary>
        /// 判断文件是否为隐藏文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsHidden(string filePath)
        {
            return RetryFile.GetAttributes(filePath).HasFlag(FileAttributes.Hidden);
        }


        /// <summary>
        /// 取消文件的只读设置
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static void ClearReadonly(string filePath)
        {
            FileAttributes attributes = RetryFile.GetAttributes(filePath);

            if (attributes.HasFlag(FileAttributes.ReadOnly) == false)
                return;

            // 清除只读属性
            attributes &= ~FileAttributes.ReadOnly;

            CreateRetry().Run(() => {
                File.SetAttributes(filePath, attributes);
                return 1;
            });
        }



        /// <summary>
        /// 等同于：System.IO.File.OpenRead()
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileStream OpenRead(string filePath)
        {
            return CreateRetry().Run(() => File.OpenRead(filePath));
        }



        /// <summary>
        /// 等同于：System.IO.File.OpenWrite()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileStream OpenWrite(string filePath)
        {
            return CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => File.OpenWrite(filePath));
        }


        /// <summary>
        /// 打开或者创建文件，后面以追加方式操作
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileStream OpenAppend(string filePath)
        {
            return CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan));
        }


        /// <summary>
        /// 等同于：File.Create() ，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileStream Create(string filePath)
        {
            return CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(filePath));
                })
                .Run(() => File.Create(filePath));
        }


        /// <summary>
        /// 等同于：System.IO.File.Copy()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="overwrite"></param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite = true)
        {
            if (File.Exists(sourceFileName) == false)
                throw new FileNotFoundException("File not found: " + sourceFileName);

            CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(destFileName));
                })
                .Run(() => {
                    File.Copy(sourceFileName, destFileName, overwrite);
                    return 1;
                });
        }


        /// <summary>
        /// 等同于：System.IO.File.Move()，且当目录不存在时自动创建。
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public static void Move(string sourceFileName, string destFileName)
        {
            if (File.Exists(sourceFileName) == false)
                throw new FileNotFoundException("File not found: " + sourceFileName);

            CreateRetry()
                .Filter<DirectoryNotFoundException>()
                .OnException((ex, n) => {
                    if (n == 1 && ex is DirectoryNotFoundException)
                        SafeCreateDirectory(Path.GetDirectoryName(destFileName));
                })
                .Run(() => {
                    File.Move(sourceFileName, destFileName);
                    return 1;
                });
        }


    }
}
