# MOBA Game MVP - Quick Start Guide

## 快速开始指南

这是一个可运行的 Unity 2022.3 LTS MOBA MVP 原型，包含客户端和 .NET TCP 服务器。

### 最低要求

**Unity 客户端:**
- Unity 2022.3 LTS 或更高版本
- 支持 Android 平台（也可在 Unity Editor 中运行）

**.NET 服务器:**
- .NET 8.0 SDK 或更高版本

---

## 第一步：启动服务器

```bash
cd MobaGame/DotNetServer/MobaServer
dotnet run
```

服务器将在端口 **8888** 上启动。你应该看到：

```
=== MOBA Game Server ===
Starting server...

Server started on port 8888
Waiting for connections...
```

按 'q' 键停止服务器。

---

## 第二步：打开 Unity 项目

1. 打开 **Unity Hub**
2. 点击 **"Add"（添加）**
3. 导航到 `MobaGame/UnityClient/` 文件夹
4. 选择该文件夹并在 Unity 2022.3 LTS 中打开
5. 等待 Unity 导入所有资源并编译脚本

---

## 第三步：运行游戏

### 在 Unity Editor 中运行（推荐用于测试）

1. 在 Unity 中，找到 `Assets/Scenes/` 文件夹
2. 如果场景不存在，使用菜单：**MOBA → Setup → Create All Scenes**
3. 双击打开 `LoginScene.unity`
4. 点击顶部的 **Play（播放）** 按钮
5. 按照游戏流程进行：
   - 登录界面：输入任意用户名
   - 大厅：点击"快速匹配"
   - 选择英雄：选择 5 个职业中的一个
   - 战斗：开始游戏！

### 编辑器控制键

- **WASD** / **方向键**: 移动英雄
- **Space（空格）**: 普通攻击
- **Q**: 使用技能 1
- **E**: 使用技能 2
- **R**: 使用大招
- **B**: 打开商店
- **Enter（回车）**: 打开聊天

---

## 第四步：构建到 Android（可选）

1. 在 Unity 中：**File（文件） → Build Settings（构建设置）**
2. 选择 **Android** 平台
3. 点击 **Switch Platform（切换平台）**
4. 点击 **Player Settings（玩家设置）** 配置：
   - 设置公司名称和产品名称
   - 设置最低 API 级别为 22
5. 连接 Android 设备或配置模拟器
6. 点击 **Build and Run（构建并运行）**

### 移动端控制

- **虚拟摇杆**（左下角）: 移动英雄
- **攻击按钮**: 普通攻击
- **技能按钮**: 使用技能 1、2 和大招
- **商店按钮**: 打开物品商店
- **聊天按钮**: 打开聊天

---

## 游戏功能概览

### ✅ 已实现功能

**地图与环境:**
- 5v5 三路地图（上路/中路/下路）
- 每路 3 座防御塔 + 基地水晶
- 野区 Buff 生成点（红/蓝）

**英雄系统:**
- 5 个职业：坦克/战士/刺客/法师/射手
- 每个英雄有：普通攻击、2个普通技能、1个大招、1个被动
- 使用 Capsule/Primitive 作为占位模型

**战斗系统:**
- 小兵三路定时刷新
- 小兵自动索敌并攻击
- 防御塔自动索敌（优先小兵）
- 基地水晶被摧毁后游戏结束

**经济系统:**
- 击杀小兵/英雄/防御塔获得金币
- 死亡后自动复活
- 经验值和等级系统

**装备系统:**
- 10 件基础装备
- 商店购买功能
- 每个职业的推荐装备顺序

**社交功能:**
- 战斗内文字聊天
- 战绩显示（K/D/A、金币、等级）

**网络功能:**
- TCP 连接到服务器
- 聊天消息转发
- 玩家状态同步框架

---

## 网络配置

### 修改服务器 IP 地址

如果你的服务器不在本机上运行，需要修改客户端配置：

1. 在 Unity 中打开：`Assets/Scripts/Network/TCPClient.cs`
2. 找到这一行：
   ```csharp
   public string serverIP = "127.0.0.1";
   ```
3. 将 `127.0.0.1` 改为你的服务器 IP 地址
4. 保存文件

### 修改服务器端口

默认端口是 `8888`，如需修改：

**服务器端:**
- 修改 `MobaServer/Program.cs` 中的 `new TCPServer(8888)`

**客户端:**
- 修改 `TCPClient.cs` 中的 `public int serverPort = 8888;`

---

## 故障排除

### Unity 无法打开项目
- 确保安装了 Unity 2022.3 LTS
- 检查 Unity Hub 中的版本是否正确

### 服务器无法启动
- 确保已安装 .NET 8.0 SDK：运行 `dotnet --version`
- 检查端口 8888 是否被占用
- Windows 用户可能需要以管理员身份运行

### 无法连接到服务器
- 验证服务器正在运行
- 检查防火墙设置
- 确认客户端代码中的 IP 地址正确
- 本地测试使用 `127.0.0.1`

### 游戏崩溃或卡顿
- 检查 Unity Console 中的错误信息
- 确保所有必需的场景都在 Build Settings 中
- 使用 **MOBA → Setup → Add Scenes to Build Settings** 添加场景

### 场景不存在
- 使用菜单：**MOBA → Setup → Create All Scenes**
- 或手动创建场景并保存在 `Assets/Scenes/` 文件夹中

---

## 性能优化建议

- 目标帧率：30 FPS（移动端）
- 质量设置：Medium（中等）
- 建议在真实 Android 设备上测试性能

---

## 下一步开发建议

1. **美术资源**: 用实际 3D 模型替换 Primitive 占位
2. **网络同步**: 实现权威服务器架构
3. **匹配系统**: 真实的匹配算法
4. **账号系统**: 正式的身份验证系统
5. **高级 AI**: 更智能的 Bot 玩家
6. **特效**: VFX、SFX、动画
7. **平衡性**: 英雄和装备平衡调整

---

## 项目结构

```
MobaGame/
├── README.md                    # 项目说明（英文）
├── QUICKSTART.md               # 本快速开始指南
├── UnityClient/                # Unity 客户端项目
│   ├── Assets/
│   │   ├── Scenes/            # 游戏场景
│   │   ├── Scripts/           # C# 脚本
│   │   │   ├── Core/         # 核心系统
│   │   │   ├── Data/         # 数据定义
│   │   │   ├── Gameplay/     # 游戏玩法
│   │   │   ├── Network/      # 网络通信
│   │   │   └── UI/           # 用户界面
│   │   ├── Prefabs/          # 预制体
│   │   └── Resources/        # 资源文件
│   ├── ProjectSettings/       # Unity 项目设置
│   └── Packages/             # Unity 包管理
└── DotNetServer/             # .NET 服务器
    └── MobaServer/           # 服务器控制台应用
        └── Program.cs        # 服务器主程序
```

---

## 技术栈

- **Unity**: 2022.3 LTS
- **C#**: 游戏逻辑和服务器
- **.NET**: 8.0
- **网络**: TCP Socket
- **目标平台**: Android (API 22+)

---

## 联系方式

如有问题或建议，请在项目仓库提交 Issue。

## 许可证

本项目为 MVP 演示原型。
