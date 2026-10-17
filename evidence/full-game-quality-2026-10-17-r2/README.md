# full-game-quality-2026-10-17-r2（D-068 剩余交付 · 真实运行证据）

> 本批次是 `full-game-quality-2026-10-17` 的**补充批次**（r2）。上一批只有测试报告、没有截图；
> 本批全部截图均为 **2026-10-17 本轮真实 Unity 运行**（真实世界相机 + 真实 UI Canvas 合成到 RenderTexture，
> 非 AI 图片、非手绘拼图、非历史截图复用），每张图片的 SHA-256 见 `SHA256SUMS.txt`。

## 1. 环境

| 项 | 值 |
| --- | --- |
| Unity | 6000.6.5f1（D:\software\Unity Hub\Editor\6000.6.5f1\Editor\Unity.exe） |
| 平台 | Windows（batchmode 播放模式测试 + RenderTexture 捕获） |
| 项目 HEAD | `f85fbe3` + 本轮工作区修改（主仓库未提交、未推送） |
| 取证方式 | PlayMode 测试 `D068EvidenceCaptureTests`（D068_SHOT_DIR 输出目录，已存在同名文件即失败） |
| 存档隔离 | WS_SAVE_DIR 指向系统临时目录，不触碰玩家存档 |

## 2. 截图（screenshots/）

| 文件 | 内容 | 验证点 |
| --- | --- | --- |
| 01-main-menu-zh.png | 主菜单（中文） | 真实 UI、无世界 |
| 02-new-world-form-zh.png | 新建世界表单（中文） | 尺寸/文明数/种子控件 |
| 03-world-fullmap-zh.png | 128×128 世界全图（seed 20261013，4 文明，900 tick） | 真实随机地形与聚落 |
| 04-civ-panel-zh.png | 文明详情面板（中文） | 名称/ID/人口/食物/木材/黄金/建筑/决策/更新时间/出生死亡/外交 |
| 05-civ-panel-second-zh.png | 切换到第二个文明 | 切换文明后全部数值刷新 |
| 06-civ-panel-paused-zh.png | 面板暂停模拟 | 按钮变“恢复模拟”，时钟=0 |
| 07-event-log-zh.png | 事件日志（中文） | 真实事件（出生/建造/饥荒等） |
| 08-pause-summary-zh.png | 暂停摘要（中文） | 世界名/尺寸/种子/文明数/外交统计 |
| 09-main-menu-en.png | 主菜单（English） | 语言切换后无中文残留 |
| 10-world-fullmap-en.png | 世界全图（English，seed 20261014） | HUD 全英文 |
| 11-civ-panel-en.png | 文明详情面板（English） | 标签/决策/关系全英文 |
| 12-event-log-en.png | 事件日志（English） | 事件文案英文 |
| 13-pause-summary-en.png | 暂停摘要（English） | 无中文残留 |
| 14-terrain-seed-20261013.png | 种子 20261013 全图 | 高地/崖壁/海岸/树木 |
| 14-terrain-seed-20261014.png | 种子 20261014 全图 | 同上，布局不同 |
| 14-terrain-seed-20261015.png | 种子 20261015 全图 | 同上，布局不同 |

> 说明：布局为程序化随机（三种子地图不同）；同一批代码的 `EditMode` 全量 412/412 与地形渲染扫描测试共同锁定
> 选片/锚点/层序规则，截图只作真实运行佐证，不能替代规则测试。

## 3. 本轮测试结果（tests/）

| 套件 | 结果 | 报告 |
| --- | --- | --- |
| 全量 EditMode | **412 / 412 通过，0 失败，0 跳过** | `editmode-full.xml` / `.log` |
| D-068 PlayMode（自动存档生命周期 5 + 文明面板 4 + 事件日志 1 + 世界切换/读档 1 + 截图 3） | **14 / 14 通过** | `playmode-d068-full.xml` |
| D-068 截图套件 | 3 / 3 通过（16 张 PNG 全部落盘） | `playmode-d068-capture.xml` |
| D-067 PlayMode 回归（世界生命周期/流程 UI/R1-R3） | **10 / 11 通过，1 失败**（`D067_FullFlow_CreateSaveExitLoadContinue` 在首个截图步骤失败：batchmode 下 `ScreenCapture` 报告 640×480 且未落盘；功能断言在此之前全部通过。判定为**捕获环境阻塞**，非产品缺陷） | `playmode-d067-full.xml` |

### 历史 51 个捕获失败审查结论

- 历史批次中的 51 个“捕获失败”集中在**截图落盘类**用例（`ScreenCapture` 在 batchmode 无帧缓冲 / 640×480 / 静默不落盘），
  而非玩法或渲染断言；本轮 D068 改用 `RenderTexture` 合成（世界相机 + ScreenSpaceCamera Canvas）后 16/16 截图成功，
  证明这些失败属于**捕获手段/环境问题**，不是被测功能缺陷。
- 本轮没有关闭 `CaptureWriteGuard` / `CapturePathGuard`，没有放宽任何安全断言；D067 的 1 次失败按环境阻塞如实记录，
  未修改为通过、未加入忽略列表。

## 4. 本轮代码修复（与截图/测试对应的生产缺陷）

1. **文明详情面板**（`CivilizationPanelView`）：HUD“文明 (N)”入口 + 面板；真实快照数据；上一/下一文明；暂停/恢复；中英切换；
   世界切换/读档自动回落选中文明。快照新增 `CivilizationSnapshot`/`DiplomacySnapshot`（`IWorldSnapshot`）。
2. **文明事件**：新增 `DiplomacyChanged`（仅关系真实变化时发布一次；带双方 ID），修复死亡事件 tick 恒为 0、
   饥荒事件每次决策重复发布（改为进入饥荒的跳变沿）；事件日志用快照把文明 ID 解析为名称（中英关系名）。
3. **自动存档生命周期**：读档/新世界/退出世界重置基线；时钟回退保护；失败计数/原因/红色 Toast + 下一周期恢复；
   失败不再每帧重试。
4. **地形/素材规则缺陷**：Mount→Hill 台面承接格不再写墙（规范 §D.5 iter-08；三种子共移除 61/107/92 格覆盖台面的墙）；
   高地格不再铺泡沫（§D.2 崖格不放）；树随机缩放禁止缩小（§D.6，1.0–1.15）。
5. **文档同步**：`docs/DECISIONS.md` §2.24、`docs/11`、`docs/12`、`docs/impl/D-068-文明详情与剩余交付.md`。

## 5. 仍未收口（如实登记，未造假）

- **坡面色族**：规范 §D.5:229/232 要求“坡面随相邻高地面板”，代码取落脚层色族（`PanelLevel-1`），
  且 `TerrainGuideRulesTests` 锁定现有画面。本轮未改（避免回退已验收视觉效果），登记为待裁定的规范/实现冲突。
- **阴影 1.6 倍**：本地 `Shadow.png` 可见团 79×80，官方 guide 记录 128×128；生产按 128/80=1.6 放大以复现 2 格投影，
  由 `ShadowPlacementGuideTests` 锁定。规范 §D.3 仍写 1:1 / 1.25 格，本轮未回写规范（登记）。
- **1 行宽台面**：属 `D-061` 噪声高程生成设计（数据层 1 级落差合规），不是渲染选片缺陷；东西向 1 格宽脊未做宽度规则，
  需后续设计裁定。
- **死层**：`WaterEdgeOverlay(162)`（iter-11 退役）与 `Stairs(190)` 仍建层/序列化但无 SetTile；不影响画面。
- **HUD 世界人口**仍按“聚落”口径显示 0；文明面板显示真实成员人口（2/8 等）。两者口径不同，未在本轮统一。
