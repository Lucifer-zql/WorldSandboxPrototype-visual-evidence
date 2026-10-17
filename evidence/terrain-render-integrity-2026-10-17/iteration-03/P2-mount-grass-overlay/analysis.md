# P2 · Mount 顶面草地纹理覆盖层（D-065 iter-03 整改）

> 制作人要求（原文要点）：**"本轮必须实际实施纹理覆盖，并以真实 Unity 画面验证。不得仅再次输出
> 『这是 elevation=2、素材没有问题』的报告"**；**"如果采用派生纹理或材质，必须记录生成方法、
> 引用的原始素材和导入设置"**；**"禁止直接将不透明普通草地整块铺到 Mount 上而抹掉原有高地色阶"**。

## 一、实现（生产代码）

**文件**：`Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs`、`SortingOrders.cs`

1. **新增层 `MountGrassOverlay`（04 R-LAYER-07b，Order in Layer = 161）**——
   `EnsureTilemap` 自建子对象；`RebuildCore` 全量清铺；公开属性 `MountOverlayTilemap`。
   161 = 紧随 ElevatedTerrain(160) 之上、Slope(170) 之下。
2. **铺格条件（`ApplyCell`）**：仅 `elev ≥ MountLevel(2)` 且 **内芯格**（正交四邻皆 elev≥2、不越界，
   `IsMountInterior`）——顶面边缘格（边件/流苏/岸线语义）与台阶上段（外缘，非内芯）不铺。
3. **覆盖片（`EnsureMountGrassOverlayTile`，懒构建、随色板缓存）**：
   - `sprite` = 注入 `grassTile`（RT_Grass）经 `ExtractUnderlayTile` 提取的 **m_Id=5 槽完整片**（实测 0% 透明）
   - `Tile.color = (1,1,1, α)`，**α = 0.35**（`MountGrassOverlayAlpha` 常量）
   - `colliderType = None`；`SetGrassTile` 时缓存失效
4. **`IsMountInterior`**：四邻皆 Mount 高地；对角不参与（与顶面选片正交掩码口径一致）。

## 二、生成方法记录（任务书硬要求逐项）

| 要求项 | 记录 |
| --- | --- |
| 原始素材引用 | `Assets/_Project/Art/Terrain/RuleTiles/RT_Grass.asset` 的 `m_TilingRules` 中 `m_Id=5` 规则的首个 sprite（Table: `Tilemap_color1.png`，切片 (64,256)，64×64）——与 iter-02 底色垫底**同源同片** |
| 导入设置 | **原素材与导入设置零改动**（未修改任何 `.png` / `.meta` / PPU / 切片 / Pivot） |
| 是否派生纹理/材质 | **否**——无新增 PNG、无新增材质资产；sprite 引用 + 运行时 `Tile.color` 逐像素**乘算混合** |
| 混合方式 | Tile 渲染标准乘算混合：输出 = sprite 像素 × (1,1,1,0.35)，与下层（Mount color3 顶面 160 层）alpha 混合 |
| α 取值 | **0.35（实现决定，`未验证` → 以本批真实截图初步标定；如制作人视觉裁决需调整，仅改 `MountGrassOverlayAlpha` 常量）** |
| 为何非不透明整块铺 | α<1 保留下层 color3 色相（见验证②，实测青色相 B 通道保持）；满足"禁止抹掉色阶" |

## 三、验证

### ① EditMode 契约测试（真实 Unity，318/318 全过）
`CliffTileSelectionTests.Render_MountInterior_GetsAlphaGrassTextureOverlay_Iter03P2`：
内芯格有片且 `Is.InstanceOf<Tile>`；`color.a ∈ (0,1)` 且 == 0.35±0.001；sprite Is.SameAs 完整片；
顶面层同格仍绘制（覆盖=叠加非替换）；边缘格/角格 null；层 order == 161 > ElevatedTerrain；Hill 格 null。

### ② 画面级验证（真实 Unity 截图 + 像素对照）
- `before-twotier-seed13.png`（iteration-02-final）vs `after-twotier-seed13.png`（iteration-03）
  + `pixel-diff-twotier-02final-vs-03.txt`：
  全图显著差异 **3057 像素（0.44%）**，**全部集中在 Mount 内芯格**（48px 桶成格状分布）；
  **非 Mount 区零差异**。
- 典型像素对照：`pre=(95,171,98)`【纯 Mount color3 单色】→ `post=(142,198,154)`【带纹理的多值分布】——
  **B 通道保持高位（青冷色相保留 = 色阶保留）**，同时出现细密纹理变化（草地纹理覆盖生效）。
- `before/after-TS2d-seed15.png` + 对应 diff：同口径（2292px/0.33%，差异仅 Mount 带，**墙/转角区零差异**）。

### ③ 逐处对账（r3 测试）
`Render_OverlayInstances_SlotLevelReconciliation_Iter03P3`：三种子全图 **双向对账**——
"内芯判定 ↔ 覆盖片存在性"逐格一致 + 覆盖层格数 == 内芯格数（无多余/无漏铺）。

## 四、如实声明
- α=0.35 为**实现决定（未验证的视觉参数）**——证据为本批截图的可见纹理与色相保留；最终视觉标准以
  制作人裁决为准（调整成本 = 一个常量）。
- "纹理可见度"为渲染层事实（diff 实测多值分布），未做主观视觉打分。
