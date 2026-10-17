# iteration-10 —— D-065 悬崖/三角坡路/多层地表/内陆水缝 全面修复（final-r3 终版）

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 依据：制作人 Iteration-10 任务书（以真实游戏画面为最终标准）+ **官方 Tilemap Guide 复核**
> （https://pixelfrog-assets.itch.io/tiny-swords/devlog/1138989/tilemap-guide，本轮实抓全文与 4 张示例图）。

## 一、官方依据（本轮新增的关键事实）

1. **Elevated Ground** 每一级都含面向下方地形的悬崖；**石墙只在南面**，东西向高差只有草边深描边、
   **无石墙**（东西邻 Mount 的 Hill 格是正常台面：有顶面、无墙）。
2. **Stairs（楼梯）= 2 格高列**（36 上 + 45 下 / 39 + 48 镜像），贴高台侧、占高台南缘两行
   （底行-1 与底行），底边与低地齐平；接崖壁时 36 在底行、45 在崖壁行、崖壁用 42。
3. **素材像素实测**：`_45` 石脚 x 重心 = 42（右）、`_48` = 21（左）——**石脚必须在低端**；
   配对为 `36+45`（低端右）与 `39+48`（低端左）。
4. Shadow 与 Elevated 层逐级重复；Shadow 向下平移一整格。

## 二、根因与修复（相对 iteration-09）

### P0：悬崖与逐层地表

- **根因**：iter-08 以"墙覆盖中间台面"为由删除了 **Mount→Hill 的悬崖墙**，导致逐级立面缺失
  （Mount 与 Hill 之间无任何可见崖面）；同时把 Mount 的立面格误当可走台面。
- **修复**：`CollectWalls` 恢复 **Mount→Hill 南面墙**；`IsMountFacadeCell` **只认北邻 Mount**
  （Mount 的南邻 Hill 格）——该格不画 Hill 台面顶面、不铺 band，由崖壁墙承担；东西邻 Mount 的
  Hill 格恢复为正常台面（r25 修正：此前北/东/西三向豁免造成"无顶面又无墙"的空洞）。
- **结果**：Mount 顶面 → 南面崖壁墙 → Hill 台面 → Hill 南缘墙 → 低地，逐级完整（TS9f/TS2a）。

### P1：三角坡路 → 官方两格楼梯

- **根因**：旧实现把 1 格高差拉成 2 格且下段左右配对反了；`_36/_39` 无石脚被误当完整楼梯；
  孤立坡被剔除；东西缘被自创石墙。
- **修复**（`CollectSlopes` 重写 + `LayOverlays`）：
  - 楼梯 = **2 格列**：上段 (X,Y)=36/39、下段 (X,LowerY)=45/48；`Left`（低端左）→ **39+48**，
    `Right` → **36+45**（素材实测配对）。
  - 生成条件：高台南缘两行、北邻恰为 **Hill**（elev=1，不跨 Mount 两级）、低端侧两行干燥低地、
    高侧两行必须为 Hill（r25 排除水格误判）。
  - 楼梯两格不画台面顶面、不铺水向 band、不写墙；`ReservedByOverlay` 扩展楼梯两格/南侧落点/
    低端侧向两格让位——**例外**：北邻 Mount 的格（Mount 立面墙优先）。
  - **东西缘石墙全部移除**（`CollectSideWall` 调用删除）；`PostProcessSlopes` 保留全部合法坡。
- **结果**：楼梯两格嵌入高台南缘、石脚在低端、贴台侧（TS9d、TS6c-1、TS9f 左右侧）；东西高差仅
  草边描边（TS8f/TS9g）。

### P2：内陆水缝与海岸素材

- iter-09 的混合缘/纯水缘分族维持；本轮消除楼梯格冗余 band（计划↔渲染一致）。
- 结果：混合缘全 15 掩码（TS9a）、纯水缘全掩码（TS9i/j）、全图无内陆水色穿透（TS1a）。

## 三、生产代码差异（相对 iteration-09）

| 文件 | 改动 |
| --- | --- |
| `TerrainOverlayPlan.cs` | 恢复 Mount→Hill 南面墙；删 CollectSideWall 调用；CollectSlopes 重写为官方两格楼梯（High side == Hill 校验）；ReservedByOverlay 扩展 + Mount 立面优先；PostProcessSlopes 保留合法坡 |
| `TerrainTilemapRenderer.cs` | `IsMountFacadeCell`（仅北邻 Mount：无顶面、无 band）；两格楼梯渲染（39/48 左、36/45 右）；楼梯格顶面/band 豁免 |
| 测试 | `CliffTileSelectionTests`（AssertEdgeCovered 语义断言 + 逐例更新）、`Iter09RenderOutputScanTests`（P0/P1/P2 两格语义）、`Iter10AcceptanceTests`（真实输出 + 计划↔结构）、`TerrainFoamPlacementTests`（楼梯落点让位） |

**未修改**：地形生成、SortingOrders、Main.unity、官方原始 PNG、素材导入设置。

## 四、测试与扫描

- **EditMode 368/368 全过**（`test-evidence/final-r3/editmode-r29.xml`）。
- PlayMode 三种子：`final-r3/seed-*/`（每种子 73-79 图 + test-results.xml + test-run.log；
  16 passed / 1 skipped / 0 failed，skip 为既有 tpr3）。
- 判据：**实际 `Tilemap.GetTile` 输出** + 计划↔实际双向 + CSV 逐格留痕（`%TEMP%/iter09-scan/`、
  `%TEMP%/iter10-scan/`，关键 CSV 归档 `test-evidence/`）。

## 五、目录

```
iteration-10/
  final-r3/seed-*/            # **最终代码三种子全量真实截图（本轮验收依据）**
  P0-cliff-and-layer-stack/   # post-fix：TS9f/g、TS2a/b（多层逐级立面实拍）
  P1-triangle-slope-placement/# post-fix：TS9d/e、TS7c（两格楼梯、石脚低端）
  P2-inland-water-gap/        # post-fix：TS9a/i/j、TS1a、TS7e（水向边带/无内陆水缝）
  regression-final/seed-*/    # 中间版（单格楼梯时代）——保留回溯，不冒充最终
  verify-r1/ P0P1-experiment/ P1-single-stair/ P1-official-r2/
                              # 修复过程轨迹（保留，不冒充最终）
  baseline/ test-evidence/ failed-attempts/
  README.md / final-report.md / SHA256SUMS.txt
```

**pre-fix 引用**：`iteration-09/regression-final-r2/seed-*`。

## 六、如实声明与待验证

1. **官方多级连续楼梯**：Guide 未展示两级连续楼梯；本项目 Mount→低地不生成 Hill 楼梯（素材为
   color1），需制作人目视确认可接受。
2. **`_36/_39` 用途**：现作为楼梯上段（配合 45/48），像素实测配对；官方示意图的差异**待验证**。
3. **color3 边带色差**：Mount 顶面水缘使用 color1 band（已知局限）。
4. **立面格可走性**：数据层 Hill 可走、视觉为悬崖格（官方素材体系固有）——**待制作人确认**。
5. 历史证据 iter-00~09 未覆盖；主仓库零 git 写（生产修改仅留本地工作区）。
