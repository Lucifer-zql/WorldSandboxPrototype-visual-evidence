# D-065 Iteration-08 最终报告

> 生成：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜三种子 20261013/14/15｜证据仓库
> `evidence/terrain-render-integrity-2026-10-17/iteration-08/`

## A. 生产代码修改

| 文件 | 函数/行为 | 根因与效果 |
| --- | --- | --- |
| `TerrainOverlayPlan.cs` | `CollectWalls` 南缘 + `CollectSideWall` 东西缘：**Mount→Hill 不写墙** | 墙片整格高，原把 Hill 中间台面当墙格覆盖（143/231/156 格）——修复后 Mount 顶面→Hill 台面→Hill 墙→低地逐层可见 |
| 同上 | `SlopeCell.LowerY`（=Y−1）+ `CollectSlopes` **坡下端条件**（南邻低地干燥陆格） | 坡道是 1 宽×2 高连接件；41/28/16 个坡下端原落在水/更高地，现 0 |
| 同上 | `ReservedByOverlay` 覆盖坡**上段+下段**两格；`PostProcessSlopes` 剔除时**上/下段补墙**（`AddFallbackWall`）；新公开 `IsSlopeUpper` | 修复豁口残留（missingWall 41/15/12→0） |
| `TerrainTilemapRenderer.cs` | `ResolveFlatBaseTile`/`ResolveElevatedTopTile` 增加 **waterMask/landMask 逐方向拆分**；混合缘取**右族** + `WaterEdgeOverlay(162)` 层铺水向边带（`GetWaterEdgeBandTile` 缓存） | 单格单 Tile 无法逐方向混族——band 方案让水向有浅青白、陆向无白 |
| `TerrainTilemapRenderer.cs` | `LayOverlays`：坡**下段** `_45/_48`（仅坡列底部） | 坡道成对 |
| `SortingOrders.cs` | `WaterEdgeOverlay = 162`（04 R-LAYER-10c'） | 新层序 |
| `Main.unity` | `waterEdgeBandSprites` 16 槽 | band 素材接线 |
| `Art/Terrain/Derived/WaterEdgeBand/*.png(+meta)` | 8 张水岸边带派生件（左族掩码片 16px 边带，其余透明） | 原始素材零改动、确定性派生 |

规则文档同步：`docs/04` R-LAYER-10b（坡成对）/10c'（新层）。

## B. P0 逐层地形

**结构**：Mount（elev2，color3 顶面）→ 顶面边缘件（1 级高差）→ **Hill 中间台面（elev1，完整）**
→ Hill 南缘墙（Row4/5 视下邻）→ 低层陆地（Flat `_10` 100% 不透明底片）；东西缘墙列沿 Hill 外缘；
北缘顶面过渡（官方无墙）；Mount 直落低地时双段墙保持；Mount 临水时情形 A 深水收口。
**中间层数据**：Mount 南/东/西直邻低地 0/0/0（无跳级）；渲染层被覆盖台面 143/231/156 → **0**。

**全图扫描**（`P0-layered-terrain-stack/p0-connections-seed-*.csv`，独立判定实现）：

| 种子 | 应存在连接 | 合法 | 缺墙 | 缺低层地表 | 缺中间层 | 非法水脚 | 内陆墙下水背景 | 无法解释 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 20261013 | 807 | 807 | 0 | 0 | 0 | 0 | 0 | 0 |
| 20261014 | 1135 | 1135 | 0 | 0 | 0 | 0 | 0 | 0 |
| 20261015 | 978 | 978 | 0 | 0 | 0 | 0 | 0 | 0 |

单列合法语义：北缘顶面过渡 190/294/285；窄水沟不写水格 13/4/11；**岩壁入水（水格墙，官方形态）
132/76/91**（墙底缘烘焙水线，设计内）。
**排序核查**：`SortingOrders` 常量与 04 表 18 条逐条一致（EditMode `MatchSpec` 全过）；
底片顺序经渲染层实测（Flat=0 低于所有覆盖层）。

**本轮两级退台真实运行时截图**（固定场景经生产渲染器，三种子各一份）：
`diagnostic-scene/TS8a-p0-two-tier-south-seed-*.png`（南缘逐层）、`TS8b-…-west-edge-slope`（侧缘）、
`TS8c-…-far-terrace`（远机位）；**三种子随机地图全图与近景**见 `regression-final/seed-*/`。
**测试**：EditMode 328/328（`test-evidence/editmode-r4.xml`）；PlayMode 三种子 15/14/0/1。

## C. P1 内陆边缘

**海岸素材调用路径（全链审计并修复）**：Flat 选族（`ResolveFlatBaseTile`）、顶面选族
（`ResolveElevatedTopTile`）、林界件（`ResolveForestEdgeTile`，iter-07 已改为始终右族）、森林自身边
（`ResolveForestSelfEdgeTile`，同）、RuleTile 兜底（异常路径，未改）。修复：混合缘逐方向分族 +
band 覆盖（见 A）。
**全图扫描**（`P1-inland-edge-select/p1-edges-seed-*.csv`）：内陆暴露边 1836/1804/1300，
水缘边 964/678/916；**内陆误用海岸整片 = 0**；混合缘格 67/90/95 全 band 覆盖（掩码 1/2/4/8，
均在派生件集合内，无回退）。判定不依赖固定 RGB（地形数据+掩码+族规则）。
**修复前后对照**：iter-07/regression-final vs iter-08/seed-* 固定机位 TS4a/MVO3b 差异 2.3–7.6%
（混合缘格区域）；混合缘定向场景 `diagnostic-scene/TS8d/TS8e-…-seed-*`（水向白/林向深色/西 Hill 坡
逐方向正确）。真正水岸回归 `TS4b`（差异 0.1–3.3%，水岸白线与泡沫保持）。

## D. P2 三角坡路

**官方对照**：`official-reference/`（官方示意图裁剪 + iter-07 观察记录）；坡成对依据由主 Agent 复测：
`_36` 底行 ↔ `_45` 顶行 **alpha 64/64 一致**、平均 RGB 差 8.1、**反向零重叠**；`_39/_48` 同（60/60）。
**真实方向/坐标/高程**：`P2-slope-connector/p2-slopes-seed-*.csv` 每行含坐标/切片/方向/东西邻
地形高程/上端合格/下段坐标地形高程/下端合格/邻墙/顶面。
**全图坡道扫描**：115/156/92 个坡 **全部合法**（上端错误 0、下端错误 0、孤立 0）；
坡总数较 iter-07 下降 = 剔除非法（下端无承接/双侧高/孤立）后的合法集。
**真实运行时近景**：`diagnostic-scene/TS8f/TS8g-…-slope-pair-*-seed-*`（坡列上段斜线连续 +
直角缘贴高台）、`TS8h/TS8i-…-slope-bottom-*-seed-*`（坡列底部下段断面落到低地）。
**测试**：坡/墙/台阶计数与上下端断言在 EditMode 中（r1→r4 独立重算过程留证）。

## E. 三种子完整回归

| 项 | 20261013 | 20261014 | 20261015 |
| --- | --- | --- | --- |
| 尺寸 | 128×128 | 128×128 | 128×128 |
| 高程（水/Hill/Mount） | 见扫描 summary | 同 | 同 |
| P0 连接合法率 | 807/807 | 1135/1135 | 978/978 |
| P1 内陆误用 | 0 | 0 | 0 |
| P2 坡合法 | 115/115 | 156/156 | 92/92 |
| PlayMode | 15/14/0/1 | 15/14/0/1 | 15/14/0/1 |
| 全图/近景 | `regression-final/seed-20261013/` | `…14/` | `…15/` |

## F. 证据保护

- 证据仓库提交 SHA 与远端核验：见提交记录与 `SHA256SUMS.txt`（远端 tarball 全量复算流程见打包记录）；
- **历史证据零覆盖**：iter-00~07 全量 `sha256sum -c` 通过（本轮仅新增 iteration-08 目录）；
- CaptureWriteGuard/CapturePathGuard 全程启用（`tri-` 白名单、已存在拒绝、历史根默认禁止）；
- 游戏主仓库：零 git 写（HEAD 未变；405 项既有未提交状态保留）。

## G. 剩余问题与证据局限
1. 水格墙（岩壁入水，132/76/91）为官方形态，扫描单列不计违规——若验收要求"内陆绝不出现水背景"，
   需另行裁决水格墙的保留范围（当前与官方示意图一致）。
2. band 派生件覆盖掩码 {1,2,3,4,6,8,9,12}；三种子实测无超出组合，未来地形若产生其它组合将回退左族。
3. 条件搜索机位（tpr2/TS2/TS6c）修复后命中实例可能不同，不作为同机位对照（固定机位 7 组为准）。
4. PlayMode 跳过项 1 个（`MVO_TPR3_StateContrast_SameFraming`，既有行为，未隐藏）。
5. Agent A/B 本轮超时（任务书允许主 Agent 继续）；其事后报告与主 Agent 结论互证，无冲突项。
