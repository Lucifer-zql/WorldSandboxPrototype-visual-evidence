# 分析口径说明

- `t1200-before-top-after-bottom-2x.png`：r5 批次 `screenshots/07-world-t1200.png` 与本批同名帧，
  同裁剪区间 (350,150)-(1350,750)，上下拼接后 2x NEAREST 放大；两批同种子 20261013、同机位（audit 测试固定构图）。
- `before-castle-area-3x.png` / `after-castle-area-3x.png`：同帧裁剪 (450,180)-(1150,700) 3x NEAREST。
  before 中可见 ~15 人 4×4 方阵（全员同目标）且除城堡外 0 民居；after 中民居环带散布、村民自然分布。
- `after-village-wide-2x.png`：本批 07 帧裁剪 (300,100)-(1400,800) 2x，展示村落广角（民居 + 分散村民）。
- `after-eventlog-3x.png`：本批 15 帧事件面板裁剪 3x，展示带文明名的出生事件与"树已被采尽"文案。
- 截图为运行中捕获帧，非逐帧确定性渲染；对照只用于"行为/可见性前后差异"判据。
