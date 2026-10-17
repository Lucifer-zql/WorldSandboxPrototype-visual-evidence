# P4 水面禁石与散布回归（iteration-07）

> 任务书 §六：保留水面禁石、肉块/鸭子移除与资源大小变体成果；本轮不得回退。

## 一、本轮对散布的间接影响（如实）
- `ScatterStep.cs` **零改动**（本轮未触碰）；
- 但 `TerrainOverlayPlan` 墙格从 176/314/311 扩至 344/483/424 → **植被净空集扩大**
  （`VegetationClearance` = 墙格自身 + 根部三格横带）→ 部分装饰/资源点被净空剔除 →
  **树木/装饰数量较修复前减少**。这是官方台地边缘墙环的预期结构变化（官方示意图台地边缘
  同样少植被），不是散布逻辑回归；
- 肉块/鸭子 discard 契约未改动；水岩 discard 契约未改动；资源大小变体接线未改动。

## 二、自动化回归（EditMode 328/328 全过，含）
- `ScatterExclusionTests.D065I5_NoWaterRocksOnOpenWater`（三种子）：水面无独立水岩 ✓
- `D065I3_NoRubberDuckDecorations` / `D065I3_NoMeatFoodNodes`：鸭子/肉块零产出 ✓
- `D065I5_GroundRocksAndTreesStillLandOnly`：陆岩/树仅陆地 ✓
- `P4` 相关旧断言（水岩=0、陆岩落水=0）全部通过 ✓

## 三、真实画面回归
- `TS7g-open-water-no-rocks.png`（三种子各一）：开阔水面（5×5 全水机位）无任何独立石头 ✓
- 全图 `TS1a/TS1b`（三种子）：水面区域无新增装饰层异常（与修复前同机位对照见
  `../P3-layered-cliff-ground-stack/pairs-diff.csv`——差异集中在墙环/坡，水面区域无差异突变）。

## 四、结论
P4 成果**保持通过**。本轮唯一相关变化是净空扩大导致的植被数量减少（结构性、预期、非回归），
已在报告中如实列出。
