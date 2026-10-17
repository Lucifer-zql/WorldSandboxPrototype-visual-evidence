# P3 · 三角/坡面/对角/崖壁放置（D-065 iter-03 整改）

> 制作人要求（原文要点）：**"禁止只输出『复算数量一致』『素材方向正确』便结束任务"**；
> **"不允许统一移动全部三角形素材、隐藏悬崖、删除合理地形细节、改变随机种子"**。
> 本轮将此前的"数量一致 + 方向正确 + 抽样叠加"升级为**逐实例、槽位级的全量对账** +
> **真实画面的逐图核查**；未发现需要修复的缺陷处如实声明（不作"免检"结案，见 §四）。

## 一、方法（三层验证，全部为"每处"级）

### ① 逐实例槽位级对账（新增强制测试，真实 Unity 运行）
`CliffTileSelectionTests.Render_OverlayInstances_SlotLevelReconciliation_Iter03P3`（三种子）：
- **每一个** `plan.SlopeCells` 实例：`SlopeTilemap.GetTile(cell)` **Is.SameAs 期望切片**
  （side=Left→`_36`/Right→`_39`；面板 Hill/Mount 分表——与 `LayOverlays` 映射同式）；
- **每一个** `plan.CliffCells` 实例：`CliffWallTilemap.GetTile(cell)` Is.SameAs
  `_41.._44`（Row<5 陆脚）/`_50.._53`（Row≥5 水脚）按 Variant；
- **每一个** `plan.Stairs` 实例的**上/下两段**：Is.SameAs `_36/_45`（左对）或 `_39/_48`（右对）；
- **反向计数**：坡/墙/台阶渲染格数 == 计划实例数（台阶 ×2 段）——**无漏铺、无多余**；
- 输出（`p3-instance-reconciliation.txt`）：seed13 坡 155/墙 176/台阶 1；seed14 坡 234/墙 314/台阶 3；
  seed15 坡 156/墙 311/台阶 3 —— **全部 Passed**。

### ② 真实画面逐图核查（截图）
- `after-slope-seed13.png`：坡件特写——高低交界逐格"斜切三角"拼接正确（放大 3×
  `slope-center-3x-zoom.png`：斜线+三角延伸高地色，方向与高地方位一致）；低地侧"藤蔓纹"
  为草地暴露边片内绘（非缺陷）。
- `after-stairs-seed13.png`：台阶特写——崖壁行开口处上下两段成对（上段斜台 + 下段半截柱剪墙），
  落点干燥；开口两侧崖柱完整。
- `after-wall-corner-seed15.png`：墙转角特写（配 `pixel-diff-TS2d-02final-vs-03.txt`：
  墙/转角区与修改前**零像素差异**——2292px 差异全部为 Mount 覆盖带，见 P2）。

### ③ 对角语义（素材与计划两层）
- **素材层**：RT_Grass/RT_Forest 的 24 条规则**全部只含正交谓词**（N/E/S/W；对角位恒 DontCare）——
  "对角不参与选片"是官方素材自身的语义（实测 asset 规则解析：9 基础槽 + 15 marker 槽均无对角非零位）。
- **计划层**：TerrainOverlayPlan 的对角处理由 **D-064 P1a 契约测试**锁定（`Plan_WallVariants_DiagonalStaircase_*`
  镜像斜率用例，含"对角邻续接"的判定与产出链)；
  顶面 Option F 显式选片掩码同样只看正交（`ComputeElevatedEdgeMask`，源码契约注释）。
- **画面层**：三种子全部 `TS2d-wall-corner` / `TS2g-two-tier-wall` 截图随批发布（regression-final/）。

## 二、未采用的规避手段（对照制作人禁令逐项）
- 未移动/替换任何三角素材（改动仅渲染层的**层归属**，切片映射逐实例与 iter-02 相同）；
- 未隐藏任何悬崖/覆盖层（墙/坡/台阶渲染格数与计划实例数逐种子恒等，见 ①）；
- 未删除地形细节；未改变随机种子（三种子 20261013/14/15 与历史批相同）；
- 本批未对坡/台阶/墙的**切片映射**做任何修改（P1 只改"件"的层归属、P2 只增 Mount 覆盖层）。

## 三、回归与采集
- EditMode 318/318（含上述对账 3 例 + D-064 全部契约锁定用例）；
- PlayMode 截图套件三种子 9 passed + 1 skipped ×3，特写机位（tpr2-closeup-*）随批发布。

## 四、如实声明（"不是免检结案"）
- 本轮的"修复"面为 **P1/P2 渲染层**；P3 方向上**未发现实锤缺陷**——本批以**逐实例对账 + 画面核查**
  交出"每处叠加验证"，而非"数量一致"结论。
- 若制作人掌握**具体缺陷位置**（哪张图哪个坐标），可直接按逐实例清单定位（对账测试输出含每实例
  槽位期望值），再做针对性修复。
- `p3-instance-reconciliation.txt` 中的"部分地形瓦片字段未接线"警告来自**测试 harness 未接线
  cliffTile 旧字段**（D-059 兼容告警），非生产数据问题（生产场景接线完整，见 regression-final 截图）。
