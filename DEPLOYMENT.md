# Jellyfin 文件大小过滤器插件 - 部署指南

## 📋 部署前准备

### 环境要求
- ✅ Jellyfin 10.10.x 版本
- ✅ Docker 环境（推荐）
- ✅ 管理员权限

### 文件检查
确认编译生成的文件：
```bash
ls -la bin/Release/net8.0/publish/Jellyfin.Plugin.FileSizeFilter.dll
```

## 🚀 Docker 环境部署

### 1. 查找Jellyfin容器
```bash
docker ps | grep jellyfin
```
记下你的容器名称（例如：`jellyfin_container`）

### 2. 查看容器的插件目录
```bash
docker exec jellyfin_container ls -la /config/plugins/
```

### 3. 创建插件目录
```bash
docker exec jellyfin_container mkdir -p /config/plugins/FileSizeFilter
```

### 4. 复制插件文件到容器
```bash
docker cp bin/Release/net8.0/publish/Jellyfin.Plugin.FileSizeFilter.dll jellyfin_container:/config/plugins/FileSizeFilter/
```

### 5. 验证文件复制成功
```bash
docker exec jellyfin_container ls -la /config/plugins/FileSizeFilter/
```

### 6. 重启Jellyfin容器
```bash
docker restart jellyfin_container
```

### 7. 查看启动日志（可选）
```bash
docker logs -f jellyfin_container
```
查找类似信息：
```
[INF] Loaded plugin: "文件大小过滤器" "1.0.0.0"
```

## 🔧 Jellyfin 管理界面配置

### 1. 访问管理界面
- 打开浏览器，访问你的Jellyfin地址（例如：`http://your-server:8096`）
- 使用管理员账户登录

### 2. 找到插件设置
1. 点击右上角的 **设置齿轮图标**
2. 选择 **控制台**
3. 在左侧菜单中找到 **插件**

### 3. 配置文件大小过滤器
1. 在插件列表中找到 **"文件大小过滤器"**
2. 点击插件右侧的 **"..."** 菜单
3. 选择 **设置**
4. 配置以下选项：
   - ✅ **启用文件大小过滤**：勾选此选项
   - 📏 **最小文件大小阈值 (MB)**：设置合适的值（建议100-500MB）

### 4. 保存配置
点击 **保存配置** 按钮

## 📚 触发文件过滤

### 方法1：重新扫描媒体库
1. 进入 **控制台** → **库**
2. 选择要过滤的媒体库
3. 点击 **扫描媒体库**

### 方法2：手动运行清理任务
1. 进入 **控制台** → **计划任务**
2. 找到 **"清理小文件"** 任务
3. 点击 **▶️ 立即运行**

## 🔍 验证插件工作

### 查看日志
```bash
docker logs jellyfin_container | grep -i "文件大小\|FileSizeFilter"
```

### 预期日志输出
```
[INF] 开始清理小于 100MB 的文件
[DBG] 标记移除小文件: small_file.mp4 (大小: 50.5MB)
[INF] 已完成小文件清理，移除了 5 个文件
```

## ⚠️ 故障排除

### 插件未出现在列表中
1. **检查文件权限**：
   ```bash
   docker exec jellyfin_container ls -la /config/plugins/FileSizeFilter/
   ```

2. **重启容器**：
   ```bash
   docker restart jellyfin_container
   ```

3. **查看错误日志**：
   ```bash
   docker logs jellyfin_container | grep -i error
   ```

### 插件不工作
1. **确认插件已启用**：检查插件配置页面
2. **检查阈值设置**：确保设置的MB值合理
3. **手动触发**：运行"清理小文件"任务

### OpenWrt特别说明
1. **存储空间**：确保Docker有足够空间
2. **权限问题**：可能需要调整文件权限：
   ```bash
   chmod 644 /path/to/plugin/*.dll
   chown root:root /path/to/plugin/*.dll
   ```

## 📝 推荐设置

### 不同媒体类型的建议阈值

| 媒体类型 | 建议最小大小 | 说明 |
|---------|------------|------|
| 电影 | 500MB | 过滤预告片、片段 |
| 电视剧 | 200MB | 过滤预览、花絮 |
| 音乐 | 50MB | 过滤单曲demo、片段 |
| 动漫 | 100MB | 过滤OP/ED、PV |

### 安全建议
- ⚠️ 首次使用时建议设置较小的阈值进行测试
- 📋 建议在重要媒体库上先做备份
- 🔄 定期检查过滤结果，避免误删重要文件

## 🆘 支持

如果遇到问题：
1. 查看Jellyfin日志中的错误信息
2. 确认Docker容器运行正常
3. 检查插件配置是否正确
4. 确认文件权限设置

插件只会从Jellyfin库中移除文件记录，**不会删除物理文件**，可以安全使用。 