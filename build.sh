#!/bin/bash

# Jellyfin 文件大小过滤器插件构建脚本

set -e  # 遇到错误时退出

echo "========================================"
echo "Jellyfin 文件大小过滤器插件构建脚本"
echo "========================================"

# 检查 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "错误: 未找到 .NET SDK"
    echo "请先安装 .NET 8.0 SDK:"
    echo "sudo apt update"
    echo "sudo apt install -y dotnet-sdk-8.0"
    exit 1
fi

echo "✅ .NET SDK 版本: $(dotnet --version)"

# 清理之前的构建
echo "🧹 清理之前的构建..."
if [ -d "bin" ]; then
    rm -rf bin
fi
if [ -d "obj" ]; then
    rm -rf obj
fi

# 恢复依赖
echo "📦 恢复 NuGet 包..."
dotnet restore

# 构建项目
echo "🔨 构建项目..."
dotnet build -c Release --no-restore

# 发布插件
echo "📦 发布插件..."
dotnet publish -c Release --no-build

echo ""
echo "✅ 构建完成!"
echo "插件文件位置: bin/Release/net8.0/publish/"
echo ""
echo "========================================"
echo "部署到 Docker 容器:"
echo "========================================"
echo "1. 找到你的 Jellyfin 容器名称:"
echo "   docker ps | grep jellyfin"
echo ""
echo "2. 创建插件目录:"
echo "   docker exec <容器名> mkdir -p /config/plugins/FileSizeFilter"
echo ""
echo "3. 复制插件文件:"
echo "   docker cp bin/Release/net8.0/publish/Jellyfin.Plugin.FileSizeFilter.dll <容器名>:/config/plugins/FileSizeFilter/"
echo ""
echo "4. 重启 Jellyfin 容器:"
echo "   docker restart <容器名>"
echo ""
echo "5. 在 Jellyfin 管理界面中配置插件"
echo "========================================" 