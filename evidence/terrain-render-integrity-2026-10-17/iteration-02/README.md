# iteration-02 —— P1 修复「中间版本 v1」证据批（保留作为迭代过程留档）

> **说明**：本目录是 D-065 iter-02 修复的**第一版（v1）**采集结果（seed-20261013 / seed-20261014，
> 2026-10-17 上午），按任务书"保留所有中间证据"要求原样留档，**不是最终交付版本**。
> 最终交付见同级 **`iteration-02-final/`**。

## v1 修复内容（已被 v2 取代）

ApplyCell 改为"陆地格底色层铺 grassTile/forestTile（RuleTile 本身）"。

## v1 复验结果（seed-20261013 TS1b 像素级扫描对照）

| 版本 | 数据非水格含水色像素（总数 / 深处内陆） |
| --- | --- |
| v0（未修复，iteration-00） | 226 / 146 |
| **v1（本目录）** | **103 / 57** |
| v2（iteration-02-final） | 见 final 批 |

## v1 的残余根因（→ v2 的修复动机）

底色层铺 **RuleTile 本身**时，RuleTile 在背景层**仍按邻接选片**——"邻林/邻水的陆地格"在背景层
同样会选**透明边缘片**，其透明像素继续露出相机背景水色。v2 改为"**显式完整片**"（RuleTile
m_Id=5 槽 sprite 的独立 Tile，实测 0% 透明）并将规则提取失败时的回退限定为原行为。

## 本目录其他内容

- `seed-20261013/`、`seed-20261014/`：v1 版三种子截图中前两种子（各含 test-results.xml、
  test-run.log——本轮起采集命令已带 -testResults/-logFile 持久化，9 passed + 1 skipped + 0 failed）。
- v1 的 EditMode 证据（首次引入 Render_BackgroundUnderlay 契约测试）见主仓库
  `docs/impl/parallel-repair/d065i2-editmode-p1.xml`。
- 根因判定全文见 `../iteration-02-final/analysis/p1-root-cause.md`。
