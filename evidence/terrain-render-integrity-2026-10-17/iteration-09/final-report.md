# D-065 Iteration-09 最终报告

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜三固定种子 20261013/14/15
> 任务书依据：§四（P1 回退关闭/全掩码/实际输出扫描）、§六（P0 逐格实际输出）、
> §八（P2 扫描器缺陷与结构区分）、§十（报告结构）、§十一（门禁）。

## A. 整改项与完成状态

### A1. P1 —— 关闭"band 缺失回退左族整片"的不安全回退（完成）

- **生产修改**：`TerrainTilemapRenderer.ResolveFlatBaseTile` 与 `ResolveElevatedTopTile` 两处选族：
  - 修改前：`(waterMask > 0 && (!mixed || !HasWaterEdgeBand(waterMask))) ? pair.shore : pair.land`
  - 修改后：**`(waterMask > 0 && landMask == 0) ? pair.shore : pair.land`**
  - 语义：混合缘（同格既有水向暴露又有陆向暴露）**一律取右族**（陆向无白），水向浅青白由
    `WaterEdgeOverlay(162)` 的边带承担；band 缺失时无 band（安全退化），**任何情况下不回退左族整片**。
- **全掩码补齐**：原 8 张（1/2/3/4/6/8/9/12）→ **15 张（1..15）**。新增 5/7/10/11/13/14/15，
  来源为左族对应切片（`_28/_29/_12/_3/_27/_21/_30`）的暴露边 16px 边带裁剪；
  `Main.unity` 的 `waterEdgeBandSprites` 16 槽全接线（槽 0 空）。
- **验证（实际输出，非规则推算）**：
  - `Iter09P1_EachWaterMask_MixedEdge_RightFamilyPlusBand`（15 TestCase）：
    每个掩码的中心 Grass 格（四邻按掩码放水、其余放 Forest）→ 实际 `TerrainTilemap.GetTile`
    的 sprite ∈ {右族 15 片, 中心片 `_10`}，`WaterEdgeTilemap.GetTile` 的 sprite == `WaterEdgeBand_Mask{n}`；
    **15/15 通过**；掩码 15（纯全水缘）取中心片且无 band（如实记录素材事实）。
  - 三种子全图：混合缘格 85/71/100（低地=地形掩码；高格=顶面高程掩码，与渲染器同口径），
    **0 违规（256 格全通过）**；实际输出列（face_sprite/band_sprite）逐格留痕于 CSV。

### A2. P2 —— 扫描器自身缺陷修复与结构区分（完成）

| 旧缺陷（任务书 §八.1） | 本轮处理 |
| --- | --- |
| `slopeConflictWall` 无递增逻辑 | 新扫描器按坡逐条判定，所有违规计数参与最终断言（`p2Fail==0`） |
| `topElevated` 计算后未参与判定 | `topOk`（上端 Elevated）纳入 `verdictOk`，未通过即 FAIL |
| `nb_wall_s/n` 只记录不判定 | 豁口判定读取**实际** `CliffWallTilemap.GetTile`（坡上段格与坡列底部下段格），参与 verdict |
| 下段合法性未核对实际 Sprite | 下段读取 `SlopeTilemap` 实际 Tile，经 RuleTile `m_DefaultSprite` 解析，须 == `_45/_48` |
| 未区分独立坡路/连续坡列 | `lowerIsOtherUpper` 区分：连续坡列的下段格由下方坡上段占据（`_36/_39`），坡列底部为 `_45/_48` |
| 官方摆法【待验证】混同 | 见 §F1：官方示意图无可证实坡道实例，如实区分 |

- **实际输出结果**：三种子坡 115/156/92 = **363 个，0 违规**；连续坡列/短坡列/孤立坡反例三种结构
  均在 `diagnostic-scenes` 有真实运行时截图（TS9d/e/h）。

### A3. P0 —— 全图逐格核验实际 Tilemap 输出（完成）

- **方法**：`Iter09_RenderOutputScan_Seed(20261013/14/15)`：`WorldGenerator.Generate` →
  `renderer.RenderWorld(store, elevation, plan)` → 逐格 `GetTile` 读取实际 Tile；
  **计划与实际不一致即记失败**（不按"底层地格类型"判合法）。
- **覆盖**：
  - 全图 **128×128×3 = 49152 格**：陆地格 Flat 存在、水格无 Terrain、elev≥1 有 Elevated、
    墙「计划↔实际」双向一致 + Row/Variant→Sprite 名匹配；
  - 中间台面 **181/279/193 = 653 格**：Mount 邻 Hill 不得有 CliffWall、必须有 Elevated 顶面；
- **结果**：**全部 0 违规**（P0 49152 格 + midbench 653 格）；计划墙 217/285/289 条全部实际绘制且片名正确。

## B. 生产代码修改（仅主 Agent；本地工作区保留，未提交/推送主仓库）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | 两处选族关闭回退；新增 `WaterEdgeTilemap` 公开属性 |
| `Assets/_Project/Scenes/Main.unity` | `waterEdgeBandSprites` 16 槽（1..15）接线 |
| `Assets/_Project/Art/Terrain/Derived/WaterEdgeBand/` | 新增 7 PNG+meta（5/7/10/11/13/14/15），共 15 张 |
| `Assets/Tests/EditMode/Iter09RenderOutputScanTests.cs` | 新增（18 项） |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 新增 `TS9_Iter09DiagnosticScenes`（8 图） |

未修改：地形生成器、`TerrainOverlayPlan` 规则、`SortingOrders`、既有 328 项测试断言、官方原始 PNG。

## C. 测试结果

| 测试 | 结果 | 证据 |
| --- | --- | --- |
| EditMode 全量 | **349/349 通过**（0 失败 0 跳过） | `test-evidence/editmode-full-349-pass.xml` |
| Iter09 扫描（15 掩码 + 3 种子） | **18/18 通过**（r5 全量逐格版） | `test-evidence/iter09-scan-tests-r5-full-grid-pass.xml` |
| Iter09 扫描过程留证 | r2（墙 cast 失败）、r3（Mount 色表失败）保留 | `test-evidence/iter09-scan-r2/3-*.xml` |
| PlayMode 三种子 | 各 **16 passed / 1 skipped / 0 failed** | `test-evidence/playmode-post-seed*.xml` |
| TS9 诊断 | 三种子各 8 图成功 | `diagnostic-scenes/seed-*/` |

## D. 扫描数据（全部为实际 Tilemap 输出）

| 项 | seed13 | seed14 | seed15 | 违规 |
| --- | --- | --- | --- | --- |
| P0 全格 | 16384 | 16384 | 16384 | **0** |
| P0 中间台面 | 181 | 279 | 193 | **0** |
| P1 混合缘 | 85 | 71 | 100 | **0** |
| P2 坡 | 115 | 156 | 92 | **0** |

CSV 列包含实际 Sprite 名与判定列（`fail`/`verdict`），全部通过行均保留，可独立复算。

## E. 视觉证据（真实 Unity 运行时截图）

- `diagnostic-scenes/seed-*/`：TS9a 全 15 掩码网格、TS9b/c 近景（P1 实际右族+边带）；
  TS9d 连续坡列、TS9e 短坡列、TS9h 孤立坡反例（P2 三种结构）；
  TS9f/g P0 Mount→Hill 台面→墙→低地逐层退台。
- `regression-final/seed-*/`：55 随机图 + TS1–TS8 全量（修复后，与 iter-08 同机位可对照）。
- `pre-fix/seed-20261013/`：修复前有效基线（与 iter-08 终态逐字节一致，见 README §四）。

## F. 局限与待验证

1. **官方坡路摆法【待验证】**：官方示意图无可证实的 `_36+_45` 成对坡道实例；本项目坡道为素材形状
   事实（接缝 α 连续）+ 项目组合，**不得表述为官方摆法**。
2. **纯全水缘（掩码 15）**：无 15 掩码岸线片 → 中心片 + 无边带（素材事实）。
3. **孤立单坡**：按 iter-07 规则剔除并补侧墙（TS9h 反例如实记录，非缺陷）。
4. **pre-fix seed14/15 时序交叉**（修复与采集并行）：不作为基线，权威修复前对照 = iteration-08 终态。
5. **水格墙（岩壁入水）**：保留官方形态（iter-08 结论），不计违规。

## 门禁（任务书 §十一）

- [x] P1 回退已删除（生产代码两处）；无任何"缺 band 回退左族"路径。
- [x] 全部可达水向掩码（1..15）均有派生件与接线；全掩码穷举测试通过。
- [x] 扫描器读取实际 `GetTile` 输出（Flat/Elevated/CliffWall/Slope/Stairs/WaterEdge），不以规则推算代替。
- [x] P2 计数器全部参与判定；连续坡列/短坡列/孤立坡区分；下段核对实际 Sprite。
- [x] P0 全图逐格实际输出核验（49152 格 + 653 台面格），计划↔实际双向一致。
- [x] 官方摆法【待验证】如实标注；未将项目规则冒充官方证据。
- [x] 未调整地形生成/未删墙/未整格覆盖；Guard 未关闭；主仓库零提交零推送。
- [x] 每项修复后即时真实截图 + 发布（GitHub 证据仓库），发布后执行远端字节 SHA-256 复算。
