# Jellyfin 文件大小过滤器插件

## 功能说明

这个插件可以根据文件大小过滤Jellyfin影音库中的文件，自动忽略小于指定大小的文件。特别适用于：

- 过滤预览图、缩略图等小文件
- 忽略字幕文件、NFO文件等元数据文件
- 避免扫描损坏的或不完整的影音文件
- 保持媒体库整洁，只显示真正的影音内容

## 特性

- ✅ 基于文件大小进行精确过滤（单个文件级别）
- ✅ 简单易用的配置界面
- ✅ 支持MB为单位的阈值设置
- ✅ 可随时启用/禁用过滤功能
- ✅ 详细的日志记录
- ✅ 默认过滤小于100MB的文件

## 项目结构

```
jellyfin-filesize-filter/
├── .git/                                    # Git仓库
├── .gitignore                              # Git忽略文件
├── Configuration/
│   └── configPage.html                     # 插件配置页面
├── FileSizeFilterPlugin.cs                 # 主插件类
├── FileSizeScheduledTask.cs                # 手动执行任务
├── FileSizePostScanTask.cs                 # 自动执行任务
├── PluginConfiguration.cs                  # 配置类
├── Jellyfin.Plugin.FileSizeFilter.csproj   # 项目文件
├── README.md                               # 项目说明
├── DEPLOYMENT.md                           # 部署指南
├── build.sh                                # 构建脚本
├── bin/                                    # 编译输出（被忽略）
└── obj/                                    # 编译缓存（被忽略）
```

### 核心文件说明

- **FileSizeFilterPlugin.cs**: 插件主类，提供基本信息和配置页面
- **FileSizeScheduledTask.cs**: 实现计划任务接口，可在任务列表中手动执行
- **FileSizePostScanTask.cs**: 实现库扫描后自动执行的任务
- **PluginConfiguration.cs**: 配置类，定义插件设置项
- **Configuration/configPage.html**: 中文配置界面，符合Jellyfin标准

## 编译方法

### 前提条件
- .NET 8.0 SDK
- Visual Studio Code 或 Visual Studio

### 编译步骤
```bash
# 1. 进入项目目录
cd jellyfin-filesize-filter

# 2. 恢复依赖包
dotnet restore

# 3. 编译项目
dotnet build -c Release

# 4. 发布插件
dotnet publish -c Release
```

编译完成后，插件文件位于：`bin/Release/net8.0/publish/`

## Docker环境部署

### 方法一：手动部署

```bash
# 1. 编译插件（在开发环境中）
dotnet publish -c Release

# 2. 找到Docker容器的插件目录
# 假设你的Jellyfin容器名为 'jellyfin'
docker exec jellyfin ls -la /config/plugins/

# 3. 创建插件目录
docker exec jellyfin mkdir -p /config/plugins/FileSizeFilter

# 4. 复制插件文件到容器
docker cp bin/Release/net8.0/publish/Jellyfin.Plugin.FileSizeFilter.dll jellyfin:/config/plugins/FileSizeFilter/

# 5. 重启Jellyfin容器
docker restart jellyfin
```

### 方法二：使用数据卷映射

如果你的Jellyfin配置目录映射到宿主机：

```bash
# 1. 编译插件
dotnet publish -c Release

# 2. 复制到映射的配置目录
# 假设配置目录映射到 /path/to/jellyfin/config
mkdir -p /path/to/jellyfin/config/plugins/FileSizeFilter
cp bin/Release/net8.0/publish/Jellyfin.Plugin.FileSizeFilter.dll /path/to/jellyfin/config/plugins/FileSizeFilter/

# 3. 重启Jellyfin容器
docker restart jellyfin
```

### OpenWrt环境特别说明

对于OpenWrt环境：

1. 确保Docker有足够的存储空间
2. 插件目录通常在：`/var/lib/docker/volumes/jellyfin-config/_data/plugins/`
3. 可能需要调整文件权限：
   ```bash
   chmod 644 /path/to/plugin/*.dll
   chown root:root /path/to/plugin/*.dll
   ```

## 使用方法

1. **安装插件**后重启Jellyfin
2. 进入**管理后台** → **插件** → **文件大小过滤器配置**
3. 设置**最小文件大小阈值**（默认100MB）
4. 确保**启用文件大小过滤**被勾选
5. 点击**保存配置**
6. **重新扫描媒体库**以应用新的过滤规则

## 工作机制

### 执行方式
- **自动执行**: 每次媒体库扫描完成后自动运行过滤
- **手动执行**: 在"管理 → 任务"中找到"文件大小过滤器"手动执行

### 工作流程
1. 获取媒体库中所有视频和音频文件
2. 逐个检查文件大小
3. 将小于阈值的文件从媒体库中移除（仅移除显示记录）
4. 物理文件保持不变，不会被删除

### 对新增媒体的处理
- ✅ 新增文件会自动受到过滤规则影响
- ✅ 每次扫描后都会自动执行过滤
- ✅ 无需手动干预

## 配置选项

- **启用文件大小过滤**: 开启/关闭过滤功能
- **最小文件大小阈值 (MB)**: 小于此大小的文件将被忽略

## 常见问题

**Q: 插件安装后没有效果？**
A: 需要重新扫描媒体库，新的过滤规则才会生效。

**Q: 设置多少MB比较合适？**
A: 建议设置为100-500MB之间，具体取决于你的文件类型。一般电影建议500MB+，电视剧可以设置100-200MB。

**Q: 插件会影响扫描性能吗？**
A: 影响很小，只是读取文件大小信息，不会显著影响扫描速度。

**Q: 可以针对不同媒体库设置不同阈值吗？**
A: 当前版本是全局设置，后续版本可能会添加按库配置的功能。

## 技术信息

- **兼容性**: Jellyfin 10.10.x
- **框架**: .NET 8.0
- **插件ID**: 6ac83d8b-8a8c-4f4e-9b7c-2d5e3f1a3b1c

## 开发调试

```bash
# 启用调试日志
# 在Jellyfin的日志配置中设置
# Jellyfin.Plugin.FileSizeFilter 的日志级别为 Debug
```

调试时可以在Jellyfin日志中看到类似信息：
```
[DBG] [Jellyfin.Plugin.FileSizeFilter.FileSizeIgnoreRule] 忽略小文件: sample.jpg (大小: 2MB < 阈值: 100MB)
``` 