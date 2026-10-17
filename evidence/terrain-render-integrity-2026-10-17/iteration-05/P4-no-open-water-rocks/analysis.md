# P4 · 开放海面禁止独立石头（D-065 iter-05，已修复）

> 制作人明令："本项目当前地图生成规则禁止把独立石头摆放在开放水面上"；"应从散布与放置契约上禁止"。

## 一、修复（生产代码：ScatterStep.cs）
水中岩石（WaterRock）与肉块/鸭子同模式：**数量抽取与放置流程照跑**（保 rng 流位/掩码/ID 空间逐位
一致），**输出写 discard store 即弃**——散布契约零产出。**未用缩小尺寸/降概率/隐藏 Sprite**。
悬崖/岸线地形 Tile 内嵌石纹不受影响（非散布装饰）。

## 二、验证（四路）
1. **离线散布清单**（`regression-final` 对应构建）：三种子 **waterRock=0**；decor 总数减少量 =
   原水石数（24/30/30 精确吻合）；树/金矿/灌木/云/陆岩数量与位置逐位不变。
2. **EditMode 断言**（327/327）：`D065I5_NoWaterRocksOnOpenWater`×3（=0）+
   `D065I5_GroundRocksAndTreesStillLandOnly`×3（陆岩>0 且**落水=0**）；旧"水岩区间"断言按新契约更新。
3. **真实画面对照**：`before-iter04-water-rocks-seed15.png`（崖脚/水面散布多颗青白小石）vs
   `after-iter05-no-water-rocks-seed15.png`（**同机位全部消失**；仅草地上一颗陆岩合法保留）。
   `water-rock-removal-diff.txt`（全批差异总账；TS2a 差异集中于原水石位置与 Mount 区贴花移除）。
4. 三种子全批截图随附（regression-final/seed-*）。
