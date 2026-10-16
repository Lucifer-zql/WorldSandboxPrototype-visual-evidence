# 基础地图生成质量整改 —— 视觉证据批次（D-061，2026-10-16）

> 任务：制作人 D-061《基础地图生成质量整改》。在 D-060（FastNoiseLite 单场 FBm）基础上，把高程生成升级为
> **多尺度组合噪声场 + 域扭曲**，消除"人工感封闭环形高地 / 重复轮廓 / 地形碎片化"，并用**实际 Unity 游戏画面**证明改善。
> 渲染语义（Tiny Swords 七层序）**不变**，仅换生成算法与分类/后处理。
>
> 对照基线：**L1 改造前** = `docs/impl/evidence/fastnoise-terrain-2026-10-16/`（D-060 单场 FBm，直接对照）；
> **L2 更早旧模型** = `docs/impl/evidence/terrain-structure-2026-10-16/`（D-059 手工团块，历史参照，不作达标判据）。

---

## 一、证据边界声明（务必先读）

1. **主仓库 HEAD 不含本批改动。** 采集时点 `HEAD=47d8185`（分支 `main`），工作区脏计数 **360**
   （含 D-059/D-060/D-061 全部未提交改动）。**本批截图无法用主仓库 commit 复现**，只能由工作区快照复现。
2. **截图来自未提交工作区。** 所有 PNG 由 Unity 编辑器 PlayMode 用例
   `WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests` 实时渲染落盘，非离线合成。
3. **本批为工作区证据快照，不覆盖任何既有证据批次。** 采集前后对 `docs/impl/evidence/` 下
   **全部历史批次**（排除本轮）做 sha256 复核：**1236 项全 OK、0 FAILED**（见 §八）。
4. **本批目录内 `comparison/` 为主 Agent 用 Pillow 合成的三层对照图**（L2/L1/L0 并排），
   其像素来自上述真实截图，非新渲染。

---

## 二、本轮改动（生产代码）

| 项 | 内容 |
| --- | --- |
| **唯一生产改动文件** | `Assets/_Project/Scripts/World/GenSteps/PlateauStep.cs`（类 `ElevationStep`，D-061 全量重写） |
| 新增测试 | `Assets/Tests/EditMode/BaseTerrainRefinementTests.cs`（孔洞回填 + 清理语义定向补测，补 Agent C 审计缺口 G1） |
| 捕获路由 | `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` 新增 `bt-` 前缀路由（仅接线，断言未改） |
| 第三方 | FastNoiseLite v1.1.1（MIT，`Assets/_Project/ThirdParty/FastNoiseLite/`，D-060 引入，本批未改） |

### 2.1 最终噪声组合与参数（冻结）

| 场 | 类型 | 参数 | 权重 |
| --- | --- | --- | --- |
| 大尺度主导场 | OpenSimplex2 + FBm | freq **0.012** / 3 octaves / lacunarity 2.0 / gain 0.5 | **0.78** |
| 中尺度细节场 | OpenSimplex2 + FBm | freq **0.035** / 2 octaves / lacunarity 2.0 / gain 0.5 | **0.22** |
| 域扭曲 | 独立 FastNoiseLite 实例，`DomainWarpType.OpenSimplex2` | freq **0.018**，`SetDomainWarpAmp(25)`（实测 max 位移 ≈5.5 格） | — |

- 组合后权重和 = 1（两场各归一化至 ±1）→ 统一高程场，分类与渲染同源。
- **分类**：分位数阈值，Hill(level1) 目标 **10%**、Mount(level2) 目标 **4%**（占总格数）。
- **后处理**：Mount 碎簇 `<6` 格 → Hill；纯 Hill 足迹（不含 Mount）`<12` 格 → 低地；
  被高地完全包围、不连水、`≤64` 格的封闭干孔洞 → Hill（消内部环形平台观感）。
- 形状**完全由噪声场自然形成**，无手工圆形/椭圆/中心点/环形区域。

> **域扭曲振幅说明（偏离任务书字面值，已登记）**：任务书"振幅 12 个地图格"按物理位移解读对应入参 ≈55；
> 实测该档产生"重复弯曲窄带 + 更大内部孔洞"（违反任务书 §2.5/§5-5），依任务书"仅允许针对实际统计与画面微调"授权，
> 取入参 **25**（实测 max≈5.5 格、mean≈2.7 格）。标定与全档扫描见
> `docs/impl/parallel-repair/d061-preflight-2026-10-16.md` §一/§二。

---

## 三、采集方法（可复现命令）

**逐种子运行**（Unity 6000.6.5f1 / Windows 11 / Git Bash；串行；**不加 `-batchmode`**，窗口化渲染）：

```bash
export MVO_CAPTURE_MODE=bt-seed-20261013   # 14/15 依次替换
export WS_WORLD_SEED=20261013
"D:/software/Unity Hub/Editor/6000.6.5f1/Editor/Unity.exe" \
  -projectPath "D:/data/UnityProjects/WorldSandboxPrototype" \
  -runTests -testPlatform PlayMode \
  -testFilter WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests \
  -testResults ".../base-terrain-refinement-2026-10-16/test-results/bt-seed-20261013.xml" \
  -logFile     ".../base-terrain-refinement-2026-10-16/test-results/bt-seed-20261013.log"
```

- 每种子约 40s（8 用例），三种子串行，互不干扰。
- 落盘核验：每轮生成 `seed-2026xxxx/`（**26 PNG + `capture-manifest.txt` = 27 文件**）。
- **路由安全**：`MVO_CAPTURE_MODE` **未设置时默认 `before`**，会写入历史目录
  `map-visual-overhaul-2026-10-15/before/`——本轮每次运行均显式 export `bt-*`（Agent D 独立复核路由，见 §八）。

---

## 四、三种子地形分类比例统计（对验收带结论）

数据源：各 `seed-2026xxxx/capture-manifest.txt` 的 `stat ts1` 行（分母 = 128×128 = 16384）。

| 种子 | water% | lowland% | hill%(elev1) | mount%(elev2) | hillBlocks | mountBlocks | 墙格 | 坡格 | 台阶 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 20261013 | 32.8857 | 53.1006 | **10.0586** | **3.9551** | **4** | **5** | 203 | 155 | 1 |
| 20261014 | 22.0032 | 63.9954 | **10.0220** | **3.9795** | **9** | **10** | 367 | 234 | 3 |
| 20261015 | 26.3916 | 59.4543 | **10.1563** | **3.9978** | **7** | **5** | 344 | 156 | 3 |

**对验收带结论**（Water ∈ [20,35]、Hill ∈ [8,18]、Mount ∈ [2,8]）：

- ✅ **Water** 32.89 / 22.00 / 26.39 % —— 三种子全在带内（与 L1 逐位相同：水陆逻辑未改）。
- ✅ **Hill** 10.06 / 10.02 / 10.16 % —— 三种子全在带内。
- ✅ **Mount** 3.96 / 3.98 / 4.00 % —— 三种子全在带内。
- ✅ 四类（water/lowland/hill/mount）构成干净划分，和 = 100%（±舍入）。

### 4.1 碎片化收敛（本轮核心量化指标）

| 指标 | L2 旧块状 (D-059) | L1 改造前 (D-060) | **L0 本批 (D-061)** | 结论 |
| --- | --- | --- | --- | --- |
| `hillBlocks`（Hill 4 连通分量） | 1 / 1 / 1 | **25 / 25 / 26** | **4 / 9 / 7** | **碎片化大幅收敛（−84% / −64% / −73%）** |
| `mountBlocks`（Mount 分量） | 1 / 1 / 1 | **17 / 22 / 26** | **5 / 10 / 5** | **显著收敛（−71% / −55% / −81%）** |
| `cliffWallCells` | 166 / 182 / 172 | 533 / 610 / 629 | 203 / 367 / 344 | 墙格随分量周长下降（碎斑边缘减少） |
| `slopeCells` | 120 / 100 / 84 | 405 / 473 / 442 | 155 / 234 / 156 | 同上 |
| `stairs` | 0 / 2 / 1 | 9 / 7 / 9 | 1 / 3 / 3 | 分量更少更大 → 南向入口上限下降（规则未动，见 §七.3） |

> 高地覆盖面积（`hillCells`）稳定在 2296 / 2294 / 2319（≈14.0% 总格数，分位数锁定），
> 即**在保持覆盖带的同时把分量数降下来**——正是 D-061 的验收核心。

---

## 五、逐张"能证明 / 不能证明"清单

> **能证明** = 该图在像素级确凿可见的事实；**不能证明** = 该图不足以支撑的结论（需转数据/其它证据）。
> 图内 UI（顶栏/底部工具条/事件面板）为游戏正常 HUD，不属地形证据。

| 文件 | 能证明 | 不能证明 |
| --- | --- | --- |
| `TS1a-fullmap-ortho40.png` | 全图俯视下高地呈**自然连片有机形状**；无大量独立小块、无重复规则平台 | 具体比例数值（需 `stat ts1`）；单格级邻接正确性 |
| `TS1b-fullmap-entire-ortho64.png` | 整图 128×128 满幅入画；宏观上高地连片、水域分布自然 | 同上 |
| `TS2a-waterfoot-cliff.png` | 临水收口墙格（Row==5）近景：崖壁立于**陆格**、下为真实水面＋泡沫线 | 该墙格是否为该种子唯一水脚（需 manifest） |
| `TS2b-longest-horizontal-wall.png` | 最长水平墙段（len=9/13/14）连续成行、无断裂 | 段长精确值（需 manifest） |
| `TS2c-longest-vertical-wall.png` | 竖直墙段（len=2/3/3）存在，呈阶梯状南缘 | 该形态在全图占比 |
| `TS2d-wall-corner.png` | 墙段转角（东邻＋北邻皆墙）近景，转角选片正确 | 转角覆盖率 |
| `TS2e-wall-endcap.png` | 端帽墙格（variant=3）近景；`stat ts2e` 直方图 v0~v3 | 端帽判据是否与官方逐像素一致 |
| `TS2f-hilltop-and-wall.png` | **高地顶面 + 南缘墙身同框**：顶面草色九宫格、其南缘为墙身，层级正确 | 顶面与低地色差定量 |
| `TS2g-two-tier-wall.png` | **两级台**（Mount 南缘 + Hill 南缘双段墙）存在（三种子均命中） | 双段是否在所有 Mount 前缘成立（仅取一处样本） |
| `TS3a-stairs.png` | **台阶**成对上下两段、同列同侧（side=RightPair）；落点周围为干燥陆地 | 台阶是否连通到预期可行走路网（需寻路验证） |
| `TS3b-slope.png` | 侧坡（左右单侧高差低地格）存在，颜色随相邻高地面板 | 坡面与崖壁交界样式是否逐像素符合规范 |
| `TS3c-shadow-and-transparency.png` | 高格**阴影每格一张**、下移一格；崖壁透明像素处露出真实基础地表 | 阴影数量精确值（需 manifest） |
| `TS4a-grass-forest-edge.png` | 草/林九宫格边界（流苏）存在，基础平地层拼接正常 | 具体边界格坐标 |
| `TS4b-shore-foam.png` | 水岸**细泡沫线**贴陆缘，泡沫在平地之下 | 泡沫像素宽度精确测量 |
| `TS4c-hill-forest.png` | 高地邻接森林格近景 | 林地下是否有高差误判 |
| `TS4d-hill-grass.png` | 高地邻接低地草地：顶面与低地**靠崖壁轮廓可辨** | 色差 ΔRGB 精确值 |
| `TS4e-hill-water.png` | 高地邻接水域：墙格立于陆格、不压真实水格 | 各处临水墙是否全为 Row==5 |
| `TS4f-clearance.png` | 净空锚点周边可视范围内**未见树木进入净空带**；`treesInsideClearance=0` | 全图所有净空带零树（仅取一处样本） |
| `comparison/compare-*.png` | L2/L1/L0 同种子同取景并排：宏观形状差异（碎片化 → 连片） | 逐像素差异定量 |
| `MVO1a/1b`、`MVO3a/b`、`MVO4a/b`、`MVO5a/b-*` | 旧 MVO 口径平地层/水岸/树簇/岩石/云-树近景（随同一用例链产出） | 同 TS 各行 |

---

## 六、7 条规则视觉验收结论

| # | 规则 | 结论 | 依据 |
| --- | --- | --- | --- |
| ① | 平地与水花绘制关系 | **符合**：水格无地形砖；岸线仅露细泡沫线，泡沫在平地之下 | `TS4b`、`TS2a` |
| ② | 高地顶面素材选择 | **符合**：Hill 顶草色九宫格、Mount 顶青冷色；Hill↔低地靠**轮廓**可辨 | `TS2f`、`TS4d`、`TS2g` |
| ③ | 陆地侧悬崖 vs 水边悬崖 | **符合**：陆脚（r4）与水脚（r5）分列；临水墙格下邻为真实水 | `TS2a`、`TS4e` |
| ④ | 悬崖放置位置（不覆盖错误底层/水格） | **符合**：墙格仅落低地陆格；未见墙压水格或压 Mount 顶面 | `TS2a`~`TS2g`、`TS4e` |
| ⑤ | 阴影偏移量与层级 | **符合**：`shadowRenderers`=2296/2294/2319 **恰等于** `hillCells`；下移一格 | `TS3c`、manifest `stat ts3c` |
| ⑥ | 台阶与高低地连接 | **符合**：上下两段成对、同列同侧、落点干燥 | `TS3a`、manifest `stat ts3a` |
| ⑦ | 植被避让（**本项目约定**，非官方条文） | **符合（样本级）**：三种子 `treesInsideClearance=0` | `TS4f` |

> ⚠️ 规则⑦**无官方依据**——官方 tilemap-guide 正文无"植被避让"条款，为项目约定设计决定，
> 以净空带（墙格根扩展 + 台阶 3×6）内零树为准，**不得声称"官方要求"**。

---

## 七、专项关注项结论（D-061 重点）

| 关注项 | 结论 |
| --- | --- |
| ① **人工感封闭环形平台是否再见** | **未再观察到此前那种"大小相近、轮廓雷同"的环形平台群**。三种子 `TS1b` 全图为大块连片有机高地；仍存在少量**大尺度**闭合/半闭合轮廓（如种子 13 中部、种子 15 左上），属自然地形允许的谷地/凹地（任务书 §4.5 明确允许），**非人工环形平台**。 |
| ② **碎片化是否收敛** | **显著收敛**：`hillBlocks` 25/25/26 → **4/9/7**，`mountBlocks` 17/22/26 → **5/10/5**（§4.1）。 |
| ③ **轮廓重复度** | 三种子高地轮廓**互不相同**、无明显"同一模板重复拼贴"感；对照 `comparison/` 三层图，L0 的轮廓复杂度明显高于 L2（旧块状）与 L1。 |
| ④ **边缘锯齿化** | `TS2f`/`TS4d`/`TS4a` 近景未见异常锯齿；域扭曲取 amp=25（max≈5.5 格）未引入高频抖动。 |
| ⑤ **大尺度清晰 + 中尺度变化** | 大尺度主场（0.012）给出主要高地带与低地分区，中尺度场（0.035，权重 0.22）打破边缘过度平滑——两者兼得。 |
| ⑥ **前后对照差异** | L1→L0：碎片化收敛、形状更连片有机；水陆海岸线**逐位不变**（WaterLandStep 未改，占比与 L1 完全一致）。详见 `comparison/`。 |
| ⑦ **台阶数下降（9/7/9 → 1/3/3）** | **属规则不变下的自然结果**，非缺陷：台阶 = 每"纯 Hill 分量（不含 Mount）"至多一个南向入口；分量更少更大后，大分量更易包住 Mount（含 Mount 分量不设台阶），上限随之下降。坡道（155/234/156）保证高低地连通。 |
| ⑧ **净空带内无树** | 三种子 `treesInsideClearance=0`（样本级）。 |

---

## 八、测试证据（`test-results/` 逐文件产源标注）

> 每对 `*.xml`（结果）+ `*.log`（原始日志）**按产源逐文件标注**，全部**原样收录**以保留完整审计链。

| 文件（`.xml`/`.log` 成对） | 产源 | 结果 |
| --- | --- | --- |
| `editmode-d061-final.*` | **EditMode 终态**（含新增 `BaseTerrainRefinementTests`）——**交付口径** | **244 / 244 passed** |
| `editmode-d061.*` | EditMode 首轮（新增测试之前） | 238 / 238 passed |
| `playmode-whitelist-d061.*` | **PlayMode 白名单回归**（10 个零写盘类 + `CliffAuditCaptureTests`，`AIA_CAPTURE_DIR=bt-main-aux`）——**交付口径** | **28 / 28 passed** |
| `bt-seed-20261013.*` | 本批种子 20261013 捕获运行 | **8 / 8 passed** |
| `bt-seed-20261014.*` | 本批种子 20261014 捕获运行 | **8 / 8 passed** |
| `bt-seed-20261015.*` | 本批种子 20261015 捕获运行 | **8 / 8 passed** |

**测试计数汇总**：

| 套件 | 结果 | 口径文件 |
| --- | --- | --- |
| EditMode（含 D-061 定向补测） | **244 / 244** | `editmode-d061-final.xml` |
| PlayMode 白名单回归 | **28 / 28** | `playmode-whitelist-d061.xml` |
| 本批三种子捕获（各 8 用例） | **8 / 8 ×3** | `bt-seed-2026xxxx.xml` |

**受保护历史证据零覆盖复核**：采集前后对 `docs/impl/evidence/` 全部历史批次
（排除本轮）执行 `sha256sum -c`，基线 1236 项 → **全 OK、0 FAILED**（基线文件：
`docs/impl/parallel-repair/d061b-protected-before.sha256.txt`）。

> **与 D-060 事故对比**：D-060 曾因 `-testFilter` 逗号语法静默失效，全量运行覆盖 61 个历史文件。
> 本轮改为**显式白名单 + 单类名过滤**（无逗号），并加**运行前后 sha256 双复核**，零覆盖。

---

## 九、第三方源码记录

- **FastNoiseLite v1.1.1**（Jordan Peck，MIT License）。上游提交 SHA：`785f37a9ad76e283586a379675085f2063ae03f7`。
- 本地路径：`Assets/_Project/ThirdParty/FastNoiseLite/`（含 `FastNoiseLite.cs` + `LICENSE.txt` + `PROVENANCE.md` + 独立 asmdef）。
- 渲染依赖：Unity Tilemap Extras（现有依赖，本批未改动）。

---

## 十、已知项与残余问题（非阻断，登记备查）

1. **大尺度闭合轮廓仍存在**（种子 13 中部、种子 15 左上等）：属自然地形允许的谷地/凹地，**非人工环形平台**；任务书 §4.5 明确"不得过度修正"。是否进一步收敛由制作人视觉终审决定。
2. **台阶数下降**（1/3/3）：规则未动的自然结果（§七.7），**非缺陷**；近景证据容量相应下降，`TS3a` 三种子仍均命中。
3. **`TS2g` 三种子 `strictSouthZero=False`**：两级台前缘取的是"经 Hill 过渡"的回退位（非 Mount 直邻低地），与 L1 的 D-060 同现象；两级台墙仍存在。
4. **`treesInsideClearance` 仅"记录不断言"**：净空过滤落地后复验，本轮同样只记录。
5. **`stat ts1` 为 attempt=1 口径**：与预检 harness 数值一致（hill 10.06/10.02/10.16、mount 3.96/3.98/4.00）。
6. **本批为工作区证据**：`HEAD 47d8185` 不含本批改动，复现需工作区快照（见 §一）。
7. **`capture-manifest.txt` 中 `HEAD=<待主Agent补记>` 占位**：采集时点 HEAD 与脏计数以本 README §一 为准（`47d8185` / 360）。
8. **跨平台逐位一致性 `未验证`**：同机同二进制重复生成逐位一致（R-MAP-15 口径）；相对 D-060 的种子流已变更（现消费 3 个 rng 种子），**跨版本同种子布局不同属预期**。

---

## 十一、批内校验

- `SHA256SUMS.txt`：本批次全部文件的 sha256 清单 —— 覆盖 `seed-20261013/` + `seed-20261014/` +
  `seed-20261015/` + `comparison/` + `test-results/` + 本 `README.md`（**不含** `SHA256SUMS.txt` 自身）。
- 校验命令：`sha256sum -c SHA256SUMS.txt`（在本目录下执行）。
