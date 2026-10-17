# D-066 阻断项修复报告（Iteration-11 独立验收阻断）

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜固定种子 **20261013 / 20261014 / 20261015**
> 证据目录：`evidence/terrain-render-integrity-2026-10-17/iteration-11/`
> 规则基线：`tinyswords-tileset-guide@a89e226242b839760dbe533d8d1a3ba6738e3746`（guideVersion 0.0.0.7）
> 游戏主仓库：HEAD `f85fbe3`，**零 git 写**（本轮改动全部留在本地工作区）

---

## 0. 结论摘要

| 阻断项 | 根因 | 修复 | 证据 |
| --- | --- | --- | --- |
| **P0-1 坡路被崖壁覆盖** | `SortingOrders.Slope(170) < CliffWall(180)`——坡上段所在格同时是高格南邻（背景岩壁），岩壁画在坡**之上**；且背景墙取了窄件 `_44` 而非 guide 指定的中段（sheet 43） | ① `Slope` 上调为 **185**（崖壁 180 之上、台阶 190 之下，`docs/04` R-LAYER-10b 同步）；② `AssignWallVariants` 对"坡上段格 + 正北=平台级"强制背景取**中段 18** | 56 处坡（三种子）逐项 `verdict=ok`；`d066/S7-ramp-backdrop.png`、`game-d066-seed-*/G4b|G5b-ramp-*-2x.png` |
| **P0-2 阴影测试只比数量** | PlayMode 断言只比"阴影数 == 高格数"；生产按 guide 只投影南缘，二者本就不应相等 | 新增 `ShadowPlacementGuideTests`（逐格期望 vs 实际、边界/水抑制/多级/缩放/排序）+ PlayMode 同口径逐格断言；生产行为不变 | `d066/shadow-placement-seed-*.csv`（三种子逐格）、`d066/shadow-sprite-audit.csv` |
| **P1 PlayMode 未闭环** | 51 例失败全部是证据捕获套件的 D-063-A 失败关闭（缺 `AIA_CAPTURE_DIR` / `MVO_CAPTURE_MODE` / 历史硬编码目录） | 用**合规新目录**重跑：`AIA_CAPTURE_DIR`、`MVO_CAPTURE_MODE=tri-d066-mvo2`、`WS_ALLOW_LEGACY_CAPTURE=1 + WS_LEGACY_CAPTURE_OUT_DIR`（重定向到新目录，绝不覆盖历史）；并修正 1 条失效断言（水中岩石零产出） | 见 §4 逐条分类 |
| **P1 证据哈希不一致** | 报告在清单生成后又被修改 | 全部稳定后重算清单 → 远端 sparse clone 逐文件复算 | 见 §5 |

---

## 1. 工作区与基线核对

- 主仓库 `git rev-parse HEAD` = `f85fbe3…`（未变），`git status` 与本轮开始时一致（生产改动均未提交）。
- Iteration-11 既有证据目录结构、`SHA256SUMS.txt`、`tests/remote-verify.txt` 逐项复核；旧清单报告哈希与当时报告不一致
  （报告在清单后更新过），本轮在**全部内容稳定后**重算并重验。
- guide.json：SHA-256 `8F934430…B43A0E`（未变）。

---

## 2. P0-1 坡路与崖壁重叠 / 坡路不可辨

### 2.1 复现（修复前）

`capture-final/seed-20261013-ramp-cells.csv` 坐标 `(120,26)`：

```
slope     = Tilemap_color1_39   （右坡上段 raw40）
cliffWall = Tilemap_color1_44   （窄件崖脚 raw45）
```

生产调用链：`CollectRamps`（坡上段 T=(120,26)）→ `CollectWalls`（高格 (120,27) 南邻 = T ⇒ 墙脚）→
`AssignWallVariants`（T 左右皆无墙 ⇒ 窄件 `_44`）→ `LayOverlays`（CliffWall 层）→ `SortingOrders`（170 < 180）。

**根因**：固定版 guide.json `autotile.stairsRules.rampBackdrop` 明确：

> "Sheet IDs 37 (left-ramp top) and 40 (right-ramp top) MAY have a cliff-face-middle backdrop
> (sheet 43, semantic elevatedGround id 18) painted in the same cell **underneath the ramp top sprite**."

即：坡上段与背景岩壁**同格**是 guide 允许且有意的组合，但背景必须在坡**之下**，且背景片 = sheet 43 = 语义 18 = **中段**。
旧实现两点都错：图层反了（岩壁 180 盖住坡 170），背景取的是窄件 `_44`。

### 2.2 修复

| 文件 | 修改 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/SortingOrders.cs` | `Slope` 170 → **185**（> CliffWall 180，< Stairs 190），常量注释写明 guide 依据 |
| `docs/04-核心玩法与模拟规则.md` | R-LAYER-10b 行同步为 185 + D-066 说明 |
| `Assets/_Project/Scripts/World/GenSteps/TerrainOverlayPlan.cs` | `AssignWallVariants` 新增：坡上段格若有墙脚且正北高程 == 坡的 `PanelLevel` ⇒ 背景强制中段（1） |
| `Assets/Tests/EditMode/SortingOrdersTests.cs` | 期望表 Slope 185 |

未做：删除崖脚、抬高无关图层、改水格、改地形。

### 2.3 验证（数据）

`d066/ramp-placement-seed-*.csv`（三种子，逐坡一行）：

| 列 | 含义 | 结果 |
| --- | --- | --- |
| top/lower sprite | 上段/下段实际片 | 全部 raw37/40 + raw46/49 |
| corner (x,y,elev) | 平台角坐标与高程 | 全部 = 上段水平相邻、高程 = PanelLevel |
| backdrop_wall / expected | 同格背景片与是否应当存在 | 6 处应为背景者全部 `_42`（中段） |
| apron_ok | 落脚 2×2 + 正下方 | 全部 1 |
| slope_order / wall_order | 图层排序 | 185 / 180 |
| **verdict** | 综合判定 | **56/56 ok，bad=0** |

三种子坡数：13 → 16，14 → 18，15 → 22（合计 56）。

### 2.4 验证（真实画面）

- `d066/S7-ramp-backdrop.png`：确定性场景（坡上段正上方即平台格）——可清楚看到草坡上段、石脚下段、
  平台角与背景岩壁的前后关系。
- `game-d066-seed-20261013/G4b-ramp-left-2x.png`、`G5b-ramp-right-2x.png`（2× 像素对齐近景，真实 Main 场景）；
  另 `G4/G5` 为 1:1 版。
- 对照：`game-seed-20261013-prefix/`（D-065 修复前同机位）。

---

## 3. P0-2 阴影逐格位置

### 3.1 guide 规则（独立解析）

`assets[id=shadow].placementRules.elevatedGroundOnly`：

- 当前格 `elevation ≥ 1`；
- 南邻为**更低陆地**（水 -1 不承接）；
- 南南邻仍为**陆地**（阴影可见区向南两行，落水即整张抑制）；
- 锚点向南整格（`placement.offsetFromElevatedGround.y = 64`）。

### 3.2 新增测试（独立预期，不调用生产函数）

`Assets/Tests/EditMode/ShadowPlacementGuideTests.cs`（6 例）：

1. `GuideShadow_NumericContract_MatchesLocalSpriteAndProduction`：解析 guide 的 192/128/32/64 数值；直接读本地
   `Shadow.png` 原始字节求 Alpha 边界（79×80）→ 断言生产缩放 1.6 = 128/80，可见覆盖 = 2.0000 格（= guide），
   PPU=64、排序 150；
2. `Shadow_ThreeSeeds_EveryCellMatchesGuideRule(13/14/15)`：**逐格**期望集合 == 实际锚点集合（缺一即失败）；
3. 南邻水 ⇒ 0 张；4. 南南邻水 ⇒ 0 张；5. 南邻同高 ⇒ 只有最南缘 8 张、锚点 y=3.5；
6. 低地/地图边界 ⇒ 0 张；7. 多级台地 ⇒ 每级南缘各自投影（Mount 6 + Hill 10）。

PlayMode `FastNoiseRenderIntegrationTests` 同步改为逐格集合比对（不再只比数量）。

### 3.3 逐格证据

`d066/shadow-placement-seed-<seed>.csv`（每高格一行：期望/实际锚点、南邻与南南邻高程与地形、是否允许、
实际 sprite/缩放/排序、verdict）：

| 种子 | 高格行数 | 期望阴影 | 实际阴影 | mismatches |
| --- | --- | --- | --- | --- |
| 20261013 | 2296 | 143 | 143 | **0** |
| 20261014 | 2296 | 265 | 265 | **0** |
| 20261015 | 2296 | 251 | 251 | **0** |

### 3.4 缩放 1.6 的依据

`d066/shadow-sprite-audit.csv`：画布 192×192、Alpha 可见 79×80、PPU 64、Pivot (96,96)、
`required_scale = 128/80 = 1.6000`、`visible_footprint_tiles = 2.0000`（= guide 的 128px / 2 格）。
真实画面 `game-d066-seed-20261013/G9-shadow.png`：软边带、无方形直角、无长条、无水溢出。

---

## 4. P1 PlayMode 闭环

### 4.1 第一次全量（无捕获环境）——逐条分类

`74 例：23 通过 / 51 失败 / 0 跳过`；51 例失败**全部**是 `CaptureRejectedException`（D-063-A 失败关闭）：

| 类别 | 例数 | 原因 |
| --- | --- | --- |
| `AiAMapEnvCaptureTests` + `CliffAuditCaptureTests` | 11 | 缺 `AIA_CAPTURE_DIR` |
| `MapVisualOverhaulCaptureTests`（TS*/MVO*） | 17 | 缺 `MVO_CAPTURE_MODE` |
| 历史硬编码目录套件（VisualVerification / NineSlice / UI / Foam / LifeView / 等） | 22 | 硬编码历史目录被拒 |
| `Iter11GameCaptureTests` | 1 | 目标文件已存在（拒绝覆盖，符合预期） |
| 功能类（CameraInitialFraming / FastNoiseRender / FinalIntegration / FxTriggerPath / LifeAnimCoverage / PlayModeSmoke / ToolInputChain / TreeOverlapRender / UIInteraction / UISnapshotConsistency） | **22 通过 / 0 失败** | — |

### 4.2 第二次全量（合规捕获目录）

环境：`AIA_CAPTURE_DIR=…/d066/aia-captures`、`MVO_CAPTURE_MODE=tri-d066-mvo2`、
`WS_ALLOW_LEGACY_CAPTURE=1` + `WS_LEGACY_CAPTURE_OUT_DIR=…/d066/legacy-captures2`（重定向到**新目录**，
不触碰历史）、`WS_WORLD_SEED=20261013`。

结果：**74 例：73 通过 / 0 失败 / 1 跳过**（`tests/d066-playmode-full.xml`）。

| 分组 | 例数 | 结果 |
| --- | --- | --- |
| 功能性（CameraInitialFraming / FastNoiseRender / FinalIntegration / FxTriggerPath / LifeAnimCoverage / PlayModeSmoke / ToolInputChain / TreeOverlapRender / UIInteraction / UISnapshotConsistency） | 22 | **22 通过** |
| `AiAMapEnvCaptureTests` + `CliffAuditCaptureTests`（新目录 `d066/aia-captures`） | 11 | **11 通过**（产出 17 张审计图，随本报告发布） |
| `MapVisualOverhaulCaptureTests`（`tri-d066-mvo3` → 新目录） | 17 | **16 通过 + 1 跳过**（`MVO_TPR3_StateContrast_SameFraming`：套件内既有 `Assert.Ignore`，缺历史对照基线，非本轮引入） |
| 历史硬编码目录套件（`WS_LEGACY_CAPTURE_OUT_DIR` 重定向新目录） | 23 | **23 通过**（192 张截图写入新目录，历史目录零写入） |
| `Iter11GameCaptureTests`（本轮真实游戏截图，重新生成） | 1 | **1 通过** |

### 4.3 最终逐条结果

- **0 失败**；唯一跳过为 `MapVisualOverhaulCaptureTests.MVO_TPR3_StateContrast_SameFraming`（套件内 `Assert.Ignore`：
  需要历史 `before/` 基线图，本轮不覆盖历史故不生成；属既有跳过，非环境错误、非本轮功能缺陷）。
- 捕获套件输出目录（均新建、均经 Guard 放行、均不触碰历史）：
  - `d066/aia-captures/`（17 张，随证据发布）
  - `d066/legacy-captures3/`（192 张；镜像历史套件的截图，体积 22.7 MB，**不随证据发布**，仅以 XML 结果与计数为证）
  - `terrain-render-integrity-2026-10-17/d066-mvo3/`（74 张，8.5 MB；同上不发布）
- 说明：`WS_ALLOW_LEGACY_CAPTURE=1` 仅解锁"允许写历史硬编码路径"的判定，配合
  `WS_LEGACY_CAPTURE_OUT_DIR` 把落盘重定向到全新目录；历史目录文件时间戳保持 2026-10-16，无任何写入。

### 4.4 失效断言登记

| 用例 | 旧断言 | 失效原因 | 替换 |
| --- | --- | --- | --- |
| `TinySwordsPlacementEvidenceTests.CapturePlacementEvidenceV04` | "应存在水中岩石" | D-065 iter-05 P4 起水中岩石禁止散落开放海面（`ScatterStep` 写 discard，产量恒 0） | 改为负向断言（不得存在）+ 水面全景捕获；零产出契约另由 `ScatterExclusionTests.D065I5`、`WorldGeneratorTests` 锁定 |

---

## 5. P1 证据与哈希

**校验提交 `d2e899c7947360b3ffd3e2b873502116aef0eeb3`（D-066 发布）：清单 385 文件 → `ok=385 / bad=0 / missing=0`。**
其后追加 `tests/remote-verify.txt` 记录（提交 `d208eeb`），该文件与 `SHA256SUMS.txt` 自身不入清单，
被清单覆盖的 385 个文件字节未变，已在 `d208eeb` 上再次逐文件复算通过（同一结果）。

- 清单覆盖 `iteration-11/` 下全部文件（含本报告、D-066 逐格 CSV、三种子真实游戏截图、测试 XML/日志）；
- 远端用 `git clone --depth 1 --filter=blob:none --sparse`（经本地代理 127.0.0.1:7897）检出同一提交，
  按 `SHA256SUMS.txt` 逐文件读取原始字节复算 SHA-256；
- 历史零覆盖：`git diff --name-only 3d23e70 <HEAD>` 全部落在 `iteration-11/` 之下；
  `iteration-00…10` 文件数 111/134/71/145/170/147/242/388/310/755/866 未变（基线提交 `3d23e70`）。

---

## 5.1 测试汇总

| 套件 | 结果 | 证据 |
| --- | --- | --- |
| EditMode 全量 | **373 / 373 通过，0 失败，0 跳过** | `tests/d066-editmode.xml`（日志 `.log`） |
| PlayMode 全量（合规捕获目录） | **73 通过 / 0 失败 / 1 跳过（既有）** | `tests/d066-playmode-full.xml` |
| 真实游戏截图（三种子） | 种子 13：12 张；14：13 张；15：13 张 | `game-d066-seed-20261013|14|15/` |
| 坡路逐格 | 56 处，`verdict=ok` 56 | `d066/ramp-placement-seed-*.csv` |
| 阴影逐格 | 143 / 265 / 251 张，mismatch 0 | `d066/shadow-placement-seed-*.csv` |
| 阴影素材审计 | 79×80 可见 / 192 画布 / PPU 64 / scale 1.6 / 2.0 格 | `d066/shadow-sprite-audit.csv` |

## 6. 修改文件清单

生产代码：

| 文件 | 修改 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/SortingOrders.cs` | `Slope` 170 → 185（+ guide 依据注释） |
| `Assets/_Project/Scripts/World/GenSteps/TerrainOverlayPlan.cs` | `AssignWallVariants`：坡上段背景墙强制中段（guide rampBackdrop） |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | （D-065 已改；本轮仅随排序生效，无新改动） |

测试：

| 文件 | 修改 |
| --- | --- |
| `Assets/Tests/EditMode/SortingOrdersTests.cs` | Slope 期望 185 |
| `Assets/Tests/EditMode/TerrainGuideRulesTests.cs` | +3 例（排序/三种子背景中段/确定性坡+背景叠放） |
| `Assets/Tests/EditMode/ShadowPlacementGuideTests.cs` | 新增 6 例（guide 数值/三种子逐格/边界/多级） |
| `Assets/Tests/PlayMode/FastNoiseRenderIntegrationTests.cs` | 阴影改为逐格集合比对 + 缩放/排序断言 |
| `Assets/Tests/PlayMode/TinySwordsPlacementEvidenceTests.cs` | 水中岩石失效断言替换 |
| `Assets/Tests/PlayMode/Iter11GameCaptureTests.cs` | 输出 `game-d066-seed-*`；新增 2× 坡路近景 |

工具：

| 文件 | 用途 |
| --- | --- |
| `Assets/_Project/Editor/TerrainDiagnostics/D066Capture.cs` | 生成 S7 场景、三种子坡路/阴影逐格 CSV、坡路近景、Shadow Alpha 审计 |

文档：`docs/04`（R-LAYER-10b 185）、`docs/DECISIONS.md`（D-066 待补）。

---

## 7. 剩余问题（不写成通过）

1. 历史硬编码目录的 22 个证据捕获套件只能通过 `WS_LEGACY_CAPTURE_OUT_DIR` 重定向运行；
   它们**不能**在"写回历史目录"的语义下运行（本轮禁止覆盖历史），重定向后的截图不并入历史迭代目录。
2. `MapVisualOverhaulCaptureTests` 的 17 个套件与 `Iter11GameCaptureTests` 输出的截图集合存在内容重叠
   （前者是历史套件重跑，后者是本轮 canonical 证据），发布证据以 `game-d066-seed-*` 为准。
3. 1 行宽台面的视觉密度仍属生成器设计（D-061），未改地貌比例。
4. 本地 `Shadow.png` 可见团 80px，与 guide 记录的 128px 素材存在版本差异，生产用 1.6 缩放适配；
   若换用 guide 版素材应恢复 1:1。
