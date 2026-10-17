# iteration-00-baseline —— D-065 地形渲染完整性逐轮整改 · 第 0 轮真实画面基线

> 采集：2026-10-17 08:04~08:06｜Unity 6000.6.5f1 / URP 2D / Windows 11｜窗口化真实渲染（非离线合成）
> **代码状态声明**：本批为**未修改游戏的现状基线**——自 D-064 收尾（证据 `8de3b2f`）以来，
> **游戏生产代码/素材/场景零改动**；期间仅做测试侧「批次路由与门禁白名单滚动」到本轮批次
> （`tri-` 前缀 → 本目录；`tpr2-/tpr3-/tpr4-` 模式前缀已按纪律关闭），不改变任何游戏画面行为。
> 主仓库 `HEAD=47d8185`，未提交工作区保持（392 项脏，快照与关键文件 SHA-256 见 `git-state.txt`）；
> 历史证据采前/采后 **1998/1998 零覆盖**。

## 一、采集参数与产出

- 种子：`20261013` / `20261014` / `20261015`（各独立子目录）；分辨率 1280×720；窗口化 `-runTests`。
- 命令形态：`WS_WORLD_SEED=<种子> MVO_CAPTURE_MODE=tri-iteration-00-baseline <Unity.exe> -projectPath <仓库> -runTests -testPlatform PlayMode -testFilter WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests`（串行单实例）。
- 每种子构成：标准取景（TS 组 + MVO 组）+ 近景 + capture-manifest.txt + test-results.xml + test-run.log。
- 产出：seed-20261013 **33 文件**（24 标准 + 6 近景 + 3 记录）；seed-20261014/15 各 **36 文件**（26 标准 + 7 近景 + 3 记录）。
- **未命中项（如实报告，不产图、不以其他区域图冒充）**：seed-20261013 的 `TS2c-longest-vertical-wall` /
  `TS2d-wall-corner` / `tpr2-closeup-vertwall` ——该种子无"≥2 格相邻竖列/转角"实例；程序仅记
  Debug.Log 跳过（该种子 `test-run.log` 行 3146 / 3172 / 2574）。seed-20261014/15 三项均命中（竖列 run=2）。
- 命名说明：近景文件沿用测试代码内固定的 `tpr2-closeup-` 文件名前缀（历史命名），与本轮批次模式 `tri-` 无关。
- 测试：每种子 9 passed + 1 设计性 Skipped（state 对照拍非本轮目标模式），0 failed。

## 二、重点问题 → 本批取景对应（供审查定位）

| 制作人问题 | 优先查看 |
| --- | --- |
| ① 陆地内部水色缝隙/空洞/地表不连续 | `TS1a-fullmap-ortho40.png`、`TS1b-fullmap-ortho64.png`（全图扫查）；`TS4c-hill-forest.png`、`TS4d-hill-grass.png`（陆地内部） |
| ② 深绿地块缺草地纹理/颜色断层 | `TS4c-hill-forest.png`（深绿区块）、`TS2f-hilltop-and-wall.png`、`TS1a` 全图 |
| ③ 三角形/斜坡/对角边缘放置 | `TS3a-stairs.png`、`TS3b-slope.png`、`tpr2-closeup-stairs.png`、`tpr2-closeup-slope.png` |
| ④ 孤立悬崖块/接缝/边界拼接 | `TS2a-waterfoot-cliff.png`、`TS2b/2c/2d`（崖段/转角；2c/2d 仅种子 14/15）、`tpr2-closeup-endcap.png`、`tpr2-closeup-clifftop.png`、`tpr2-closeup-vertwall.png`（仅 14/15） |
| ⑤ 系统性素材/图层关系 | `TS2e/TS2f/TS2g`、`TS4a~4f`、`MVO1a/1b` 全图 |

## 三、取景说明（全表）

| 组 | 文件 | 机位/用途 |
| --- | --- | --- |
| 全图 | `TS1a`(ortho40) / `TS1b`(ortho64) / `MVO1a`(ortho40) / `MVO1b`(整图) | 完整地图总览（跨轮同构图口径） |
| 悬崖 | `TS2a`(水脚崖 ortho5) / `TS2b`(最长横墙 6) / `TS2c`(最长竖墙 5；仅14/15) / `TS2d`(转角 5；仅14/15) / `TS2e`(端帽 4) / `TS2f`(顶面+墙 5) / `TS2g`(两级 6) | 崖壁/转角/端帽/两级（锚点数据驱动） |
| 台阶坡面阴影 | `TS3a`(台阶) / `TS3b`(坡面) / `TS3c`(阴影与透明) | §重点③ |
| 边缘 | `TS4a`(草林边) / `TS4b`(岸泡沫) / `TS4c`(高地-林) / `TS4d`(高地-草) / `TS4e`(高地-水) / `TS4f`(净空) | §重点①②⑤ |
| MVO 场景 | `MVO3a/3b`(岸/林近景) / `MVO4a/4b`(树/石) / 云双帧：种子13/14 为 `MVO5a-cloud-over-forest-frame1/2`；种子15 为 `MVO5b-cloud-adjacent-tree-frame1/2`（屏幕空间触发未满足，manifest 如实标 `adjacent-not-overlapped`） | 装饰与回归 |
| 近景 | `tpr2-closeup-`{`stairs`(3),`slope`(3.5),`twotier`(4.5),`endcap`(4),`clearance`(4),`clifftop`(5)}；种子14/15 另有 `vertwall`(3，v5 竖列变体观察位) | 逐类邻接组合特写 |
| 记录 | `capture-manifest.txt`（逐拍时间/种子/相机/stat 注记） | 数据侧核对 |
| 状态 | `git-state.txt`（HEAD/工作区/关键生产文件 SHA-256）+ `git-status-porcelain.txt`（392 行全量） | 代码状态指纹（见 §五） |
| 捕获安全 | `capture-safety/d065-capture-pre.sha256.txt`（采前 1998 项历史证据快照）+ `capture-safety/capture-post-verify.txt`（采后零覆盖核验方法与结果） | 历史证据零覆盖的可核验材料（见 §五） |

## 四、逐组"能/不能证明"

- 全图（TS1a/1b）：能证明地图级分布与显著异常；**不能**证明单格级素材正确性（分辨率）。
- 崖组/近景：能证明对应邻接组合的拼接形态；**不能**证明全图该类组合的占比（需 manifest/stat 统计）。
- `TS3a/3b` 与 `closeup-stairs/slope`：能证明台阶/坡面方向与连接可读性；**不能**证明通行逻辑（需寻路验证）。
- 未命中项（seed13 的 TS2c/TS2d/vertwall）：**不能**以本批其他区域图冒充该组合；审查该组合请用 seed14/15 对应图。

## 五、测试、安全与代码状态指纹

- 测试 XML：`seed-20261013|14|15/test-results.xml`（各 9 passed + 1 skipped + 0 failed）；原始日志 `test-run.log`。
- 捕获安全：全部产出经 `CaptureWriteGuard`（白名单仅本轮批次根、模式仅 `tri-`、文件已存在拒绝）；
  采前/采后历史哈希 **1998/1998 零覆盖**——采前快照与采后核验（含可复现命令）见 `capture-safety/`。
- 代码状态指纹：`git-status-porcelain.txt`（采集时点主仓库 392 项脏全量清单）；
  `git-state.txt`（HEAD `47d8185`/分支 main/关键地形生产文件完整 SHA-256）——后续每轮迭代重算同表即可
  逐位判定哪些生产文件被改。manifest 内 `<待主Agent补记>` 占位符已由各文件末行 backfill 行闭环。
- SHA-256：`SHA256SUMS.txt`（本目录全文件，含本 README）。
