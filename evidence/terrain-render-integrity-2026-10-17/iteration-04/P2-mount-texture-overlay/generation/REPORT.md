# P2 派生透明草纹理 · 生成记录（D-065 iter-04）

> 制作人要求："不使用 AI 生成草地纹理，不覆盖/修改原始 PNG；派生纹理必须保留来源文件、生成脚本、
> 处理规则、alpha 生成方法和尺寸等信息；纹理必须可连续平铺，不能出现每 64 像素重复一次的明显接缝
> 或矩形框线。"

## 一、来源

| 项 | 值 |
| --- | --- |
| 源文件 | `Assets/_Project/Art/Terrain/Tilemap_color1.png`（576×384，9×6 切片，64×64/片） |
| 源切片 | `_10`（c1r1，第 1 行第 1 列）——**实测**：100.0% 不透明、浅青白像素 0%、无格边框线（边缘 3px 带亮度 160.5 ≈ 全片均值 161.7）、自身 wrap 接缝 左右 2.4 / 上下 5.2 通道差（近可平铺） |
| 原文件处置 | **零改动**（未修改原 PNG / 未改导入设置；仅读取像素） |

## 二、处理规则（确定性；生成器见同目录 `gen_mount_grass_detail.py`）

1. `low = GaussianBlur(源切片, radius=3)`（PIL ImageFilter.GaussianBlur）
2. `d(x,y) = max_ch | pixel_ch − low_ch |`（高频残差，通道取最大）
3. **alpha 生成**：`α = clamp((d − 4) × 0.024, 0, 0.6)`
   - 阈值 4 = 过滤平区噪声；标定依据：残差 p90=25 → α≈0.50、p95=28 → α≈0.576、上限 0.6
   - 输出 alpha 分桶（0..6 of 0-255）：`[2517, 294, 499, 571, 215, 0, 0]`（约 38.5% 像素带纹理）
4. **RGB**：亮细节（像素亮度 ≥ 模糊亮度）→ 原色 ×1.35 提亮；暗细节 → 原色 ×0.62 压暗
   （保留草地色相、放大明暗对比；**不含底色、不含格边线**——只有纹理点）
5. **4 变体**：对上述输出做 wrap 平移 `(0,0)/(21,13)/(43,37)/(11,52)`
   ——点状内容 + 变体交替 ⇒ 消除 64px 周期感；渲染侧按 `(x*73856093 ^ y*19349663) % 4` 选变体
6. **自检**：`tile_preview.png` = 8×8 格平铺模拟（同哈希规则）——目视无方格线/矩形框/接缝

## 三、产物与导入

| 项 | 值 |
| --- | --- |
| 文件 | `Assets/_Project/Art/Terrain/Derived/MountGrassDetail_1..4.png` |
| 尺寸/格式 | 64×64 RGBA PNG |
| 导入预设 | 模板 = `Shadow.png.meta`（Sprite / PPU 64 / Point / 无 mipmap / AlphaIsTransparency），唯二改动：`spriteMeshType: 0`（FullRect，贴花为全幅透明图）+ 确定性 guid |
| guid 生成 | `md5("WorldSandbox/D-065-iter04/P2/MountGrassDetail_N")`（可复现） |
| 使用接线 | `TerrainTilemapRenderer.mountGrassDetailSprites`（4 槽，场景已接线）；运行时 `Tile.color=(1,1,1,strength)` |

## 四、被拒历史方案（iter-03）

iter-03 的 `MountGrassOverlay` = 整张 100% 不透明草片（`_9`）× `Tile.color.a=0.35` 叠加 →
每格四边内部边线与底色逐格重复 → 真实画面出现明显矩形方格线（证据：iteration-03/P2/after-twotier-seed13.png，
已发布、未改动）。本批保留该历史对照，不删除不覆盖。

## 五、局限性

- α 总量纲（`MountGrassOverlayStrength`，运行时乘在贴花 alpha 上）：初值 0.85，经 TS6a 三档
  （0.60/0.85/1.00）真实截图对照后在本批 analysis.md 中记录实际采用值。
- 生成器依赖 Pillow（`GaussianBlur` 半径语义与 Pillow 版本相关——本机 Pillow 行为见脚本运行输出）。
