# iteration-10 —— D-065 悬崖/三角坡路/多层地表/内陆水缝 全面修复

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 依据：制作人 Iteration-10 任务书（以真实游戏画面为最终标准）+ **官方 Tilemap Guide 复核对齐**
> （https://pixelfrog-assets.itch.io/tiny-swords/devlog/1138989/tilemap-guide，本轮实抓全文与 4 张示例图）。

## 一、官方依据（本轮新增的关键事实）

官方 Guide 全文关键规则（本地留存：`guide-elevated.png` 等 4 图，下载于临时目录并经主 Agent 逐图查看）：

1. **Elevated Ground** = 抬升地形，**每一级都包含面向下方地形的悬崖**（"contains two cliffs, one facing
   the terrain below it and the other directly facing the water"——两类悬崖，陆地侧与水侧）。
2. **Stairs（楼梯）** = 连接任意下层地形与上层地形的路径；**官方图例为单格（64×64）内的
   "三角草面 + 底部石脚"**。Example 1：楼梯旁为可走地面（无悬崖）时，顶部仅需一块 Elevated 顶面片连接；
   Example 2：楼梯旁为 Cliff 时，需两片（顶面片 + 悬崖底片）。
3. **Shadow** 与 Elevated 层**逐级重复**；Shadow 向下平移一整格。
4. **Flat Ground 图例无三角片**——三角/楼梯片只属于 Stairs。

**素材像素实测（本轮）**：`_45`（c0r5）石脚 x 重心 = 42（右）、`_48`（c3r5）石脚 x 重心 = 21（左）——
**石脚必须在坡的低端**；`_36/_39` 为纯三角（α≈50%），**不是完整楼梯**。

## 二、根因与修复

### P0：悬崖与逐层地表

- **根因**：iter-08 以"墙覆盖中间台面"为由删除了 **Mount→Hill 的悬崖墙**，导致 Mount 与 Hill 之间
  **无任何可见立面**（官方要求每级都有面向下方的悬崖）；同时把 Mount 的立面格误当作可走台面。
- **修复**：`TerrainOverlayPlan.CollectWalls/CollectSideWall` **恢复 Mount→Hill 墙**（南/东/西缘）；
  `TerrainTilemapRenderer.IsMountFacadeCell`（Hill 且**北/东/西**邻 Mount）——该格是**Mount 立面格**，
  **不画 Hill 台面顶面**（官方悬崖格不可走），由墙承担可见面；立面格不铺冗余 band。
- **结果**：Mount 顶面 → Mount 立面墙 → Hill 台面 → Hill 南缘墙 → 低地，**逐层完整**（TS9f/TS2a）。

### P1：三角坡路

- **根因（三重）**：① iter-08 的"上段 `_36/_39` + 下段 `_45/_48`"把 **1 格高差拉成 2 格**，下段额外
  插入低地；② **下段左右配对反了**（Left→`_45` 石脚在高端）；③ **孤立坡剔除**（iter-07 推断
  "坡必须与墙连成一体"）删除了官方允许的独立楼梯入口。
- **修复**：坡渲染改为**单格楼梯**——`Side==Left`（高地在东、低端在左）→ `stairTiles[3]`（`_48`，石脚左下）；
  `Side==Right` → `stairTiles[1]`（`_45`，石脚右下）；**不再画 `_36/_39` 上段、不再有下段格**；
  台阶下段同规则（LeftPair→`_48`、RightPair→`_45`）；`PostProcessSlopes` **保留全部合法坡**
  （不再剔除/补墙）；`ReservedByOverlay` 只占用坡格本身（下段不再占用，释放其下方低格/水格墙）。
- **结果**：楼梯单格、石脚在低端、顶部紧贴高地顶面（TS9d/e、TS2a 实拍）；随机地图入口自然。

### P2：内陆水缝与海岸素材

- **复核**：iter-09 的"混合缘/纯水缘 → 右族 + band"与 color1/color3 边带维持；本轮修复**立面格冗余
  band**（墙整格覆盖，4 格计划外 band 消除）。
- **结果**：混合缘全 15 掩码网格（TS9a）水向白线只在水的方向、陆向（Forest）无白浪；纯水缘全掩码
  （TS9i/j）water 侧白线正确；全图（TS1a）无内陆水色穿透。

## 三、生产代码差异（相对 iteration-09）

| 文件 | 改动 |
| --- | --- |
| `TerrainOverlayPlan.cs` | 恢复 Mount→Hill 墙（南/东/西缘）；`ReservedByOverlay` 移除坡下段占用；`PostProcessSlopes` 不再剔除孤立坡/补墙 |
| `TerrainTilemapRenderer.cs` | `IsMountFacadeCell`（北/东/西邻 Mount 的 Hill 格不画台面顶面、不铺 band）；坡改单格楼梯（`_48`/`_45`，石脚低端）；台阶下段同规则 |
| 测试 | `CliffTileSelectionTests`：墙数/变体/坡断言按新语义更新（墙或坡覆盖高差边缘；Mount 立面格无顶面有墙）；`TerrainFoamPlacementTests` 坡/墙数更新 |

**未修改**：地形生成（TerrainStore/ElevationStore/WorldGenerator）、`SortingOrders`、官方原始 PNG、`Main.unity`。

## 四、测试与扫描

- **EditMode 368/368 全过**（含 Agent C 新增 `Iter10AcceptanceTests`：真实 Tilemap 输出 + 计划↔结构校验 +
  确定性多层场景 + band 全掩码；以及 Iter09 的真实输出扫描）。
- PlayMode 三种子：`regression-final/seed-*`（每个 79 图 + XML + log）。
- 判据：**实际 `Tilemap.GetTile` 输出**（Flat/Elevated/CliffWall/Slope/Stairs/WaterEdge）+ 计划↔实际双向 +
  CSV 逐格留痕（`C:\Users\18871\AppData\Local\Temp\iter10-scan\`，发布时归档至本目录）。

## 五、目录

```
iteration-10/
  baseline/                     # HEAD f85fbe3、生产文件 SHA-256、repo-status、iter-09 完整性
  P0-cliff-and-layer-stack/     # post-fix：TS9f/g、TS2a/b（多层逐级立面实拍）
  P1-triangle-slope-placement/  # post-fix：TS9d/e、TS7c（单格楼梯、石脚低端）
  P2-inland-water-gap/          # post-fix：TS9a/i/j、TS1a、TS7e（水向边带/无内陆水缝）
  regression-final/seed-*/      # 最终代码三种子全量真实截图（79 图/种子 + XML + log）
  verify-r1/seed-*/             # 最终代码前的中间验证批（保留回溯；不冒充最终证据）
  P0P1-experiment/seed-*/       # P0/P1 修复过程中的实验批（TS9，保留修复轨迹）
  P1-single-stair/seed-*/       # 单格楼梯实验批（保留轨迹）
  test-evidence/                # EditMode 全量与过程失败 XML（待补）
  README.md / final-report.md / SHA256SUMS.txt
```

**pre-fix 引用**：`iteration-09/regression-final-r2/seed-*`（iter-09 终态 = 本轮修复前代码的真实画面）；
`iteration-10/P0P1-experiment` 为 P0/P1 首个修复（恢复 Mount 立面 + 下段反向）后的中间态。

## 六、如实声明与待验证

1. **官方坡路摆法**：Guide 提供 Stairs 图例（单格三角草+石脚）与 Example 1/2 规则；**项目实现已按此对齐**
   （单格楼梯、顶部贴顶面、旁 Cliff 时与墙相接）。但 Guide 未展示多级连续楼梯的官方实例——本项目"每个
   低格入口一个坡"的生成规则为**项目约定**，需以真实画面持续验收。
2. **`_36/_39` 三角片**：本轮判断其不是完整楼梯（无石脚），改由 `_45/_48` 承担楼梯；`_36/_39` 目前**不再
   用于坡渲染**（资产保留）。官方 Guide 的三角形态与 `_45` 的差异（示意图 vs 实际素材）**待验证**。
3. **立面格语义**：Mount 南/东/西邻 Hill 格被渲染为 Mount 立面（无台面顶面）；数据层仍是 Hill（可走性
   与视觉不一致是官方悬崖格的固有语义）——**待制作人目视确认可接受**。
4. **Agent A/B/D**：本轮运行中（A 素材实测、B 链路审计、D 视觉挑刺），主 Agent 独立完成官方 Guide 抓取、
   素材像素实测、生产修复与全部测试；Agent 报告送达后补记。
5. 历史证据 iter-00~09 未覆盖；主仓库零 git 写。
