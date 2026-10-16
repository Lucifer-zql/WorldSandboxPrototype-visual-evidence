# verification-notes —— terrain-structure-2026-10-16 批次终化附注

> 批次：D-059 基础地图地形结构（2026-10-16）。证据根 = 本目录。
> 终化人：Agent F（独立验证工程师）；终化时点 2026-10-16，工作区 HEAD=`47d8185`，dirtyFiles=`349`（`git status --porcelain` 实测计数）。
> 三份种子 manifest 的 `HEAD=<待主Agent补记> dirtyFiles=<待主Agent补记>` 占位已逐行回填并带 `[backfilled 2026-10-16]` 标注。

---

## 一、本轮时间线（时点以 test-results/*.xml 内嵌时间戳与文件 mtime 为准，UTC）

1. **契约冻结**（主 Agent，D-059 简报 §三）：`ElevationStore` 三级高程（`Levels=3`、0 低地/1 Hill/2 Mount、`FromLevels` 正规构造）；`WorldGenResult.Elevation`（byte[]，index = y×W+x）由生成显式产出；`SortingOrders` Slope=170 / CliffWall=180 / Stairs=190；`TerrainType` 枚举不变、**正规生成不再产出 Cliff 格**（CliffShoreStep 退役）；水比/陆块/边圈/无 None/同种子自一致/散布计数等既有门禁不回退。
2. **五域并行**：C 世界生成（PlateauStep 重写为高程场生成、CliffShoreStep 摘除）、D 渲染（`TerrainTilemapRenderer` 分层重构 + `TerrainOverlayPlan` 纯函数权威净空/占用分析 + 坡/台阶新 Tile 资产）、E 装饰（ScatterStep 候选剔除净空带）、B 素材只读审计、F 测试证据（SeedMatrix 高程断言重写、捕获套件 TS1~TS4、TerrainPlacementEvidence 一处高程语义迁移）。
3. **集成编译修复 6 处**（主 Agent 统一实施，集成期；明细见主 Agent 集成记录）。
4. **SeedMatrix 谓词语义 bug 修复**（09:10 mtime）：`CollectComponents` 连通分量谓词原误传**格索引**、改传**格值**（`match(values[i])`）；PlayMode 捕获文件同型 `CollectComponents4` 修复落盘于 10:00——**晚于捕获批次（09:53–09:55），披露见 §二.1**。
5. **有机性阈值重标**（TerrainCompositionTests 高程场组③）：P²/A 有机比阈值经 dotnet harness 202 种子 × 2 类地图标定为 [12, 45]，阈值标注 `未验证`，定位为退化护栏（防机械盖章/度量异常），非有机性强判别。
6. **定位器重写**（PlayMode 全量首轮 7 失败归 F）：`CliffAuditCaptureTests` C1~C5 改 Plan 定位器 + 审计断言（09:23，水平段变体分配规则/行语义契约审计，C2 阈值 ≥3→≥2 按新模型注明）；`VisualVerificationTests.B_CliffElevationShadow` 改「墙格 + 阴影层」语义（09:24，Shadow150 < CliffWall180 层级契约 + 基础地表永保留断言）。
7. **终态全绿**：EditMode **220/220**（editmode-t13.xml，01:11Z）；PlayMode 全量 **63/63**（playmode-full-t2.xml，01:50Z）；捕获批次 ts-s13 / ts-s14 / ts-s15 全绿（01:53–01:55Z，捕获套件 8 用例 × 3 种子）。

## 二、已知残留与披露（均不阻塞交付）

1. **【证据披露】三份 manifest 的 `stat ts1` 行 `hillBlocks/mountBlocks=1` 为谓词修复前旧跑残留**：捕获批次运行时 `CollectComponents4` 仍为索引谓词（索引≥1 恒真 → 全图计为单分量），故该两字段恒为 1，**不代表真实高程分区数**；权威口径 = 修复后 EditMode `MapVisualOverhaulSeedMatrixTests`（同种子同配置断言 HILL 3~6 块、MOUNT ≥1 块，220/220 绿）。同行的 `hillCells/mountCells`（逐格直接计数）与墙格/坡格/台阶/净空/变体直方图统计（Plan 扫描）不受该 bug 影响、有效。10:00 的修复仅影响 TS1 记录性统计、无断言参与，不改 PlayMode 63/63 判定；如需统计正确的 ts1 行，补跑三种子捕获批次即可（批次已冻结，是否补跑由主 Agent 裁定）。
2. **A 终审登记四项打磨建议**（不阻塞，清单以主 Agent 终审记录为准）：其中两项判定口径——TS3b 坡面条带 = 官方描边 + 顶面边件**双线，非缺陷**；阴影带 = Godot 同源表现。
3. **岩石贴崖偏好退化**：旧定位/摆放语义中「岩石优先贴 Cliff|Water」的 Cliff 分支在正规地图（无 Cliff 格）不再命中，岩格落点分布随 E 净空过滤变化——视觉偏好项，登记后续打磨。
4. **灌木/台阶格边界 case**：E 候选剔除以格为粒度；灌木冠幅（跨格画布）与台阶/墙格的像素级边界表现本轮未覆盖（`treesInsideClearance=0` 为格级口径），待 A 视觉后续裁决。
5. **连通性 4/8 口径说明**：D-059 文档契约与 F 两侧实现（SeedMatrix / 捕获 ElevationStats）均为 **4 连通**；`TerrainCompositionTests` 的分量辅助实现为 8 邻（其注释口径为 4 连通）。三种子上两种实现各自断言同绿（F：4 连通 3~6 块；C：8 邻计数落在规格内）——当前有机 blob 形态下两口径等价；若未来生成出现仅对角桥接形态，两侧分量数将分歧，届时以文档口径 **4 连通** 为准修实现。
6. **MVO5 云树证据分支**：s13 / s14 走 MVO5a（screen-space 触发命中，overlap=contested-px>=20）；s15 走 MVO5b（cloud-right-of-crown-x-domain 结构不可达，如实声明 adjacent-not-overlapped，双帧仍过漂移门 ≥8px）——判定纪律照旧，无弱化。
7. **TS2g（两级台与低地直邻）三种子均按设计跳过**：MOUNT 边距不变式（切比雪夫半径 2 内全高地）结构性排除 elev==2 与 elev==0 直邻——该清单项的缺席是**规格符合**而非证据缺口；两级台墙面证据由 TS2c（三种子最长竖直墙段 len=3，即 Mount 南缘双段 + 邻格）与 TS1 全景承载。

## 三、终化操作记录

1. 三份 `seed-*/capture-manifest.txt`：占位回填 `HEAD=47d8185 dirtyFiles=349 [backfilled 2026-10-16]`（s13=24 行、s14=25 行、s15=25 行），各追加一行 backfilled 汇总注记。
2. 新建本文件与 `README.md`。
3. `SHA256SUMS.txt` 全量生成（排除自身）并 `sha256sum -c` 自验通过（见 README §七）。
