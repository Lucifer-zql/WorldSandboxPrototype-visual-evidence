# P1 · 内陆地表以透明边缘件替代基础地表（D-065 iter-03 整改）

> 制作人要求（原文要点）：**"不得把海岸专用素材、仅用于外轮廓的流苏件或透明边缘件无条件用于内陆"**；
> **"基础地表必须始终完整覆盖所有应有地表的陆地格。透明的装饰或边缘素材应在正确的基础地表之上绘制，
> 不得以含透明区域的边缘件直接代替基础地表"**；**"不能只保留垫底方案，却让内陆仍然调用错误的海岸边缘素材"**。

## 一、问题与根因（承接 iter-02）

- iter-02 定位：地形主层存在"件替换基础片"路径——草地格贴森林时主层直接铺**林界流苏件**、
  森林格暴露缘主层直接铺**森林自身边件**（均为带透明区的边缘件，槽位实测透明 3.1%~49.8%）。
- iter-02 的修复（v2）只在 **WaterBackground(−100)** 层给陆地格垫了"显式完整片"（防露相机背景水色）——
  **但主层本身仍是"件"**：基础地表并未"始终完整覆盖"，架构上仍是"透明边缘件直接代替基础地表"。
- iter-03 整改目标：把"件"从地形主层**迁出**，使主层恒为基础地表。

## 二、修复实现（生产代码）

**文件**：`Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs`、`SortingOrders.cs`

1. **新增层 `ForestEdgeOverlay`（04 R-LAYER-07a，Order in Layer = 10）**——
   `EnsureTilemap` 自建子对象；`RebuildCore` 全量清铺；公开属性 `ForestEdgeTilemap`。
2. **`ApplyCell` 主层重构**：
   - **件格（草贴森林 / 森林暴露缘，含森林右族流苏）**：主层铺**显式完整片**
     （`ExtractUnderlayTile` = RuleTile m_Id=5 槽 sprite，实测 0% 透明——与 iter-02 底色同源）；
     件改铺 `ForestEdgeOverlay` 层。
   - **非件格**：主层维持 RuleTile 直铺（邻接自拼，既有行为不变）；件层为 null。
   - 水格/旧 Cliff 格：件层为 null（件仅陆地语义）。
3. **底色层垫底保留**（iter-02 v2 机制不回退，双保险）。

**逐像素等价性设计**：v2 的合成 = 底色完整片 + 主层件；v3 的合成 = 底色完整片 + 主层完整片 + 件层件。
两层完整片同源同色，件的不透明/透明像素合成路径完全一致 ⇒ **件格渲染输出与 v2 逐像素等价**
（P1 不改变任何画面像素——这是设计目标，见下验证 ③）。

## 三、验证

### ① EditMode 契约测试（真实 Unity，318/318 全过，见 ../regression-final/test-evidence/）
`CliffTileSelectionTests.Render_ForestEdge_OnOverlayLayer_MainLayerAlwaysCompleteBase_Iter03P1`：
- 件格主层 = 显式完整片（m_Id=5，Is.SameAs 断言）；**不是**件、**不是** RuleTile 直铺
- 件格件层 = 流苏件（显式 Tile、sprite ≠ 完整片）
- 件层 order = 10（Terrain 之上、Decoration 之下）
- 非件格：主层 = RuleTile 直铺、件层 null
- 水格件层 null

### ② 画面级验证（真实 Unity 截图对照）
- `before-TS4a-seed15.png`（iteration-02-final，P1 修改前）vs `after-TS4a-seed15.png`（iteration-03，修改后）
- `pixel-diff-TS4a-02final-vs-03.txt`：全图显著差异 **609 像素（0.09%）**，全部集中于
  **P4 被移除物（鸭子簇 607px @(999,501)——打标图实证为真鸭子）**；**件格区域零像素差异**——
  实测确认"件层迁移不改变任何像素"的设计等价性。
- （过程留档：P4 曾有 v1 中间迭代态产生散布流位微扰，已由 v2 精修恢复逐位一致——见 P4 analysis
  §二；v1 批次不属发布面。）

### ③ 回归
- EditMode 全量 318/318（含全部既有用例与 iter-02 P1 底色契约用例）；
- MapVisualOverhaulCaptureTests 三种子 9 passed + 1 skipped ×3（PlayMode 截图套件）；
- PlayMode 生产文件相对 iter-02 仅上表 2 文件（见 ../README.md §五）。

## 四、如实声明
- 件层格分布审计（"件格"全图计数）由对账测试间接覆盖（件层格数与主层"位"存在性一致性的直接审计
  未单列断言——主层完整片存在性已由 ①③ 覆盖）。
- `ExtractUnderlayTile` 取 m_Id=5 槽为 iter-02 既定实现（实测 0% 透明），其"完整覆盖"属性以
  像素实测（0% 透明）为据，非槽位语义推断（该槽在 RuleTile 中是"西边片"规则位，但像素全不透明）。
