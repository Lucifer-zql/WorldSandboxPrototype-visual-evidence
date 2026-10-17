# iteration-09 —— D-065 选族回退关闭、全掩码补齐、真实渲染输出验收

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 本轮依据任务书 §四/§五/§六/§八：P1 关闭 band 缺失回退左族 + 补齐全部可达水向掩码；
> P2 修复扫描器自身缺陷（递增/判定/实际 Sprite）；P0 全图逐格核验**实际 Tilemap 输出**。
> 全部结论来自生产渲染器的真实 Tilemap `GetTile` 输出（非规则推算），并由真实 Unity 截图交叉印证。

## 一、结论速览

1. **P1（已修复）**：
   - **关闭不安全回退**：`TerrainTilemapRenderer.ResolveFlatBaseTile` / `ResolveElevatedTopTile` 的选族
     由 `(waterMask > 0 && (!mixed || !HasWaterEdgeBand(waterMask))) ? shore : land`
     改为 **`(waterMask > 0 && landMask == 0 && shore[mask] != null) ? shore : land`**——混合缘（水向+陆向同暴露）
     **一律右族（陆向无白）**；band 缺失时无 band（安全退化），**绝不回退左族整片**。
   - **全掩码补齐**：新增 **7 张**边带派生件（掩码 5/7/10/11/13/14/15）→ 现共 **15 张**（1..15 全覆盖），
     `Main.unity` 的 `waterEdgeBandSprites` 16 槽（索引 1..15）全部接线。
   - **iter-09-r2 纯水缘补口（Agent B 评审发现，可达）**：左族素材只有 **8 个掩码整片 + 中心**
     （`_0,_1,_2,_9,_11,_18,_19,_20`）；纯水缘（landMask==0）且掩码 ∈ {5,7,10,11,13,14} 时旧逻辑
     静默退中心片 → **水岸线缺失**。修复：此类格改取**右族 + band**（水向白线由 band 承担，
     与混合缘同一合成口径）。三种子实测命中 **4/1/2 = 7 格**（掩码 11/13/14），修复后 `p1Fail=0`。
   - **实际输出验证**：15 个掩码 × 确定性构造（混合缘 + 纯水缘两组），`TerrainTilemap.GetTile` 的
     sprite 名与期望族一致、`WaterEdgeTilemap.GetTile` 的 sprite 名 == `WaterEdgeBand_Mask{n}`，**30/30 通过**；
     三种子全图水向暴露格 **760/579/756**（混合缘 85/71/100 + 纯水缘 675/508/656），**0 违规**；
     另核验计划外 band = 0（低地按地形掩码、高格按顶面高程掩码，与渲染器同口径）。
2. **P2（已修复扫描器 + 结构判定）**：
   - 旧离线扫描器缺陷全部关闭：计数器全部参与最终判定（`topElevated`、`nb_wall_n/s` 不再只记录）、
     下段合法性**核对实际 Sprite**、**区分连续坡列与短坡列/孤立坡**。
   - 实际输出（三种子，`SlopeTilemap`/`StairsTilemap`/`CliffWallTilemap` 逐坡核验）：
     **连续坡列**（≥3 坡）上段全为 `Slope_Hill_Left/Right`（`_36/_39`）、坡列底部为 `Stair_*_Lower`（`_45/_48`）；
     **短坡列**（2 坡）同上；**孤立单坡**按 iter-07 `PostProcessSlopes` 规则剔除并补侧墙（TS9h 反例如实记录）。
     **坡 115/156/92 = 363 个，0 违规**；上端 Elevated、下端 Terrain、豁口无墙全部满足。
   - **iter-09-r2 补口**：`StairsTilemap` 台阶上/下段实际 Sprite 核验（`_36/_39` + `_45/_48`）、
     台阶格与坡/墙重叠检查、**计划外坡/台阶 Tile = 0**（原扫描器只打印台阶数量，未读实际层）。
   - **官方示意图坡路摆法**：官方图**未见** `_36+_45` 坡道成对实例（Agent A 与主 Agent 独立复核一致）——
     坡片用法为**素材事实+项目合理组合**，**不是官方可证实的摆法**，登记为【待验证】；
     本项目的"台阶（Stairs）"= 南向坡道入口语义，与坡（Slope）在计划层区分。
3. **P0（实际输出验收）**：全图逐格 **128×128×3 = 49152 格**，逐格核验
   地形/高程/Flat/Elevated/墙计划↔实际/Sprite 名；**Shadow 层**逐格核验（elev≥1 且南邻非水 ⇒
   一张激活 Shadow SpriteRenderer，位置与 `RebuildShadows` 同口径，三种子数量/位置 0 不符）；
   另 **中间台面 181/279/193 = 653 格**逐格核验（Mount 邻 Hill 不得有墙、必须有顶面）——**全部 0 违规**。
   计划中存在但实际未绘制的墙 = 0，实际存在但计划外的墙 = 0。Mount→Hill 不写墙、Hill 南缘墙、低地承接逐层完整。
4. **测试**：**EditMode 364/364 全过**（含本轮新增 33 项：15 混合缘掩码 + 15 纯水缘掩码穷举 +
   3 种子真实输出扫描；另 iter-04 全图选片审计更新为 iter-08/09-r2 口径）；
   **PlayMode 三种子各 16 passed / 1 skipped / 0 failed**（skip = `MVO_TPR3_StateContrast_SameFraming`
   既有 tpr3 模式跳过；r2 重采三种子见 `regression-final-r2`）。
5. **确定性诊断场景**（`diagnostic-scenes/seed-*`，8 图 × 3 种子，固定地形经场景内真实生产渲染器）：
   `TS9a` 全 15 掩码混合缘网格、`TS9b/c` 近景、`TS9d` 连续坡列（3 坡+下段）、
   `TS9e` 短坡列（2 坡+下段 = 独立坡路形态）、`TS9f/g` P0 逐层退台、`TS9h` 孤立坡剔除补墙反例。

## 二、目录

```
iteration-09/
  baseline/                     # 起检基线（HEAD f85fbe3、生产文件 SHA-256、git status 406 行）
  pre-fix/seed-*/               # 本轮启动时采集（见 §四 时序声明：仅 seed-20261013 为有效修复前基线）
  regression-final/seed-*/      # 修复后三种子全量初版（纯水缘修复前；55 随机 + TS1–TS8 + 初版 TS9）
  regression-final-r2/seed-*/   # **最终**修复后三种子全量（含纯水缘修复 + TS9a–j 共 10 图 + XML/log）
  diagnostic-scenes/seed-*/     # TS9 初版诊断（纯水缘修复前；8 图 × 3 种子；保留作对照）
  P0-rendered-ground-coverage/post-fix/
                                # p0-output-seed-*.csv（16384 行/种子，全格）+ p0-midbench-seed-*.csv
  P1-directional-edge/post-fix/ # p1-output-seed-*.csv（760/579/756 行水向暴露格，含通过）
  P2-slope-rendered-connectivity/post-fix/
                                # p2-output-seed-*.csv（115/156/92 行坡，含判定列）
  test-evidence/                # EditMode 349 全量、Iter09 扫描 r2/r3/r4/r5、PlayMode 三种子、TS9 XML
  README.md / final-report.md / SHA256SUMS.txt
```

## 三、生产代码差异（相对 iteration-08）

| 文件 | 改动 |
| --- | --- |
| `TerrainTilemapRenderer.cs` | **P1 关闭回退**：Flat/顶面选族改为 `(waterMask>0 && landMask==0 && shore[mask]!=null) ? shore : land`（两处）；**r2 纯水缘**：左族缺片格改右族 + band（`faceIsLand` 输出参数驱动 band 门控）；**新增公开属性** `WaterEdgeTilemap`；band 槽校验加入 `WarnMissingTilesOnce` |
| `Main.unity` | `waterEdgeBandSprites` 16 槽补齐（掩码 1..15 全部派生件接线；槽 0 空） |
| `Art/Terrain/Derived/WaterEdgeBand/*.png(+meta)` | **新增 7 张**（掩码 5/7/10/11/13/14/15）→ 共 15 张；确定性生成（左族对应片暴露边 16px 边带，其余 alpha=0），原始 PNG 零改动 |
| `Assets/Tests/EditMode/Iter09RenderOutputScanTests.cs` | **新增 33 项测试**：15 掩码混合缘穷举 + 15 掩码纯水缘穷举 + 三种子真实 Tilemap 输出扫描（P0 全格/Shadow/墙、P1 水向暴露含纯水缘与计划外 band、P2 坡+台阶+计划外，全量 CSV） |
| `Assets/Tests/EditMode/TerrainTileSelectionAuditTests.cs` | iter-04 全图选片审计更新为 iter-08/09-r2 分族口径（右族 15 片全集、混合缘/纯水缺片 band 补偿断言） |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 新增 `TS9_Iter09DiagnosticScenes`（**10 图**：混合缘全掩码/纯水缘全掩码/连续坡列/短坡列/孤立坡反例/逐层退台） |

**未改动**：地形生成（TerrainStore/ElevationStore/WorldGenerator）、`TerrainOverlayPlan` 规则、
`SortingOrders`、既有 328 项测试断言。

## 四、采集时序如实声明（pre-fix）

本轮修复与修复前基线采集**并行**执行，造成如下时序事实（如实记录，不掩盖）：

- `Library/ScriptAssemblies/WorldSandbox.Presentation.dll` 修复后编译时间 = **12:14:40**；
  `pre-fix/seed-20261013` 全部截图时间 **12:14:04–12:14:30**，早于新 DLL，且其 TS8a–i/tpr2 等
  同名图与 `iteration-08/regression-final/seed-20261013/` **逐字节相同（SHA-256 一致）**——
  **确认 seed-20261013 为有效的修复前基线（iter-08 终态）**。
- `pre-fix/seed-20261014` / `seed-20261015` 的第一个 Unity 进程在 12:14:40 之后启动，
  **实际运行的是修复后代码**（与 `regression-final` 同名图部分一致、部分因动画/相机相位不同）——
  **这两个目录不作为修复前基线**；本轮的修复前权威对照统一引用
  `iteration-08/regression-final/seed-*`（iter-08 终态，见其 README；iter-09 生产代码仅上述 P1 修改）。
- 结论：P1/P2/P0 的"修复后"证据（本目录 `regression-final` + CSV + `diagnostic-scenes`）
  不受影响；"修复前"对照以 iteration-08 终态为准。

## 五、局限与如实声明

1. **官方坡路摆法【待验证】**：官方示意图中未见 `_36+_45` 成对坡道实例。本项目"坡（Slope）=
   1 宽×2 高连接件、下段仅坡列底部"为**素材形状事实（α 接缝连续）+ 项目合理组合**，
   **不是官方可证实的摆法**；不得表述为"官方摆法"。
2. **纯全水缘（掩码 15）**：左族 9 槽岸线集无 15 掩码片，中心格取 `_10` 中心片（素材事实）；
   该格无陆向暴露，不铺边带（无波浪风险）。
3. **孤立单坡**：`PostProcessSlopes` 会剔除南北皆无墙/坡/台阶的孤立坡并补侧墙（iter-07 规则）；
   TS9h 以真实渲染器输出如实记录该边界行为（单格高台 → 侧墙 + 南向台阶），**不是坡路缺陷**。
4. **高格顶面掩码语义**：顶面暴露按**高程**（邻格 elev < 本格），与低地的**地形级**掩码不同；
   边带按可见面掩码铺（`ApplyCell`）。本轮扫描器已按此分口径核验。
5. **CSV 判定列**：`fail` 列为该格/该坡的违规计数（0 = 通过）；全部通过行也写入，便于独立复算。
6. **既有跳过**：PlayMode `MVO_TPR3_StateContrast_SameFraming` 在 `tri-` 模式外跳过（iter-08 起既有）。
7. **Agent 报告互证（事后送达，如实纳入）**：
   - **Agent A（素材实测）**：16 掩码→切片映射与主 Agent 独立校验完全一致
     （左族 1=`_1`,2=`_11`,3=`_2`,4=`_19`,5=`_28`,6=`_20`,7=`_29`,8=`_9`,9=`_0`,10=`_12`,
     11=`_3`,12=`_18`,13=`_27`,14=`_21`,15=`_30`）；16px 边带对 15 掩码充分（合成重建 8 掩码
     0 像素差异，残差在 16–31px 内侧纹理区）；**官方示意图不能证实**坡道实例（与 iter-08 一致，
     覆盖更全）→ 官方摆法【待验证】维持。
   - **Agent B（只读审计）**：发现**纯水缘缺左族整片槽**的可达缺口（建议已采纳并修复，见 §一.1）；
     指出台阶实际输出未核验、计划外坡/台/band 未检查、Shadow 未核验——**已全部补入扫描器**；
     其"纯水缘 {5,7,10,11,13,14} 可达、{15} 不可达（单格陆岛被 FillSmallBlobs 翻水）"与三种子
     实测一致（缺片命中 7 格、mask 15 未出现）。
   - **Agent C（测试设计）**：独立给出的层×格矩阵与选片/band/墙/坡/台阶公式与主 Agent 实现一致；
     其"必补 `forestTile` / `waterEdgeBandSprites` 接线"与实测的测试基建缺口一致（已补）。
