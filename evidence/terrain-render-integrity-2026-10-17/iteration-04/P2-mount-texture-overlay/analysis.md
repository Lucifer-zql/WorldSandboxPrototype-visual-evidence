# P2 · Mount 顶面草地纹理覆盖重做（D-065 iter-04 整改）

> 制作人判定：iter-03 的"整张不透明草片 × alpha 0.35"叠加在真实画面出现**明显、规律的矩形方格线**，
> 必须整改（被拒方案截图 `rejected-iter03-grid-twotier-seed13.png` 原样保留）。

## 一、重做方案

1. **弃用整片叠加**：不再用任何完整草 Tile 做半透明叠加（其四边内部边线/底色逐格重复=方格线根因）。
2. **派生透明草纹理贴花**（确定性生成器与完整记录见 `generation/REPORT.md` + `gen_mount_grass_detail.py`）：
   - 源 = `Tilemap_color1.png` 切片 `_10`（100% 不透明、0% 浅青白、无边框线）——**原 PNG 零改动**；
   - 高频残差 → 仅纹理点输出：`α=clamp((d−4)×0.024, 0, 0.6)`；RGB 亮细节×1.35/暗×0.62；
   - **4 变体**（wrap 平移）+ 渲染侧按 `(x*73856093 ^ y*19349663)％4` 选变体——消除 64px 周期感；
   - 产物 `Assets/_Project/Art/Terrain/Derived/MountGrassDetail_1..4.png`（64×64 RGBA，导入预设同
     Shadow.png：PPU64/Point/无 mipmap/FullRect；确定性 guid）——**无 AI 生成、未改任何原素材**。
3. **覆盖范围**：elev≥2 的**全部 Mount 顶面格**（含边缘——贴花为纯点状透明层，不遮挡顶面边缘件），
   仅排除**台阶上段格**；墙身/坡面（低地格）天然排除。取代 iter-03 的"内芯格"（"四邻皆 Mount"
   在顶面形成规整矩形补丁边界）。
4. **强度**：`Tile.color.a = MountGrassOverlayStrength`（作用于贴花自带 alpha；初值 **0.85**）。

## 二、验收（真实 Unity 截图）

### ① 方格伪影消除（同机位对照）
- `rejected-iter03-grid-twotier-seed13.png`（iter-03，被拒：Mount 区可见规律纵向格纹/裂纹重复）
- `after-twotier-seed13.png`（iter-04 同机位）：**纹理均匀细密、无矩形方格线/竖纹/接缝**；
  Mount 原 color3 色阶保留（青灰底+草纹理颗粒）；顶面边缘件/崖壁接缝不受影响。
- 另有三种子全机位图（regression-final/seed-*，含 tpr2-closeup-twotier 等）供复核。

### ② 强度三档对照（TS6a，同机位真实截图）
`TS6a-mount-strength-060/085/100.png`（seed13；0.60/0.85/1.00）：
0.60 纹理偏淡、清晰度略弱；0.85 纹理清晰可见且色阶保持（**采用**）；1.00 纹理更重、
对色阶压制略强。**最终采用 0.85**（`MountGrassOverlayStrength` 常量默认值即 0.85）。
（TS6a 为运行时反射改 Tile.color.a + RefreshAllTiles 的标定采集，**不改生产常量**。）

### ③ 覆盖契约（EditMode 324/324 全过）
- `Render_MountTop_ContinuousDetailOverlay_Iter04P2`：顶面全格（含四角/四边）均有贴花、sprite 名
  `MountGrassDetail_*`、`color.a=0.85`、5×5 块内 ≥2 变体、低地/Hill 不铺、层序 161；
- `Render_OverlayInstances_SlotLevelReconciliation_Iter03P3`：三种子**双向对账**——"elev≥2 且非
  台阶上段 ↔ 贴花存在性"逐格一致（贴花格数 648/652/655=对应种子全部 Mount 格数）。

## 三、如实声明
- 贴花 alpha 分布 71.1% 半透明/28.9% 全透明（纯纹理点，无底色）——这是"无方格"的结构性保证；
- 最终视觉裁定权在制作人：可复核 0.60/0.85/1.00 三档图后如需调整，改一个常量即可。
