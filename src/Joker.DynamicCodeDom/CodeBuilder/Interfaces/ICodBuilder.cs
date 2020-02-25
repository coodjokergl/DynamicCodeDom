namespace Joker.DynamicCodeDom.CodeBuilder.Interfaces
{
    /// <summary>
    /// 代码编译
    /// </summary>
    public interface ICodBuilder
    {
        /// <summary>
        /// 源码
        /// </summary>
        /// <returns></returns>
        string Code();

        /// <summary>
        /// 导出名称
        /// </summary>
        string ExportName { get; }
    }
}
