# D-068 剩余项 r4 收官证据（2026-10-18）

本批是主仓库 `D:\data\UnityProjects\WorldSandboxPrototype` 工作区（HEAD `f85fbe3`，未提交）在 Unity 6000.6.5f1 / Windows 上的**真实运行证据**。主仓库**未提交、未推送**；本证据仓库单独提交。

## 1. 测试基线（真实 XML）

`tests/baseline-full-playmode.xml`：全量 PlayMode 110 个用例，**101 passed / 8 failed / 1 skipped**（duration 560.5s）。8 条失败逐条见 `tests/baseline-failures.txt`；本批没有把基线写成“全部通过”。

基线使用唯一临时捕获根（`%TEMP%\ws-playmode-capture-20261018-074739-8c246e`），未写历史证据树；`CaptureWriteGuard`/路径守卫保持启用，`WS_ALLOW_LEGACY_CAPTURE=1` 仅授权到唯一临时目录。

### 1.1 修复后最终全量（权威结果）

- **PlayMode（最终树）**：`tests/final-full-playmode.xml` **109 passed / 0 failed / 1 skipped**（110 total，563.5s，00:21:49–00:31:13Z）。
- **PlayMode（前一版）**：`tests/final-full-playmode-first.xml` 同样 **109/0/1**（551.8s）；该次之后仅补了未参与运行时的 `LocalizationKeys` 8 个常量，最终树已由上一行重跑覆盖。
- **EditMode**：`tests/final-full-editmode.xml` **417 passed / 0 failed / 0 skipped**（67.1s）。
- 唯一 skipped：`MapVisualOverhaulCaptureTests.MVO_TPR3_StateContrast_SameFraming`，要求 `tpr3-*` 模式而本轮 `tri-playmode-full`，属模式约束，不是失败。
- EditMode 首跑为 416/417：`LocalizationKeyTests.AllKeysHaveBothLanguages` 发现 8 个 `civ.name.N` 文案表键缺少 `LocalizationKeys` 常量（表键孤儿）；补 8 个稳定常量后定向 `tests/localization-targeted.xml` 2/2、全量 417/417。

### 1.2 基线 8 失败的真实归因与修复

| # | 用例 | 真实归因 | 修复 |
|---|---|---|---|
| 1 | `FinalIntegrationTests.FullStack...` | 期望 6 个阶段系统；D-068 新增 `CivilizationSystem` 后真实 7 | 断言改 7，注明 TimeAdvance→Civilization→Snapshot |
| 2 | `Iter11GameCaptureTests.Iter11_GameTerrainShots` | 仍写历史取证目录，被 `CapturePathGuard` 以文件已存在拒绝 | `AuthorizeLegacyOutDir` 重定向唯一临时根，守卫不关闭 |
| 3 | `LifeAnimCoverageTests.RunAndSheep...` | `Clock.Advance(1f)` 触发完整模拟，繁衍/移动改写状态 | 暂停自动 tick，改用 `SnapshotSource.Tick` |
| 4 | `LifeViewIntegrationTests...` | 新世界自带 12 个成员，测试仍期望 0 视图 | 清空/复位/`ClearAll` 后重绑 |
| 5 | `NineSliceEvidenceTests...` | 主菜单未建世界致 `UIRoot.IsBound=false`；640×480 无帧缓冲 | `EnsureWorld` + 1280×720 `SetGameViewSize` |
| 6 | `ToolInputChainTests.LeftClick...` | 快照基线未 tick（0），点击后真实 13；完整 tick 还会繁衍 | 计数前刷新快照，暂停自动 tick，只验证 +1 链 |
| 7 | `UiAssetsEvidenceTests.DetailCard...` | id=1 取到初始文明成员 `Tool_04`，非 Axe | 清空/复位 ID，再生成 10 个展示单位 |
| 8 | `VisualVerificationTests.E_CreaturesAnimation` | `ClearAll` 解绑 provider 后未重绑 | ClearAll 后用帧集库 Bind+Sync，并暂停自动 tick |

8 条均为**测试夹具/顺序假设**，不是生产逻辑缺陷；修复后全量重跑为 109/110。`F_BuildingsFx` 基线为 0 建筑的根因同样是测试未显式建世界；`PlayModeTestWorld.EnsureWorld` 后真实放置 4 座（Castle + 3 House），最终全量通过。

## 2. 截图（真实运行，非重绘）

- `screenshots/d068/`：D-068 r3 最终 26 张（1600×900 / 1280×720），世界相机 + ScreenSpaceCamera Canvas 合成 RenderTexture。
- `screenshots/d067/`：D-067 全流程 zh/en 33 张（主菜单、新世界、校验、HUD、暂停、保存/覆盖、读档、第二世界、多分辨率、设置）。
- `screenshots/buildings/`：F1 4 建筑、F2 Explosion+Fire 2 张；真实 `Main.unity` + `PlaceBuilding`/事件链。
- `screenshots/terrain/`：MVO/TS 的 TS8a 两级台、TS8f 坡对、TS9f 两级台南缘 3 张。

合计 **64 PNG**。所有 PNG 均由 PlayMode 真实相机/屏幕捕获产生；未用静态图、AI 生成或另建演示场景替代。

## 3. 地形裁定（D-068 r4）

1. **坡面色族 = 坡脚低地层**：`TerrainTilemapRenderer.LayOverlays` 使用 `FamilyFor(Grass, s.PanelLevel - 1)`；Hill 坡取低地 color1，Mount 坡取 Hill 层 color2。规范 §D.5 与代码/测试一致，旧“高地面板”表述作废。
2. **阴影 ×1.6**：本地 Shadow.png 可见约 80px，guide 128px；`FastNoiseRenderIntegrationTests` 与 `ShadowPlacementGuideTests` 锁定 `lossyScale=1.6`，不再保留“1:1”。
3. **1 行宽台面保留**：属 D-061 噪声高程设计，不新增宽度规则。
4. **162/190 为历史预留**：`WaterEdgeOverlay` / `StairsTilemap` 在渲染器中只有建层、清空/重置；没有正向 `SetTile`。排序契约保留，不声称“已实现”。

## 4. 关键文件与复现

- `tests/baseline-full-playmode.xml`、`tests/baseline-failures.txt`：基线 101/8/1 与 8 条失败原文。
- `tests/final-full-playmode.xml`（最终树重跑，563.5s）、`tests/final-full-playmode-first.xml`（前一版 551.8s）、`tests/final-full-editmode.xml`：最终 109/0/1、417/0/0。
- `tests/localization-targeted.xml` 与 `tests/targeted-*.xml`：关键定向回归（D-067、D-068、8 条失败类）。
- `logs/`：Unity CLI 控制台输出（完整断言/堆栈以 XML 为准）。
- 复现：`unity test . --mode PlayMode --output Logs/r4-playmode-final.xml`；`unity test . --mode EditMode --output Logs/r4-editmode-final.xml`。
- 主仓库失败分类与修复细节见 `docs/impl/D-068-文明详情与剩余交付.md` §8；决策见 `docs/DECISIONS.md` r4。

## 5. 限制与口径

- 本批为**待 ChatGPT 独立验收**的材料，不是自我宣布 PASS/G9。
- 基线 8 失败保留在 `tests/baseline-*` 作为对照；最终以修复后全量 XML 为准，不用定向 XML 替代全量。
- 唯一 skip 是 `MVO_TPR3` 的模式约束；没有把它算作通过。
- 证据仓库提交哈希与远端 SHA-256 见顶层 `SHA256SUMS.txt` / 提交记录。
