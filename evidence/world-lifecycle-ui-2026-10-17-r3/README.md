# D-067-R3 证据：全流程 UI 中英文本地化

## 本轮内容

| 项 | 结果 |
| --- | --- |
| EditMode 全量 | 397 / 397 通过（0 失败 / 0 跳过） |
| `D067R3LocalizationTests` | 2 / 2 通过 |
| `D067WorldFlowTests` | 3 / 3 通过 |
| `D067R1TerrainRebuildTests` | 2 / 2 通过 |
| `D067R2ToastLayoutTests` | 2 / 2 通过 |
| `D067CaptureTests` | 2 / 2 通过（本目录 `shots/` 25 张，中英各一套） |

## 本地化接线（生产代码）

- 语言唯一状态源：`UIRoot.Localization`（`ILocalization`）；`UIRoot.PrepareLocalization()` 使主菜单阶段即可切换。
- `ScreenFlowUI` 全部可见文本走 `T(key)` / `F(key,args)`：主菜单、新建世界（字段/占位/尺寸/文明/摘要）、
  存档列表（行/详情/空态/损坏/分页）、暂停、设置、加载、Toast、确认框；`RefreshLanguage()` 重建当前界面并保留
  草稿（名称/种子/尺寸/文明）、选中存档、页码、暂停摘要。
- `GameFlowController` 的 Toast/确认框/加载/错误文案与 `GameBootstrap` 的 HUD 世界信息条同源（`D067Strings`）。
- `WorldCreationValidator.Validate(config, language)`：校验错误双语，默认重载保持中文兼容。
- `LocalizationKeyTests` 的 D-067 豁免已删除；新增 `D067LocalizationTests`（中英键一一对应、英文不含 CJK、
  校验双语、尺寸标签）；语言自称项（`d067.settings.zh` / `d067.settings.en`）按"母语显示"精确豁免。

## 测试修复（本轮）

1. **用例级语言隔离**：两条 PlayMode 用例开头显式 `PlayerPrefs` 置 0，结束复原；不再依赖 NUnit 执行顺序。
2. **场景重载引用失效**：重启恢复段改用新的 `GameBootstrap` 实例（`reboot`），旧实例不再访问。

## 截图

`shots/10-*` … `25-*`（中文，16 张）；`shots/30-en-main-menu.png` … `38-en-settings.png`（英文，9 张）。
全部来自真实 Play Mode：世界相机 + 真实 uGUI Canvas 合成到 RenderTexture 后 ReadPixels。

## 已知限制

- 存档内的世界名与文明名是用户数据，不翻译。
- 分辨率/窗口模式文案为项目既有三档，未新增档位。
