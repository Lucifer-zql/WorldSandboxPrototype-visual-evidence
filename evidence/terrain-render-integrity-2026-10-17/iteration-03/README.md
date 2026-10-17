# iteration-03 —— D-065 四类问题综合整改（D-065 iter-03 发布批）

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 seed-20261013/14/15
> **本轮为制作人指定的四类"必须实际实施"的修复**：P1 内陆件/基础地表、P2 Mount 纹理覆盖、
> P3 三角/坡面/对角放置、P4 移除肉块与鸭子散布。修改前后真实截图逐项对照，测试与日志持久化。

## 一、目录结构

```
iteration-03/
  P1-interior-ground-tiles/          # 件迁层：前后对照 + 像素 diff + 契约测试说明（analysis.md）
  P2-mount-grass-overlay/            # Mount 纹理覆盖：前后对照 + 像素 diff + 生成方法记录（analysis.md）
  P3-slope-triangle-cliff-placement/ # 逐实例槽位级对账 + 特写/放大图（analysis.md）
  P4-remove-meat-duck-map-spawns/    # 离线对照 + 画面扫描/diff + 打标图（analysis.md）
  regression-final/
    seed-20261013|14|15/             # 最终版全量截图（30/33/33 张）+ PlayMode XML/log（9P+1S）
    test-evidence/                   # EditMode r1（历史失败留证）/r2/r3（**318/318 全过**）XML+log
  README.md / SHA256SUMS.txt
```

## 二、四专项结论速览

1. **P1｜件不得替代基础地表（已修复）**：地形主层不再铺任何透明边缘件——件格主层=显式完整片
   （m_Id=5，0% 透明），林界流苏/森林自身边件迁至新增 **ForestEdgeOverlay 层（04 R-LAYER-07a，
   order 10）**；非件格维持 RuleTile 直铺；底色垫底保留。**逐像素等价**（diff 实测：件格区零差异）。
2. **P2｜Mount 顶面草地纹理覆盖（已实施）**：新增 **MountGrassOverlay 层（04 R-LAYER-07b，order 161）**，
   仅 Mount 内芯格（四邻皆 Mount）叠低地草完整片（`Tile.color α=0.35` 乘算混合）——纹理可见且
   **保留 color3 色阶**（非不透明整块铺）；无派生 PNG/材质，原素材与导入设置零改动（生成方法全录见
   P2/analysis.md §二）。画面 diff：差异全部位于 Mount 内芯格（3057px/0.44% 与 2292px/0.33% 两组）。
3. **P3｜三角/坡面/对角/崖壁（逐处对账通过）**：新增**逐实例槽位级对账测试**（三种子全量：
   seed13 坡155/墙176/台阶1；seed14 坡234/墙314/台阶3；seed15 坡156/墙311/台阶3——Is.SameAs 期望
   切片 + 双向计数），配特写核查与"墙/转角区零像素差异"的 diff；未发现实锤缺陷处如实声明
   （非"数量一致"结案，见 P3/analysis.md §四）。
4. **P4｜移除肉块与鸭子散布（已实施）**：ScatterStep 采用 discard 设计——数量抽取与放置流程照跑
   （rng 流位/掩码/nodeCells/ID 空间逐位保持），输出写 discard store 即弃——**零实例、零绘制**；
   离线逐位对照 + 画面扫描（meat 1979px→**0**；duck 残留=金矿同色客观误报）+ 打标图实拍（前）证链。
   原始 PNG/Sprite/枚举/配置/渲染接线全保留。

## 三、修复代码差异（相对 iteration-02-final 工作区）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/SortingOrders.cs` | +2 常量（ForestEdgeOverlay=10 / MountGrassOverlay=161，含 04 规则号） |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | P1 件迁层（ApplyCell 主层重构 + 新层）+ P2 覆盖层（IsMountInterior / EnsureMountGrassOverlayTile / α 常量）+ 契约注释 |
| `Assets/_Project/Scripts/World/GenSteps/ScatterStep.cs` | P4 discard（food/duck 两段照跑弃输出） |
| `Assets/Tests/EditMode/CliffTileSelectionTests.cs` | +P1 契约 +P2 契约 +P3 逐实例对账（3 种子） |
| `Assets/Tests/EditMode/ScatterExclusionTests.cs` | 新增（P4 零产出契约，3 种子 ×3 断言） |
| `Assets/Tests/EditMode/SortingOrdersTests.cs` | 期望表 16→18（+07a/07b，行号同步 04 表） |
| `Assets/Tests/EditMode/MapVisualOverhaulSeedMatrixTests.cs` / `WorldGeneratorTests.cs` | 鸭子/食物断言改零产出（其余散布区间保留；NextEntityId 口径适配） |
| `docs/04-核心玩法与模拟规则.md` | §3.2 表 +R-LAYER-07a/07b |

（素材、场景、ProjectSettings、Packages 零改动；主仓库无 git 写。）

## 四、回归证据

- **EditMode：318/318 全过**（`regression-final/test-evidence/editmode-full-r3.xml`；r1 为测试更新前的
  5 失败留证——其失败项即"鸭子/食物旧区间断言与 P4 新契约冲突"，已按新契约更新）；
- **PlayMode 截图套件**：三种子 9 passed + 1 skipped ×3（seed13 30 图；14/15 33 图）；
- **历史证据零覆盖**：`sha256sum -c` 采前清单（2312 项）**2312 OK / 0 FAILED**；
  本轮写盘全部经 CaptureWriteGuard（白名单根 + tri- 前缀 + 已存在拒绝）。

## 五、局限性（如实声明）

1. P2 的 α=0.35 为实现决定（`未验证`视觉参数），以本批截图初步标定；调整成本为一个常量。
2. P4 画面扫描口径下限：面积 <6 采样点的孤立同色像素不计簇（零产出另有离线/断言双证）。
3. P3 未发现实锤缺陷系"逐处对账 + 画面核查"的结论——若制作人持有具体缺陷坐标，可依逐实例清单
   定位复修（见 P3/analysis.md §四）。
4. 本轮 P4 曾有 v1 中间迭代态（散布流位微扰），经 v2 精修恢复逐位一致；v1 批不属发布面。
