# 捕获清单（悬崖修复专项，2026-10-15）

本文件记录 `cliff-before/`、`cliff-after/`、`iteration-v1-rejected/` 三组截图的完整捕获环境、命令与逐张说明。

## 捕获环境

| 项 | 值 |
|---|---|
| 引擎 | Unity 6000.6.5f1（Windows） |
| 场景 | `Assets/_Project/Scenes/Main.unity`（真实生产场景，非 Showcase/测试场景） |
| 地图种子 | 20261013（GameBootstrap 默认种子；两次捕获完全相同） |
| GameView 分辨率 | 1280×720（"HD" 档位；运行日志 `Screen=1280x720` 每用例实测） |
| 运行模式 | **窗口化**（非 `-batchmode`；ScreenCapture 需要真实帧缓冲） |
| 驱动测试 | `Assets/Tests/PlayMode/CliffAuditCaptureTests.cs`（6 个用例，见下） |
| 截图方式 | `cam.Render()` 强制渲染当前帧 → 等待 2 帧 → `ScreenCapture.CaptureScreenshot(path, 1)` → 轮询文件生成 |
| 画面类型 | **真实 GameView 画面**（含完整游戏内 UI：顶栏资源计数/日期/暂停钮、底部工具与事件栏） |

### 用例与镜头参数

| 用例 | 文件 | 定位（确定性扫描结果，见日志） | 相机 ortho |
|---|---|---|---|
| C1_HorizontalRun_Closeup | `C1-horizontal-run.png` | 水平崖段（≥3 格）中心 **(5,5)** | 4 |
| C2_VerticalRun_Closeup | `C2-vertical-run.png` | 垂直崖段（≥3 格）中心 **(55,10)** | 4 |
| C3_Corner_Closeup | `C3-corner.png` | L 形转角格 **(123,18)**（镜头取 (124,19)） | 4 |
| C4_WaterFoot_Closeup | `C4-water-foot.png` | 水脚崖格 **(67,4)**（下邻 = Water） | 4 |
| C5_LandFoot_Closeup | `C5-land-foot.png` | 陆脚崖格 **(68,4)**（下邻 = Grass/Forest） | 4 |
| C6_Overview | `C6-overview.png` | 地图中心 (64,64) | 40（全景） |

- 定位规则：自 (4,4) 起扫全图，取**第一处**满足条件的崖格（同种子 + 同扫描规则 ⇒ 前后两轮、v1/v2 轮全部命中同一坐标，可逐像素对照）。
- 坐标均为地图格坐标（128×128 世界，原点左下）。

## 同条件对照机制（唯一变量 = 选片规则）

- **修复前**（mode=legacy）：测试经反射将 `TerrainTilemapRenderer.cliffLandTiles / cliffWaterTiles` 置空 → 渲染器回退 `RT_Cliff` 旧行为（4 片随机取片），随后 `RebuildAll` 整图重铺。
- **修复后**（mode=phase8 = v2 段锚）：测试注入 8 个切片资产（`Cliff_Land_41..44` / `Cliff_Water_50..53`）→ 底脚按邻接分组 + 段锚统一选片，`RebuildAll` 整图重铺。
- 除选片规则外，场景、种子、相机、镜头、分辨率、UI 状态全部相同。

## 捕获运行记录

### ① 修复前（before）

```
AIA_CAPTURE_DIR=cliff-before AIA_CLIFF_LEGACY=1 \
Unity.exe -projectPath <主仓库> -runTests -testPlatform PlayMode \
  -testFilter "WorldSandbox.Tests.PlayMode.CliffAuditCaptureTests" \
  -testResults .workbuddy/ai-a-runs/cliff-before2.xml \
  -logFile    .workbuddy/ai-a-runs/cliff-before2.log
```

- 时间：2026-10-15 09:53:17Z 起（本地 17:53），持续约 4.4s
- 结果：6/6 通过（`test-results/cliff-before2.xml`）
- 日志标记：`[CLIFF] mode=legacy(RT_Cliff); Screen=1280x720`（每用例 1 条）

### ② 迭代 v1 捕获（后被否定，仅存档）

```
AIA_CAPTURE_DIR=cliff-after \
Unity.exe ... -testResults .workbuddy/ai-a-runs/cliff-after.xml -logFile .../cliff-after.log
```

- 时间：2026-10-15 17:54（本地）
- 说明：当时为 v1 环序实现（`(x+y) mod 4` 连续取片）；该方案经真实画面判定失败（柱形格间跳变），截图重命名并存档至本批 `iteration-v1-rejected/`。
- 日志标记：`[CLIFF] mode=phase8; Screen=1280x720`

### ③ 修复后 v2（after，最终）

```
AIA_CAPTURE_DIR=cliff-after \
Unity.exe -projectPath <主仓库> -runTests -testPlatform PlayMode \
  -testFilter "WorldSandbox.Tests.PlayMode.CliffAuditCaptureTests" \
  -testResults .workbuddy/ai-a-runs/cliff-after2.xml \
  -logFile    .workbuddy/ai-a-runs/cliff-after2.log
```

- 时间：2026-10-15 09:59:04Z 起（本地 17:59），持续约 4.4s
- 结果：6/6 通过（`test-results/cliff-after2.xml`）
- 日志标记：`[CLIFF] mode=phase8; Screen=1280x720`；定位坐标与 ① 完全一致

### ④ 全量回归（发布前最终）

```
# EditMode（batchmode + nographics；无截图）
Unity.exe -projectPath <主仓库> -runTests -testPlatform EditMode \
  -testResults ...editmode-final.xml -logFile ...editmode-final.log -batchmode -nographics

# PlayMode（窗口化；含 AIA_CAPTURE_DIR=full-run 输出目录）
AIA_CAPTURE_DIR=full-run \
Unity.exe ... -runTests -testPlatform PlayMode \
  -testResults ...playmode-final2.xml -logFile ...playmode-final2.log
```

- EditMode：2026-10-15 10:10:38Z–10:10:40Z（本地 18:10），**199/199 通过**
- PlayMode：2026-10-15 10:11:02Z–10:12:19Z（本地 18:11–18:12），**55/55 通过**
- 全量回归捕获的截图（full-run）**不在本批发布范围内**（本批视频/截图证据以 ①–③ 为准）。

## 逐张文件说明

| 文件 | 组 | 内容 | 对应日志行 |
|---|---|---|---|
| `cliff-before/C1-horizontal-run.png` | before | 水平段旧随机选片（柱形随格跳变） | `水平段中心=(5,5)` |
| `cliff-before/C2-vertical-run.png` | before | 垂直段旧行为 | `垂直段中心=(55,10)` |
| `cliff-before/C3-corner.png` | before | L 形转角旧行为 | `转角格=(123,18)` |
| `cliff-before/C4-water-foot.png` | before | 水脚格旧行为（未按邻接分组） | `水脚格=(67,4)` |
| `cliff-before/C5-land-foot.png` | before | 陆脚格旧行为 | `陆脚格=(68,4)` |
| `cliff-before/C6-overview.png` | before | 全景（ortho 40） | （C6 无定位行） |
| `cliff-after/C1-horizontal-run.png` | after(v2) | 水平段段锚统一（柱形统一、缝等宽） | `水平段中心=(5,5)` |
| `cliff-after/C2-vertical-run.png` | after(v2) | 垂直段全段同片、柱脚按邻接 | `垂直段中心=(55,10)` |
| `cliff-after/C3-corner.png` | after(v2) | 转角无错片 | `转角格=(123,18)` |
| `cliff-after/C4-water-foot.png` | after(v2) | 水脚：下邻=水 → `_50.._53`（柱脚水线） | `水脚格=(67,4)` |
| `cliff-after/C5-land-foot.png` | after(v2) | 陆脚：下邻=草 → `_41.._44`（柱脚暗收边） | `陆脚格=(68,4)` |
| `cliff-after/C6-overview.png` | after(v2) | 全景（ortho 40） | （C6 无定位行） |
| `iteration-v1-rejected/*.png` | v1（失败存档） | v1 环序实现当时的同 6 用例捕获（17:54） | 同左，mode=phase8(v1) |

**注意**：`cliff-after/` 中 C4 与 C5 使用**相邻格**（67,4）与（68,4）——两图对照可直接核对"同一段崖、同样镜头下，水脚与陆脚两种底脚版本的区别"。

## 发布范围与哈希

- 本批发布的全部文件与其 SHA-256 见 `SHA256SUMS.txt`（仓库根另有一份全仓库汇总清单）。
- 捕获过程产生的日志（`*.log`）与全量回归轮截图（`full-run/`）**不在发布范围**；如需要可另行整理。
