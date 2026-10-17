# D-065 Iteration-11 最终报告：Guide-driven Terrain Placement Correction

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜固定种子 **20261013 / 20261014 / 20261015**
> 规则基线：`tinyswords-tileset-guide` commit `a89e226242b839760dbe533d8d1a3ba6738e3746`、
> `guideVersion = 0.0.0.7`、`guide.json` SHA-256 `8F93443009C89FBBCCD84C707DE759AAB84F919D18C1E681BBC8666E27B43A0E`
> 生产代码：`TerrainTileRules.cs`（新）、`TerrainSheetFamily`（新）、`TerrainOverlayPlan.cs`（重写）、
> `TerrainTilemapRenderer.cs`（选片/阴影/坡路重写）、`PlateauStep.cs`（南向落差规范化）
> 主仓库：HEAD `f85fbe3`，**零 git 写**；本轮生产修改仅存本地工作区（未提交/未推送）。

---

## A. 规则基线

### A.1 固定版本与来源分级

| 级别 | 内容 | 本轮核对方式 |
| --- | --- | --- |
| ① 制作人不变量 | 悬崖/墙体位置方向正确；三角坡接入高地；内陆无水色缝；逐层（Mount→Hill→低地）可见 | 任务书 §五/§六/§七/§八 + 真实截图验收 |
| ② 官方素材可直接确认 | 单一 rock 行崖脚、南面朝向、2 格坡路对、色族按高程递进 | **本地官方示意图** `Tiny-Swords-官方素材示意图.jpg` 逐图目检（本轮重新查看；见本报告 §B.0） |
| ③ 固定版 guide.json 明确规定 | `flatGroundLookup` 16 项、`cliffRules`、`stairsRules`、`seamCoverRule`、`shadow.placementRules`、`foam.placementRules` | 逐字段解析（`rule-mapping.csv` 40 行，程序生成） |
| ④ 原始 PNG 实测 | 9×6 / 64px 切片几何、raw 编号、Alpha 覆盖 | 直接读 `Tilemap_color1..5.png` 与 `.meta`（54 片逐片） |
| ⑤ 项目自定义约定 | 草/林色族选择、林界深色流苏、泡沫只挂陆格 | 与既有 `docs/Tiny-Swords-地形素材使用规范.md` 对齐并标注 |

> **未重新抓取**：`pixelfrog-assets.itch.io` 官方页面本轮未联网重抓（环境网络受限）；官方口径以**本地官方示意图 + 本地原始 PNG + guide 仓库同 commit 图片**为准，不把第三方说明当作官方声明。

### A.2 三编号分离（历史缺陷根因）

| 编号 | 定义 | 示例 |
| --- | --- | --- |
| 语义编号 | 类别内 1..16（Flat/Elevated）/ 25-32（Stairs），仅规则表内部使用 | 语义 5 = 中心 |
| raw 图集编号 | 9 列 × 6 行 row-major 的 **1..54**（guide "sheet id"） | raw 11 / raw 16 |
| Unity sprite | 切图名 `Tilemap_colorN_{raw−1}`（**0 基**） | `Tilemap_color1_10` / `Tilemap_color1_15` |

历史代码把三者混用（`_36/_39/_45/_48` 当方向名、`_10` 当高程中心），本轮在 `TerrainTileRules` 中显式建表并用**独立测试**（`TerrainGuideRulesTests` 直接解析 `guide.json` 坐标自算 raw）锁定。

### A.3 关键差异矩阵（摘要；完整 40 行见 `rule-mapping.csv`）

| 规则 | 参考（guide/官方） | 旧生产实现 | 本轮修改 |
| --- | --- | --- | --- |
| 平地选片 | `flatGroundLookup` **16 掩码**（含 10/11/12 横条、13/14/15 竖条、16 孤立） | RuleTile 邻接匹配：仅 8 个单边片 + 中心；掩码 5/7/10/11/13/14 无片 → 退中心或用左族海岸片补偿 | `TerrainTileRules.FlatSheetId(mask)` 显式查表 |
| 高程顶面内芯 | 语义 5 → **raw 16** | RuleTile 兜底槽用 **Flat 中心 raw 11** ⇒ 台面与低地同色 | `ElevatedCenterRaw=16`；`TerrainSheetFamily.FillMissingFromTexture` 补规则未引用的切片 |
| 高程顶面边缘 | 语义 = 同一 lookup 结果 | 同上（8 片） | `ElevatedSheetId(mask)` |
| 崖脚 | **单行**，落点 = 高格**南邻格**；南邻水 ⇒ 水脚族画在**水格** | 高格南邻；Mount→低地额外叠一行（y−2）；东西缘也写墙 | `CollectWalls` 只写一行、只写南面；水脚落水格 |
| 端帽 | 侧向**无续接墙**才收口；两侧开放=窄件 | 由三行邻域墙格推导（近似） | `CliffVariant(leftOpen,rightOpen)` 严格按同行续接 |
| 坡路 | 2 格列（上 raw37/40，下 raw46/49），平台角**水平相邻上段**（对角接入锚点），角换中心片，落脚 2×2 口袋 | 单格三角坡落在台地南缘角（`CollectSlopes` 旧几何），且配对反向 | `CollectRamps` guide 几何 + `sideRampAdjacency`/`cliffBaseAdjacency`/`rampBaseGroundApron` |
| 阴影 | 仅南缘「南邻更低陆 + 南南邻陆」；水格抑制 | **每高格一张** ⇒ 台地东/西缘叠成方形暗带（制作人 Iteration-10 反馈伪影） | `RebuildShadows` 按 guide 条件 |
| 海岸白浪 | 泡沫在 el-0 地表下；高地临水用水脚件自带水线 | `WaterEdgeBand` 派生层补偿左族缺片，易与内陆混用 | band 层**整体退役**（16 掩码齐全后不再需要补偿） |

---

## B. 悬崖与多层地表

### B.0 官方示意图核对（本轮重新查看）

`Tiny-Swords-官方素材示意图.jpg`（1520×1155，官方素材页主图）可见：

1. 所有石质崖壁都是**单一 rock 行**贴在草唇下方，**没有上下叠两行**；
2. 台地**东/西/北缘没有石墙**，只有草边深描边；
3. 坡路为**2 格成对**，草坡朝下、石脚在低端，贴着平台角；
4. 高地与低地之间是**色族递进**（不同绿）而不是同色。

本轮生产实现的四项直接对应（见 §B.1–B.3 的逐格数据与截图）。

### B.1 单级台地（教材式，确定性场景 S1）

`capture-final/S1-plateau-hill.png` + `S1-plateau-hill-cells.csv`：

| 格 | 层 | 实际 sprite | 期望（guide） |
| --- | --- | --- | --- |
| (7,7) 内芯 | Elevated | `Tilemap_color2_15`（raw 16） | 语义 5（中心）✅ |
| (7,7) 南缘唇 | Elevated | `Tilemap_color2_24`（raw 25） | mask 1101 → 语义 8 ✅ |
| (7,6) 崖脚 | CliffWall | `Tilemap_color1_42`（raw 43，中段） | 两侧续接 → 18 ✅ |
| (4,6)/(11,6) 端帽 | CliffWall | `_41` / `_43` | 左/右端帽 ✅ |
| (7,5) 下方 | CliffWall | 空 | 单行，无叠墙 ✅ |
| 低地 (1,1) | Terrain | `Tilemap_color1_10`（raw 11） | Flat 中心 ✅ |

### B.2 两级台地（S2，三层必须可见）

`capture-final/S2-two-tier.png`：

- Mount 内芯 `Tilemap_color3_15`（raw 16）；
- Mount 南缘唇 `Tilemap_color3_24`（raw 25）+ 崖脚 `Tilemap_color1_42`（下一行）；
- **中间 Hill 台面 `Tilemap_color2_15` 连续 3 行可见**，未被上位崖脚覆盖；
- Hill 南缘唇 `Tilemap_color2_24` + 崖脚 `Tilemap_color1_42`；
- 低地 `Tilemap_color1_10`。

旧实现此处的两处错误已被回归测试锁定：① 台面内芯用 Flat 中心（同色）；② 旧 `IsMountFacadeCell` 让 Mount 南邻 Hill 格**不画顶面**且**被墙覆盖** ⇒ 中间层消失。

### B.3 东西向高差 / 北缘（S3）

`capture-final/S3-edges-and-nwe.png`：高格西缘为 `elevatedGround 4`（草边深描边），东缘 `elevatedGround 6`，北缘 `elevatedGround 2`；**三个方向的低邻格均无崖脚片**（`Plan_NorthEastWestLowerNeighbors_ProduceNoWall` 断言）。低地邻格仍是 Flat 中心片（高程同类 ⇒ 无水岸片）。

### B.4 水脚墙（真实水岸）

`capture-final/S6-water-cliff.png`、`game-seed-20261013/G6-water-foot.png`：高格南邻为水 ⇒ 崖脚画在**水格**上，取 raw 51..54（`_50.._53`，底缘自带泡沫水线）；水格地形/水底色不被改写（`Renderer_CliffIntoWater_UsesWaterFootOnWaterCell`）。

### B.5 阴影

`game-seed-20261013/G9-shadow.png`（修正后）vs `game-seed-20261013-prefix/G9-shadow.png`（修正前）：
旧实现每个高格一张 ⇒ 台地东/西缘纵向叠成**方形暗带**；现仅南缘承接面一张，软边、无直角。抑制条件按 guide：南邻为水或南南邻为水 ⇒ 整张抑制。

---

## C. 坡路

`capture-final/S1-plateau-hill-cells.csv` 的 Slope 层实取：

| 格 | 实际 sprite | 含义 |
| --- | --- | --- |
| (5,7) | `Tilemap_color1_36`（raw 37） | 左坡上段 |
| (5,6) | `Tilemap_color1_45`（raw 46） | 左坡下段（石脚） |
| (18,7) | `Tilemap_color1_39`（raw 40） | 右坡上段 |
| (18,6) | `Tilemap_color1_48`（raw 49） | 右坡下段 |
| (6,7) | `Tilemap_color2_15`（中心） | 平台角按 `sideRampAdjacency` 去边 |

几何（Unity y 向上，guide y 向下已显式转换）：

- 平台角 P=(6,7) 高程 1；坡上段 T=(5,7)、下段 L=(5,6) 均为高程 0；
- P 与 T **同排水平相邻**，锚点 L 与 P 成对角（dx=−1, dy=+1）——与 guide `leftRampTouchOffset` 转换后一致；
- 坡脚紧邻的平台角崖脚 (6,6) 按 `cliffBaseAdjacency` 取中段（`_42`）；
- 落脚侧 2×2 可行走口袋 + 坡底正下方一格全为高程 0 陆地（`Plan_Ramp_RequiresWalkableApron`）；
- 0 级落脚不得临水（`Plan_Ramp_Level0Landing_MustNotTouchWater`）；
- 每同高程区域 ≥40 格才配坡，至多 `max(1, size/40)` 处、间距 ≥8（`accessibilityRules`）。

真实场景：`game-seed-20261013/G4-ramp-left.png`、`G5-ramp-right.png`；三种子全部坡路的几何一致性由 `Plan_Ramp_GeometryMatchesGuide_ThreeSeedMaps` 断言。

---

## D. 内陆接缝与水岸

### D.1 16 掩码全量实绘

`Iter11_FlatMask_WaterExposure_UsesGuideSheet(0..15)`（16 个用例）逐掩码断言：中心格 flat 片 = `guide.json` 查表所得 raw。用例覆盖 0000（孤立）→ 1111（内芯）全部。

### D.2 同高程不同材质（草↔林）

`Iter11_InlandMaterialBoundary_NeverUsesShoreArt(0..15)`：草格四周为森林（同高程陆地）时，掩码恒 1111 ⇒ 必须取**中心片**（100% 不透明），且 `WaterEdgeTilemap` 无任何片。旧实现按 TerrainType 判暴露 ⇒ 每个草↔林边界都取到边缘/水岸片，是"内陆水色缝 + 错误边缘"的直接原因。真实画面：`capture-final/S5-grass-forest.png`、`game-seed-20261013/G8-forest-edge.png`。

### D.3 海岸白浪不再内陆误用

`WaterEdgeBand` 派生层整体退役（字段保留但不再铺设）：`Iter11_RenderOutputScan_Seed` 三种子全图断言 `WaterEdgeTilemap` 任一格皆空。真实海岸白浪仍由 Flat 层水岸片 + `WaterFoamRenderer` 承担（`game-seed-20261013/G10-coast-foam.png`）。

### D.4 色族匹配（color1 / color3）

| 高程 | 草地色族 | 森林色族 |
| --- | --- | --- |
| 0 | color1（`RT_Grass`） | color4（`RT_Forest`） |
| 1（Hill） | **color2**（`RT_Grass_color2`，`Main.unity` 已接线） | color4 |
| 2（Mount） | color3（`RT_Grass_color3`） | color4 |

同一几何角色一律取**同语义编号**的切片，只是色族不同（`TerrainTileRules.ElevatedSheetId` 与色族正交）。旧实现 Hill 与低地同为 color1 ⇒ 台面不可读。

### D.5 泡沫

`NeedsFoam`：仅 Grass/Forest 且 4 邻有水；水格不铺；水脚墙落水格 ⇒ 天然不铺。`TerrainFoamPlacementTests` 两种子用例逐格断言"泡沫存在 ⇔ 邻水陆格"。

---

## E. 测试

| 套件 | 结果 | 证据 |
| --- | --- | --- |
| EditMode 全量 | **361 / 361 通过，0 失败，0 跳过** | `tests/editmode-final.xml` + `tests/editmode-final.log` |
| PlayMode 全量 | 见 `tests/playmode-final.xml`（函数性用例全过；证据捕获套件按 D-063-A 失败关闭） | `tests/playmode-final.xml` + `.log` |
| 真实游戏截图 | 三种子各 8–10 张（`game-seed-*`），全部来自 Main.unity + GameBootstrap 正式装配 | 见目录 |

### E.1 新增/重写的独立测试

- `TerrainGuideRulesTests`（17 例）：直接解析固定版 `guide.json` 自算 raw 与掩码表，逐格断言三种子全图（`Renderer_ThreeSeeds_EveryCellMatchesGuidePiece`，每种子 >8000 陆格）、崖脚单行/南邻、坡路几何、阴影条件、16 掩码。
- `Iter11RenderOutputScanTests`（35 例）：16 掩码实绘 + 16 例内陆材质边界 + 三种子全图输出扫描（层覆盖、水格、墙脚族、坡路对）。
- `CliffTileSelectionTests`：旧模型用例（双段墙 / 东西缘墙 / 单格三角坡 / 旧台阶选位 / 每高格阴影）**整体退役**，替换为 12 例 guide 口径用例（单行墙脚、端帽续接、水脚落水、坡路 2 格/对角锚点/口袋/临水/区域预算、两层可见、水脚族、林界件）。
- `TerrainTileSelectionAuditTests`：全图选片扫描改为 guide 16 掩码 + 色族 + 顶面；掩码表本身由 `TerrainGuideRulesTests` 独立校验。

### E.2 退役与替代（如实登记）

| 退役文件/用例 | 原因 | 替代 |
| --- | --- | --- |
| `Iter09RenderOutputScanTests.cs`（33 例） | 断言"左族/右族 + WaterEdgeBand 补偿"，前提是 RuleTile 只有 8 片；band 层已退役 | `Iter11RenderOutputScanTests`（35 例） |
| `Iter10AcceptanceTests.cs`（4 例） | P0–P3 扫描器编码 `IsMountFacade`/band/双段墙旧语义 | `Iter11RenderOutputScanTests` + `TerrainGuideRulesTests` |
| `CliffTileSelectionTests` 中 34 例 | 旧几何/旧行选择/旧台阶 | 上述 12 例 guide 用例 + `TerrainGuideRulesTests` |
| `TerrainCompositionTests` 2 例 | `ComputeElevatedEdgeMask` API 随 RuleTile 选片路径删除 | 同文件改为 `TerrainTileRules` 掩码互转/方向性用例 |

> 未删除任何"仍然正确"的旧用例：资产切片映射、旧 Cliff 兼容路径、无高程向后兼容、WarnMissingTiles、确定性、背景垫底、泡沫排除等仍保留并通过。

---

## F. 证据与仓库保护

- 视觉证据仓库：`https://github.com/Lucifer-zql/WorldSandboxPrototype-visual-evidence.git`
- 本轮目录：`evidence/terrain-render-integrity-2026-10-17/iteration-11/`
- 上传方式：仅新增（`iteration-00 … iteration-10` 未触碰；发布前后 `git ls-tree` 核对）
- 远端校验：对新增文件逐个下载原始字节复算 SHA-256，与 `SHA256SUMS.txt` 比对（结果见 §F.2）
- 游戏主仓库：HEAD `f85fbe3`，`git status` 与本轮开始时一致（生产修改仍为本地未提交）

### F.2 远端校验结果

- 证据仓库提交链：`bb8370c`（iteration-11 首次发布，295 文件）→ `5666909`（清单路径修正）→ `c848585`（清单定稿）；
  其后仅追加本报告终稿与校验记录，**以仓库最终 HEAD 为准**。
- 远端字节校验（sparse clone 经本地代理取真实字节）：清单所列 **294 文件全部 ok，bad=0，missing=0**。
- 独立 raw 抓取：`final-report.md` 原始字节复算 SHA-256 与清单一致（`75F21039…B2F299`，当时版本）。
- 历史零覆盖：`git diff --name-only 3d23e70 HEAD` 仅列 `iteration-11/` 之下文件；
  `iteration-00…10` 文件数 111/134/71/145/170/147/242/388/310/755/866 保持。
- 明细：`tests/remote-verify.txt`。

---

## G. 剩余问题（不写成通过）

1. **细碎台地（1 行台面）**：FastNoise 高程场常产生 1 行宽的台面，渲染上是"唇行 + 墙行"逐级楼梯，视觉密度高。数据层不违反 guide 的 1 级落差约束，但不如官方示意图的宽台地舒展。**属于地形生成设计（D-061）而非渲染缺陷**，本轮未改地貌比例。
2. **临水 1 行台面**：当 Mount→Hill 的中间台面正下方就是水时，台面无法加宽（不得改写水格），该列会出现上下相邻两行墙（陆脚 + 水脚）。已由 `Plan_ThreeSeeds_WallsOnlyAtSouthDrops_SingleRow` 显式放行并注明原因。
3. **阴影可见度适配**：本地 `Shadow.png` 可见 alpha 仅 ~80×80（guide 记录其素材为 192 画布 / 128 可见）。按 1:1 摆放会被单行崖脚完全遮住，故按 128/80=1.6 放大可见团。此为**素材版本差异下的显式适配**，已写入代码注释；若后续换用 guide 版 Shadow 素材应恢复 1:1。
4. **seed 20261013 无 Mount→Hill→低地三段结构**：该种子只有 Mount→Hill→水/低地 或 1 级台地，`G3-two-tier.png` 缺席（14/15 有）。属数据分布，不是渲染缺失。
5. **Pixel Frog itch.io 页面未重新抓取**：以本地官方示意图 + 原始 PNG 为准，官方文本口径本轮未复核。
6. **PlayMode 证据捕获套件**：按 D-063-A 失败关闭（缺 `AIA_CAPTURE_DIR` / 历史目录），非本轮功能回归；真实游戏截图改用 `Iter11GameCaptureTests`（RenderTexture 路径）完成。
