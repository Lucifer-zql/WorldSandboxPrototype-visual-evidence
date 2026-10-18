# 全流程真实游玩质量审计 r5（2026-10-18）

本批是主仓库 `D:\data\UnityProjects\WorldSandboxPrototype` 工作区（HEAD `f85fbe346118e6ee5a6a0e2a33f76f8e653d4259`，
**未提交、未推送**）在 Unity 6000.6.5f1 / Windows 上的**真实运行证据**，是 r1–r4 之后的**增量批次**。
r1–r4 的历史文件**未被修改或覆盖**；本批全部内容位于 `evidence/full-game-quality-2026-10-18-r5/`。

## 0. 本批概要

| 项 | 内容 |
| --- | --- |
| 全量回归 | EditMode **418/418**；PlayMode **116 用例 / 115 passed / 0 failed / 1 skipped** |
| 全流程截图 | **33 帧**，由 6 条 `Audit_0x` PlayMode 用例在真实运行中捕获 |
| 生产改动 | 仅一项：**U-45** 工具栏“当前工具”金色文字加 `Shadow`（不改布局/字号/颜色） |
| 修复验证 | 同一像素区间（200×56 @ 1400,828）前后对照 + 逐帧峰值亮度一致 + 字形形状差分 |
| 差分复核 | 与 audit-18 逐帧 33 帧差分 + 13 张二值掩膜，无结构性变化 |
| 素材复核 | 185 个 PNG 全量重建后 `check_assets.py`：**PASS=18 / WARN=0 / FAIL=0** |

## 1. 全量回归（真实 XML，非摘要）

| 套件 | 文件 | total | passed | failed | skipped | duration | 起止（UTC） |
| --- | --- | --- | --- | --- | --- | --- | --- |
| EditMode | `tests/r5-final-full-editmode.xml` | 418 | **418** | 0 | 0 | 71.6s | 06:52:33 → 06:53:45 |
| PlayMode | `tests/r5-final-full-playmode.xml` | 116 | **115** | 0 | 1 | 568.8s | 06:57:30 → 07:06:59 |

- 两次运行均为 `result="Passed"` / `result="Skipped:Ignored"`（`test-run` 行原文，见 XML 首行）。
- 唯一 skipped：`MapVisualOverhaulCaptureTests.MVO_TPR3_StateContrast_SameFraming`，
  要求 `tpr3-*` 捕获模式而本轮为 `tri-playmode-full`，属**模式约束**，**未计作通过**。

### 1.1 与 r4 的口径对照（不是“变好”，是用例集合变了）

| 项 | r4 | r5 | 说明 |
| --- | --- | --- | --- |
| PlayMode total | 110 | 116 | +6 = 本批新增的 6 条 `Audit_0x` 全流程审计用例 |
| PlayMode failed | 0 | 0 | 两批最终均为 0 失败 |
| PlayMode skipped | 1 | 1 | 同一 `MVO_TPR3` 模式约束 |
| EditMode total | 417 | 418 | +1 |
| EditMode failed | 0 | 0 | — |

两批都**没有**把基线失败写成通过；r4 的基线 8 失败仍保留在 `tests/baseline-*`（r4 批次内）。

## 2. 全流程截图（33 帧）

- 目录：`screenshots/`，共 **33 PNG** + `metrics.log`。
- 分辨率：**31 帧 1600×900**，**2 帧 1280×720**（`20-hud-1280x720.png`、`21-civ-panel-1280x720.png`）。
- 全部由 `Assets/Tests/PlayMode/PlaythroughAuditTests.cs` 在真实 PlayMode 运行中捕获（无静态图、无 AI 生成、无另建演示场景）。
- 帧 → 用例归属、逐用例耗时、行号依据：见 `tests/audit-06-summary.md`。

覆盖流程：主菜单 → 新世界 → 世界总览/生命视角/1:1 → 时间推进（t0 / t+300 / t+1200）→ 五种工具（水域/树/羊/人/删除）
→ 文明详情（中/英）→ 事件日志 → 暂停 → 设置 → HUD（中/英）→ 1280×720 双视图 → 保存 toast → 退出确认
→ 读档列表 → 读档后世界 → 第二世界（64）→ 切回第一世界 → 工具栏副行（折叠/展开/水域选中/资源展开/删除/再展开）。

`metrics.log` 给出与截图同步的模拟快照，其中**存档/读档一致性**是本批的关键不变量：

| 快照点 | tick | pop | creatures | buildings | civs | trees |
| --- | --- | --- | --- | --- | --- | --- |
| `[world-a-before-save]` | 263 | 24 | 28 | 9 | 3 | 141 |
| `[world-a-loaded]` | 263 | 24 | 28 | 9 | 3 | 141 |
| `[world-a-switched-back]` | 266 | 24 | 28 | 9 | 3 | 141 |

读档后计数**逐项相同**（不跳变）；切回第一世界后 tick 由 263 → 266（真实推进 3 tick），其余计数不变。

## 3. U-45 修复（本轮唯一新增生产改动）

### 3.1 改动内容

工具名“当前：<工具>”是**金色文字压在木纹面板右端雕饰**上，字形与底色亮度差约 1 个分带 → 真机难读。
修复：只给该文字**叠加深色投影**（`UnityEngine.UI.Shadow`），不改字号/锚点/颜色/布局。

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Editor/UIPrefabBuilder.cs` (L654–659) | `AddComponent<Shadow>()`；`effectColor=(0.10,0.07,0.05,0.85)`、`effectDistance=(1.5,-1.5)`、`useGraphicAlpha=true` |
| `Assets/_Project/Prefabs/UI/MainCanvas.prefab` | CurrentTool 序列化新增 Shadow 组件（`m_Script` guid `cfabb0440166ab443bba8876756fdfa9`，`m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Shadow`） |

### 3.2 验证（可复核，详见 `analysis/u45/u45-shadow-metrics.md`）

1. **对照区间是精确匹配出来的**：4 张 4x 放大图以 NEAREST 还原到 1:1 后做全帧精确匹配，
   `before` 各唯一命中 `audit-18` 帧、`after` 各唯一命中 `audit-19` 帧，另一侧**零命中** → 前后归属确定。
2. **不是改字色**：同一区间 6 帧的峰值 luma，`audit-18` 与 `audit-19` 都是 **112.1**，逐帧一致。
3. **变化是字形形状**：变化像素的行范围恒为 y845–859（14 px，文字行高），列数随工具名字符数变化
   （26 / 71 / 84 列），不外溢到面板其它区域 → 不是面板入场动画。
4. **方向是“压暗”**：显示工具名的帧变暗像素 230–268、变亮 54–63；不显示工具名的帧仅 88/11
   → 与“新增半透明深色描边”一致。
5. **一眼可验**：`analysis/u45/f30_stacked_before_top_after_bottom.png`（帧 30，上=前/下=后）、
   `f32_stacked_...png`（帧 32）、`f30_diff_x6_x8.png`（6 倍增益差分，变化贴着字形）。

### 3.3 如实记录的既有特征（非本次引入）

工具名文字右端在**修复前后都**结束于 x≈1579，而工具栏面板右边界在 x=1578 → 文字右端基本**零右内边距**。
前后一致、且首尾字形完整（非裁切），属既有布局特征，本批**未改动**、也**不声称已优化**。

## 4. 差分复核（audit-19 vs audit-18，逐帧 33 帧）

### 4.1 口径（已固化，可复算）

像素差异 = `max(|ΔR|,|ΔG|,|ΔB|)`，阈值 `> 12`；`diff%` = 超过阈值的像素数 / 帧总像素。
`analysis/_frame-diff-vs-18-recomputed.txt` 同时给出 `lum_gt12%`（按亮度差）作为对照列，
并**验证了**初版表 `_frame-diff-vs-18.txt` 的数值与该 `maxch_gt12%` 口径 33/33 逐帧一致。
`analysis/diffmasks/*.png` 的白色像素数与同一列的百分比**逐帧完全吻合**（已核对）。

### 4.2 全部 33 帧

| 帧 | diff% | 帧 | diff% | 帧 | diff% |
| --- | --- | --- | --- | --- | --- |
| 01-main-menu-zh | 0.757 | 02-new-world-default-zh | 1.266 | 03-world-overview-t0 | 4.202 |
| 04-world-life-t0 | 13.856 | 05-world-life-1to1 | 16.429 | 06-world-t300 | 4.172 |
| 07-world-t1200 | 4.299 | 08-world-overview-t1200 | 3.693 | 09-tool-water | 3.683 |
| 10-tool-tree | 4.335 | 11-tool-sheep | 4.457 | 12-tool-human | 4.322 |
| 13-tool-remove | 4.577 | 14-civ-panel-zh | 10.157 | 15-event-log-zh | 11.456 |
| 16-pause-zh | 11.833 | 17-settings-zh | 11.500 | 18-civ-panel-en | 26.375 |
| 19-hud-en | 30.941 | 20-hud-1280x720 | 31.068 | 21-civ-panel-1280x720 | 26.119 |
| 22-save-toast-zh | 5.062 | 23-exit-confirm-zh | 5.860 | 24-load-list-zh | 2.289 |
| 25-loaded-world-zh | 5.805 | 26-second-world-64 | 4.367 | 27-switch-back-first-world | 4.789 |
| 28-toolbar-collapsed | 17.865 | 29-toolbar-terrain-open | 17.707 | 30-toolbar-water-selected | 17.498 |
| 31-toolbar-resource-open | 4.693 | 32-toolbar-remove | 4.468 | 33-toolbar-terrain-reopen | 4.627 |

### 4.3 机位一致性前置检查（先排除“相机漂移”这一解释）

在做逐帧解释前先验证：两批是否同一机位/构图。方法为 8×8 均值池化下采样后做 ±48 px 的 SAD 整数偏移搜索
（`analysis/_global-alignment-check.txt`，抽样 12 帧）：

- **12 帧最优对齐偏移全部为 (0,0)**，`SAD_zero / SAD_best = 1.00`（帧 04 为 1.07，且最优点在 +2 下采样像素处，
  属地形纹理噪声的近似最优）→ **机位与构图一致，无相机位移/缩放漂移**。

因此下面的 diff% 只能解释为**内容差异**（动画相位、周期性 UI、±1–2 tick 的世界差异），而不是取景变化。

### 4.4 分档解释（每档均有掩膜可查）

| 档 | 帧 | diff% | 依据 |
| --- | --- | --- | --- |
| 近静态 | 01、02、24 | 0.76–2.29 | 菜单/新世界/读档列表：仅 UI 入场与 toast 滑入 |
| 世界动画 | 03、06–08、09–13、26、27 | 3.7–4.6 | 活体单位/羊/树摇曳与地形动画叠加的相位差 + ±1–2 tick 采集时序 |
| 生命近景 | 04、05 | 13.9、16.4 | 近景下地形动画叠加/单位/树占屏比大，相位差被放大；掩膜 `diffmask_04/05` 呈地形纹理+树+单位形状，大片区域不变 |
| 面板/事件 | 14–17 | 10.2–11.8 | 侧栏与事件日志的入场动画、滚动位置；掩膜 `diffmask_14`–`17` |
| 英文/1280×720 | 18–21 | 26.1–31.1 | 面板/HUD 内容与入场差异、1280×720 下的不同画布；掩膜 `diffmask_18`–`21` |
| 工具栏副行 | 28–30 | 17.5–17.9 | 副行滑入/展开的过渡相位；掩膜 `diffmask_28`–`30` |
| 工具栏稳定 | 31–33 | 4.5–4.7 | 帧已稳定；差异含 **U-45 描边**（文字区间见 `analysis/u45/`） |

逐帧核对得到的结论：**差异均呈内容形状**（文字字形、地形纹理/边缘、树/单位轮廓、提示条与面板），
**未出现“布局重排”“面板/锚点位移”“整块区域被填充或替换”这类结构性变化**；
`diffmask_04` 中一处实心矩形差异经放大核对为**底部操作提示条在两次采集的不同可见阶段**
（`audit-18` 未显示、`audit-19` 已显示），属周期性 UI 的时序差异，不是新增/缺失控件。
U-45 描边是本批**已知且预期**的差异来源，仅影响显示工具名的帧。

## 5. 素材复核（真实重建后）

| 证据 | 内容 |
| --- | --- |
| `logs/rebuild-inventory-log.txt` | Tiny Swords 素材清单全量重建日志（185 条） |
| `logs/check-assets-after-rebuild.txt` | `check_assets.py --sync-only` 类校验：**PASS=18 / WARN=0 / FAIL=0** |

校验覆盖：Skill 唯一源与两处副本 4 文件 SHA256 全部一致；PNG 哈希清单 185/185 匹配；机读清单 185 条与磁盘完全一致；
切片统计 961 个 Sprite 子资源、被场景/Prefab/SO 引用 162/185；Alpha bbox 全部一致；层序常量 12 项与权威规范一致；
规范/矩阵/许可等 8 份文档在位。

复核（发布前）：以同一命令重跑 `check_assets.py`，**逐行输出与 `logs/check-assets-after-rebuild.txt` 完全一致**
（PASS=18 / WARN=0 / FAIL=0）——即该日志可复现，不是一次性产物。

## 6. 限制与口径

- 本批是**待独立验收的材料**，**不是**自我宣布 PASS，也**不声称 G9 用户验收通过**。
- 唯一 skipped 未计通过；未跑的套件/平台（如 Web、Android）**本批未执行、无结论**。
- 截图是**运行中捕获**的帧，不是逐帧确定性渲染；跨批次差分只用于“无结构性变化”的判据，不用于像素级回归基线。
- `analysis/` 下的脚本为临时分析工具（`locate_u45.py`、`u45_classify.py`、`region_geom.py`、`final_masks.py`、
  `recompute_diff.py`），**未入库**，文件中已注明各自口径。
- 主仓库工作区仍有大量未提交改动（含本批 U-45 修复与新增审计用例），**本批未提交主仓库**。

## 7. 文件清单

```
evidence/full-game-quality-2026-10-18-r5/
├── README.md                      本文件
├── SHA256SUMS.txt                 本批全部文件的 SHA-256
├── screenshots/                   33 帧 PNG + metrics.log
├── tests/
│   ├── r5-final-full-editmode.xml    EditMode 418/418 原始 XML
│   ├── r5-final-full-playmode.xml    PlayMode 116/115/0/1 原始 XML
│   └── audit-06-summary.md           6 条 Audit_0x 用例摘要与帧归属
├── logs/
│   ├── check-assets-after-rebuild.txt
│   └── rebuild-inventory-log.txt
└── analysis/
    ├── _frame-diff-vs-18.txt               初版逐帧差分表
    ├── _frame-diff-vs-18-recomputed.txt    口径固化的重算表（含口径一致性的验证）
    ├── _global-alignment-check.txt         机位一致性检查（8× 下采样 SAD 搜索，12 帧）
    ├── _u45_before_4x.png / _u45_after_4x.png      帧 30 文字区间 4x 前后对照
    ├── _u45b_before_4x.png / _u45b_after_4x.png    帧 32 文字区间 4x 前后对照
    ├── diffmasks/                    13 张二值掩膜（白 = 差异 > 12）
    ├── diffimages/                   4 张原始灰度差分图
    └── u45/                          U-45 佐证：上下对照、差分、上下文、指标说明
```

## 8. 复现

```
# 全量回归（Unity 编辑器/CLI；PlayMode 截图落点由 WS_AUDIT_SHOT_DIR 指定）
unity test . --mode EditMode --output Logs\editmode-full.xml
unity test . --mode PlayMode --output Logs\playmode-full.xml

# 素材校验
python .agents/skills/tiny-swords-assets/scripts/check_assets.py
```

哈希校验：`SHA256SUMS.txt` 位于本目录，覆盖本批全部文件；远端提交哈希见仓库提交记录。
