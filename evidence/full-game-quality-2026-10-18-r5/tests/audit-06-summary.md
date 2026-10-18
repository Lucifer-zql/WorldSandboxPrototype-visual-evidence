# audit-19 六条全流程审计用例摘要（真实 PlayMode 运行）

来源：`tests/r5-final-full-playmode.xml`（全量 PlayMode，116 用例 / 115 passed / 0 failed / 1 skipped，568.8s）。
截图由 `Assets/Tests/PlayMode/PlaythroughAuditTests.cs` 在真实运行中捕获到 `WS_AUDIT_SHOT_DIR`，
本批 33 张 PNG 全部出自下列 6 条用例（捕获目录 `screenshots/`）。

| # | 用例（full name） | 结果 | 耗时(s) | 产出帧 | 帧号 |
| --- | --- | --- | --- | --- | --- |
| 1 | `WorldSandbox.Tests.PlayMode.PlaythroughAuditTests.Audit_01_MainMenu_NewWorld_FirstLook` | Passed | 1.634 | `01-main-menu-zh.png`、`02-new-world-default-zh.png` | 01–02 |
| 2 | `...PlaythroughAuditTests.Audit_02_TimeProgression` | Passed | 1.451 | `03-world-overview-t0` … `08-world-overview-t1200` | 03–08 |
| 3 | `...PlaythroughAuditTests.Audit_03_Tools` | Passed | 1.304 | `09-tool-water` … `13-tool-remove` | 09–13 |
| 4 | `...PlaythroughAuditTests.Audit_04_UiFlow` | Passed | 2.406 | `14-civ-panel-zh` … `21-civ-panel-1280x720` | 14–21 |
| 5 | `...PlaythroughAuditTests.Audit_05_SaveLoadAndWorldSwitch` | Passed | 3.100 | `22-save-toast-zh` … `27-switch-back-first-world` | 22–27 |
| 6 | `...PlaythroughAuditTests.Audit_06_ToolbarSubRow` | Passed | 1.347 | `28-toolbar-collapsed` … `33-toolbar-terrain-reopen` | 28–33 |

合计 **6/6 Passed**，产出 **33 帧**。

## 帧 → 用例 归属依据

`PlaythroughAuditTests.cs` 内各用例方法起始行与实际写帧调用行一一对应（行号为源码事实）：

| 用例 | 方法起始行 | 帧写入行 |
| --- | --- | --- |
| `Audit_01` | L334 | L341、L345 |
| `Audit_02` | L376 | L359、L363、L368、L398、L420、L425 |
| `Audit_03` | L433 | L467、L480、L490、L506、L531 |
| `Audit_04` | L539 | L554、L561、L566、L570、L581、L583、L588、L591 |
| `Audit_05` | L604 | L625、L632、L638、L656、L674、L685 |
| `Audit_06` | L693 | L709、L729、L744、L791、L803、L818 |

（注意 `Audit_06` 内 L818 写 `32-toolbar-remove.png` 位于 L803 之后，故第 32 帧在文件序中靠后生成，与帧号顺序不同。）

## 世界状态对照（`screenshots/metrics.log`）

同一次运行导出的模拟快照，可作为帧内容的事实对照（tick / pop / creatures / buildings / civs / trees）：

| 快照点 | tick | pop | creatures | buildings | civs | trees |
| --- | --- | --- | --- | --- | --- | --- |
| `[world-t0-overview]` | 25 | 12 | 16 | 4 | 4 | 153 |
| `[t-start]` | 7 | 9 | 13 | 4 | 4 | 153 |
| `[t+300]` | 309 | 32 | 36 | 16 | 4 | 133 |
| `[t+1200]` | 1214 | 55 | 59 | 20 | 4 | 73 |
| `[world-a-before-save]` | 263 | 24 | 28 | 9 | 3 | 141 |
| `[world-a-loaded]` | 263 | 24 | 28 | 9 | 3 | 141 |
| `[world-b]` | 62 | 8 | 12 | 4 | 2 | 182 |
| `[world-a-switched-back]` | 266 | 24 | 28 | 9 | 3 | 141 |

存档/读档一致性（关键不变量）：`world-a-before-save` 与 `world-a-loaded` 的 tick / pop / creatures / settlements / nodes / buildings / civs / trees 全部相同；
切回第一世界后 tick 由 263 → 266（真实推进 3 tick），其余计数不变，符合「读档不跳变、切世界保留」的预期。

## 未通过项

无。本批 6 条用例全部 Passed；全量 PlayMode 唯一 skipped 为 `MapVisualOverhaulCaptureTests.MVO_TPR3_StateContrast_SameFraming`
（要求 `tpr3-*` 捕获模式，本轮 `tri-playmode-full`，属模式约束，未计通过）。
