# 测试原始输出（D-068 r3）

本目录是 2026-10-17 r3 批次实际执行产生的 XML/日志，未编辑测试结果。
被测工作树：`D:\data\UnityProjects\WorldSandboxPrototype`，Unity **6000.6.5f1**，主仓库**未提交、未推送**。

## 一览

| 文件 | 命令 | 实测结果 |
| --- | --- | --- |
| `editmode-full.xml` | `unity test . --mode EditMode` | **417/417 通过** |
| `playmode-full.xml` | `unity test . --mode PlayMode` | **109 总：37 通过 / 72 失败**，逐条分类见 `playmode-full-failures.md` |
| `d068-ui.xml` | `--filter WorldSandbox.Tests.PlayMode.D068FinalUiTests` | **3/3 通过** |
| `d068-capture.xml` | 设置 `D068_FINAL_SHOT_DIR` 后运行 `D068FinalCaptureTests` | **4/4 通过，26/26 PNG 落盘** |
| `d068-character.xml` | `--filter WorldSandbox.Tests.PlayMode.D068CharacterRenderTests` | **3/3 通过**（含移动不穿水/崖/建筑） |
| `d068-life-editmode.xml` | `D068FinalLifeTests` | 5 个 EditMode 生命周期用例，包含在 417 全量中 |
| `hud-targeted.xml` | `HudEqualsSnapshot` | 修复后 **1/1 通过**；全量 XML 中该用例也已通过 |
| `hardcoded-ui-test.xml` | `NoHardcodedChineseInUIScripts` | **1/1 通过**（默认世界名改为常量后） |
| `d067-full.xml` | 设置 `D067_SHOT_DIR` 后运行 `D067` | **8/11 通过**；逐条说明见下 |
| `d067-english-capture-individual.xml` | 单独运行 `D067_CaptureEnglishScreens` | 0/1；8 张截图已写出，收尾按旧控件名 `Btn_Done` 取不到按钮 |
| `d067-english-flow-individual.xml` | 单独运行 `R3_EnglishFlow...` | **1/1 通过** |

## D-067 的 8/11 说明（不伪报）

1. `D067_CaptureEnglishScreens`：**产品英文流程与截图正常**；测试在写完 `30-en-main-menu` 到 `38-en-settings` 共 8 张后，仍按旧控件名 `Btn_Done` 关闭设置页。当前设置页关闭按钮 GameObject 名为本地化标签（`Btn_完成`），因此测试在最后一行失败。归因为**旧测试选择器漂移**，未改旧测试、未改产品。
2. `R3_EnglishFlow_ToastAndHud_UseCurrentLanguage`：**单独运行 1/1 通过**；全量批次中前一用例留下的场景未在 3000 帧内就绪，取到 null。归因为**测试顺序隔离**。
3. `D067_FullFlow_CreateSaveExitLoadContinue`：640×480 `ScreenCapture` 在 `-batchmode` 不落盘；D-067 §8 已登记，未伪造通过。

因此：**同批 8/11**；把顺序相关的 `R3_EnglishFlow` 单独运行后为 **9/11**；剩余 `D067_CaptureEnglishScreens` 是选择器漂移、`D067_FullFlow` 是批处理 ScreenCapture 环境阻塞。没有把任何失败写成通过。

## 全量 PlayMode 72 失败的口径

- **33** 个历史取证用例被 `CaptureWriteGuard` 失败关闭（写死 `docs/impl/evidence/...` 或未设置 `MVO_CAPTURE_MODE`）。
- **9** 个 D-067/D-068 截图用例因未设置对应 `*_SHOT_DIR` 在 OneTimeSetUp 拒绝（这是设计好的环境前置）。
- **28** 个旧用例依赖前置用例装配 `WireWorld/MapReader/SimClock/CurrentPlan`，全量顺序下为 null。
- **1** 个 `R3_EnglishFlow` 顺序隔离（单独通过）。
- **1** 个 `F_BuildingsFx` 旧场景状态断言（期望 4 建筑、实际 0；未单独复现产品建筑缺失，**待逐条复核，不计入通过**）。
- 没有“其他”类；如果后续新增失败，必须重新分类。

## 命令与环境

```powershell
# EditMode 全量
unity test . --mode EditMode --output Logs/d068-r3-editmode.xml

# 全量 PlayMode（不设截图目录，故意保留环境前置失败）
unity test . --mode PlayMode --output Logs/d068-r3-playmode-all.xml

# D-068 最终截图（唯一写入 r3 截图的命令）
$env:D068_FINAL_SHOT_DIR='D:\data\UnityProjects\WorldSandboxPrototype-visual-evidence\evidence\full-game-quality-2026-10-17-r3\screenshots'
unity test . --mode PlayMode --filter WorldSandbox.Tests.PlayMode.D068FinalCaptureTests --output Logs/d068-r3-capture-playmode.xml
```

> `unity test` 在全部通过时也可能返回 shell exit code 1/2；**以 XML 的 result 属性为准**，本目录保留原始 XML。

## 结果解释边界

- 这些结果达到 `G4 测试通过` 的**局部证据**，不是 `G5 游戏实际运行通过`，更不是 `G9 用户验收通过`。
- 主仓库没有提交；本目录所在证据仓库只新增 r3 批次。
