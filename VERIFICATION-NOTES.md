# 发布前核查说明（派生材料）

本文件与 `SHA256SUMS.txt` 均由 2026-10-15 发布流程在上传前生成，属于**派生索引材料**，非原始证据。原始证据文件（PNG / MD / XML）均按原样保留，未做任何修改、重绘或替换。

## 一、本地结构与完整性核查（发布前）

对主项目内批次目录 `docs/impl/evidence/independent-visual-verification-2026-10-15/` 逐文件核查：

1. **文件清点**：39 张分类原始 PNG 全部存在；`capture-manifest.md`、`results.md`、`overview.png` 均存在。另将 `results.md` 所引用的 3 份原始测试 XML（final5-em / final5-pm / vv-pm7）纳入发布（`test-results/`，经敏感信息检查，无凭据类内容）。
2. **PNG 结构校验（逐文件）**：文件签名、全部 chunk CRC32、IDAT zlib 解码、解码字节数与宽高严格一致、IEND 后无尾部字节 —— **40/40 全部通过，无截断、无损坏**。
3. **尺寸**：39 张原始图为 1280×720；`overview.png` 为 1280×1440 总览拼图。色彩类型均为 RGB。
4. **复制核查**：复制到分类目录（`images/A–H`）前后逐文件 SHA-256 一致；`SHA256SUMS.txt` 由副本生成，与原始文件哈希逐一相同。
5. **抽查目检**：A2、E0（与 E1 对照）、E2、overview 的画面与清单描述相符（真实 GameView 画面，含完整游戏 UI）。

## 二、已知问题（如实记录，原始文件未删改）

### 1. E2 与 E3-t0 字节级重复

`images/E-creatures/E2-creatures-closeup.png` 与 `images/E-creatures/E3-idle-anim-t0.png` 的 SHA-256 完全相同（`5366b6c1aa6d3fff...`，完整值见 SHA256SUMS.txt），即两文件为同一字节内容。

- 影响：E3 的 t0 帧与 E2 特写图**无法互为独立佐证**（捕获清单记载两者镜头不同：E2 ortho=3、E3 ortho=6，实际字节却相同）。
- 处理：两文件均**按原样保留发布**，未删除、未替换。请制作人/复核者知悉并对该项捕获复核。

### 2. `cam-*.png` 辅助轨未包含

捕获清单"捕获环境与可靠性"提及辅助轨 `cam-*.png`（`cam.Render`+`ReadPixels` 直渲染帧）。该辅助轨文件**不在原始证据目录中**（发布前全项目检索亦未发现），未包含在本次发布。本批 39 张均为主证据轨（GameView ScreenCapture）。

### 3. results.md 测试计数与原始 XML 存在差异

| 套件 | results.md 记载 | 原始 XML 实际 | 说明 |
|---|---|---|---|
| EditMode 全量 | 178/178 | **177/177（全通过）** — final5-em.xml | 差异 1 例，计数口径待复核 |
| PlayMode 全量 | 31/31 | **39/39（全通过）** — final5-pm.xml | 实际已含 VisualVerificationTests 8 例 |
| VisualVerificationTests | 8/8（39 图捕获） | **8/8** — vv-pm7.xml | 一致 |

两者均为**全部通过**，差异属计数口径问题。原始 XML 已随本仓库发布，**以其为准**；建议复核 `results.md` 汇总数字。

## 三、发布范围声明

- 本仓库仅含证据材料；不包含游戏源码、Scene、Prefab、ScriptableObject、Packages、ProjectSettings 或主项目工作区内容。
- 仓库使用 `.gitattributes`（`* -text`）禁用换行符转换，保证仓库内文件字节与原始证据完全一致，克隆后可直接 `sha256sum -c SHA256SUMS.txt` 校验。
- 全部发布操作（复制、提交、推送）在独立临时目录与独立公开仓库完成；未对主游戏仓库执行任何 Git 写操作。
- 本文件为派生说明材料，不构成对证据结论的替代；证据结论请以原始 PNG、`capture-manifest.md`、`results.md` 与 `test-results/` 为准。
