# iteration-08 —— D-065 内陆选片纠正、悬崖坡路连接、逐层陆地堆叠强制整改

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 本轮依据任务书 §四/§五/§六：P0 最高优先级（逐层地形 + 内陆悬崖承接）→ P1（内陆边缘选片）→
> P2（坡路连接件），三项全部实际修改生产代码并以真实 Unity 运行时截图 + 全图扫描 + 独立测试交付。

## 一、结论速览

1. **P0（已修复——核心）**：根因=**墙片整格高，Mount 南邻/东西邻的 Hill 中间台面被当墙格覆盖**
   （三种子实测被覆盖台面 143/231/156 格）；地形数据层无缺中间层（Mount 南/东/西直邻低地
   **0/0/0**）。修复：`CollectWalls`/`CollectSideWall` 对 **Mount→Hill 不写墙**（1 级高差由
   iter-06 顶面边缘件表达）→ **Mount 顶面 → Hill 中间台面（完整）→ Hill 南缘墙 → 低地** 逐层可见。
   另修复豁口残留（孤立坡剔除时上/下段补墙）。**全图扫描三种子：应存在连接 807/1135/978 全部合法；
   缺墙/缺低层地表/缺中间层/非法水脚/内陆墙下水背景/无法解释 = 0/0/0。**
2. **P1（已修复）**：混合缘逐方向分族——新增 **8 张水岸边带派生件**（左族掩码片 16px 边带裁剪，
   其余透明，原始 PNG 零改动）+ **WaterEdgeOverlay 层 162**（04 R-LAYER-10c'）：混合缘格
   Flat/顶面取**右族（陆向无白）**、水向浅青白由 band 承担；band 缺失回退左族（安全退化）。
   **全图扫描三种子：内陆误用海岸整片 = 0；混合缘格 67/90/95 全部 band 覆盖（掩码均在
   派生件集合内）。**
3. **P2（已修复）**：坡道=**1 宽×2 高连接件**（上段 `_36/_39` + 下段 `_45/_48`；主 Agent 复测
   `_36` 底行 ↔ `_45` 顶行 alpha 64/64 一致、反向零重叠）。修复：`SlopeCell.LowerY` + 渲染下段
   （仅坡列底部）+ **坡下端条件（南邻低地干燥陆格）** + 豁口两格让位 + 孤立坡剔除补墙。
   **全图扫描三种子：坡 115/156/92 全部"上端高台 + 下端低层陆地"合法（上端错误 0、下端错误 0）。**
4. **测试**：**EditMode 328/328 全过**（r1 30 失败→r2 328→r3 3 失败→r4 终版；失败均为旧断言
   按新规则独立重算，过程留证）；**PlayMode 三种子各 15 total / 14 passed / 0 failed / 1 skipped**
   （skip = `MVO_TPR3_StateContrast_SameFraming` 旧方法既有跳过）。
5. **诊断场景**（任务书 §4.5/§6.4）：新增 `TS8_Iter08DiagnosticScenes` + `TS8h_Iter08SlopeBottomCloseup`
   ——固定地形（Mount 9×9 + Hill 13×13 环 + 低地；混合缘四邻异质格）经**场景内真实
   TerrainTilemapRenderer.RenderWorld** 绘制，装饰层临时隐藏并恢复；三种子各一份带种子后缀。

## 二、目录

```
iteration-08/
  P0-layered-terrain-stack/   # 全图逐层扫描 CSV×3 + summary + scanner 源码 + analysis
  P1-inland-edge-select/      # 内陆暴露边扫描 CSV×3 + analysis
  P2-slope-connector/         # 坡道合法性扫描 CSV×3 + analysis
  diagnostic-scene/           # TS8a–TS8i 三种子诊断图 + 场景数据说明
  official-reference/         # 官方示意图裁剪（iter-07 同源复制）+ 观察记录 + 坡成对复测
  pre-fix/ → 基线 = ../iteration-07/regression-final/seed-*/（同机位同方法，本轮未复制以保历史单份）
  regression-final/seed-*/    # 本轮三种子全量（55 随机 + 7–9 诊断 + XML + log）
  test-evidence/              # EditMode r1–r4 XML/日志
  README.md / final-report.md / SHA256SUMS.txt
```

## 三、生产代码差异（相对 iteration-07）

| 文件 | 改动 |
| --- | --- |
| `TerrainOverlayPlan.cs` | P0：Mount 邻 Hill 不写墙（南缘+东西缘）；P2：`SlopeCell.LowerY`、坡下端条件、豁口两格、`AddFallbackWall`/`IsSlopeUpper` |
| `TerrainTilemapRenderer.cs` | P1：Flat/顶面 waterMask/landMask 拆分 + 混合缘右族 + WaterEdgeOverlay(162) 层/band 选片/缓存；P2：坡下段渲染；`SortingOrders.WaterEdgeOverlay=162` |
| `SortingOrders.cs` | 新增 `WaterEdgeOverlay = 162`（04 R-LAYER-10c'） |
| `Main.unity` | `waterEdgeBandSprites` 16 槽接线（8 派生 Sprite + 空槽） |
| `Art/Terrain/Derived/WaterEdgeBand/*.png(+meta)` | 8 张水岸边带派生件（确定性生成，来源可复现） |
| 测试 4 文件 | P0/P2 断言按规则独立重算（r1→r4 全过程留证）+ TS8/TS8h 诊断场景 |

## 四、局限与如实声明
1. **水格墙（岩壁入水）** 仍存在（132/76/91 处）——官方示意图台地临水墙列的直接入水形态，
   底缘水线为设计内观感；扫描器将其与"窄水沟不写水格"分别单列，不计违规。
2. **水岸边带派生件**仅覆盖掩码 {1,2,3,4,6,8,9,12}（左族 9 槽对应集）——三种子实测混合缘
   水向掩码全部落在此集合内；若未来地形产生其它组合，band 缺失时自动回退左族整片（行为同 iter-07）。
3. **P0 判定**将北缘顶面过渡（官方无墙）与窄水沟（官方保留形态）单列为合法语义，不计违规。
4. **Agent A/B 本轮运行超时**（任务书允许主 Agent 继续）：素材复测（坡成对接缝）、地形生成器
   通读、扫描器与全部生产修复由主 Agent 亲自完成；事后送达的 Agent A 素材实测结论与主 Agent
   复测一致（`_36/_39`+`_45/_48` 成对、每对 alpha 连续反向零重叠）。
   **事后报告互证补记（批发布后送达，如实纳入）**：
   - **Agent A（素材实测）**：坡成对接缝 alpha 64/64、反向 0 ✓；**左右族同掩码实心区 alpha 100%
     相同、RGB 差 0.00–0.06**（支持 band 叠加方案）；边带白≤8px/流苏≤6px/描边≈2px（16px 裁剪充分
     ✓）；对官方示意图**未见 `_36+_45` 坡道实例**（与主 Agent 观察一致于"无孤立三角"，坡道官方
     摆法保留【待验证】）；指出 `_36+_45` 为"对角草面+石脚"——作坡道合理、作**水平楼梯**不成立
     （本项目"台阶"=南向坡道入口语义，沿用素材事实）。
   - **Agent B（调用链）**：**自我更正**早期"Mount 跳级 14/10/11"统计口径（误把水格当低地）——
     地形+高程双条件复算 **Mount 南/东/西干低地邻 0/0/0，与主 Agent 完全一致**；确认"Mount 邻 Hill
     不写墙"修复方向正确；坡下段缺口（其审计的修复前代码）已由本轮 `southDryLow` 条件消除；
     建议"水格高程恒 0"显式断言——**已采纳并新增** `D065I8_WaterCellsElevationAlwaysZero`
     （EditMode r5）；其 R1"高格可 4-邻水、墙只能写水格"经核对为**官方岩壁入水形态**（任务书
     §4.3 规则4：真实水域用合法水脚），扫描器已单列不计违规，**不采纳"数据层修复"**（改变水域
     即违反"不得改变地形生成"）。
5. 条件搜索机位（tpr2/TS2/TS6c 等）修复后可能命中不同实例——严格同机位对照仅固定机位图
   （TS1a/TS1b/MVO1a/MVO1b/TS4a/TS4b/MVO3b）；iter-07 与 iter-08 的同名固定机位图可直接 diff。
