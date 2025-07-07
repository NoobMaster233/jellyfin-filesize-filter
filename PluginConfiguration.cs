using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.FileSizeFilter;

/// <summary>
/// 文件大小过滤器插件配置类
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// 最小文件大小阈值（MB）
    /// 小于此大小的文件将被忽略
    /// </summary>
    public int MinFileSizeMB { get; set; } = 100;

    /// <summary>
    /// 是否启用文件大小过滤
    /// </summary>
    public bool Enabled { get; set; } = true;
} 