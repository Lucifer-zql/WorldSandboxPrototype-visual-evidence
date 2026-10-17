# P3 · 坡面/台阶/悬崖与官方示例对应（D-065 iter-06）

## 一、官方规则 → 项目实现映射表（任务书 §五 要求）

| 官方示例组合 | 项目素材/Tile | 生产规则（方法） | 实例与验证 |
| --- | --- | --- | --- |
| Elevated 顶面按 Flat Ground 拼接 | RT_Grass/RT_Grass_color3 双族显式选片（`ResolveElevatedTopTile`） | 内芯中心片/水缘左族/陆缘右族；**iter-06 起含 1 级降边** | `TS6c`/twotier 图；`Render_Elevated_OneLevelDropShowsEdgeTiles_Iter06P2` |
| 顶面边缘与悬崖墙匹配 | 顶面边件（Elevated 160）+ 墙（CliffWall 180） | 顶面按掩码选件、墙按 plan（LayOverlays） | `TS6c-5/6`（内外角）、`TS2d` |
| 朝水墙面 vs 朝低地可行走面 | _50-53（水脚，带浅青白 4.7%）/ _41-44（陆脚，0%） | `Row>=5 ? water : land`（plan 墙行） | `TS6c-7/8`；墙脚族↔南邻地形 **0 不一致**（176/314/311） |
| 坡面实心侧与高地方向 | `_36`（实心右下）/`_39`（镜像左下） | `Left(高地在东)→_36`、`Right→_39`（LayOverlays） | `TS6c-1/2`；方向级 0 违规（96/125/86 与 59/109/70） |
| 台阶接普通可行走顶面 | Stair 左/右对（_36+_45 / _39+_48） | 上段=`(cx,cy-1)`（低地，北邻崖顶）、下段=`(cx,cy-2)`；`PruneOverlapsUnderStairs` 剪墙 | `TS6c-3`；合法性 0 违规 |
| 台阶接悬崖（完整上下结构） | 同上（两段齐） | 同上（剪墙=相接情形） | `TS6c-4-stairs-next-wall.png`（上段+下段+两侧墙柱完整） |
| 内角/外角/端帽/竖墙/长墙 | _41/_44 帽、_42/_43 段 | 段锚+变体直方图（D-062/D-064） | `TS6c-5/6`、`TS2b/2c` |
| 阴影偏移一格 | Shadow.png 192×192 | 格心下移一格 `(x+0.5,y-0.5)`，Shadow 层 150 | `TS3c`；`Render_Shadows_*` |
| 水岸泡沫外缘 | Water Foam 16 帧 | 水缘左族+泡沫层（-50） | `TS4b` |

## 二、8 类组合近景（本轮新增 `TS6c`，正常地图真实渲染、3×3 无装饰优先）
`TS6c-1-slope-left / -2-slope-right / -3-stairs / -4-stairs-next-wall / -5-wall-outer-corner /
-6-wall-inner-corner / -7-wall-foot-water / -8-wall-foot-land`（seed14 全 8 张；seed13/15 按可用性）。

## 三、数据验证（`overlay-legality.txt` + `wall-foot-vs-south-terrain.txt`）
- 逐实例地形合法性：坡/台阶/墙 0 违规（三种子）；
- 墙脚族 vs 南邻：0 不一致；
- 坡向：0 违规；
- 槽位对账（EditMode 328 全过含逐实例 Is.SameAs）。

## 四、生产代码修改
- **P3 方向无生产改动**（四层验证未发现需修复项）——**本轮 P3 相关的生产修复即 P2 的顶面暴露
  判定**（官方 Elevated 顶面规则的一部分，见 P2）。
- 如实声明：若制作人指认具体组合缺陷（哪张图/坐标），可依逐实例清单即时定位。
