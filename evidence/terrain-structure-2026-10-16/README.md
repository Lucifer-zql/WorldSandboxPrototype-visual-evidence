# terrain-structure-2026-10-16 —— D-059 地形结构证据批次

> 批次负责人：主 Agent（集成人）；证据生产与终化：Agent F。
> 工作区 HEAD=`47d8185`，dirtyFiles=`349`（终化时点实测）。**批次已冻结待交付**：本轮不推公开仓库——制作人验收后随主仓库一并处理，或另获授权再推。
> 证据边界惯例沿用 cliff-repair / MVO 批次：README 只述「能证明/不能证明」，manifest 记录逐帧实测，SHA256SUMS 固化终态。

## 一、批次目标

落实 **D-059 地形结构模型**（2026-10-16）的三种子可视化与数据化证据：

1. 高程与地形分离——`WorldGenResult.Elevation`（0 低地 / 1 Hill / 2 Mount）由生成显式产出；高地格本体仍为 Grass/Forest 陆格，**正规生成不再产出 `TerrainType.Cliff` 格**（CliffShoreStep 退役）。
2. 崖壁/坡/台阶由渲染层 `TerrainOverlayPlan`（纯函数权威分析，World 程序集）按高程差与邻接推导：墙格（高地南缘下方低地陆格，r4 陆脚 / r5 临水收口）、坡格（左右单侧高差干燥陆格）、台阶（每 HILL 分量至多一个南向入口、成对上下两格、落点 3×3 全干燥）、净空（VegetationClearance / StairClearance）——渲染（D）与散布（E）共同消费、唯一权威。
3. 层序（自下而上）：水底 −100 → 泡沫 −50 → 基础平地 0（所有陆格永保留，崖壁透明区露出）→ Shadow 150 → 高地顶面 160 → Slope 170 → CliffWall 180 → Stairs 190。
4. 崖壁变体沿用 Godot 端帽感知语义：列变体 = `(x*13+y*7)%4`，段首 0 / 行锚中段 / 段尾 2 / 孤立单格 3；台阶端点择一 = `(x*13+y*7)%2`。

## 二、运行口径

| 项 | 值 |
| --- | --- |
| 场景 | `Assets/_Project/Scenes/Main.unity`（真实生产场景，Play Mode） |
| 种子 | 20261013 / 20261014 / 20261015（经 `WS_WORLD_SEED` 三连跑） |
| 分辨率 | 1280×720（GameView 实测一致，见 manifest 行） |
| 输出根 | `MVO_CAPTURE_MODE=ts-seed-<seed>` → `terrain-structure-2026-10-16/<seed>/` |
| 捕获方式 | `ScreenCapture.CaptureScreenshot`（先删旧图防短路；cam.Render 强制渲染 + 泵帧） |
| 测试链 | EditMode `MapVisualOverhaulSeedMatrixTests`（数据级特征锁）+ PlayMode 捕获套件（TS1~TS4 + 沿用 MVO1/3/4/5）+ `CliffAuditCaptureTests`（审计）+ `VisualVerificationTests`（独立复验） |

## 三、manifest 汇总（三种子，均 128×128 地图）

| 统计（格） | seed-20261013 | seed-20261014 | seed-20261015 |
| --- | --- | --- | --- |
| 高地格（elev≥1，直接计数） | 1028 | 926 | 884 |
| 山格（elev==2，直接计数） | 270 | 252 | 220 |
| HILL 分区数（4 连通） | 3~6（EditMode 断言绿*） | 同左* | 同左* |
| 崖壁墙格（Plan.CliffCells） | 166 | 182 | 172 |
| 侧坡格（Plan.SlopeCells） | 120 | 100 | 84 |
| 台阶（Plan.Stairs） | 0 | 2（LeftPair @15,24） | 1（RightPair @110,102） |
| 最长水平墙段 | 4 | 9 | 8 |
| 最长竖直墙段 | 3 | 3 | 3 |
| 变体直方图 v0/v1/v2/v3 | 41/6/39/80 | 57/1/46/78 | 45/2/39/86 |
| 阴影精灵（Shadow150） | 1028 | 926 | 884 |
| 净空带内树数（复验口径） | 0 | 0 | 0 |
| MVO5 云树 | A 分支（contested-px>=20） | A 分支 | B 分支（结构不可达如实声明） |

\* `stat ts1` 行的 hillBlocks/mountBlocks=1 系谓词修复前旧跑残留，不代表真实分区数——权威口径为修复后 EditMode SeedMatrix（HILL 3~6 块、MOUNT ≥1 块断言绿）；完整披露见 `verification-notes.md` §二.1。

## 四、逐目录「能证明 / 不能证明」

### seed-20261013/ （24 张 PNG + capture-manifest.txt，25 行捕获）
**能证明**：全图两帧（ortho40 构图口径 / ortho64 满幅）；草林九宫格边界、水岸泡沫、水脚收口（Row==5 @86,6）、最长水平/竖直墙段、墙段转角、端帽（v3 孤立双缘帽）、高地顶面+南缘墙身同框、坡面接入（slopeCells=120）、阴影与崖壁透明区（阴影精灵数=高地格数）、高地邻林/邻草/临水三组合、净空带内零树。
**不能证明**：台阶接入（该种子 0 处——HILL 分量落点 3×3 非全干燥，与海岸不强行打通为规格行为）；两级台直邻低地（被 MOUNT 边距不变式结构性排除，见 notes §二.7）；HILL 分区数（ts1 stat 残留，EditMode 为准）。

### seed-20261014/ （25 张 PNG + manifest，26 行捕获）
**能证明**：同上全套 + 台阶接入 2 处（LeftPair 成对上下两格 @15,24-15,23）；最长水平段 9；净空锚点=台阶下格、最近树仅距 1 格且带内零树（E 过滤有效）。
**不能证明**：同 s13 通用缺口（两级台直邻低地、HILL 分区数）。

### seed-20261015/ （25 张 PNG + manifest，26 行捕获）
**能证明**：同上全套 + 台阶接入 1 处（RightPair @110,102-110,101）；右翼坡面（side=Right，验证坡片镜像）；MVO5 云树「相邻不重叠」如实记录分支（漂移门仍 ≥8px 有效）。
**不能证明**：同上通用缺口。

### test-results/ （29 个文件：EditMode t1~t13、PlayMode 全量 t1/t2、捕获批次 ts-s13/14/15）
**能证明**：终态测试链——EditMode **220/220**（editmode-t13.xml，含 SeedMatrix 高程矩阵断言）；PlayMode 全量 **63/63**（playmode-full-t2.xml，含重写后的 CliffAudit C1~C6 与 VisualVerification B）；捕获批次三种子全绿（ts-s*.xml，各 8 用例）；以及首跑 7 失败（旧 Cliff 定位器）→ 重写 → 全绿的修复链（playmode-full-t1 vs t2）。
**不能证明**：游戏实际运行的长时间稳定性、性能与手感（不在本轮验收范围）。

## 五、终态测试链

| 门 | 结果 | 证据 |
| --- | --- | --- |
| EditMode（含 SeedMatrix 高程矩阵：水比/陆块/边圈/无 None/高程契约/无 Cliff/HILL 3~6 块/MOUNT ≥1/水格零高程/成簇率/计数区间/FNV 互异） | 220/220 绿 | `test-results/editmode-t13.xml`（total=220 passed=220 failed=0） |
| PlayMode 全量（含 CliffAudit 审计 C1~C6、VisualVerification A~H、捕获套件 TS1~TS4 + MVO1/3/4/5） | 63/63 绿 | `test-results/playmode-full-t2.xml` |
| 捕获批次（三种子） | 8/8 × 3 绿 | `test-results/ts-s13.xml` / `ts-s14.xml` / `ts-s15.xml` |
| 游戏实际运行（G5+） | 捕获批次即真实场景 Play Mode 运行证据 | 本目录 PNG + manifest |
| 用户验收（G9） | **A 终审通过**（制作人侧验收另行） | 见 §六 |

## 六、A 终审结论（引用）

A 终审判定**通过**：八条标准全过。其中两争议项判定——TS3b 条带 = 官方描边 + 顶面边件双线、**非缺陷**（D 诊断互证）；阴影带 = Godot 同源表现。另登记**四项打磨建议**（不阻塞，清单以主 Agent 终审记录为准）。

## 七、完整性

`SHA256SUMS.txt` 覆盖本目录全部文件（排除自身），生成后经 `sha256sum -c` 全量自验通过；冻结后任何字节级改动都会破坏校验和。已知残留与披露（ts1 stat 残留、A 四项打磨、岩石贴崖偏好、灌木冠幅边界、4/8 连通口径、MVO5 s15 分支、TS2g 结构性跳过）见 `verification-notes.md`。
