# D-065 Iteration-10 最终报告

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜三种子 20261013/14/15
> 依据：制作人 Iteration-10 任务书 + **官方 Tiny Swords Tilemap Guide**（本轮实抓 / 4 张官方示例图逐图查看 / 素材像素实测）。

## 1. 每个缺陷的真实根因

| 缺陷 | 根因（本轮实证） |
| --- | --- |
| **P0 悬崖位置与多层地表** | iter-08 以"墙覆盖中间台面"为由**删除了 Mount→Hill 的悬崖墙**——官方 Guide 明确"Elevated Ground **每一级**都包含面向下方地形的悬崖"，删除后 Mount 与 Hill 之间无立面（"缺层"）。同时把 Mount 的立面格误当可走台面。 |
| **P1 三角坡路孤立/错位** | ① 双格结构（`_36/_39` 上段 + `_45/_48` 下段）把 **1 格高差拉成 2 格**，下段额外插入低地；② 下段**左右配对反了**（像素实测 `_45` 石脚 x=42 在右、`_48` 石脚 x=21 在左——左向坡低端在左却用了石脚在右的 `_45`）；③ `PostProcessSlopes` 的"孤立坡剔除"（iter-07 推断）删除了官方允许的独立楼梯。 |
| **P2 内陆水缝/海岸误用** | iter-09 的混合缘/纯水缘分族已正确；本轮发现**Mount 立面格的冗余 band**（墙整格覆盖，band 无意义，4 格计划外）；color1/color3 边带维持 iter-09 方案。 |

## 2. 修改的生产文件与函数

| 文件 | 函数/位置 | 修改 |
| --- | --- | --- |
| `World/GenSteps/TerrainOverlayPlan.cs` | `CollectWalls`（南缘）、`CollectSideWall`（东/西缘） | **恢复 Mount→Hill 墙**（删除 iter-08 的两处跳过） |
| 同上 | `ReservedByOverlay` | 坡**只占用坡格本身**（移除 `LowerY` 占用——下段不再渲染） |
| 同上 | `PostProcessSlopes` | **保留全部合法坡**（不再剔除孤立坡/补墙） |
| `Presentation/TerrainTilemapRenderer.cs` | 新增 `IsMountFacadeCell` | Hill 且**北/东/西**邻 Mount ⇒ Mount 立面格：**不画 Hill 台面顶面**、**不铺 band** |
| 同上 | `LayOverlays`（坡） | **单格楼梯**：`Side==Left → stairTiles[3]`（`_48`，石脚左下）、`Right → stairTiles[1]`（`_45`，石脚右下）；不再画上段/下段格 |
| 同上 | `LayOverlays`（台阶） | 下段：`LeftPair → stairTiles[3]`、`RightPair → stairTiles[1]` |
| 测试 | `CliffTileSelectionTests` / `TerrainFoamPlacementTests` | 墙数/变体/坡断言按新语义更新（高差边缘由"墙或坡"覆盖；Mount 立面格无顶面有墙） |

**未修改**：地形生成、`SortingOrders`、`Main.unity`、官方原始 PNG、素材导入设置。

## 3. 修改前后行为对照

| 场景 | 修改前（iter-09） | 修改后（iter-10） |
| --- | --- | --- |
| Mount 南缘 | Mount 顶面直接接 Hill 顶面（**无立面**） | Mount 顶面 → **Mount 立面墙**（Hill 格上的 Row4 墙，无台面顶面）→ Hill 台面 |
| 坡（1 格高差） | `_36` 三角（低格）+ 下段 `_45`（再低一格）→ **2 格**，石脚在高端 | **单格** `_48`/`_45`，石脚在**低端**，顶部紧贴高地顶面 |
| 孤立坡 | 剔除并补墙（合法入口被删） | **保留**（官方 Example 1 语义） |
| 立面格 band | 铺（被墙覆盖，冗余） | 不铺（计划外 band = 0） |

## 4. 悬崖位置/方向/端帽/转角

- 墙坐标：Mount/Hill 的**南邻/东邻/西邻低格**（CollectWalls/SideWall），逐格核对（Iter09 扫描 P0 全格 + Iter10 计划↔实际）。
- 端帽/转角：Variant 由相邻墙连续性推导；坡替代墙后相关端帽同步更新（测试按实际输出更新）。
- 立面格（Mount 邻 Hill）：**有墙、无台面顶面**（官方悬崖格语义）——P0 midbench 扫描反转验证。
- 实拍：`P0-cliff-and-layer-stack/post-fix/TS9f-p0-two-tier-south-seed-20261013.png`（Mount→立面→Hill 台→Hill 墙→低地）、`TS2a-waterfoot-cliff.png`（真实地图水岸悬崖）。

## 5. 坡路位置/方向/上下端

- 单格楼梯：低格（高差入口），`Side` 由高地在东/西决定；上端紧贴高格顶面（实拍 TS9d/e、TS2a 可见楼梯顶端与顶面相连）。
- 下端：石脚在低端（`_48` 左 / `_45` 右）；**不再有下段格**（其下方低地不被覆盖）。
- 墙连接：坡格由 `ReservedByOverlay` 让位，不写字；坡旁墙保持（豁口自然）。
- 实拍：`P1-triangle-slope-placement/post-fix/TS9d-p2-slope-run-west-seed-20261013.png`（连续坡列）、`TS9e-p2-short-slope-run-seed-20261013.png`（短坡列）、`TS2a-waterfoot-cliff.png`（真实地图单楼梯）。

## 6. 内陆水缝与海岸素材扫描

- 扫描器：Iter09 真实输出扫描（P0 全格 49152、P1 水向暴露 760/579/756、P2 坡+台阶+计划外）+ Iter10 测试。
- 结果：**0 违规**（含计划外 band = 0）；混合缘全 15 掩码/纯水缘全掩码确定性场景实拍（TS9a/i/j）。
- 视觉：`P2-inland-water-gap/post-fix/TS1a-fullmap-ortho40.png`（全图无内陆水色穿透）、`TS7e`（混合缘）。

## 7. Mount/Hill/低地逐层覆盖

- 判定（实际 Tilemap 输出）：每组相邻高程差都由"墙或坡"覆盖（Iter10 P1 结构校验 4050 项）；Mount 立面格有墙无顶面；Hill 台面有顶面；低地有 Terrain。
- 实拍：TS9f/TS9g（确定性）、TS2a/TS2b（随机地图）、TS1a（全图）。

## 8. color1 / color3 边带

- 维持 iter-09：混合缘/纯水缘缺片 → 右族 + band；band 派生件为 **color1 色带**（15 掩码）。
- **color3 Mount 顶面使用 color1 色带**的色差为**已知局限**（iter-09 起如实登记；本轮未新增 color3 派生件——**待制作人目视裁定是否需要**）。

## 9-10. 截图

- 三种子全量：`regression-final/seed-20261013/`、`seed-20261014/`、`seed-20261015/`（各 79 图）。
- P0：`P0-cliff-and-layer-stack/post-fix/`（TS9f、TS9g、TS2a、TS2b）。
- P1：`P1-triangle-slope-placement/post-fix/`（TS9d、TS9e、TS7c）。
- P2：`P2-inland-water-gap/post-fix/`（TS9a、TS9i、TS9j、TS1a、TS7e）。
- 中间轨迹：`verify-r1/`、`P0P1-experiment/`、`P1-single-stair/`（保留，不冒充最终）。

## 11. 自动化测试

- **EditMode 368/368 全过**（含 `Iter10AcceptanceTests` 与 Iter09 真实输出扫描）；过程失败 XML 保留于 `test-evidence/`（待归档）。
- PlayMode 三种子：`regression-final/seed-*/test-results.xml`（16 passed / 1 skipped / 0 failed，skip 为既有 tpr3）。

## 12. 证据仓库提交

（发布后补：`iteration-10` 提交 SHA）

## 13. 远端 SHA-256 核验

（发布后补：稀疏 clone + `sha256sum -c` 结果）

## 14. 历史证据零覆盖

（发布后补：iter-00~09 目录核验）

## 15. 主仓库状态

- HEAD 仍为 `f85fbe3`（零 git 写）；工作区保留本轮生产修改与测试更新（未提交/未推送）。

## 16. 仍然存在的缺陷、未验证项与限制

1. **官方多级连续楼梯实例缺失**：Guide 未展示多级连续楼梯；本项目"每低格入口一个坡"是项目约定，真实地图中可能出现连续楼梯（TS9d 构造展示 3 个纵列楼梯）——**需制作人目视确认可接受**。
2. **`_36/_39` 三角片用途**：本轮判定其非完整楼梯（无石脚），不再用于坡渲染；官方示意图的三角形态与 `_45` 的对应关系**待验证**。
3. **color3 边带色差**：Mount 顶面水缘使用 color1 band（已知局限）。
4. **立面格可走性**：数据层 Hill 可走，但视觉是悬崖格（官方素材体系固有）；**待制作人确认**。
5. **Agent A/B/D**：本轮未在交付前返回报告（主 Agent 独立完成官方 Guide 抓取与修复）；报告送达后补记。
