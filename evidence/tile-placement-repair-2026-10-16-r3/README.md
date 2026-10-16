# tile-placement-repair-2026-10-16-r3 —— D-063 残余渲染缺陷与证据写入安全整改证据

> 生成：2026-10-16（Claude Code 会话，D-063）｜Unity 6000.6.5f1 / URP 2D / Windows 11
> 基线：D-062 verify-r2（r2 批次，公开提交 `b7d5414`）；主仓库全程零 git 写（HEAD=47d8185 不变）
> **安全门禁（D-063 任务 A）**：本批全部产出经 `CaptureWriteGuard` 失败关闭守卫落盘——
> 白名单仅本目录、文件已存在拒绝、历史目录默认拒绝；采前/采后历史哈希复核 **1701/1701 零覆盖**。

## 一、本轮修复（证据对应）

| 修复 | 根因 | 验证 |
| --- | --- | --- |
| **P1 云米色矩形** | 8 张云 PNG（576×256 整朵大云+烘焙暗影）被误切 64×64 碎片并接线上屏 | `.meta` 追加 `_whole` 整图条目 + 场景重接线；对照见 §三 |
| **P5a 阴影落水暗带** | 阴影投影中心落点不查水陆 | 落点为水格不画；行为用例 `Render_Shadows_SkipWhenLandingOnWater` 锁定 |
| **P4 林缘白描边** | RT_Forest 9 槽全用左族白圈片（白线=水岸语义误上林缘） | RT_Forest 追加 15 右族 marker + `ResolveForestSelfEdgeTile` 按邻水选族（水岸白线保留）；TS4c 形态：林缘深绿毛边、水岸白线在 |
| **P2 深绿矩形** | **非工具预览/非缺陷**——Mount 顶面 `RT_Grass_color3` 真实渲染（B 像素匹配 (95,171,98)、D 证实 base 批次已有同位矩形） | `TPR3-state-normal` vs `TPR3-state-toolpreview` 双帧逐像素级一致（工具预览态无新增覆盖） |

## 二、运行环境与参数

- 窗口化 `-runTests`（截图需帧缓冲）；过滤单类名 `WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests`；
  `MVO_CAPTURE_MODE=tpr3-*` + `WS_WORLD_SEED=<种子>`（种子 20261013/14/15）；屏幕 1280×720；串行单实例。
- 取景干净化：`LoadWorld` 内 `HandleCancel` 工具态复位 + manifest 注记鼠标（旧 Input API 弃用——r3 首跑
  10 用例因 `UnityEngine.Input` 与 Input System 包冲突全败，已修，教训记档）。
- 布局：`seed-20261013/`（28 文件）、`seed-20261014|15/`（各 35 文件＝28 标准+7 近景）、
  `all/`（seed13 近景补拍 35 文件，含 `TPR3-state-normal.png` 同机位对照）、`state-toolpreview/`（35 文件，含 `TPR3-state-toolpreview.png`）。

## 三、逐张"能证明 / 不能证明"（核心项）

| 文件 | 能证明 | 不能证明 |
| --- | --- | --- |
| `seed-*/MVO5a-cloud-over-forest-frame*.png` | **云修复**：完整蓬松云（r2 同名图为米色直角矩形块） | 云漂移动画全相位 |
| `all/TPR3-state-normal.png` vs `state-toolpreview/TPR3-state-toolpreview.png` | **深绿矩形非工具预览**：双帧一致；结合 B/D 证据=Mount 顶面正常内容 | 其他工具（建筑/生物笔刷）的预览形态 |
| `all/tpr2-closeup-slope.png` | 坡面斜线方向 + 右上完整云 + 无米色矩形 | 坡体色块强度（草上草坡固有低对比） |
| `all/tpr2-closeup-twotier.png` | 两级墙三层（上带/草台/下带）可读 | 退台语义是否满足设计预期（待制作人） |
| `seed-*/TS4c-hill-forest.png` | 林缘深绿毛边（右族无白）+ 水岸白线保留 | 林缘全边界逐格正确性 |
| `seed-*/TS4e-hill-water.png` | 高地临水岸线正常 | 阴影落水修复（该取景无"高格南临水"实例——行为用例锁定） |
| `seed-*/TS1a/TS2*/TS3*/TS4*` | D-061 地形未变、D-062 崖壁/崖顶修复未回退、台阶/坡面近景比 r2 清晰 | 台阶通行逻辑（需寻路验证） |

## 四、测试结果（`test-results/`）

| 套件 | 结果 | 文件 |
| --- | --- | --- |
| EditMode 终态全量 | 见 `docs/impl/parallel-repair/d063-editmode-final.xml`（含 14 守卫用例 + 阴影新用例） | parallel-repair 目录 |
| PlayMode 捕获 ×6 运行 | 各 10/10（seed×3 + all + state-toolpreview；r3 首跑 10/10 失败系 Input API 冲突已修，留档于本 README §二） | `test-results/tp-*.xml`（+.log） |

## 五、SHA-256 清单

见 `SHA256SUMS.txt`。历史证据保护基线：`docs/impl/parallel-repair/d063-capture-pre.sha256.txt`（采前 1701 项）。
