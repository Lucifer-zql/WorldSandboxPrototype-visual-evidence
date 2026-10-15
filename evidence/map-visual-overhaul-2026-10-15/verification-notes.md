# 独立核验记录（Agent F，2026-10-15）

> 核验人：Agent F（Independent Verification Engineer，《神之沙盘》MVO 专项）。
> 核验方式：只读取证 + 本目录内新增产物（本文件、各 capture-manifest.txt 占位补记、SHA256SUMS.txt）。
> 未修改任何既有文件的其他内容；未启动 Unity；未执行任何 git 写操作。

## 一、manifest 占位补记（清单 4）

- 补记值：`HEAD=47d8185`（工作树未提交状态，`git rev-parse --short HEAD` 实测，完整 SHA-1 `47d8185d92e0d248f252460b535a39679eb71262`）；`dirtyFiles=327`（`git status --short | wc -l` 补记时刻实测；批次 `workspace-state.txt` 18:51:15 快照为 306，其后 MVO 证据文件新增使计数增长）。
- 覆盖 7 份 capture-manifest.txt 共 68 行捕获记录：after=12、cloud=2、iter1-after=10、iter1-seed-20261014=10、iter1-seed-20261015=10、seed-20261014=12、seed-20261015=12。
- 每份文件末尾追加一行 `; backfilled 2026-10-15 by Agent F`；补记后占位符残留为 0（grep 实测）。
- 此补记使 `capture-manifest.md` §捕获环境「各 capture-manifest.txt 已补记 HEAD 与脏文件数」的声明成为事实。

## 二、截图新鲜度与归属交叉印证（清单 1）

各捕获目录 PNG 的文件修改时间与驱动测试 XML 的 start/end（UTC，本地=UTC+8）逐一吻合，且捕获由测试内 `Snap()` 直接产出（先删旧图、等新文件落盘，防同名旧图冒充）：

| 捕获目录 | PNG mtime（本地） | 驱动 XML | XML 起止（UTC → 本地） | 判定 |
|---|---|---|---|---|
| baseline/placement、baseline/framing | 18:54:14 | playmode-baseline-placement / -framing | 10:52:48–10:53:25Z → 18:52–18:53 | 吻合（见§四-R2） |
| iter1-after | 20:35:26–27 | playmode-after-results（4 用例） | 12:35:25–27Z → 20:35:25–27 | 吻合 |
| iter1-seed-20261014 | 20:37:14–16 | playmode-seed14-results（4 用例） | 12:37:13–16Z → 20:37:13–16 | 吻合 |
| iter1-seed-20261015 | 20:37:39–41 | playmode-seed15-results（4 用例） | 12:37:37–41Z → 20:37:37–41 | 吻合 |
| after | 21:25:26–31 | playmode-after2-results（5 用例） | 13:25:25–31Z → 21:25:25–31 | 吻合 |
| seed-20261014 | 21:25:54–59 | playmode-seed14b-results（5 用例） | 13:25:53–59Z → 21:25:53–59 | 吻合 |
| seed-20261015 | 21:26:21–26 | playmode-seed15b-results（5 用例） | 13:26:20–26Z → 21:26:20–26 | 吻合 |
| cloud | 21:26:48–50 | playmode-cloud-results（1 用例） | 13:26:47–50Z → 21:26:47–50 | 吻合 |

内容合理性：`MVO1a`（ortho40）与 `MVO1b`（ortho64）目视均为全图视图（128×128 全岛 + HUD），与 manifest 的 ortho/seed/screen 标注一致；全部 PNG 实测 1280×720（`file` 扫描，112 张）与 manifest `screen=1280x720` 一致。`longest-horizontal-cliff-run` 统计在同种子两轮间一致（20261013=8、20261014=11、20261015=7），同种子地形稳定性佐证。

## 三、误导性证据排查与两轮口径判别（清单 2）

1. **`before/` 已删除确证**：目录递归列表中无 `before/`；`git status` 中本批次整体为单一 untracked 条目（`?? docs/impl/evidence/map-visual-overhaul-2026-10-15/`），无删除痕迹（该目录从未入库，删除不产生 git 记录，属预期）。事故与处置已在 `README.md`（运行方式节）与 `capture-manifest.md` §逐目录代码版本口径、`docs/impl/parallel-repair/mvo-worklog.md` §三 如实登记。终批全量回归（13:27:12Z）之后无任何新 PNG 落入本批次（最晚 PNG mtime 21:26:50），无"新代码画面冒充 before"残留。
2. **iter1 与 after 的代码版本口径**：各 capture-manifest.txt 行格式两轮完全相同（mode 同为 `after`、无版本字段、HEAD 占位未填），**单看 manifest 不可区分**；但批次级证据链可区分，本核验独立复核如下：
   - 时间线：iter1 捕获于本地 20:35–20:37，其前一刻 EditMode `editmode-mvo-integration4.xml`（209/209）结束于 20:34:44；终批捕获于本地 21:25–21:26，其前 `editmode-mvo-r2.xml`（211/211）结束于 21:24:09。两轮之间用例数 209→211（+2，对应二轮 D 域林界流苏选片新增断言）、PlayMode 捕获用例 4→5（+1，对应二轮新增 MVO5 云树用例），与 worklog §一.7「二轮迭代」一致。
   - 画面证据：同种子 20261013 下 `iter1-after/MVO1a` 与 `after/MVO1a` 大陆基形相同（同种子），但 `after/` 明显多出林心树簇、绿灌木变体与草林深绿流苏边——与二轮三项改动的声明吻合；`baseline/framing/hud_1280x720.png` 呈"黄色单色草地 + 碎斑水域 + 零散崖条"的修复前形态，与两者均不同。
   - 结论：两轮归属可信，`capture-manifest.md` §逐目录代码版本口径的判别依据与本核验独立推导一致。
3. **baseline 与 `_backup-prev-state` 的字节级比对**：baseline/placement 24 张 PNG 中 20 张与 `_backup-prev-state/tiny-swords-placement-v04/` 同名文件 SHA-256 完全一致，4 张不同（cliff-elevated-ground-closeup、tree-closeup-conifer、water-rock-placement、hud_2560x1440）；两份 capture-manifest.txt 逐字节一致。4 张差异图目视逐对核验（cliff 特写两版完全同构：v2 崖渲染、无高草/双色草地）——差异为亚可见像素级噪声，**全部 24 张内容均为修复前状态**，不存在新代码冒充 before。机制备注：v04 证据捕获于 2026-10-15 07:45 本地（playmode-testresults.xml start 2026-10-14 23:45:34Z）；20 张跨会话字节一致更符合"沿用 v04 产出 + 4 张 18:53 重拍覆盖"，与 worklog「新鲜截图」的表述存在口径差（见§四-R2），但不构成内容失实。

## 四、发现项（真实性风险清单）

- **R1（高）【已处置闭环，2026-10-16 复核确认，详见附录 A】MVO5a 云树同框证据不成立——8 张 PNG 均无云精灵像素**。像素取证：以 `_backup-prev-state/.../cloud-animation-frames.png` 实采云精灵色（R≈194,G≈240,B≈226 淡青白）全帧扫描 `after/`、`cloud/`、`seed-20261014/`、`seed-20261015/` 全部 8 张 MVO5a 帧，命中仅为底部 HUD 按钮色（各 8 个采样点，位于 y≥636 的工具栏）；按 manifest 云坐标换算的预期屏幕位置（如 after：云(55.57,63.50)→约(714,432)）实测均为纯草地色。驱动日志显示云渲染对象存在（18/18/14/15 个、坐标与 manifest 一致）、测试断言通过，但画面无云——即云渲染在终版代码中未产出可见像素（疑似渲染回归，根因待 D/E 域排查，`MapVisualOverhaulCaptureTests.CollectLiveCloudRenderers` 仅校验 `gameObject.activeInHierarchy` 而非组件 `enabled`/像素产出，注释亦自述"不做像素断言"）。另：README §cloud 声称"相对位移可辨/帧间差异即云漂移"，实测 `after/`、`cloud/`、`seed-20261014/` 三对帧**字节完全相同**（SHA-256 一致），唯一有差异的 `seed-20261015/` 两帧差异区域为顶部水岸泡沫动画带（diff bbox=(0,43)-(1175,180)），亦非云漂移。**G10"云盖树冠 order250"的视觉证明当前不被这批图片支持**（对照：baseline/placement 云漂移两帧确有云且两帧不同）。建议：修复后以 `MVO_CAPTURE_MODE=cloud` 重拍，或在 README/批次说明中如实降级该声明。
- **R2（低）baseline"新鲜截图"口径不精确**：baseline/placement 24 张中 20 张与 v04 备份字节一致（沿用），仅 4 张为 18:52–18:53 测试运行后的新拍；内容均为修复前状态（见§三.3），不构成误导，但 worklog「TinySwordsPlacement/CameraFraming 新鲜截图存 baseline/」的表述对多数文件不成立，建议后续批次描述改为"沿用 + 抽样重拍"。
- **R3（低）baseline/framing 仅落地 1/3 分辨率**：其 capture-manifest.txt 记录 1280x720/1920x1080/2560x1440 三目标均 match=True，目录仅含 hud_1280x720.png；README 与 capture-manifest.md 仅引用 720p 一张，批次文档自洽，但 manifest 行与落地文件的对应关系建议注明。
- **R4（低）shadow-overlap-closeup.png 遗留矛盾**：baseline/placement 中该文件存在（与 v04 备份字节一致），而 manifest 同行标注 `SKIPPED（固定种子无带影树）`——系自 v04 批次继承的遗留文件，非本轮伪证；建议遗留文件在复制时剔除或注明。
- **R5（低）首轮 integration 无 XML**：`editmode-mvo-integration.log`（792 行）显示运行终止于脚本编译失败（WaterLandStep.cs(108) CS1503/CS1660，ExitCode 1），测试未启动故无 XML；该事件与 worklog §一.5「`Array.Sort` 无 (T[],int,int,Comparison) 重载（集成改 Comparer.Create）」对应，已在工作记录留痕，README §test-results 未单列该 log，建议补一句说明以免被误读为缺文件。
- **R6（低）capture-manifest.md 一处笔误**：§捕获运行记录"一轮捕获 … 5/5×3"应为"4/4×3"（一轮无 MVO5，`playmode-after/seed14/seed15-results.xml` 实测各 4 用例）。

## 五、测试结果链完整性（清单 3）

全部 17 份 XML 的 test-run 头实测（用例/通过/失败/结果），与声明链一致，无隐匿失败：

| XML | 用例 | 通过 | 失败 | 结果 | 时间(UTC) |
|---|---|---|---|---|---|
| editmode-baseline | 199 | 199 | 0 | Passed | 10:51:47 |
| editmode-mvo-integration2 | 209 | 205 | 4 | Failed(Child) | 11:52:11 |
| editmode-mvo-integration3 | 209 | 208 | 1 | Failed(Child) | 12:07:11 |
| editmode-mvo-integration4 | 209 | 209 | 0 | Passed | 12:34:44 |
| editmode-mvo-r2 | 211 | 211 | 0 | Passed | 13:24:09 |
| playmode-baseline-placement | 1 | 1 | 0 | Passed | 10:52:48 |
| playmode-baseline-framing | 1 | 1 | 0 | Passed | 10:53:21 |
| playmode-after-results | 4 | 4 | 0 | Passed | 12:35:25 |
| playmode-full-regression | 59 | 57 | 2 | Failed(Child) | 12:38:02 |
| playmode-full-regression2 | 59 | 59 | 0 | Passed | 12:41:20 |
| playmode-after2-results | 5 | 5 | 0 | Passed | 13:25:25 |
| playmode-seed14b-results | 5 | 5 | 0 | Passed | 13:25:53 |
| playmode-seed15b-results | 5 | 5 | 0 | Passed | 13:26:20 |
| playmode-cloud-results | 1 | 1 | 0 | Passed | 13:26:47 |
| playmode-full-regression3 | 60 | 60 | 0 | Passed | 13:27:12 |
| playmode-seed14-results | 4 | 4 | 0 | Passed | 12:37:13 |
| playmode-seed15-results | 4 | 4 | 0 | Passed | 12:37:37 |

- 声明链核对：EditMode 199/199 → 209/209（integration4）→ **211/211（r2 终态）**；PlayMode 59 → **60/60（regression3 终态）**。✓
- 中间失败与处置核对：integration2 失败 4 例（CliffTileSelectionTests.HighGrass_EnclosedPlateauInterior_GapTolerant、CliffTileSelectionTests.WaterFoot_SelectsWaterGroup、TerrainCompositionTests.PlateauStep_RingClosed_NoIsolated_InteriorGrass_Deterministic、TerrainCompositionTests.RuleTiles_GrassAndForest_UseCanonicalNineSlot）；integration3 仅剩 PlateauStep_RingClosed 一例——与 worklog §一.5「color4 断言同步 / WaterFoot 锚 hash 校正、豁口格裁定 / 平台环豁口两轮退回修复」逐项对应。playmode-full-regression 失败 2 例（TinySwordsPlacementEvidenceTests.CapturePlacementEvidenceV04、UiWiringEvidenceTests.ButtonsPanelsPalettes_WiredWithEvidence）——与 worklog §一.5 及 README §test-results「两个测试期望语义随新世界修正（非弱化）」对应，修正理由在测试注释。✓

## 六、多种子互异核验（清单 6）

各 manifest `effectiveSeed` 与目录名一致且互异：iter1 批（iter1-after=20261013、iter1-seed-20261014=20261014、iter1-seed-20261015=20261015）；终批（after=20261013、seed-20261014=20261014、seed-20261015=20261015、cloud=20261013）。终批三份全图 PNG（MVO1a）SHA-256 互异且目视布局显著不同（20261013 东侧大海湾 / 20261014 左侧与右下水域 / 20261015 左上角水域与中部双湖），地形规则（双色草地、台地崖环、树簇）三种子均保持。✓

## 七、SHA256SUMS.txt（清单 5）

- 生成于批次根，覆盖当时全部 239 个文件（排除 SHA256SUMS.txt 自身；本核验记录与各 manifest 补记先于哈希生成，故已纳入清单）。
- 逐文件清单：`find . -type f ! -name SHA256SUMS.txt | sort | xargs sha256sum`（text 模式）→ **239 行 = 239 文件，行数核对一致**；`sha256sum -c SHA256SUMS.txt` 全量回验 239 OK / 0 失败。
- 批次指纹（字面管道 `find . -type f ! -name SHA256SUMS.txt | sort | sha256sum`，即对排序清单整体求哈希）：`38df00ef632a4d7b8e88c21fa7bb810be069042956835807367105a20f8895c2`，供整批一致性快速比对。
- 注意：若此后批次内任何文件再被改动，上述两者均失效，需重新生成。（2026-10-16 G10 收尾后已按附录 A 重新生成，见附录 A 末行。）

## 附录 A：G10 云证据五轮攻坚闭环记录（Agent F 复核，2026-10-16）

> 背景：本记录 §四-R1 曾判定 MVO5a 云证据不成立。经 A 终审五轮攻坚与两项生产缺陷修复，G10 已闭环；A 终审终裁**通过**（争议像素 120 ≥ 20 阈值达标）。以下为时间线、判据与 Agent F 独立复核结果。

### A.1 时间线与轮次（以 cloud/capture-manifest.txt 行级记录为准，每轮无效行均带 superseded 标注）

| 轮 | 时间（本地） | 内容与判定 | 判据/标注 |
|---|---|---|---|
| v1 | 10-15 21:26 | 初版捕获（即 R1 所指 8 张之一组） | A 终审判无效：两帧逐字节相同且全帧无云体（与本记录 §四-R1 像素取证一致）；v2 行 note 引用 |
| v2/v3 | 10-15 21:58 / 23:00 | 云为锚取景（ortho7）+ 视口/字节双自检 | A 复核判无效：云位重合、字节差被 1px 噪声假阳性满足；行标 `superseded by v4` |
| 探针定位 | 10-15 22:02–23:44 | `test-results/probe-cloud{1..9}.log`（含 probe-cloud5/6/7/9.xml）：渲染器状态 → 视口/字节差自检 → 反射 `_clouds` → SendMessage 直调 Update 五层探针 | 定位两项生产缺陷（见 A.2）；worklog §三.5 记载 3 次探针自身编译错进 Safe Mode 的处置 |
| v4 | 10-15 23:21 / 23:46 | 遮挡取景（切比雪夫≤1）+ 漂移门（云屏幕位移≥8px）；MVO5b 轮文件事后清理 | dx=19.3px/3.1s 过门；`overlap=overlapped` 系画布推断，行标 `superseded by v6` |
| v6（note 头标 v5） | 10-16 00:05 | 屏幕空间触发像素级重叠帧（\|dx\|≤40px 且云屏点高于树根） | gap 仅 1-3px、争议像素=0，行标 `superseded by v7` |
| **v7（终版）** | **10-16 00:16–00:17** | 双帧间隔延至 ≥6s（0.5s×12 次泵帧，云再进 ~38px，右半叶完整滑入松针带）；ortho6，帧泵 QueuePlayerLoopUpdate+RepaintAllViews | 成功行记 `overlap=contested-px>=20`；check 行 `drift-screen-px=37.5 gate=min8px byteDiff=differ`；**A 终审实测争议像素 120 ≥ 20 判通过** |

### A.2 两项生产缺陷与修复（判据：worklog §三.5、README §修复范围行 33）

1. **云切片空白**：`Main.unity` cloudSprites 所接 8 个 `_0` 切片全为 36 片图集的空白格——云自接入以来从未渲染（缺陷长期潜伏，v04 时代证据图同样无云）。修复：像素取证后切换为含画稿切片（Clouds_01_21/02_13/…/08_13），B 复测 ratified。
2. **漂移结构体副本失效**：`DecorationRenderer.Update` 对 `CloudState`（struct）副本变异不写回列表，漂移自接入以来从未实际发生（SendMessage 直调 Update 增量=0 证实与引擎无关）。修复：`_clouds[i] = cloud` 写回一行；修后实测漂移 19.3px/3.1s（理论 18.6）。

### A.3 Agent F 独立复核（v7 终版文件，与 R1 同方法同口径）

- `cloud/MVO5a-cloud-over-forest-frame{1,2}.png`（00:16:55 / 00:17:02 落盘）SHA-256 互异（457e69c4… / 4fcd4d15…）。
- 全帧像素差仅 2347px，diff bbox=(630,356)-(726,389)——**恰为云所在区域**，与 check 行 37.5px 屏幕位移互证；全帧其余像素静止（区别于 v1 时代"差异在顶部水岸泡沫带"的假阳性形态）。
- 目视 frame1：灰白云体真实入画，位于中央松树冠层左缘，与 manifest `cloudScreen=(659.7,360.0) treeScreen=(696.7,360.0) dxScreen=-37.1px` 对应；云色实测采样 (229,233,205)。
- 反向复核更正：以 v7 云色反扫 v04 时代 `baseline/placement/cloud-drift-a.png` 无相干灰白云团（223 个稀疏命中为 HUD/白桦树干散点）——**更正本记录 §四-R1 中「v04 云漂移两帧确有云」的对照表述**：v04 证据图同样无云，其两帧字节差异为水岸泡沫动画；此与 README/worklog「切片空白缺陷长期潜伏」的结论一致。R1 的核心判定（v1 8 张无云像素）不变，处置闭环成立。

### A.4 冻结前残余备注（不阻塞，R2-R6 维持原状）

- README §`cloud/` 导览行仍写「间隔 3.1s 实测位移 19.3px」（v4 轮数字），当前终版文件为 v7（6.1s / 37.5px）——manifest 行级记录为准，建议交付时一并订正（Agent F 不改 README）。
- `capture-manifest.md`（10-15 21:32 版）尚未收录 G10 攻坚轮次，批级汇总以 README + cloud/capture-manifest.txt 行级历史为准。

### A.5 冻结哈希与终态链条（G10 收尾后重新生成，2026-10-16）

- **测试链补全**（G10 攻坚期间新增 test-results 文件）：`playmode-cloud-v{2..7}-results.xml`+log（v2–v7 各轮捕获运行；v7 = 1/1 Passed）；`playmode-full-regression4.xml`（23:03，60 用例 59/1——**唯一失败即 MVO5 自身**，系缺陷修复中间轮）→ `playmode-full-regression5.xml`（23:48，**60/60 Passed**，缺陷修复后全绿）；`probe-cloud{1..9}.log` + `probe-cloud{5,6,7,9}.xml`（探针定位）。终态：最新 PlayMode 全量（regression5）与最新 MVO5 运行（cloud-v7）均绿。
- 残余备注（事实登记，不阻塞）：EditMode 末次运行为 21:24 r2（211/211），早于 23:0x 两项缺陷修复；两项修复均为表现层（Main.unity cloudSprites 块 / DecorationRenderer.Update 写回），EditMode 套件不含装饰渲染断言（SortingOrdersTests 仅断言常量），风险极低，如实登记。
- **SHA256SUMS.txt 重新生成**：`find . -type f ! -name SHA256SUMS.txt | sort | xargs sha256sum` → **268 行 = 268 文件**（较上版 +29：probe log×9 + probe XML×4 + cloud-v{2..7} XML+log×12 + regression4/5 XML+log×4），`sha256sum -c` 全量回验 **268 OK / 0 失败**。
- 批次指纹（字面单行管道 `find . -type f ! -name SHA256SUMS.txt | sort | sha256sum`）：`3f03daa8c586f0476c9aba9928ede895bedfdcd48067d6f81f8aa59f5e9210fe`。
