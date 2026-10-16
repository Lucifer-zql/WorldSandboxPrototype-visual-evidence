# FastNoiseLite 地形高程换代 —— 视觉证据批次（2026-10-16）

> 任务：把《神之沙盘》的**高程生成算法**由旧「手工团块（先建平台再扰动边缘）」替换为
> **FastNoiseLite 连续噪声场**（D-060）。渲染语义（Tiny Swords 七层序）**不变**，仅换生成算法。
> 本批次为**改造后（after）**的三种子视觉与统计证据，配套**改造前（before）**基线批次
> `docs/impl/evidence/terrain-structure-2026-10-16/`（D-059）。

---

## 一、证据边界声明（务必先读）

1. **主仓库 HEAD 不含本批改动。** 采集时点本地主仓库 `HEAD=47d8185d92e0d248f252460b535a39679eb71262`
   （分支 `main`）；**本批全部改动（FastNoiseLite 依赖、`PlateauStep` 重写、`fn-` 捕获路由等）
   均为未提交工作区内容**，工作区脏计数 360（含本批新增未跟踪文件）。因此**本批截图无法用主仓库
   commit 复现**，只能由工作区快照复现。
2. **截图来自未提交工作区。** 所有 PNG 由 Unity 编辑器 PlayMode 用例
   `WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests` 实时渲染落盘，非离线合成。
3. **证据范围。** 本地批次目录 `docs/impl/evidence/fastnoise-terrain-2026-10-16/` 另含
   `main/`（Run 2「fn-main」全量运行副产物，内容与本批 `seed-20261013/` 同种子、确定性一致）；
   因与种子目录重复且归属 Run 2，**`main/` 不纳入本公开批次**（仅本地保留）。
4. **本批为工作区证据快照，不覆盖任何既有证据仓库目录**；公开仓库其余批次（
   `terrain-structure-2026-10-16` / `map-visual-overhaul-2026-10-15` / `cliff-repair-2026-10-15` /
   `independent-visual-verification-2026-10-15`）零改动。

---

## 二、改造前 / 改造后对照说明

| 维度 | 改造前（before，D-059） | 改造后（after，本批，D-060） |
| --- | --- | --- |
| 高程来源 | 手工团块：先播种平台多边形再扰动边缘 | **FastNoiseLite** OpenSimplex2 + FBm（Freq 0.035 / Octaves 4 / Lac 2.0 / Gain 0.5），按分位数取 Hill 10% / Mount 4% |
| 形状特征 | 偏几何/块状，边缘较直 | **自然连片、边缘起伏有机** |
| 台阶 | `stairs=0`（旧模型不含南向入口台阶） | `stairs=7~9`（每 HILL 分量至多一个南向入口） |
| 两级台 | 无（before 无 `TS2g` 产物） | **present**（三种子均命中 `TS2g`） |
| 对照批次 | `docs/impl/evidence/terrain-structure-2026-10-16/seed-2026xxxx/` | 本批 `seed-2026xxxx/` |

> 对照方式：同名文件（`TS1a` / `TS2a` / `TS2f` / `TS2b` / `TS2d` / `TS3b` / `TS3c` / `TS4a..f`）**同构图、
> 同取景口径**可直接并排比对；after 新增 `TS2g` / `TS3a`（before 无对应文件，原因是旧模型结构性不产出）。

---

## 三、采集方法（可复现命令）

**唯一代码改动**（阶段一，D）：`Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` 仅新增
`fn-` 前缀路由（`FnOutRoot="docs/impl/evidence/fastnoise-terrain-2026-10-16"`，`DirName` 去前缀）。
未改任何断言。

**逐种子运行**（Unity 6000.6.5f1 / Windows 11 / Git Bash；串行；不加 `-batchmode`，窗口化渲染）：

```bash
export MVO_CAPTURE_MODE=fn-seed-20261013   # 14/15 依次替换
export WS_WORLD_SEED=20261013
"D:/software/Unity Hub/Editor/6000.6.5f1/Editor/Unity.exe" \
  -projectPath "D:/data/UnityProjects/WorldSandboxPrototype" \
  -runTests -testPlatform PlayMode \
  -testFilter WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests \
  -testResults ".../test-results/fn-seed-20261013.xml" \
  -logFile     ".../test-results/fn-seed-20261013.log"
```

- 运行前基线：`tasklist //FI "IMAGENAME eq Unity.exe"` 仅 Hub serve（PID 11076）；无编辑器进程。
- 每种子运行约 40s（8 用例，XML duration≈17.7s），三种子串行，互不干扰。
- 落盘核验：每轮生成 `seed-2026xxxx/`（26 PNG + `capture-manifest.txt` = 27 文件），
  `capture-manifest.txt` 首行含 `session mode=fn-seed-2026xxxx seedEnv=… effectiveSeed=…`。

---

## 四、三种子地形分类比例统计（对验收带结论）

数据源：各 `seed-2026xxxx/capture-manifest.txt` 的 `stat ts1` 行（口径：`totalCells`=128×128=16384；
`lowlandCells`=**干燥低地**（陆格且 elev==0）；`waterCells`=水域；`elev1Cells`=Hill 严格；
`elev2Cells`=Mount 严格；占比分母=totalCells）。

| 种子 | water% | lowland% | hill%(elev1) | mount%(elev2) | hillBlocks | mountBlocks | 墙格 | 坡格 | 台阶 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 20261013 | 32.8857 | 53.2959 | **10.2173** | **3.6011** | 25 | 17 | 533 | 405 | 9 |
| 20261014 | 22.0032 | 64.2395 | **9.9365**  | **3.8208** | 25 | 22 | 610 | 473 | 7 |
| 20261015 | 26.3916 | 59.8816 | **10.0037** | **3.7231** | 26 | 26 | 629 | 442 | 9 |

**对验收带结论**（任务书首轮验收带：water ∈ [20,35]、Hill ∈ [8,18]、Mount ∈ [2,8]）：

- ✅ **Hill**：10.22 / 9.94 / 10.00 % —— 三种子**全部落在 [8,18]**。
- ✅ **Mount**：3.60 / 3.82 / 3.72 % —— 三种子**全部落在 [2,8]**。
- ✅ **Water**：32.89 / 22.00 / 26.39 % —— 三种子**全部落在 [20,35]**。
- 高地分量数 25 / 25 / 26；`hillBlocks`≠`hillCells`（一个分量可含多格），符合连续噪声场"少数量、大块连片"特征。

> 比例分母含水域，四类（water/lowland/hill/mount）构成干净划分，和恒为 100%（±舍入）。

---

## 五、逐张"能证明 / 不能证明"清单

> **能证明**＝该图在像素级确凿可见的事实；**不能证明**＝该图不足以支撑的结论（需转由数据/其它证据）。
> 图内 UI（顶栏、底部工具条、事件面板）为游戏正常 HUD，不属地形证据内容。

| 文件 | 能证明 | 不能证明 |
| --- | --- | --- |
| `TS1a-fullmap-ortho40.png` | 全图俯视（ortho40）下高地为**自然连片有机形状**；无大量封闭环形高地、无重复规则平台、无明显错误边缘结构；水陆分布自然 | 具体比例数值（需 `stat ts1`）；单格级邻接正确性（分辨率不足） |
| `TS1b-fullmap-ortho64.png` | 整图 128×128 满幅入画；同上"无异常结构"结论；可宏观核对高地/水域分布 | 同上 |
| `TS2a-waterfoot-cliff.png` | 临水收口墙格（Row==5）近景：崖壁立于**陆格**、其下为真实水面＋细泡沫线；墙身"草顶＋岩面"分带 | 墙格是否恰为该种子唯一水脚（需 manifest `stat ts2a`） |
| `TS2b-longest-horizontal-wall.png` | 最长水平墙段（len=12）连续成行、无断裂；墙格**全在低地陆格**，其下为低地 | 段长精确值（需 manifest）；跨种子是否同长 |
| `TS2c-longest-vertical-wall.png` | 竖直墙段（len=3）存在且呈阶梯状南缘 | 该形态在全图占比 |
| `TS2d-wall-corner.png` | 墙段转角（东邻＋北邻皆墙）近景，转角片选正确 | 转角覆盖率 |
| `TS2e-wall-endcap.png` | 端帽墙格（variant=3）近景；`stat ts2e` 直方图 v0/v1/v2/v3 分布 | 端帽判据是否与官方像素级一致 |
| `TS2f-hilltop-and-wall.png` | **高地顶面 + 南缘墙身同框**：顶面为草色九宫格，其南缘为墙身；层级关系（顶面在上、墙身在下）正确 | 顶面与低地色差定量（属视觉主观点） |
| `TS2g-two-tier-wall.png` | **两级台**（Mount 南缘 + Hill 南缘双段墙）存在；三种子均命中（strictSouthZero 13=True/14=True/15=False） | 双段是否在所有 Mount 前缘都成立（仅取一处样本） |
| `TS3a-stairs.png` | **台阶**成对上下两段、同列同侧（side=Left/RightPair）；落点周围为干燥陆地 | 台阶是否连通到预期可行走路网（需寻路验证） |
| `TS3b-slope.png` | 侧坡（左右单侧高差低地格）存在，颜色随相邻高地面板；总数入 manifest | 坡面与崖壁交界样式是否逐像素符合规范 |
| `TS3c-shadow-and-transparency.png` | 高格**阴影每格一张**、整体下移一格（Shadow=150 < 顶面 160）；崖壁透明像素处**露出真实基础地表**（非水底色） | 阴影数量精确值（需 manifest `stat ts3c` shadowRenderers） |
| `TS4a-grass-forest-edge.png` | 草/林九宫格边界（流苏）存在；基础平地层拼接正常 | 具体边界格坐标 |
| `TS4b-shore-foam.png` | 水岸**细泡沫线**贴陆缘（≈6px），泡沫在平地之下、未整块浮于水面 | 泡沫像素宽度精确测量 |
| `TS4c-hill-forest.png` | 高地邻接森林格（顶面与林地相邻）近景 | 林地下是否有高差误判 |
| `TS4d-hill-grass.png` | 高地邻接低地草地：**顶面（略浅）／低地（略深）以崖壁轮廓可辨**——即规则②关注点"色差虽小但靠轮廓区分"成立 | 色差 ΔRGB 精确值 |
| `TS4e-hill-water.png` | 高地邻接水域：墙格立于陆格、不压真实水格 | 各处临水墙是否全为 Row==5 |
| `TS4f-clearance.png` | 净空锚点周边可视范围内**未见树木/灌木进入净空带**；`treesInsideClearance=0`（manifest 记录，本轮只记录不断言） | 全图所有净空带零树（仅取一处样本） |
| `MVO1a/1b`、`MVO3a/b`、`MVO4a/b`、`MVO5b-*` | 旧 MVO 口径的平地层/水岸/树簇/岩石/云-树近景（本批随每种子的同一用例链一并产出） | 同 TS 各行（未逐张展开） |

---

## 六、7 条规则视觉验收结论（对照 `agent-d-visual-plan` §一）

| # | 规则 | 结论 | 依据文件 |
| --- | --- | --- | --- |
| ① | 平地与水花绘制关系 | **符合**：水格无地形砖；岸线仅露细泡沫线，泡沫在平地之下 | `TS4b`、`TS2a` |
| ② | 高地顶面素材选择 | **符合**：Hill 顶草色九宫格、Mount 顶青冷色；Hill(色1)↔低地(色2)靠**轮廓**可辨 | `TS2f`、`TS4d`、`TS2g` |
| ③ | 陆地侧悬崖 vs 水边悬崖 | **符合**：陆脚（r4）与水脚（r5）分列；临水墙格下邻为真实水 | `TS2a`、`TS4e` |
| ④ | 悬崖放置位置（不覆盖错误底层/水格） | **符合**：墙格仅落在低地陆格；未见墙压水格或压 Mount 顶面 | `TS2a/b/c/d/e/f/g`、`TS4e` |
| ⑤ | 阴影偏移量与层级 | **符合**：每高格一张阴影、下移一格、位于顶面之下 | `TS3c` |
| ⑥ | 台阶与高低地连接 | **符合**：上下两段成对、同列同侧、落点干燥 | `TS3a` |
| ⑦ | 植被避让（**本项目约定**，非官方条文） | **符合（样本级）**：净空带样本内零树；`treesInsideClearance=0` | `TS4f` |

> ⚠️ 规则⑦**无官方依据**——官方 tilemap-guide 正文无"植被避让"条款，本条为**项目约定的设计决定**，
> 以项目净空带（墙格根扩展 + 台阶 3×6）内零树为准，**不得声称"官方要求"**。

---

## 七、专项关注项结论（任务书指定）

| 关注项 | 结论 |
| --- | --- |
| ① **鞍部墙上盖顶**（Mount－低一格－Mount）是否再见 | **未再观察到"墙盖住 Mount 顶面"形态**。集成侧已加生产守卫（`TerrainOverlayPlan` 两级台底段 `E(y-2)<e0`）；本批三种子截图均未见墙压 Mount 顶面的错误底层。 |
| ② **TS2g 两级台是否出现** | **三种子均出现**（13: cell≈(91,91) strictSouthZero=True；14: (99,21) True；15: (121,9) False）。**不缺席、无需声明结构性跳过**。 |
| ③ **Mount 顶 color3 是否被误读为水** | **未见**任何高地区域呈现与水域混淆的青冷色块；高地顶与水域在缩略图中可区分（顶面带崖壁轮廓、水域为纯色面）。属 G5 主观点，交 A 终审。 |
| ④ **Hill 顶 color1 与低地 color2 是否靠轮廓可辨** | **可辨**：两者色差微弱，但高地位于崖壁之上、带南缘墙身轮廓，可据轮廓可靠区分（见 `TS4d`）。 |
| ⑤ **台阶上下两段成对、落点 3×3 干燥** | **符合**：`TS3a` 显示成对上下段同列同侧；`stat ts3a` 记录 `side=LeftPair/RightPair`。落点干燥由生成侧 3×3 干燥约束保证。 |
| ⑥ **阴影每高格一张、层级正确** | **符合**：`stat ts3c` 记录 `shadowRenderers`=高地格数（如 seed13 2264 = hillCells）；阴影下移一格、层级 150<160。 |
| ⑦ **净空带内无树/灌木** | **样本级符合**：`stat ts4f` 三种子均 `treesInsideClearance=0`。 |
| ⑧ **全图是否出现"大量封闭环形高地/重复规则平台/明显错误边缘结构"** | **未出现**。三种子 `TS1a/TS1b` 全图均为自然连片有机形状；无环形高地群、无重复规则平台、无明显错误边缘结构。 |

---

## 八、测试计数引用（终态）

| 套件 | 结果 | XML 路径（本批次 test-results/） |
| --- | --- | --- |
| EditMode 全量（终态） | **235 / 235 passed, 0 failed** | `editmode-full-2.xml`（另 `editmode-full.xml` 同为 235/235） |
| PlayMode 全量（Run 2，64 用例） | **64 / 64 passed, 0 failed** | `playmode-full-minus-3-run1-all64.xml` |
| PlayMode 全量减 3（新 `!(A\|B\|C)` 语法生效） | **54 / 54 passed, 0 failed** | `playmode-full-minus-3.xml` |
| 本批三种子捕获（各 8 用例） | **8 / 8 passed ×3** | `fn-seed-20261013.xml` / `fn-seed-20261014.xml` / `fn-seed-20261015.xml` |

> `test-results/` 另含调查/迭代产物：`_filter-probe{,2,3}.xml/.log`（`-testFilter` 语法排查）、
> `editmode-diag.xml`（235 中 1 失败、处置前诊断态）、各 `.log` 原始运行日志。全部**原样收录**以保留审计链。

---

## 九、第三方源码记录

- **FastNoiseLite v1.1.1**（作者 Jordan Peck，MIT License）。
- 上游提交 SHA：`785f37a9ad76e283586a379675085f2063ae03f7`。
- `FastNoiseLite.cs` sha256：`605090835008f4ef58fc39cdfa15df5051a4f5575d5e01fd7d355b9f67b0160e`。
- 本地路径：`Assets/_Project/ThirdParty/FastNoiseLite/`（含 `FastNoiseLite.cs` + `LICENSE.txt` + `PROVENANCE.md` + 独立 asmdef）。
- 渲染依赖：Unity Tilemap Extras `com.unity.2d.tilemap.extras@9.0.0`（现有依赖，未改动）。

---

## 十、事故登记（必须如实记录）

本任务的 PlayMode 全量运行（本意"全量减 3"，但首轮排除语法因 `-testFilter` 按 **`;`** 而非逗号切分**未生效**）
**覆盖了两个未跟踪、无备份的本地历史目录**：

1. `docs/impl/evidence/parallel-repair-b/visual-verification/`（**53 文件**）
2. `docs/impl/evidence/foam-alignment-fix/before/` + `after/`（合计 **8 文件**）

- **公开证据仓库未受影响**（已核验该仓库 git 干净，无对应目录）。
- 其余受保护批次（`terrain-structure-2026-10-16` / `map-visual-overhaul-2026-10-15` /
  `parallel-repair-ai-a` 旧子目录）**哈希全部一致、完好**。
- 本批三种子运行前后各执行一次受保护批次 `sha256sum -c`（对照
  `docs/impl/parallel-repair/fastnoise-protected-batches-before.sha256.txt`），失配**恰为上述 61 项**
  （53+8），**零新增失配**。

---

## 十一、语法发现（`-testFilter` 排除语法）

Unity CLI `-testFilter` 的数组切分依据是 **分号 `;`**（Unity Test Framework 1.4.6 源码
`CommandLineOption.SplitStringToArray`）；**逗号分隔不会产生多过滤器**（整串成为一个过滤器 → 排除失效）。
有效排除写法为**单个过滤器内的 `!(A|B|C)` 分组**（本批 `playmode-full-minus-3.xml` 即以此为据，64→54 生效）。
本批的包含式单值过滤器（`WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests`）不受此影响。

---

## 十二、已知项清单（非阻断，登记备查）

1. **`TS2a`/`TS2d`/`TS2e`/`TS2f` 取景区域高度重合**：定位器在该种子下选中的墙格聚集于同一地貌片区，
   四张近景构图相近。**属定位确定性所致，非缺陷**；不同种子的同类图取景不同（可交叉参考 13/14/15）。
2. **Hill 顶(色1) 与 低地(色2) 色差微弱**（规范 §D.5 已登记）：层级靠崖壁/坡面轮廓区分，验收以轮廓为准。
3. **Mount 顶(色3) 青冷** 存在被俯瞰误读为水域的**风险点**（规范 §D.5 登记）：本批未观察到实际混淆，
   但最终判定交 A 视觉终审。
4. **`stat ts4f` 的 `treesInsideClearance` 仅"记录不断言"**：E 净空过滤落地后复验；本批同样只记录。
5. **`main/` 目录含两次会话**（12:06 Run 2 + 12:16 全量重跑追加，确定性统计逐位一致）：该目录
   **不纳入本公开批次**，仅本地保留。
6. **比例统计为 attempt=1 口径的生成结果**：与预检 harness 数值一致（water/hill/mount 见 §四）。
7. **本批为工作区证据**：`HEAD 47d8185` 不含本批改动，复现需工作区快照（见 §一）。

---

## 十三、批内校验

- `SHA256SUMS.txt`：**本公开批次**全部文件的 sha256 清单 —— 覆盖
  `seed-20261013/` + `seed-20261014/` + `seed-20261015/` + `test-results/` + 本 `README.md`
  （**不含** `SHA256SUMS.txt` 自身，**不含**未发布的本地 `main/`），采集体定稿后最终计算。
- 校验命令：`sha256sum -c SHA256SUMS.txt`（在本目录下执行）。
