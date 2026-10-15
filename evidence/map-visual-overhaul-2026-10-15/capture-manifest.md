# 捕获清单（地图视觉重构 MVO，2026-10-15）

## 捕获环境

| 项 | 值 |
|---|---|
| 引擎 | Unity 6000.6.5f1（Windows） |
| 场景 | `Assets/_Project/Scenes/Main.unity`（真实生产场景；GameBootstrap 默认种子 20261013） |
| GameView 分辨率 | 1280×720（"HD" 档位；每用例运行日志 `Screen=1280x720` 实测，`GameViewSizeText` 包含匹配） |
| 运行模式 | **窗口化**（非 `-batchmode`；ScreenCapture 需真实帧缓冲） |
| 驱动测试 | `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs`（MVO1 全图/MVO2 崖/MVO3 水岸草缘/MVO4 装饰/MVO5 云树） |
| 截图方式 | `cam.Render()` 强制渲染 → 等 2 帧 → `ScreenCapture.CaptureScreenshot(path,1)` → 轮询文件生成（**先删旧图再等新文件**，防同名旧图冒充） |
| 画面类型 | 真实 GameView 画面（含完整游戏内 UI：顶栏资源/日期、底部工具与事件栏） |
| 种子覆盖 | `WS_WORLD_SEED` 环境变量（GameBootstrap.Awake 3 行钩子；未设置时行为与序列化默认逐字节一致）；manifest 记录 env 与反射实测 effectiveSeed 双口径 |
| 工作区状态 | 主仓库 HEAD `47d8185`（本轮全部改动未提交）；各 capture-manifest.txt 已补记 HEAD 与脏文件数 |

## 镜头参数（确定性扫描定位，与 iter1 同机位可对照）

| 用例 | 文件（after/seed-* 目录内） | 定位逻辑 | 相机 ortho |
|---|---|---|---|
| MVO1a | `MVO1a-fullmap-overview-ortho40.png` | 地图中心 (64,64) | 40（历史对照口径） |
| MVO1b | `MVO1b-fullmap-entire-ortho64.png` | 地图中心 | 64（128×128 恰满幅） |
| MVO2a | `MVO2a-cliff-longest-run.png` | 最长横向崖段中心（manifest 记录段长） | 6 |
| MVO2b | `MVO2b-cliff-water-foot.png` | 水脚崖格（下邻 Water） | 5 |
| MVO2c | `MVO2c-cliff-land-foot.png` | 陆脚崖格（下邻 Grass/Forest） | 5 |
| MVO2d | `MVO2d-cliff-corner.png` | L 形转角 | 5 |
| MVO3a | `MVO3a-shore-grass-water.png` | 草地邻水岸格 | 5 |
| MVO3b | `MVO3b-grass-forest-edge.png` | 草林交界格 | 5 |
| MVO4a | `MVO4a-tree-cluster.png` | 树簇中心（切比雪夫 ≤2 邻树最多） | 5 |
| MVO4b | `MVO4b-rock.png` | 岩石（优先 8 邻 Cliff\|Water） | 4 |
| MVO5a | `cloud/MVO5a-cloud-over-forest-frame{1,2}.png` | 云-树切比雪夫 ≤2 组合（实时坐标，间隔 2s 双帧） | 5 |

## 捕获运行记录（UTC；本地 = UTC+8）

| 轮 | 测试结果 XML | 时间 | 用例数 | 产出目录 |
|---|---|---|---|---|
| 基线（修复前） | `editmode-baseline.xml` | 10:51:47Z，199/199 | 199 | `baseline/`（placement/framing 由 TinySwordsPlacement/CameraFraming 既有测试产出） |
| 一轮（R1 代码） | `playmode-baseline-placement/framing.xml` | 10:52–10:53Z，各 1/1 | 2 | `baseline/` |
| 一轮捕获 | `playmode-after-results.xml` 等 | 12:35Z 附近，5/5×3 | 15 | `iter1-after/`、`iter1-seed-20261014/`、`iter1-seed-20261015/` |
| 一轮全量 | `playmode-full-regression{1,2}.xml` | 12:38Z / 12:5xZ | 57/59→59/59 | —（before/ 误写已删除，见 README） |
| **终批 after** | `playmode-after2-results.xml` | **13:25:25–31Z，5/5** | 5 | `after/`（13 文件） |
| **终批 seed14** | `playmode-seed14b-results.xml` | 13:25:53–59Z，5/5 | 5 | `seed-20261014/`（WS_WORLD_SEED=20261014） |
| **终批 seed15** | `playmode-seed15b-results.xml` | 13:26:20–26Z，5/5 | 5 | `seed-20261015/`（WS_WORLD_SEED=20261015） |
| **终批 cloud** | `playmode-cloud-results.xml` | 13:26:47–50Z，1/1 | 1 | `cloud/` |
| **PlayMode 全量（终）** | `playmode-full-regression3.xml` | 13:27:12–13:28:36Z，**60/60** | 60 | `pm-fullrun-scratch`（跑后删除） |
| **EditMode 全量（终）** | `editmode-mvo-r2.xml` | 13:24:09–12Z，**211/211** | 211 | — |

## 逐目录代码版本口径（判别依据）

- `baseline/`：基线代码（= 前序 Agent 悬崖 v2 等成果，**不含**本轮任何改动）；EditMode 199/199。
- `iter1-*`：一轮代码（生成重构 + 崖 v3 + 高草 + 装饰 R1）；捕获于 20:35 本地。
- `after/`、`seed-*`、`cloud/`：二轮代码（+ 林界流苏 + 林心树簇 + 绿灌木 + MVO5）；捕获于 21:25–21:27 本地。
- 判别依据：`test-results/` 各 XML 的 start-time 与对应图片 mtime 交叉可查；两轮之间只有二轮 3 项生产改动落地（git diff 时间线）。
- 已删除的 `before/` 说明：PlayMode **全量**回归会连带执行 MVO 捕获测试（默认 mode=before），曾把新代码画面写入 `before/`——构成误导性证据，已删除两轮并在批次 README 记录；后续全量运行以 `MVO_CAPTURE_MODE=pm-fullrun-scratch` 隔离。

## 诚实记录：本轮被修正的测试期望（非弱化）

1. `UiWiringEvidenceTests.ButtonsPanelsPalettes_WiredWithEvidence`：色板对照轨原假设场景默认 color1；场景已切 color2（MVO 契约），修正为显式切 palette 0 后取帧——断言强度不变（双色像素计数照旧）。
2. `TinySwordsPlacementEvidenceTests.CapturePlacementEvidenceV04`：`FindIsland` 原找"4 邻全水"1 格孤岛；新水陆算法按设计清除碎岛（<3 格清理），语义修正为"最孤立岸线"（4 邻水域数降序）——证据价值等价（小屿/半岛尖岸线近景）。
3. `TerrainCompositionTests` 崖语义拆分：海岸礁段断言逐条保留 + 新增平台环不变式组（环连通闭合/无孤立/端点 2~4/环内 Grass）——拆分非删除。
