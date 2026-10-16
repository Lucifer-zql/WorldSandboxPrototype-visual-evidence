# tile-placement-repair-2026-10-16-r2 —— D-062 verify-r2 独立复核与补充修复证据

> 生成：2026-10-16（Claude Code 会话，D-062 verify-r2）｜Unity 6000.6.5f1 / URP 2D / Windows 11
> 主仓库基线：分支 `main`，`HEAD=47d8185`（全程未对主仓库做任何 git 写操作）
> 前置批次：`tile-placement-repair-2026-10-16/`（D-062 首轮，未覆盖）；
> 迭代记录批次：`tile-placement-repair-2026-10-16-r2-iter1/`（UQ-19 修复首轮的**回归态**留档，见 §四）

## 一、本批修复内容（详见 `docs/impl/parallel-repair/d062v-worklog-2026-10-16.md`）

| 修复 | 内容 | 机制 |
| --- | --- | --- |
| v5 竖列选片 | 双侧无续接且有竖向续接（`(x,y±1)` 有墙）⇒ 无缝中段 `_42/_51`；孤立格仍 `_44/_53` | `TerrainOverlayPlan.Analyzer.AssignWallVariants`（消竖墙逐格"花盆串珠"） |
| UQ-19 崖顶白线 | 顶面层全显式选片（Option F）：纯陆缘 ⇒ 右族无白流苏片；水缘/混合缘 ⇒ 左族岸线件（逐片等价旧行为）；内芯 ⇒ 中心片 | `TerrainTilemapRenderer.ResolveElevatedTopTile`/`BuildElevatedTopTiles`（消"双亮线"白胶带；水岸白线保留） |
| D-B1 泡沫计划同步 | 笔刷改地形后泡沫层跟随新覆盖计划（水脚墙格排除集重建） | `ITerrainRenderer.CurrentPlan/SetPlan` + `FxCommandBus` 判等同步 |
| UQ-14 改判驳回 | "官方横段有变化"前提被 Agent A 模板匹配证伪（官方即 uniform `_42` 机械重复）；flipX/旋转/Tint/混排机制全部实测不可行 | 文档改判（矩阵 §12、规范 §D.5） |

## 二、运行环境与参数

- Unity `D:\software\Unity Hub\Editor\6000.6.5f1\Editor\Unity.exe`，窗口化 `-runTests`（截图须帧缓冲，**不带 `-batchmode`**；EditMode 复跑用 `-batchmode -nographics`）。
- PlayMode 过滤：单类名 `WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests`（无逗号）；模式 `MVO_CAPTURE_MODE=tpr2-seed-<种子>`；世界种子 `WS_WORLD_SEED=<种子>`；屏幕 1280×720。
- 三种子：20261013 / 20261014 / 20261015；每种 9 用例（8 标准 + TPR2 近景）全过；串行执行（同一时间仅一个 Unity 实例）。
- 历史证据保护：`docs/impl/evidence/`（排除本批与 iter1）sha256 复核 **1346 项全 OK、0 覆盖**（基线 `docs/impl/parallel-repair/d062-protected-before.sha256.txt`）。

## 三、逐张"能证明 / 不能证明"

| 文件（每种子各一套） | 能证明 | 不能证明 |
| --- | --- | --- |
| `TS2b/TS2d/TS2g-*.png` | 横墙带连续（D-062 成果保持）；**崖顶/台地白线消失**（深草缘压岩顶） | 单格变体编号（需 manifest） |
| `TS2c-longest-vertical-wall.png` | 竖列侧缘平直连续（v5；对比 D-062 批"花盆串珠"） | 竖列在全图占比 |
| `TS3a-stairs.png` / `tpr2-closeup-stairs.png` | 台阶上下段成对、楔形件剪墙而下、落点干燥（近景可读） | 台阶是否接入可行走路网（需寻路验证） |
| `TS3b-slope.png` / `tpr2-closeup-slope.png` | 坡面斜线方向（自高侧向低侧）、与高地缘/低地连续 | 坡体色块强度（草上草坡固有低对比——素材特性） |
| `tpr2-closeup-clifftop.png` | 崖顶白线消除前后对照（配 iter1 同机位图即三段对照） | 阴影/泡沫细节 |
| `tpr2-closeup-vertwall.png` | v5 竖列选片效果（直边叠层） | 与官方逐像素一致性 |
| `tpr2-closeup-twotier.png` | 两级墙上下带 + 草台同框 | 退台可读性是否达标（地形形状问题，D-061 冻结） |
| `tpr2-closeup-endcap.png` | 端帽收口仍在（与 ts 批 TS2e 同机位可比） | 端帽判据与官方逐像素一致 |
| `tpr2-closeup-clearance.png` | 植被—墙/台阶净空主体入画（旧 TS4f 取景失败的补拍） | 净空规则全覆盖（生成期规则另有测试） |
| `TS4b/TS4e-*.png` | 普通岸线泡沫仍在（未一刀切）；水岸白线保留（契约冻结） | 泡沫呼吸相位 |
| `TS1a/TS1b-*.png` | 全图地形与 D-061 逐项一致（`stat ts1` 数据同前；地形冻结） | 单格级正确性 |

**量化（薄荷白 (198,240,218)±12 全图计数，种子 13 同取景 A/B，D-062 批 → 本批）**：
`TS2d` **3801→0**、`TS2c` 8661→2126、`TS2f` 6183→4918、`TS3a` 6793→5892（残余 = 水面泡沫 + 林格自身描边 UQ-19①）。

## 四、iter1 迭代记录（回归教训，证据保留于 `-r2-iter1/`）

UQ-19 首轮修法（仅暴露陆缘格换显式 Tile、其余仍 RuleTile）**实测回归**：混用令 RuleTile 把显式邻格
判为 NotThis，白边件画到上一格（TS2d 白像素 +850、整体上移一格；差分图定位）。**教训：顶面层不得混用
RuleTile 与显式 Tile**。iter2（Option F 全显式）定稿后复拍为本批。

## 五、测试结果（`test-results/`）

| 套件 | 结果 | 文件 |
| --- | --- | --- |
| EditMode 全量（终态 Option F） | **259 / 259 passed，0 failed** | `../../../parallel-repair/d062v-editmode-r2c.xml`（+.log；中间态 r2/r2b 见同目录） |
| PlayMode 捕获 × 3 种子 | **9 / 9 ×3 passed，0 failed** | `test-results/tp-seed-20261013|14|15.xml`（+.log） |

## 六、SHA-256 清单

见 `SHA256SUMS.txt`（本批全文件；历史批保护清单另见 parallel-repair）。
