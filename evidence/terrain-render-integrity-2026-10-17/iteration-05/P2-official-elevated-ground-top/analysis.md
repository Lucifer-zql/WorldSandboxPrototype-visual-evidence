# P2 · 高地顶面回归官方地表拼接（D-065 iter-05）

> 制作人："不再假定额外的 MountGrassOverlay 贴花就是正确方案。必须回到官方 Tilemap Guide"；
> 官方示意图（Tiny-Swords-官方素材示意图.jpg）实测：**高地顶面=草坪块（与低地相同草地拼接）+ 崖壁环**，
> 无任何额外纹理层。

## 一、整改（生产代码）

1. **移除 MountGrassOverlay 全链职责**：贴花层 Tilemap/SortingOrders 常量（161）/SerializeField/
   运行时变体构建/`IsMountOverlayCell`/`_stairUpperSet`/ApplyCell 段/场景接线全部删除——
   **顶面回归官方地表片选择**（RT_Grass=color1 / RT_Grass_color3 双族显式选片，Elevated 层 160）。
2. **保留**：派生贴花 PNG（`Art/Terrain/Derived/`）与历史截图（iter-03/04 批原样保留，未覆盖）——
   供回溯；04 表 R-LAYER-07b 行保留并标注**已废弃**。
3. 不同高度色阶保留：低地 color1 / Hill color1 顶面 / Mount color3 顶面（官方色板层次不变）。

## 二、三版对照（同机位真实截图）

| 版本 | 图 | 判定 |
| --- | --- | --- |
| iter-03（整片叠加 α0.35） | `iter03-rejected-grid-twotier-seed13.png` | 被拒：规律竖纹/格线 |
| iter-04（派生贴花） | `iter04-detail-decal-twotier-seed13.png` | 格线消除，但**非官方机制**（额外覆盖层） |
| **iter-05（官方地表拼接）** | `iter05-official-tiles-twotier-seed13/14.png`、`iter05-TS4c-hill-forest-seed15.png` | **顶面=官方草坪块本色，无格线/无贴花/色阶清晰** |

## 三、契约与回归
- EditMode `Render_MountTop_OfficialGroundTiles_Iter05P2`：顶面每格有片、sprite 属 color3 表、内芯
  =`Tilemap_color3_10`、**无 MountGrassOverlay 子对象**；全量 **327/327 全过**。
- P3 对账中"Mount 顶面存在性"逐格断言（三种子全部 Mount 格）。
