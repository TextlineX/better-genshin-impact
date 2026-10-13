# 16:10 与原生手柄适配审计记录

## 已确认的基础事实

- `dev/16x10-gamepad` 已包含自动拾取交互 ROI 的中心锚定修复；本次保留该行，没有覆盖。
- 识别资源通过 `RecognitionObjectJsonLoader` 解析 ROI，截图通常先由 `GameCaptureRegion.DeriveTo1080P()` 统一缩放。
- 输入统一经过 `InputHub`、`IInputBackend`、`IInputChannel`；原有 Win32 前台/后台通道保持不变。
- ViGEm/Xbox 360 后端只能输出虚拟手柄状态，不能读取或发送 XInput 输出；运行时必须安装 ViGEmBus 驱动。

## 本次落地范围

- 增加 1920x1080 参考画布到当前截图的等比映射，支持左上、右上、左下、右下、中心锚点和边界裁剪。
- Recognition JSON 增加 `anchorTopLeft`、`anchorTopRight`、`anchorBottomLeft`、`anchorBottomRight`、`anchorCenter`。
- 更新已确认属于中心、右上或右下布局的自动吃药、秘境、开宝箱、音游和快捷购买 ROI；自动拾取现有提交行保持不变。
- 增加 ViGEm Xbox 360 虚拟手柄通道、动作映射、摇杆/扳机、保持与释放、异常停止清理，以及可注入的测试设备。
- 增加键鼠与手柄 Recognition profile 扩展点。若没有真实手柄素材，自动回退到原键鼠素材，不生成伪模板。
- 自动拾取、剧情确认和剧情菜单关闭已优先改为动作/焦点导航调用。

## 尚需实机材料

- 手柄模式下的拾取提示、对话选项、确认/取消、菜单焦点截图，至少需要 1920x1200 和 2560x1600 各一套，截图中应包含完整游戏画面和当前 UI 提示。
- 真实 Windows 环境中安装 ViGEmBus 后的虚拟手柄枚举、原神控制器模式识别，以及真实手柄与虚拟手柄同时连接时的设备选择结果。
- 自动战斗、寻路、钓鱼、七圣召唤等任务的控制器模式截图和操作录像；当前没有据此新增模板或声称已验证。
