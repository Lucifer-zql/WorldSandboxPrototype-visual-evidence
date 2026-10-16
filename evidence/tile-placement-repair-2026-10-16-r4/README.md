# tile-placement-repair-2026-10-16-r4 —— D-064 悬崖边角、临水墙体与两级退台整改证据

> 生成：2026-10-17（Claude Code 会话，D-064）｜Unity 6000.6.5f1 / URP 2D / Windows 11
> 基线：D-063（r3 批次，公开提交 `68c1d12`）；主仓库全程零 git 写（HEAD=47d8185 不变）
> **安全门禁**：本批全部产出经 `CaptureWriteGuard`（白名单仅 r4、模式仅 `tpr4-`）落盘；
> 采前/采后历史哈希复核 **1881/1881 零覆盖**。
> **中断记录**：seed-13 首轮采集于 2026-10-16 23:34 被系统内存保护终止（零产出、无污染，
> 原启动日志留档 `test-results/tp-seed-20261013.log.interrupted-20261016`）；10-17 恢复重采。

## 一、本轮修复与登记（对照项）

| 项 | 处置 | 关键对照 |
| --- | --- | --- |
| **P1b 南向临水缺墙** | **已修**：高格正南即水且水域深≥2格 ⇒ 水格写 row5 水脚收口件（崖壁入水）；窄水沟保留 | `seed-20261014/TS2g`（中央 Mount 区块南缘：r3 无水墙 → r4 整排水脚崖壁入水）；`tpr4-closeup-waterfront`（三种子） |
| **P2 两级退台** | **已修**：Mount 底段仅 `E(x,y-1)==0`（直落）时延伸——消除"三行叠墙"吞草台 | `seed-20261014/TS2g` 与 `tpr4-closeup-twotier-land`（上带→2格草台→下带三层可辨）；对照 r3 同图 |
| **P1a 对角悬空** | **登记不改**：数据层零错判（B 穷举）+ D 目检"无真正悬空墙块（接缝 2-5px 轻微）"+ 54 片无原生补角件 | `tpr4-closeup-diagonal`（三种子）与 r3 同图（预期无变化） |
| 阴影落水边缘 | **登记不改**：素材实测非整格溢出而是 7-8px 边缘带（α≈1.5% 不可见级） | TS4e/TS2a |
| 竖列/转角命中 | seed13 无 TS2c/TS2d/vertwall（P2 后"相邻竖列"仅存于 Mount 直落地形；该种子未命中）——跳过程序正常（不产图为实）；seed14/15 命中 | v5 竖列规则由 EditMode 锚点补足验证（任务书 §六阶段 3 允许） |

## 二、运行环境与参数

- 窗口化 `-runTests`、单类名过滤 `WorldSandbox.Tests.PlayMode.MapVisualOverhaulCaptureTests`、
  `MVO_CAPTURE_MODE=tpr4-seed-<种子>` + `WS_WORLD_SEED=<种子>`（20261013/14/15）、1280×720、串行单实例。
- `MVO_TPR3_StateContrast_SameFraming` 在 tpr4 下设计性跳过（1 Skipped/运行，非失败）。
- 每种子：34（13，含 2 项未命中跳过）/37（14）/37（15）文件；近景 10 张（7 tpr2- 系 + 3 tpr4- 新系：
  `tpr4-closeup-diagonal`/`waterfront`/`twotier-land`）。

## 三、逐张"能证明 / 不能证明"（核心项）

| 文件 | 能证明 | 不能证明 |
| --- | --- | --- |
| `seed-*/TS2g-two-tier-wall.png`（对照 r3） | P1b 水脚收口件真实上屏（形态：顶接草缘/底入水/白水线）+ P2 两级三层可读 | 全部临水边界逐处正确（另见 waterfront 近景） |
| `seed-14/15 TS2c/TS2d` + `tpr2-closeup-vertwall` | P2 后仍存的"真竖列/转角"（Mount 直落）形态正常、无新伪影 | 全图竖列分布统计 |
| `tpr4-closeup-waterfront.png`（×3） | 高格南邻深水的收口件近距离形态；窄水沟未误补（若命中） | 西/东非南向边缘契约（全图看不覆盖） |
| `tpr4-closeup-twotier-land.png`（×3） | 陆上两级三层结构与草台可见高度 | 退台"官方观感"终判（制作人） |
| `tpr4-closeup-diagonal.png`（×3） | 对角阶梯形态与 r3 一致（未改动） | 是否接受"棋盘缺角"（制作人裁决，见 d064-agent-a §2） |
| `TS4e/TS2a` | 阴影边缘未压水中心（D-063 保持）；水岸白线在 | 7-8px 边缘带（α≈1.5% 不可现级）的绝对不可见性 |
| `TS1a/TS1b` | D-061 地形逐项未变 | 单格级正确性 |

## 四、测试结果（`test-results/`）

| 套件 | 结果 | 备注 |
| --- | --- | --- |
| EditMode 终态（P1b+P2+门禁 r4+契约锚） | **288/288 passed**（`d064-editmode-s2.xml`） | 含 C 两批 14 新用例 |
| PlayMode 捕获 × 3 种子 | 各 **9 passed + 1 Skipped（设计）**，0 failed | `tp-seed-2026101{3,4,5}.xml` |
| 生产回归基线 | 9 文件 8 未变（D-061 算法/D-062/D-063 修复/门禁零回退）；仅 `TerrainOverlayPlan.cs`（本轮两改动） | `d064-baseline-production.sha256.txt` |
| 历史证据 | 采前/采后 **1881/1881 零覆盖** | `d064-capture-pre.sha256.txt` |

## 五、SHA-256 清单

见 `SHA256SUMS.txt`。
