# world-lifecycle-ui-2026-10-17（D-067 证据）

> 批次：**D-067 世界管理基础设施 + 存档 + UI/UX 重构**
> 日期：2026-10-17｜Unity `6000.6.5f1` / URP 2D｜种子 20261013（流程用例 20261013 / 424242）
> 实现文档：`docs/impl/D-067-世界生命周期与UI重构.md`（游戏主仓库，未提交）

## 目录

| 路径 | 内容 |
| --- | --- |
| `shots/` | 16 张真实 Play Mode 截图（主菜单/新建世界/校验错误/HUD/暂停/保存/覆盖确认/读档列表/读档后/第二世界/多分辨率/设置） |
| `tests/editmode-full.xml` | EditMode 全量：**394 / 394 通过**（含 D-067 新增 21 例） |
| `tests/editmode-lifecycle.xml` | 仅 `WorldLifecycleTests`：21 / 21 |
| `tests/playmode-flow.xml` | PlayMode 流程：**3 / 3 通过**（创建→运行→保存→退出→读档→继续→第二世界） |
| `tests/playmode-capture.xml` | PlayMode 截图用例：1 / 1（生成 `shots/`） |
| `tests/playmode-legacy-regression.xml` | 遗留全量（`WS_AUTOSTART_WORLD=1`）：78 例 / 23 通过 / 51 失败（全部为取证套件环境守卫，D-066 已分类）/ 4 跳过（D-067 用例在 autostart 模式不适用） |

## 截图清单

| 文件 | 状态 |
| --- | --- |
| `10-main-menu-empty.png` | 空存档主菜单，「继续游戏」禁用 |
| `11-new-world-config.png` | 192×192 / 种子 20261017 / 6 文明 |
| `12-validation-error.png` | 名称为空 → 表单内错误提示，未创建 |
| `13-in-game-hud.png` | 进入世界：地图 + HUD + 世界信息条 |
| `14-pause-menu.png` | 暂停菜单（真实停表） |
| `15-save-toast.png` | 保存成功 Toast |
| `16-overwrite-confirm.png` | 覆盖存档确认框 |
| `17-main-menu-with-save.png` | 有存档主菜单（继续游戏可点） |
| `18-load-list.png` | 存档列表 + 选中详情 |
| `19-loaded-world.png` | 读档后世界（同一世界 Id，tick 恢复） |
| `20-second-world-64.png` | 第二世界 64×64 / 2 文明（无旧世界泄漏） |
| `21-hud-1920x1080.png` / `22-hud-2560x1440.png` | 多分辨率 HUD |
| `23-main-menu-1920x1080.png` / `24-main-menu-2560x1440.png` | 多分辨率主菜单 |
| `25-settings.png` | 设置界面 |

## 截图方法（真实运行，非静态图）

`-batchmode` 下没有 GameView 渲染目标：`ScreenCapture.CaptureScreenshot` 不落盘、`Camera.Render()` 在 URP 崩溃
（实测栈 `Camera::StandaloneRender`）。因此 `D067CaptureTests` 在真实 Play Mode 中把 **MainCanvas 临时切为
ScreenSpaceCamera（worldCamera = 主相机）**，主相机 `targetTexture` 指向 RenderTexture，等 2 帧后
`ReadPixels` 编码 PNG，再恢复原渲染设置。截到的是当前运行中的世界与真实 uGUI 层级。

## 复现命令

```powershell
# EditMode 全量
Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform EditMode -testResults editmode-full.xml -logFile editmode-full.log

# PlayMode 流程（必须不设 WS_AUTOSTART_WORLD）
Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform PlayMode -testFilter D067WorldFlowTests -testResults playmode-flow.xml -logFile playmode-flow.log

# PlayMode 截图（D067_SHOT_DIR 指向本目录 shots/）
$env:D067_SHOT_DIR='docs/impl/evidence/world-lifecycle-ui-2026-10-17/shots'
Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform PlayMode -testFilter D067CaptureTests -testResults playmode-capture.xml -logFile playmode-capture.log

# 遗留回归（假定加载即有世界）
$env:WS_AUTOSTART_WORLD='1'; $env:WS_WORLD_SEED='20261013'
Unity.exe -batchmode -projectPath <repo> -runTests -testPlatform PlayMode -testResults playmode-legacy-regression.xml -logFile playmode-legacy-regression.log
```

## 安全

- 存档测试根目录 = 系统临时目录（`WS_SAVE_DIR`），玩家 `persistentDataPath` 未被读写。
- 截图目标文件已存在即失败（`D067CaptureTests` 断言 `File.Exists(abs) == false`），不覆盖历史证据。
- 未使用 `WS_ALLOW_LEGACY_CAPTURE`，未写入任何历史证据目录。
