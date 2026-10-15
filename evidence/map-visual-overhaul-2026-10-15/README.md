# 地图视觉重构专项证据（MVO，2026-10-15）

《神之沙盘》（WorldSandboxPrototype，Unity 6 + URP 2D，Tiny Swords 素材）**地图视觉重构**——以官方示意图
`Tiny-Swords-官方素材示意图.jpg` 为视觉基准，对程序生成地图的构图/高差/悬崖/水岸/地表/装饰的整轮修复证据。

> 本批次只包含验证证据材料，不包含任何游戏源码、Scene、Prefab、ScriptableObject、Packages 或 ProjectSettings。
> 主游戏仓库保持私有；本轮主仓库改动**未提交**（决策 `D-058` 已挂号于 `docs/DECISIONS.md` §2.13）。

## 批次摘要

| 项 | 值 |
|---|---|
| 证据生成日期 | 2026-10-15（本地 18:51–21:29 / UTC 10:51–13:29） |
| 修复前基线 | `baseline/`（EditMode 199/199 健康快照 + 全图/局部新鲜截图） |
| 一轮迭代证据 | `iter1-after/`、`iter1-seed-20261014/`、`iter1-seed-20261015/`（A 中期复核"代际跨越、有条件通过"的实证） |
| **终批证据** | `after/`、`seed-20261014/`、`seed-20261015/`（二轮条件修复后重拍）、`cloud/`（云树同框） |
| 测试结果 | `test-results/`（**终态：EditMode 211/211（editmode-final-r4）+ PlayMode 60/60（full-regression5）+ cloud-v7 1/1**；199→209→211 与 59→60 的全部中间轮含失败与处置，原始 XML 未转格式） |
| 对照材料 | `contact-sheet-mvo.svg`（官方原图×基线×终版对照联系表）+ PNG 预览、`capture-manifest.md`、`SHA256SUMS.txt` |

## 测试基线声明（务必先读）

- 主游戏仓库本轮全部改动**未提交**：捕获时主仓库 HEAD `47d8185`，该 HEAD **不包含**本轮任何代码。
- 本批全部截图与测试结果来自**当时未提交的工作区代码**。请勿将本批证据解读为"`47d8185` 提交版本的行为"。
- 工作区自始至终保护了前序 Agent 的 306 项未提交成果（零覆盖、零回退；被本轮证据测试覆盖的 3 个证据目录已先备份至 `_backup-prev-state/`）。

## 修复范围（六域审计 → 统一方案 → 两轮实施）

| 域 | 修复内容 | 主要生产文件 |
|---|---|---|
| 世界生成（C） | 水陆重写：整数 fBm 值噪声海陆场 + 阈值精确命中水比门禁 + <3 格碎块清理（消灭"蕾丝拼布"与 1 格碎岛）；**新增高地平台步骤**（3~6 块崖壁环绕草地台地，豁口贯通不变式：源头平滑+翼格整修+双终检+摆位重试，"每环要么带有效豁口、要么不产崖"）；海岸礁石收窄为点缀（水平生长 0.75） | `WaterLandStep.cs`（重写）、`PlateauStep.cs`（新增）、`CliffShoreStep.cs`、`WorldGenerator.cs` |
| 地形渲染（D） | 崖选片 v3 端帽感知（段首/段尾/竖段中段/竖末端/孤立格分片，中段维持 v2 段锚）；高地草瓦（`SetElevation` 派生崖环包围台地→`grassTileHigh`，官方式高差双色）；缺接线告警；**林界流苏**（草↔林边界选右族深绿草穗边片：RT_Grass 四资产不可达 marker 规则 + 渲染器子集掩码选片，正交邻水格白边+泡沫冻结不变；草↔崖过渡不在掩码内，登记 UQ-19）；`RT_Forest` color5→color4（青绿读作水的最大误读源） | `TerrainTilemapRenderer.cs`、`RT_Grass*.asset`×4、`RT_Forest.asset`、`Main.unity`（renderer 块） |
| 环境装饰（E） | 树簇簇心锚定（跨度≤3、树冠搭接）+ 开阔草偏好 + **林心通道**（40% 预算簇心取 Forest 格）；灌木挂树簇、岩石挂崖/水；派生装饰除数 15→8；树 scale 0.85~1.15 + 镜像（确定性哈希）；灌木绿变体加权 0.75（Bushe1/2 绿、3/4 蓝青像素取证）；云层 250（R-LAYER-09a，云盖树冠/高地） | `ScatterStep.cs`、`ResourceNodeRenderer.cs`、`DecorationRenderer.cs` |
| 云渲染修复（A 终审证据链牵引，主 Agent 实施） | **两项长期潜伏生产缺陷修正**：① 云精灵切片接在 36 片图集的空白格上——云自接入以来从未渲染出来（每表 17–32 空白/近空白格，v04 时代证据图同样无云）；`Main.unity` cloudSprites 切换为 8 个含画稿切片（Clouds_01_21/02_13/03_13/04_13/05_23/06_13/07_13/08_13，像素取证）。② `DecorationRenderer.Update` 的 `CloudState` 结构体副本变异从不写回列表——云漂移自接入以来从未实际发生（仅 ~0.2px 抖动，反射探针定位）；副本写回一行修复，实测漂移 19.3px/3.1s（理论 18.6）。 | `Main.unity`（cloudSprites 块）、`DecorationRenderer.cs`（写回一行） |
| 契约与集成（主 Agent） | `WS_WORLD_SEED` 种子覆盖（3 行，缺省行为不变）；场景 `terrainPalette 0→1`（低地鲜绿）；`SortingOrders.Cloud=250`；R-MAP-16 四步→五步（docs/04 修订）；T_04_08 派生区间契约同步；集成编译修复与两处测试语义随新世界修正（见 capture-manifest §诚实记录） | `GameBootstrap.cs`、`Main.unity`、`SortingOrders.cs`、docs/04、DECISIONS.md（D-058） |

**保持不变**：程序化生成与种子确定性（全链 `DeterministicRandom`，新步骤 `Fork(5)` 独立盐；同种子逐位自一致）；水比 [0.20,0.35] / 最大陆块 ≥25% 门禁；边圈全水；树/金矿/食物/灌木/云数量 §13.1 区间（只调分布不调总量）；生物/建筑/资源/工具/UI 全部玩法系统。

## 迭代史（可核查）

| 轮 | 内容 | 判定 | 证据 |
|---|---|---|---|
| 基线 | 修复前（前序 Agent 悬崖 v2 等成果已含） | — | `baseline/` |
| 一轮（R1） | 六域审计后统一方案实施；EditMode 209/209、PlayMode 59/59 | A 中期复核：**代际跨越、有条件通过**（条件：G7 草↔林白边、林地块树冠密度） | `iter1-*` |
| 二轮（R2） | 两条件修复 + 灌木绿变体 + 云树证据用例；EditMode **211/211**、PlayMode **60/60** | A 终审：见仓库批次说明（终审结论同轮发布） | `after/`、`seed-*`、`cloud/` |

## 分类导览与逐张证据边界

### `baseline/`（修复前）
- `framing/hud_1280x720.png`：修复前完整地图 GameView（含 UI）。**能证明**：修复前的"黄色平面 + 碎斑水域 + 零散崖条"基线状态。**不能证明**：任何修复后行为。
- `placement/`（25 文件）：修复前局部证据（岛岸/湖心/树簇/泡沫等）。同上边界。

### `iter1-after/`、`iter1-seed-20261014/`、`iter1-seed-20261015/`（一轮，已被二轮取代）
- **能证明**：一轮代码（生成重构+崖 v3+高草+装饰一轮改动）的真实画面；A 中期复核的对象。
- **不能证明**：二轮条件修复（流苏边/林心树簇/绿灌木）后的最终状态——终态看 `after/`。
- 保留原因：迭代可核查性（中期复核结论的实证基础）。

### `after/`（终批，种子 20261013）
- `MVO1a-fullmap-overview-ortho40.png`：**完整地图全景（主对照图）**。能证明：终版整体构图（大陆基形/海湾/台地崖环/双色草地/林冠/装饰分布）。不能证明：种子泛化（看 seed-*）。
- `MVO1b-fullmap-entire-ortho64.png`：128×128 恰满幅全图。同上。
- `MVO2a-d`（崖长段/水脚/陆脚/转角近景）：崖壁 v3 端帽感知选片、底脚分组、泡沫贴崖脚。能证明：崖拼接规则在真实地图的落位。不能证明：所有可能的玩家笔刷编辑形态。
- `MVO3a-shore-grass-water.png`：水岸草缘过渡 + 泡沫。能证明：水岸白边+泡沫冻结不变（与官方岸线语义一致）。
- `MVO3b-grass-forest-edge.png`：草↔林边界**深绿流苏**（二轮条件 1；掩码仅 Forest 邻，草↔崖过渡为既有 9 槽深色边+崖片草顶衔接，登记 UQ-19）。林侧自身边片的浅圈残余仍存（A 终审裁决项，见"已知残留"）。
- `MVO4a-tree-cluster.png` / `MVO4b-rock.png`：树簇混植/大小错落/镜像；岩石挂崖挂水。能证明：装饰分布规则。

### `seed-20261014/`、`seed-20261015/`（终批，多种子）
- `WS_WORLD_SEED` 环境变量覆盖（GameBootstrap 3 行钩子；缺省行为不变）。**能证明**：布局随种子显著变化且地形/素材规则不破坏（生成可靠性）。每目录 13 文件同 `after/` 结构。

### `cloud/`
- `MVO5a-cloud-over-forest-frame{1,2}.png`：云（order 250）真实入画并**像素级压冠**——v7 终版：间隔 6.1s 实测位移 37.5px，A 量测争议像素 **120 ≥ 20**（云右半叶滑入松针带下方，order 250 遮挡语义首次由图像直接证实）。历轮 v1–v6 无效/作废行已在 capture-manifest 中逐行标注 superseded（取证五轮攻坚记 verification-notes 附录 A）。

### `test-results/`（原始 XML，未转格式）——终态链：editmode-final-r4（211/211，终代码）+ playmode-full-regression5（60/60，终代码）+ playmode-cloud-v7（1/1，G10 终版）
- `editmode-baseline.xml`（199/199，修复前健康快照）→ `editmode-mvo-integration{2,3}.xml`（中间轮失败与修复过程：205/209、208/209，处置见工作记录）→ `editmode-mvo-integration4.xml`（209/209）→ `editmode-mvo-r2.xml`（**211/211 终态**）。
- `playmode-full-regression{1,2}.xml`（57/59、59/59，两个测试语义随新世界修正）→ `playmode-full-regression3.xml`（**60/60 终态**）。
- `playmode-after2/seed14b/seed15b-results.xml`（各 5/5）、`playmode-cloud-results.xml`（1/1）。
- 两个测试的期望语义修正（非弱化）：`UiWiringEvidenceTests` 色板轨显式切 palette 0（场景默认已 0→1）；`TinySwordsPlacementEvidenceTests.FindIsland` "4 邻全水孤岛"→"最孤立岸线"（碎岛按设计清除）。理由在测试注释与工作记录。

## 已知残留（如实登记，未冒充完成）

1. **RT_Forest 自身边片浅圈**（林侧）：草侧已换深绿流苏，林侧边片仍左族（浅描边）。A 终审裁决项；机制可复用（RT_Forest 加 marker）。
2. **开阔水白色弧斑**：泡沫 192 画布向水格外溢 64px（TV-02-R4 对齐方案设计内副作用，像素取证登记 UQ-17）；官方水面本为平色，浅水亮带未做。
3. **台地环"细墙感"**（全图视角 1 格带宽）与**竖段中段岩壁片缺失**（54 片无中段片，素材能力边界，缺口已按 docs/05 流程申报 UQ-16）。
4. **G11**：笔刷改地形后物体落水不重定位（玩法工具域，超本轮范围）。
5. 树总量 §13.1 [120,200] 未动（玩法常量，只调分布）；`ForestCenterTreeShare=0.4`、`BushGreenVariantProbability=0.75` 等新调优参数均 `未验证`。

## 运行方式（可复现）

```bash
UNITY="D:/software/Unity Hub/Editor/6000.6.5f1/Editor/Unity.exe"; P=<仓库根>
# 终批捕获（5 用例/轮；真实 GameView，窗口化，禁 -batchmode）
MVO_CAPTURE_MODE=after "$UNITY" -projectPath "$P" -runTests -testPlatform PlayMode \
  -testFilter "WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests" \
  -testResults <xml> -logFile <log>
# 多种子：MVO_CAPTURE_MODE=seed-20261014 WS_WORLD_SEED=20261014（15 同理）；云：MVO_CAPTURE_MODE=cloud + MVO5 过滤
# 全量回归：EditMode -batchmode -nographics；PlayMode 全量必须带 MVO_CAPTURE_MODE=pm-fullrun-scratch（防误写 before/）
```
