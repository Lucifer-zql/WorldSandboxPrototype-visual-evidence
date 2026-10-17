# D-065 Iteration-11 — Guide-driven Terrain Placement Correction

> 日期：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜固定种子 20261013 / 20261014 / 20261015
> 规则基线：`tinyswords-tileset-guide` commit `a89e226242b839760dbe533d8d1a3ba6738e3746`，
> `guideVersion = 0.0.0.7`，`guide.json` SHA-256 `8F93443009C89FBBCCD84C707DE759AAB84F919D18C1E681BBC8666E27B43A0E`。

## 目录

| 路径 | 内容 |
| --- | --- |
| `guide.json` | 固定版机器可读规则（下载件，未修改） |
| `guide-ref/` | 同 commit 的 README / elevation-examples.html / 切片网格图 / 语义编号图 / 高程示意 SVG / 官方示例地图 |
| `rule-mapping.csv` | 40 条规则的**完整映射与差异表**（语义编号 → raw 图集编号 → Unity sprite → 生产摆放 → 测试 → 画面证据） |
| `data-dump-r1/` | 三种子真实 `(terrain, elevation)` 全量 CSV + 高程统计 + ASCII 高程图（**修复前基线**） |
| `capture-r1/` | 修复前真实 URP 渲染（旧实现缺陷对照） |
| `capture-r2 … capture-r5/` | 修复过程逐轮画面（保留，不冒充最终） |
| `capture-final/` | **最终**确定性场景 + 三种子结构特写（Editor 捕获工具，真实 URP 管线，1:1 像素） |
| `game-seed-20261013/` `game-seed-20261014/` `game-seed-20261015/` | **真实游戏场景**（Main.unity + GameBootstrap 正式装配）截图 |
| `game-seed-20261013-prefix/` | 阴影修正前的同机位真实场景截图（前后对照，保留） |
| `tests/` | EditMode / PlayMode XML 与日志 |
| `final-report.md` | 最终报告（含剩余问题） |

## 一句话结论

旧实现把「RuleTile 邻接匹配」当成选片器（只能表达 8 个单边片 + 中心片），并且把高程顶面的内芯槽接到 **Flat 中心片**，导致台面与低地同色、掩码 5/7/10/11/13/14 无片可退、内陆被迫用海岸片补偿；本轮改为 `guide.json` 的**显式 16 掩码表 + 三编号分离映射**，崖脚改为 guide 的**单行南向墙脚**、坡路改为 **2 格成对 + 对角接入平台角**、阴影按 `elevatedGroundOnly` 只画南缘承接面。三种子真实场景截图与逐格 CSV 与规则表一致。

## 快速看图

- 单级台地（教材式 2 行崖面）：`capture-final/S1-plateau-hill.png`
- 两级台地（Mount → Hill 台面 → 低地，三层逐层可见）：`capture-final/S2-two-tier.png`
- N/E/W 高差只有草边、无石墙：`capture-final/S3-edges-and-nwe.png`
- 内陆湖 / 台地湖（无海岸白浪误用）：`capture-final/S4-lakes.png`
- 草↔林同高程交界（无水色缝）：`capture-final/S5-grass-forest.png`
- 台地临水（水脚墙）：`capture-final/S6-water-cliff.png`
- 真实游戏场景：`game-seed-20261013/G1-fullmap.png`、`G4-ramp-left.png`、`G9-shadow.png`

## 守卫与历史保护

- 全部写盘经 `CapturePathGuard` / `CaptureWriteGuard`：缺模式拒绝、历史目录拒绝、已存在文件拒绝覆盖。
- 本轮仅新增 `iteration-11/` 目录；`iteration-00 … iteration-10` 未改动（见 `final-report.md` §F）。

## 复现

```powershell
# 规则/渲染证据（Editor 捕获工具，真实 URP）
Unity.exe -batchmode -projectPath . -executeMethod WorldSandbox.TerrainDiagnostics.Iter11Capture.RunCli -logFile <log> -quit

# 真实游戏截图（PlayMode，按种子）
$env:WS_WORLD_SEED='20261013'
Unity.exe -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter 'Iter11_GameTerrainShots' -testResults <xml> -logFile <log>

# 测试
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults <xml> -logFile <log>
```
