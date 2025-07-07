using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.FileSizeFilter;

/// <summary>
/// 库扫描后清理小文件的任务
/// </summary>
public class FileSizePostScanTask : ILibraryPostScanTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<FileSizePostScanTask> _logger;
    private readonly ILocalizationManager _localization;

    /// <summary>
    /// 初始化清理任务
    /// </summary>
    /// <param name="libraryManager">库管理器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="localization">本地化管理器</param>
    public FileSizePostScanTask(
        ILibraryManager libraryManager, 
        ILogger<FileSizePostScanTask> logger,
        ILocalizationManager localization)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _localization = localization;
    }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name => "清理小文件";

    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description => "扫描库后移除小于指定大小的文件";

    /// <summary>
    /// 任务类别
    /// </summary>
    public string Category => _localization.GetLocalizedString("Library");

    /// <summary>
    /// 任务关键字
    /// </summary>
    public string Key => "FileSizeCleanup";

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="progress">进度报告</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public Task Run(IProgress<double> progress, CancellationToken cancellationToken)
    {
        // 获取插件配置
        var plugin = FileSizeFilterPlugin.Instance;
        if (plugin?.Configuration == null || !plugin.Configuration.Enabled)
        {
            _logger.LogDebug("文件大小过滤器已禁用，跳过清理任务");
            return Task.CompletedTask;
        }

        var config = plugin.Configuration;
        long thresholdBytes = (long)config.MinFileSizeMB * 1024 * 1024;

        _logger.LogInformation("开始清理小于 {ThresholdMB}MB 的文件", config.MinFileSizeMB);

        try
        {
            // 获取所有媒体项目
            var allItems = _libraryManager.GetItemList(new MediaBrowser.Controller.Entities.InternalItemsQuery
            {
                Recursive = true,
                MediaTypes = new[] { Jellyfin.Data.Enums.MediaType.Video, Jellyfin.Data.Enums.MediaType.Audio }
            });

            if (allItems.Count == 0)
            {
                _logger.LogInformation("未找到媒体文件");
                return Task.CompletedTask;
            }

            var itemsToRemove = new List<MediaBrowser.Controller.Entities.BaseItem>();
            var processedCount = 0;

            foreach (var item in allItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // 更新进度
                progress?.Report((double)processedCount / allItems.Count * 100);
                processedCount++;

                // 检查文件大小
                var path = item.Path;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    continue;
                }

                var fileInfo = new FileInfo(path);
                if (fileInfo.Length < thresholdBytes)
                {
                    itemsToRemove.Add(item);
                    _logger.LogDebug("标记移除小文件: {FileName} (大小: {FileSize}MB)", 
                        fileInfo.Name, 
                        fileInfo.Length / (1024.0 * 1024.0));
                }
            }

            // 批量移除小文件
            if (itemsToRemove.Count > 0)
            {
                _logger.LogInformation("正在移除 {Count} 个小文件", itemsToRemove.Count);

                foreach (var item in itemsToRemove)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        _libraryManager.DeleteItem(item, new MediaBrowser.Controller.Library.DeleteOptions
                        {
                            DeleteFileLocation = false,
                            DeleteFromExternalProvider = false
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "移除文件失败: {Path}", item.Path);
                    }
                }

                _logger.LogInformation("已完成小文件清理，移除了 {Count} 个文件", itemsToRemove.Count);
            }
            else
            {
                _logger.LogInformation("未发现需要清理的小文件");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行文件大小清理任务时发生错误");
            throw;
        }
        finally
        {
            progress?.Report(100);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取触发器
    /// </summary>
    /// <returns>触发器列表</returns>
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // 不设置自动触发器，只在库扫描后运行
        return Enumerable.Empty<TaskTriggerInfo>();
    }
} 