using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.FileSizeFilter;

/// <summary>
/// 文件大小过滤器插件
/// </summary>
public class FileSizeFilterPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// 插件实例
    /// </summary>
    public static FileSizeFilterPlugin? Instance { get; private set; }

    /// <summary>
    /// 初始化插件
    /// </summary>
    /// <param name="applicationPaths">应用程序路径</param>
    /// <param name="xmlSerializer">XML序列化器</param>
    public FileSizeFilterPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// 插件名称
    /// </summary>
    public override string Name => "文件大小过滤器";

    /// <summary>
    /// 插件描述
    /// </summary>
    public override string Description => "根据文件大小过滤影音库中的文件，忽略小于指定大小的文件";

    /// <summary>
    /// 插件唯一标识符
    /// </summary>
    public override Guid Id => Guid.Parse("6ac83d8b-8a8c-4f4e-9b7c-2d5e8f4a3b1c");

    /// <summary>
    /// 获取插件的Web页面
    /// </summary>
    /// <returns>Web页面列表</returns>
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "文件大小过滤器配置",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        };
    }
} 