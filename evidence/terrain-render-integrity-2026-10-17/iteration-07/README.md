# iteration-07 —— D-065 内陆边缘/坡路结构/多层悬崖堆叠 强制整改

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 依据：官方素材示意图（项目根 `Tiny-Swords-官方素材示意图.jpg`，关键区域裁剪见 `official-reference/`）
> 逐项核对——**台地边缘 = 南/东/西三缘墙环（东西缘为横向墙片竖向堆叠、临水墙直接入水），北缘无墙；
> 坡片嵌在墙环中作为豁口；水岸浅青白与植被深色边分属地形边与植被边**。

## 一、目录

```
iteration-07/
  P1-inland-transition-tiles/    # 选片链路审计 + 件层修复（混合缘）+ 水色计数对照 + TS7e 三种子
  P2-slope-triangle-placement/   # 官方坡片结构核对 + 连续性后处理修复 + TS7b 三种子
  P3-layered-cliff-ground-stack/ # 官方墙环裁定 + 东西缘墙修复 + 前后 diff.csv + TS7a/c/d 三种子
  P4-water-rock-regression/      # 禁石/散布回归 + TS7g 三种子
  official-reference/            # 官方示意图裁剪 4 张 + 亲眼观察记录
  pre-fix/seed-*/                # 修复前基线（55×3 图 + XML + log，字段与 after 同名可 diff）
  regression-final/seed-*/       # 修复后全批（55×3 图 + XML + log）
  regression-final/test-evidence/# EditMode r1(30 失败留证)/r2(1 失败留证)/r3(328/328 终版)
  pre-baseline-production-sha256.txt  # 采前生产文件哈希
  README.md / SHA256SUMS.txt
```

## 二、结论速览

1. **P1（已修复）**：根因=**混合缘整格取左族**——草格/森林格同时邻水与邻林时，件层因"邻水 ⇒ 整体 null"
   不画流苏，朝林的暴露边落在 Flat 左族水岸片上（浅青白波浪出现在非水交界）。修复：件层（林界流苏/
   森林自身边）**不再因邻水置 null**，始终按朝非本类掩码画右族无白件；水岸浅青白由 Flat 层左族承担
   （与官方"白线在外圈、植被边在内侧"一致）。混合缘数量：草格 67/90/95、顶面 18/21/23。
   **水色定量**：固定机位全图 12 张（三种子）水背景色像素 **3808013 → 3792708（净减 15,305）**——
   无新增露水；条件搜索机位图（实例集合变化）不参与同机位对照。
2. **P2（已修复）**：坡候选规则本身合法（0 违规），缺陷=**与墙列无结构关联**——真实孤立坡 10/13/16 个。
   修复：执行顺序改为坡候选先于墙收集；墙跳过坡/台阶格（豁口语义）；新增 `PostProcessSlopes`
   连续性过滤（南北无墙/坡/台阶的孤立坡剔除并补为墙）。修复后：坡 155→142 / 234→202 / 156→119，
   **孤立坡 0/0/0**，坡转墙 34/59/51。
3. **P3（已修复）**：根因=**墙只在南缘**——高地 N/E/W 临水无覆盖边 102/64/96（"高地边缘直接露出水面"）。
   修复：新增东西缘墙（`CollectSideWall`）：陆格 Row 按 `IsWater(下邻)` 选 4/5；水格仅深水（水格南邻亦水）
   写 Row5（官方岩壁入水）；北缘不铺墙（官方裁定）。墙格：176→344 / 314→483 / 311→424；
   **新增来源全部可解释（东缘/西缘/深水/坡转墙），北缘新增 0，无法解释墙格 0**；墙格下邻为低地陆格时
   Flat 100% 不透明底片承接（0 露水通道）；深水水格墙=官方入水观感。
4. **P4（保持通过）**：ScatterStep 零改动；水面禁石/肉块鸭子移除/资源变体断言全过；TS7g 三种子开阔
   水面无石。本轮唯一相关变化：墙格净空扩大 → 台地边缘植被减少（结构性、预期，如实列出）。
5. **回归**：**EditMode 328/328 全过**；PlayMode 三种子各 14 total / 13 passed / 0 failed / 1 skipped
   （`MVO_TPR3_StateContrast_SameFraming` 旧方法在 tri 模式跳过，既有行为）。TS7f（两级台）自动搜索
   三种子未命中（要求过严）；同组合由 seed14 的 `tpr2-closeup-twotier.png`（iter-06 同机位）与 EditMode
   确定性构造 `Render_TwoTierFront_UpperWallOnBench_TopAndWallSameCell` 覆盖。

## 三、修复代码差异（相对 iteration-06）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/World/GenSteps/TerrainOverlayPlan.cs` | **P2/P3 核心**：Run 顺序（台阶→坡→墙→剪墙→坡后处理）；CollectWalls 增东/西缘 `CollectSideWall`+`ReservedByOverlay`；新增 `PostProcessSlopes`/`HasSlope`；类注/互斥裁定按官方墙环修订 |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | **P1**：`ResolveForestEdgeTile` 移除"邻水⇒null"；`ResolveForestSelfEdgeTile` 移除 waterEdge⇒null |
| `Assets/Tests/EditMode/CliffTileSelectionTests.cs` | 30 项旧断言按新墙环规则更新（独立手算期望）+ 新增 `AssertNoWaterCellWalls` helper |
| `Assets/Tests/EditMode/FastNoiseElevationTests.cs` | 墙格来源自洽条件扩展 (c) 东/西邻更高 |
| `Assets/Tests/EditMode/TerrainFoamPlacementTests.cs` | 泡沫构造墙计数 4→8（东西缘扩展） |
| `Assets/Tests/EditMode/TerrainCompositionTests.cs` | 件层注释按 iter-07 P1 修订 |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 新增 `TS7_Iter07DirectionalCloseups`（7 机位）+ 6 个查找 helper |

（净空随之扩大：`VegetationClearance` 由墙格集合自动扩展——装饰放置减少为预期变化。）

## 四、局限与如实声明
1. **单套官方边缘素材的固有限制**：Flat/顶面为单格单 Tile，混合缘格无法逐方向混族；采用"水向优先"
   （水岸契约），陆向波浪由件层流苏覆盖改善但未完全消除（顶面无件层）——以官方示意图为对照依据。
2. **条件搜索机位的对照局限**：`tpr2-closeup-*`/`TS2*`/`TS6c` 等按墙/坡集合搜索实例，修复后可能命中
   不同位置——前后 diff 不代表同机位差异（`pairs-diff.csv` 保留原始数据）；**严格同机位对照 =
   TS1a/TS1b/MVO1a/MVO1b/TS4a/TS4b/MVO3b**（固定机位）。
3. **TS7f 未命中**：见结论 5。
4. **Agent A/B/C 运行超时**（任务书允许主 Agent 继续）：官方素材实测、调用链通读、测试更新由主 Agent
   独立完成；本批所有结论均有主 Agent 亲自执行的证据（素材像素实测/代码行号/测试结果）。
5. 水色计数使用参考色 (71,171,169)±12 容差（URP 渲染近似）；绝对值为像素计数，方向性（三种子一致
   净减）为核心证据。
