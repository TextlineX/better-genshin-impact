# BetterGI · TextlineX 独立适配版

<div align="center">
  <a href="https://github.com/TextlineX/better-genshin-impact/releases">
    <img alt="Release" src="https://img.shields.io/github/v/release/TextlineX/better-genshin-impact?logo=github&style=flat-square&color=1E9BFA">
  </a>
  <a href="https://github.com/TextlineX/better-genshin-impact/releases">
    <img alt="Downloads" src="https://img.shields.io/github/downloads/TextlineX/better-genshin-impact/total?logo=github&style=flat-square&color=1E9BFA">
  </a>
  <a href="https://github.com/TextlineX/better-genshin-impact/actions/workflows/publish.yml">
    <img alt="Build and release" src="https://github.com/TextlineX/better-genshin-impact/actions/workflows/publish.yml/badge.svg">
  </a>
</div>

BetterGI 是一个通过计算机视觉和模拟输入辅助《原神》的开源项目。本仓库由 [TextlineX](https://github.com/TextlineX) 独立维护，当前版本为 0.66.2-alpha.3。

## 这个仓库解决什么问题

原神切换到控制器模式后，拾取提示、地图、角色 HUD、对话选项和菜单焦点都会改变；用户还可能修改游戏内按键。简单把键盘按键替换成手柄按键，不能保证识别和操作链路正确。因此本仓库围绕以下内容做独立适配：

- 原生支持 16:9 和 16:10 游戏画面：1920x1080、2560x1440、1920x1200、2560x1600。
- 键鼠模式与控制器模式分开选择，保留原有 Win32 键鼠输入。
- 控制器模式下的拾取提示、对话选项、地图和基础菜单焦点识别。
- Xbox/XInput 风格的虚拟 Xbox 360 手柄输出，以及可配置的动作映射。
- 自动拾取、自动剧情基础确认和基础焦点导航的控制器调用链。

本仓库基于原作者的 [babalae/better-genshin-impact](https://github.com/babalae/better-genshin-impact) 开发。上游项目是原始功能基线；本仓库的代码、Issue、分支和 Release 单独管理，不向上游推送、创建 PR 或执行合并。需要上游原始文档时仍可参考 [bettergi.com](https://www.bettergi.com/)。

## 当前状态

16:9 的原有功能继续保留，16:10 坐标映射和控制器模式基础识别已接入。自动战斗、自动寻路、钓鱼、七圣召唤、复杂菜单以及所有任务在控制器模式下的完整覆盖仍需按页面补充模板和实机验证。单元测试通过只代表代码路径通过，不能代替在你的游戏版本、分辨率、DPI 和自定义按键下运行验证。

你修改了原神按键时，不需要改回默认按键。BetterGI 的控制器映射位于“设置 → 按键设置”，它映射的是 BetterGI 动作到手柄控制，不读取键盘物理键码；游戏内的自定义按键不会自动生成新的识别素材。控制器模式下，必须在原神中选择“控制器”，在 BetterGI 主界面选择“手柄”输入模式。

## 下载与自动发布

正式构建只从本仓库的 [GitHub Releases](https://github.com/TextlineX/better-genshin-impact/releases) 下载。每次向本仓库推送形如 vX.Y.Z 的 tag，.github/workflows/publish.yml 会在 Windows runner 上：

1. 校验 tag 与 BetterGenshinImpact.csproj 的版本号一致。
2. 编译控制器输入和识别 Profile 的回归测试。
3. 发布自包含的 Windows x64 压缩包。
4. 自动创建对应的 GitHub Release；带预发布标识的版本会自动标为 Pre-release。

当前示例：v0.66.2-alpha.3。也可以在 GitHub Actions 手动选择一个已经存在的版本 tag 重新构建。旧的上游发布脚本曾依赖上游仓库和外部发布服务，已不再作为本仓库的发布入口。

## 运行要求

- Windows 10 或更高版本，64 位。
- 发布包已包含 .NET 8 运行时；源码运行或框架依赖包需要安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)。
- 控制器模式需要安装并正常运行 [ViGEmBus](https://github.com/ViGEm/ViGEmBus) 驱动。BetterGI 使用 Nefarius.ViGEm.Client 创建虚拟 Xbox 360 控制器，XInput 本身只能读取真实手柄，不能直接发送虚拟输入。
- 游戏以管理员权限运行时，BetterGI 也可能需要以管理员权限运行。

BetterGI 只使用截图识别和模拟输入，不修改原神游戏文件、不读取游戏内存，也不绕过反作弊机制。模拟输入可能被安全软件拦截，请按你的环境加入允许列表；使用第三方模拟输入仍存在被游戏识别的风险。

## 使用提示

1. 启动 BetterGI，在主界面选择输入模式：键鼠或手柄。
2. 使用手柄模式时，在原神设置中选择“控制器”，并确认 ViGEmBus 已安装。
3. 进入游戏后再启动需要的实时任务；切换分辨率、窗口、显示器或输入模式后建议重启 BetterGI。
4. 当前支持的参考分辨率为 1920x1080、2560x1440、1920x1200、2560x1600。请关闭 HDR、滤镜等会改变画面颜色和布局的效果。
5. 自动剧情和自动拾取在控制器模式下仍应先用当前页面实测；遇到未识别页面，请保留截图、分辨率、DPI、游戏输入模式和日志后提交 Issue。

## 主要功能

原有功能包括自动拾取、自动剧情、自动邀约、快速传送、半自动/全自动钓鱼、自动伐木、自动秘境、自动地脉花、自动音游、自动烹饪、圣遗物分解、键鼠录制和多种辅助操作。详细说明请参考 [BetterGI 文档](https://www.bettergi.com/doc.html)。控制器模式的适配范围以本 README 的“当前状态”和 Release 说明为准。

## 本地开发

需要 .NET 8 SDK、Windows 10 SDK 10.0.22621.0 或更高版本，以及支持 WPF 的 Visual Studio 2022 或 Rider。

    dotnet restore BetterGenshinImpact.sln
    dotnet build BetterGenshinImpact/BetterGenshinImpact.csproj -c Debug -p:Platform=x64
    dotnet test Test/BetterGenshinImpact.UnitTest/BetterGenshinImpact.UnitTest.csproj -c Debug --filter "FullyQualifiedName~GamepadInputBackendTests|FullyQualifiedName~RecognitionProfileResolverTests"

完整测试项目中仍有历史识别测试会导致测试宿主异常退出，发布工作流只执行当前控制器输入和识别 Profile 的回归测试；这不等同于完整测试套件全部通过。

## 反馈

请在本仓库提交 [Issue](https://github.com/TextlineX/better-genshin-impact/issues)。反馈控制器界面问题时，请附上实际截图和：分辨率、DPI、窗口/全屏状态、原神输入模式、是否修改过按键、BetterGI 日志以及触发的功能。上游项目的问题请提交到 [上游 Issue](https://github.com/babalae/better-genshin-impact/issues)。

## 致谢

本项目保留上游 BetterGI 的版权声明、GPL-3.0 许可证和致谢信息。感谢以下项目和贡献者：

- [babalae/better-genshin-impact](https://github.com/babalae/better-genshin-impact)
- [Yap](https://github.com/Alex-Beng/Yap)
- [genshin-woodmen](https://github.com/genshin-matrix/genshin-woodmen)
- [Fischless](https://github.com/genshin-matrix/Fischless)
- [MicaSetup](https://github.com/lemutec/MicaSetup)
- [cvAutoTrack](https://github.com/GengGode/cvAutoTrack)
- [genshin_impact_assistant](https://github.com/infstellar/genshin_impact_assistant)
- [HutaoFisher](https://github.com/myHuTao-qwq/HutaoFisher)
- [minimap](https://github.com/tignioj/minimap)
- [kachina-installer](https://github.com/YuehaiTeam/kachina-installer)

特别感谢 [@Lightczx](https://github.com/Lightczx) 和 [@emako](https://github.com/emako) 对上游项目的指导与贡献。

## 许可证

本项目遵循 [GNU General Public License v3.0](./LICENSE)。
