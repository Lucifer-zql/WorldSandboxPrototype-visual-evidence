# 实际游玩体验深度整改 r6（2026-10-18）

本批是主仓库 `D:\data\UnityProjects\WorldSandboxPrototype` 工作区（HEAD `f85fbe346118e6ee5a6a0e2a33f76f8e653d4259`，
**未提交、未推送**）在 Unity 6000.6.5f1 / Windows 上的**真实运行证据**，是 r1–r5 之后的**增量批次**。
r1–r5 历史批次**未被修改或覆盖**；本批全部内容位于 `evidence/full-game-quality-2026-10-18-r6/`。

## 0. 本批概要

| 项 | 内容 |
| --- | --- |
| 触发 | r5 通过后项目负责人实际试玩反馈**体验差、问题多**；r6 以玩家可见缺陷为对象（复现→根因→修复→定向测试→真实运行验证） |
| 生产修复 | **F-01** 空文明幽灵采集/违规复活（R-BIRTH-08）；**F-02** 事件叙事（文明名/死因/真实 tick）；**F-03** 人物聚团→分散活动；**F-04** 民居环带选址+间距；**F-05** 详情卡假数据（Age/饥饿/归属）；**F-06** Settlement 阶段空占位→真实聚落结算（R-ORDER-01 第 4 步 a–f）；**F-08** **文明自建民居从不渲染**（建造终于可见） |
| 全量回归 | EditMode **430/430**（418 基线 + 12 r6 定向）；PlayMode **117 用例 / 116 通过 / 0 失败 / 1 跳过**（`MVO_TPR3` 模式约束，未计通过） |
| 全流程截图 | **33 帧**（`PlaythroughAuditTests` 真实 PlayMode 捕获，1600×900 为主）+ `metrics.log` |
| 决策记录 | `DECISIONS.md` §2.25（D-069）；`04` §9/§13.4 已按台账回改；`impl/D-068` §10 |

## 1. 真实运行前后差异（同种子 20261013、t1200 同机位）

| 维度 | r5（前） | r6（后） |
| --- | --- | --- |
| 人物活动 | ~15 人挤成 4×4 方阵（全员锁定同一采集点） | 村民自然散布（作业现场 + 家域游走） |
| 民居 | **0 栋可见**（数据 20 栋建筑除城堡外全部隐形，F-08） | 城堡周围环带散布的真实村落（间距 ≥2） |
| t1200 库存/人口 | 食物 18 / 人口 55 | 食物 35 / 人口 63（消耗口径精确到 100 tick） |
| 事件日志 | "一名新居民出生""树已枯竭"（无归属） | "苍林部族的一名新居民出生""树已被采尽"；死亡分饿死/冲突 |
| 详情卡 | 年龄恒 0 天、饥饿恒 0/100、"聚落 #1" | 年龄真实累积、饥饿占位（IMPL-10 未实施不伪装）、"所属文明：晨曦王国" |

一眼可验对照：`analysis/t1200-before-top-after-bottom-2x.png`（上=r5 / 下=r6，同机位）；
`analysis/before-castle-area-3x.png` ↔ `analysis/after-castle-area-3x.png`（城堡区域 3x）；
`analysis/after-village-wide-2x.png`（村落广角）、`analysis/after-eventlog-3x.png`（事件文案）。

## 2. 测试（真实 XML）

| 文件 | 内容 |
| --- | --- |
| `tests/r6-editmode-final.xml` | EditMode **430/430**（最终态，含文案微调后复跑） |
| `tests/r6-playmode-final.xml` | PlayMode **117/116/0/1**（最终态，含 `BuildingRenderSyncTests`） |
| `tests/r6-playmode-texts.xml` | 文案微调（"{0} 的"→"{0}的"）后定向复跑 7/7 |
| `tests/r6-building-render.xml` | F-08 定向 1/1（民居渲染数量+位置双口径） |
| `tests/r6-targeted-1.xml` | **红相**：5 失败（夹具缺口 + 缺陷复现，TDD 先红） |
| `tests/r6-targeted-2.xml` | 红相收敛：31/33（余 2 项阈值/样本问题） |
| `tests/r6-targeted-3.xml` | 绿相：33/33 |
| `tests/r6-playmode-full.xml` | F-08 修复前的 PlayMode 全量（115/116，对照用） |
| `tests/r6-editmode-full.xml` | 中间态 EditMode 全量（425/430，反射签名缺口 5 例已修） |

诚实记录：r5 时代的 `CivilizationSimulationTests` 依赖两个缺陷才通过（空文明幽灵采集 + 零人口出生；
夹具未把 WorldFactory 播种生物注入 EntityRegistry）。r6 修复缺陷后这些测试转红，**修正夹具**（与载入路径
同口径）而非削弱断言——红相见 `r6-targeted-1.xml`。

## 3. 已如实登记、未在本轮修改

1. R-CIV-01 聚落自发形成、R-CIV-21~25 工作分配、R-CIV-31 建造进度未实现（D-069 ① 边界）。
2. 04 R-LAYER-12/13 层序（生物恒在建筑上）→ 北侧村民会画在城堡屋顶（设计层序后果，待裁定）。
3. 工具无悬停笔刷预览；世界信息条含种子/tick（D-067 保留）；树木不可再生、树为唯一食物来源（既有取舍）。
4. 15 帧事件文案为修正前的"{0} 的"（多一空格），交付字符串已改"{0}的"（`r6-playmode-texts.xml` 复跑 7/7）。
5. 主仓库未提交、未推送；`MVO_TPR3` 跳过未计通过；Web/Android 未执行、无结论。

## 4. 文件清单

```
evidence/full-game-quality-2026-10-18-r6/
├── README.md                 本文件
├── SHA256SUMS.txt            本批全部文件 SHA-256
├── screenshots/              33 帧 PNG + metrics.log（真实 PlayMode 捕获）
├── analysis/                 前后对比（拼图/城堡区 3x/村落广角/事件日志）+ ANALYSIS-NOTES.md
└── tests/                    9 份测试 XML（含红→绿过程与最终态）
```

## 5. 复现

```
unity test . --mode EditMode --output Logs/r6-editmode-final.xml
WS_AUDIT_SHOT_DIR=<新空目录> unity test . --mode PlayMode --output Logs/r6-playmode-final.xml
```

哈希校验：`SHA256SUMS.txt` 覆盖本批全部文件；远端校验为**远端重克隆后逐文件哈希比对 0 不符**（见批次提交记录）。
