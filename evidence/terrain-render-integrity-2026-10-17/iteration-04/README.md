# iteration-04 —— D-065 三个未通过项真正整改（P1/P2/P3）+ P4 回归

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> **本轮针对 iter-03 未通过项**：P1 内陆误用边缘素材/透明露底（已修复）、P2 Mount 纹理覆盖方格线
> （已重做）、P3 三角/坡面/对角（四层独立验证 + 无遮挡近景）、P4 回归（零改动、通过）。

## 一、目录结构

```
iteration-04/
  P1-interior-tile-selection/     # 全图选片清单 CSV×3（判定版，0 违规）+ 透明率表 + 水色扫描 + 前后图 + analysis.md
  P2-mount-texture-overlay/       # 派生纹理生成器/REPORT/预览 + 被拒方案历史对照 + 前后图 + 三档强度 + analysis.md
  P3-slope-triangle-cliff-placement/  # 素材几何/地形合法性/方向/槽位四层验证 + 无遮挡近景 + analysis.md
  P4-map-spawn-regression/        # 扫描回归 + analysis.md
  regression-final/
    seed-20261013|14|15/          # 本批全量截图（36/39/39 张；含 TS6a 三档与 TS6b 无遮挡近景）+ XML/log
    test-evidence/                # EditMode r1（审计导出）/ r2/r3（过程留证）/ r4（**324/324 全过**）
  README.md / SHA256SUMS.txt
```

## 二、结论速览

1. **P1（已修复，根因代码级实证）**：① RuleTile 9 基础槽全为左族海岸风片（浅青白 1.8%~7.6%）；
   ② iter-03 件格显式 Tile 污染邻接判定 → 件的邻格被 RuleTile 选成暴露片（iter-03 状态：seed13
   **内陆草格 431 格用暴露片**）；③ 垫底片 `_9` 仅 98.2% 不透明（1.8% 全透明）。
   **修复**=平地底片全显式（**地形数据驱动**、按族选片：水暴露→左族/陆暴露→右族/内芯与件格→
   完整中心片）+ 垫底改 100% 不透明中心片。**验证**：全图选片清单三种子 **0 违规**；水色残余扫描
   **48/35/63 → 0/0/0**；TS4a diff 2.61% 全部为"浅青白/水色/暴露纹 → 草色"。
2. **P2（已重做）**：弃用整片叠加（方格线根因），改为**派生透明草纹理 4 变体贴花**（确定性生成器、
   仅纹理点、无底色无边线；原素材零改动）；覆盖全部 Mount 顶面格（连续、非矩形补丁）；强度经
   0.60/0.85/1.00 三档真实截图对照**采用 0.85**。同机位对照：iter-03 的规律格纹/竖纹**已消除**。
3. **P3（四层独立验证通过；生产未改）**：素材几何（_36 右下/_39 左下、方向映射一致）、逐实例地形
   合法性（坡/台阶/墙 三种子 0 违规）、方向级验证（_36↔东高 0 违规、_39↔西高 0 违规）、槽位级对账
   （逐实例 Is.SameAs 全过）；新增**无遮挡近景**（真实地图内筛选、只调相机）。未发现需要修改的生产
   放置逻辑——如实声明（期望值均来自独立素材实测/规则，非生产映射回抄）。
4. **P4（回归通过、零改动）**：meat 0 像素、duck 残留 1486px 与 iter-03 完全一致（金矿同色误报）、
   EditMode 散布断言三种子全过。

## 三、修复代码差异（相对 iteration-03）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | P1：`ResolveFlatBaseTile`/`AccumulateFlatEdge`（地形数据驱动全显式底片）+ `ExtractUnderlayTile` id5→id8（100% 不透明）；P2：`MountGrassOverlay` 重做为 `mountGrassDetailSprites` 4 变体贴花 + `IsMountOverlayCell`（全顶面-台阶上段）+ `_stairUpperSet` |
| `Assets/_Project/Art/Terrain/Derived/MountGrassDetail_1..4.png(+.meta)` | P2 派生透明纹理（确定性生成，见 P2/generation/） |
| `Assets/_Project/Scenes/Main.unity` | P2 贴花 4 sprite 接线（`mountGrassDetailSprites`） |
| `Assets/Tests/EditMode/TerrainTileSelectionAuditTests.cs` | 新增：全图选片 CSV 审计 + 全图选片规则断言（三种子） |
| `Assets/Tests/EditMode/CliffTileSelectionTests.cs` | 更新：P1 契约（中心片 id8）、P2 契约重写（贴花/全顶面/0.85/变体）、P2 双向对账判据 |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 新增 TS6a（强度三档）/TS6b（无遮挡近景） |

（P4 零代码改动；原素材 PNG 零改动。）

## 四、回归与保护

- **EditMode 324/324 全过**（r4；r1=审计导出基线、r2/r3=过程留证）；PlayMode 截图套件三种子
  11 passed + 1 skipped ×3；
- 历史证据（iter-00~03）采前/采后 `sha256sum -c` 零覆盖；本批写盘全经 CaptureWriteGuard
  （白名单根+tri- 前缀+已存在拒绝）；
- 主仓库零 git 写（仅证据仓库新增本批目录）。

## 五、局限性（如实声明）
1. P2 贴花强度 0.85 为三档截图对照下的采用值（`未验证`视觉参数，调整成本=一个常量）；
2. P1 的 class 判定按"格自身暴露掩码"（RuleTile 语义），清单 CSV 逐格附分类/判定供复核；
3. P3 未发现规则缺陷属"四层独立验证"结论；若制作人持有具体缺陷位置可按逐实例清单定位复修；
4. 进程留证：EditMode r2/r3 为修测试过程中的中间失败（编译错/P1 断言口径调整），保留不删。
