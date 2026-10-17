# P4 · 水面禁石回归（D-065 iter-06，保持通过）

- **未修改** `ScatterStep` 的 WaterRock 禁用逻辑（iter-05 已实施，本轮不动）。
- **回归数据**（iter-06 三种子离线清单）：**waterRock=0**；decor 总数与 iter-05 一致；树/金矿/
  灌木/云/陆岩逐位不变。
- **画面扫描**（`scan-duck-meat` 同口径，iter-06 批）：meat **0**；duck 色残留=金矿同色误报
  （命中图数随本批新增近景机位增多而增多，像素来源仍为金矿 (233,240,68)）。
- **EditMode 回归**：`D065I5_NoWaterRocksOnOpenWater`×3、`D065I5_GroundRocksAndTreesStillLandOnly`×3
  （陆岩>0 且落水=0）全过。
- 地形 Tile 内嵌石纹（崖壁/岸线组成部分）不受影响——正常画面中崖脚石纹保留。
