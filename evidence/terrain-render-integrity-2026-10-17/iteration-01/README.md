# iteration-01 —— 陆地内部水色缝隙 · 专项审计与根因判定（零修复轮）

> 采集：2026-10-17 08:25~08:29｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染｜1280×720
> 子目录：seed-20261013（30 图）/ seed-20261014（33 图）/ seed-20261015（33 图）+ 逐目录 capture-manifest
> **生产路径零改动**（7 关键生产文件 SHA-256 与 iteration-00 逐位相同，见 `git-state.txt`）。

## 一、核心结论（先读）

对三种子"陆地内部水色缝隙/空洞/非预期水域"的全图数据扫描 + 逐格渲染一致性对照 + 与基线
（iteration-00）的全像素稳定性对照后，**本轮未发现需要修复的生产缺陷，修复清单为空**。逐项判定：

| 制作人可能观察到的现象 | 判定（A~F） | 证据 |
| --- | --- | --- |
| ① 细长/矩形感内湖（如 seed13 11×27 竖条、seed14 梯形湖） | **A 合法**——设计契约"内陆湖泊由噪声场低洼盆地自然产生"；长直边=value noise 双线性插值+分位数截断数学常态（全场海岸线直段长尾达 35~72 格，湖直边 2~19 格同族） | `analysis/inland-lake-inventory.txt`、六湖格线图、`analysis/water-audit-stats-*` |
| ② 散落于陆地上的青蓝色小块状像素 | **非水域**——全图扫描中属"数据水域"之外的青色簇 100% 为**针叶树冠**色（45,131,129 / 55,174,122 / 22,28,46），无一由水主色（71,171,168）构成；低缩放下易误认为水色 | `analysis/tree-vs-water-color-contrast.png` |
| ③ 贴地图边缘的 1 格宽竖直水带（seed13 x=0, y17~60 等） | **A 类变体（登记不改）**——世界边缘圈水（R-MAP-12）+ 距边衰减参数的自然形态；视觉上与地图外的外海连续 | `analysis/edge-strip-seed13-left.png` |
| ④ 陆地格"露底"显示水色 / 水域被"填平" | **不存在**——逐格对照 extra=0（三种子）：画面水色像素 100% 受数据水格支配 | `analysis/render-consistency-diff-iteration01.txt` |
| ⑤ 数据水但画面非水的格（61/36/74 个/种子） | 全部为**设计内覆盖**：云（漂移）、底部 UI 提示条、崖壁入水件（半透明）、贴岸泡沫、树冠悬垂、岸边黄装饰 | 同上（含分类计数与代表坐标） |

**渲染一致性对照方法**：离线逐位复算数据水格 mask（与 Unity 实跑水格数互证，见
`analysis/offline-recompute-verification.txt`）与 TS1b 截图逐格比对（每格 5.625px 区域、纯水色
占比>50% 判水）：**extra（画面水∧数据非水）= 0**；missing 全部归因为覆盖物。

## 二、采集参数与产出

- 命令：`WS_WORLD_SEED=<种子> MVO_CAPTURE_MODE=tri-iteration-01 <Unity> -runTests -testPlatform PlayMode -testFilter ...MapVisualOverhaulCaptureTests`（串行单实例；运行取证与局限见 `run-verification.txt`）。
- 图名集合与 iteration-00 **逐名一致**（30/33/33；seed13 缺 TS2c/TS2d/vertwall 系该种子无对应实例，程序跳过，与基线相同）。
- 记录三件套：`capture-manifest.txt`（逐拍 stat 行 + 末行 backfill）、`git-state.txt`、`git-status-porcelain.txt`（397 行）。

## 三、分析材料索引（`analysis/`）

| 文件 | 用途 |
| --- | --- |
| `inland-lake-inventory.txt` | **六处内陆水域清单**（bbox/面积/直边特征/判定）+ 数学基准 |
| `offline-recompute-verification.txt` | 离线逐位复算与 Unity 实跑互证（水格数 5388/3605/4324 逐位一致） |
| `render-consistency-diff-iteration01.txt` | 逐格渲染一致性对照（extra=0；missing 分类与代表坐标） |
| `stability-iteration00-vs-01.txt` | 00↔01 全像素稳定性（69/96 图逐字节一致；差异全部=动画相位，无结构差异） |
| `water-audit-stats-seed-*.txt` | 三种子数据侧全量统计（分量/内湖/窄点/直段分布） |
| `water-mask-seed-*.txt` | 数据水格 mask（128×128，逐格渲染对照的输入） |
| `lake-A..F / edge-strip / tree-vs-water-color-contrast.png` | 六湖与边缘水道格线图（可读格坐标）+ 树冠/水色对比 |
| `tools/` | 全部审计工具与离线复算源码（可独立复验；见其 README） |

## 四、能/不能证明

- **能证明**：三种子全部内陆地物的数据形态、位置的逐格事实；画面水色与数据水格的格级一致性（零多画）；
  六湖合法性（噪声契约 + 直边分布对照）；iteration-00→01 无结构差异（70+ 图逐字节一致，其余差异全为动画相位）。
- **不能证明**：其他种子的水况（本轮仅三种子）；亚像素级一致性（对照为格级）；
  "内湖观感是否符合制作人预期"（观感判断权在制作人；如认为细长湖/边缘窄水带需要观感优化，
  那属于**水陆/衰减参数调优**——当前参数冻结，可作为后续独立任务提出）。

## 五、测试与安全

- **EditMode 303/303 全过**（含本轮新增审计套件 `D065I1_*` 15 用例：水格数/内湖清单快照、边缘圈全水、
  无 <3 格水分量、水比带；XML/日志：主仓库 `docs/impl/parallel-repair/d065i1-editmode-water.xml|.log`）。
- PlayMode 三发退出码 0、产出完整（详见 `run-verification.txt` 的局限声明——本轮未带 -testResults/-logFile，已列修正措施）。
- 历史证据采前/采后 **2109/2109 零覆盖**（基线 `d065i1-capture-pre.sha256.txt`）；
  捕获门禁（白名单仅本批次根、模式仅 `tri-`、文件已存在拒绝）全程生效。
- SHA-256：`SHA256SUMS.txt`（本目录全文件）。

## 六、修复清单

**空。** 六处内陆水域与全部疑点均登记保留（详见 `analysis/inland-lake-inventory.txt` 判定列）。
