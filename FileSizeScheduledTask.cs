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
/// 手动执行的文件大小过滤计划任务
/// </summary>
public class FileSizeScheduledTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<FileSizeScheduledTask> _logger;
    private readonly ILocalizationManager _localization;

    /// <summary>
    /// 初始化计划任务
    /// </summary>
    /// <param name="libraryManager">库管理器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="localization">本地化管理器</param>
    public FileSizeScheduledTask(
        ILibraryManager libraryManager, 
        ILogger<FileSizeScheduledTask> logger,
        ILocalizationManager localization)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _localization = localization;
    }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name => "文件大小过滤器";

    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description => "移除媒体库中小于指定大小的文件";

    /// <summary>
    /// 任务类别
    /// </summary>
    public string Category => _localization.GetLocalizedString("Library");

    /// <summary>
    /// 任务关键字
    /// </summary>
    public string Key => "FileSizeFilter";

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="progress">进度报告</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        // 获取插件配置
        var plugin = FileSizeFilterPlugin.Instance;
        if (plugin?.Configuration == null || !plugin.Configuration.Enabled)
        {
            _logger.LogInformation("文件大小过滤器已禁用，跳过执行");
            progress?.Report(100);
            return Task.CompletedTask;
        }

        var config = plugin.Configuration;
        long thresholdBytes = (long)config.MinFileSizeMB * 1024 * 1024;

        _logger.LogInformation("开始执行文件大小过滤，移除小于 {ThresholdMB}MB 的文件", config.MinFileSizeMB);

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
                progress?.Report(100);
                return Task.CompletedTask;
            }

            var itemsToRemove = new List<MediaBrowser.Controller.Entities.BaseItem>();
            var processedCount = 0;

            _logger.LogInformation("正在检查 {TotalCount} 个媒体文件", allItems.Count);

            foreach (var item in allItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("任务已取消");
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
                    _logger.LogDebug("发现小文件: {FileName} (大小: {FileSize:F2}MB)", 
                        fileInfo.Name, 
                        fileInfo.Length / (1024.0 * 1024.0));
                }
            }

            // 批量移除小文件
            if (itemsToRemove.Count > 0)
            {
                _logger.LogInformation("发现 {Count} 个小文件，正在从媒体库中移除", itemsToRemove.Count);

                var removedCount = 0;
                foreach (var item in itemsToRemove)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("任务已取消，已移除 {RemovedCount}/{TotalCount} 个文件", removedCount, itemsToRemove.Count);
                        break;
                    }

                    try
                    {
                        _libraryManager.DeleteItem(item, new MediaBrowser.Controller.Library.DeleteOptions
                        {
                            DeleteFileLocation = false,
                            DeleteFromExternalProvider = false
                        });
                        removedCount++;
                        _logger.LogDebug("已移除: {Path}", item.Path);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "移除文件失败: {Path}", item.Path);
                    }
                }

                _logger.LogInformation("文件大小过滤完成，共移除了 {RemovedCount} 个小文件", removedCount);
            }
            else
            {
                _logger.LogInformation("未发现需要移除的小文件");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行文件大小过滤任务时发生错误");
            throw;
        }
        finally
        {
            progress?.Report(100);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取默认触发器
    /// </summary>
    /// <returns>触发器列表</returns>
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // 不设置自动触发器，只允许手动执行
        return Enumerable.Empty<TaskTriggerInfo>();
    }
} 