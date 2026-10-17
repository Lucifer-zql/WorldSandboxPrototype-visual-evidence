# P4 · 移除肉块与鸭子在地图生成/散布/绘制中的实例（D-065 iter-03 整改）

> 制作人要求（原文要点）：**"移除它们在地图初始化、散布和地图装饰中的生成或绘制"**；
> **"不要删除原始 PNG、Sprite、资源定义或其他系统确实需要的引用"**；
> **"禁止通过只缩小尺寸、调低生成概率或隐藏 Sprite 来规避要求"**。

## 一、对象与语义
- **肉块** = 食物点（FoodNode）的视觉（`Meat Resource.png`，"Meat 图标"）——此前由 ScatterStep
  作为资源对象散布；
- **鸭子** = 橡皮鸭（RubberDuck）装饰——此前由 ScatterStep 在水中装饰段散布。

## 二、实现（生产代码：`ScatterStep.cs`）
**Discard 设计**：被移除类型的**数量抽取与完整放置流程照跑**，但输出写入一次性 discard store 后即弃——
`WorldGenResult` 零实例；同时保持 ① rng 流位 ② `usedByNode`/`usedByDecor` 掩码 ③ `nodeCells` 内容
④ `nextId`（ID 空间分配序列）与修复前**逐位一致**（项目既有"跳过格不消耗 rng / 单 rng 流纪律"的延续）——
即：**地图上除肉块/鸭子外的一切（树/金矿/灌木/云/岩石/其余装饰）与修复前逐位相同**。

- 保留（不得删除）：枚举（`DecorationType.RubberDuck` / `ResourceNodeType.FoodNode`）、资产
  （`Rubber duck.png` / `Meat Resource.png` 及 Sprite）、渲染接线（duckSprites/foodSprites）、配置
  （`CivConfig.ResourceHitCount.FoodNode`）——全部原样，供未来掉落物/图标/玩法使用。

## 三、验证（四路证据）

### ① 离线级联复算对照（`offline-compare-prep4-vs-p4.txt`、`offline/`）
全链复算（与 WorldGenerator 装配同序）两份代码——"P4 前重建版"×"P4 最终版"：
| 种子 | decor 移除 | nodes 移除 | 新增 | 树/金矿/灌木/云/岩石 | nextId |
| --- | --- | --- | --- | --- | --- |
| 20261013 | 20（全部 RubberDuck） | 40（全部 FoodNode） | 0 | **逐位相同** | 208→208 |
| 20261014 | 26（全部 RubberDuck） | 23（全部 FoodNode） | 0 | **逐位相同** | 166→166 |
| 20261015 | 22（全部 RubberDuck） | 39（全部 FoodNode） | 0 | **逐位相同** | 222→222 |
（逐位同 = 坐标+变体+命中数全同；`removal-before-/after-scatter-seed-*.txt` 随批。）

### ② EditMode 断言（真实 Unity，318/318 全过）
- `ScatterExclusionTests`（新，9 例）：三种子 `Decorations` 无 RubberDuck、`ResourceNodes` 无 FoodNode；
  树/金矿/水岩 >0（防误删）；
- 既有回归更新为新契约：`MapVisualOverhaulSeedMatrixTests`/`WorldGeneratorTests.T_04_08`
  （鸭子/食物断言 → 零产出；其余散布区间断言保留；`NextEntityId` 改用 ≥ 口径——ID 空间含丢弃段消耗）。

### ③ 真实画面证据（三种子 × 96 同机位图）
- 色窗扫描（鸭子 (222,236,75)、肉 (239,81,92)，素材实测主色）：
  **修改前**（iteration-02-final）：46 图命中、duck 2378px、meat 1979px；
  **修改后**（iteration-03）：meat **0 像素**；duck 残留 1486px **全部为保留簇**
  （位置与修改前相同、diff"新增=0"）——经素材实测归因：**金矿主色 (233,240,68) 与鸭身色
  (222,236,75) 客观同色窗**（误报），打标图 × 大簇抽查确认（如 TS4c 左缘金矿）；
- 同机位像素 diff（`pixel-diff-02final-vs-03-TS2a.txt`）：差异集中于**被移除物所在位置**
  （`before-TS2a-seed13.png` 中打标紫框=水面橡皮鸭实拍，修改后同位置无鸭）；
- `before/after-TS2a-seed13(.marked).png` 随批（前后对照机位）。

### ④ 参照量与绘制链
- 装饰总数变化与离线一致（如 seed13 decor 272→252 = −20 鸭）；
- 渲染侧无改动（DecorationRenderer / ResourceNodeRenderer 的接线保留），"绘制"消失源于**零实例**
  而非隐藏 Sprite（对照制作人禁令：无尺寸缩小、无概率调低、无 Sprite 隐藏）。

## 四、如实声明
- 画面 diff 中另有少量与 P2（Mount 覆盖色变）重叠的像素变化，归因见 P2 analysis；
- "肉块 1979px→0"为**色窗扫描口径**（面积<6 采样点的孤立像素不计簇，未成簇像素从宽处理为噪声）；
  零产出结论另有 ①② 独立佐证。
