# P2 · 高地顶面矩形色块的调查与修复（D-065 iter-06）

> 制作人指出：iter-05 的 seed14 高地顶面截图（twotier）与 seed15 TS4d 存在"边缘规整、颜色和
> 纹理表现明显不同的矩形区域"，必须先查明来源。

## 一、来源判定（三种情况区分，数据实证）

**结论：A 情形**——矩形是**真实的不同高程区域（Mount elev2）**，不是工具预览、不是选片错误。
证据（`rect-source-analysis.txt`，seed14 高程场实测）：
- 矩形世界范围 x≈23.8-27.9 / y≈4.6-7.0（由机位 Aim(24.5,3.0,ortho6) 与像素坐标换算）；
- 数据：**x24-26,y4-5 = elev2（Mount）**；x22-28,y6-7 = elev1（Hill）；y3 = elev0（低地）；
- 矩形内=Mount 顶面（color3 青绿）、矩形外=Hill 顶面（color1 亮黄绿）——**色差=官方色板层级**。
- 非 C（工具预览）：矩形边界与高程格边界吻合（整数格），且采集已执行 HandleCancel+ESC 复位
  （D-063-A）；场景无笔刷预览对象参与绘制。

## 二、发现的真实缺陷与修复（生产代码）

**缺陷**：Mount(elev2) 与 Hill(elev1) 的**1 级高差**（矩形的北/东/西三面）只有色板切换——
顶面暴露判定使用"邻位 elev ≥ 1"，把 Hill 当作与 Mount 同级 ⇒ **没有任何顶面边缘/连接结构**
（仅南向 2 级直落有墙）。这正是任务书 §四 A 情形指出的"不能只改变草地颜色，而没有相应的高度
连接结构"。

**修复**（`TerrainTilemapRenderer.cs`）：
- 新增 `IsElevatedAtLeast(x,y,minElev)`；`ResolveElevatedTopTile` 的暴露判定由"邻位 ≥ HighGround(1)"
  改为"**邻位 elev ≥ 本格 elev**"——**任何降级边缘（含 1 级）都绘制顶面边缘件**；
- 选片族规则不变：暴露位含水⇒左族岸线；纯陆降级⇒右族（0% 浅青白）；同高/更高⇒中心片；
- **未**重新引入任何贴花（MountGrassOverlay 保持移除）；色阶层级（color1/color3）保留。

## 三、修复前后对照（同机位真实截图）
- `before-iter05-rectangle-seed14.png`：Mount 矩形四周**纯色直边**（无连接结构）。
- `after-iter06-edge-connected-seed14.png`：**同机位**——Mount 台地北/东/西缘出现完整**右族草缘边件**
  （深色草穗+描边），南缘墙行不变——矩形变成"有官方边缘的台地"，色阶保留。

## 四、契约与回归
- 新增 EditMode 契约 `Render_Elevated_OneLevelDropShowsEdgeTiles_Iter06P2`：Mount 四邻 Hill ⇒
  顶面**非中心片**（color3 边缘件）、Hill 邻 Mount 不暴露（中心片）、同高邻位不暴露——**328/328 全过**。
- 三种子全图与近景随附；P2 顶面仍为 color1/color3 官方地表拼接（iter-05 成果保持）。
