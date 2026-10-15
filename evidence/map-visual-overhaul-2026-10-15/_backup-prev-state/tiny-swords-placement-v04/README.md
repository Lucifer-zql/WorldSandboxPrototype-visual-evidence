# TV-02-R3 证据包：Tiny Swords 全素材摆放规则落地

- **任务**：全素材盘点 → 权威规范 + 素材矩阵 → 修复 → 全量回归 → 真实画面（本目录）
- **场景**：`Assets/_Project/Scenes/Main.unity`（真实 Play Mode）；**种子** `20261013`；**地图** 128×128
- **世界一致性**：三档 HUD 同一世界——Terrain 逐格 FNV-1a 哈希 `terrainHash` 记录于 manifest 并逐档断言一致
- **捕获方式**：PlayMode 测试（`TinySwordsPlacementEvidenceTests`）+ `ScreenCapture.CaptureScreenshot`；
  近景 = 真实相机移至真实地图坐标；**树桩** = 资源耗尽态渲染路径验证（`ResourceNodeStore.TryUpdate`
  RemainingHits=0 + RefreshTile，D-11；**非玩家砍树交互验收**）；**云** = 漂移两帧（位移动画未接入，素材矩阵 §5）
- **快速审阅**：`review-contact-sheet-terrain.svg` / `review-contact-sheet-objects.svg`（自包含 JPEG，≤700KB）

## 文件清单

| 文件 | 内容 |
| --- | --- |
| `hud_1280x720/1920x1080/2560x1440.png` | 三档 HUD 全景（同世界哈希） |
| `water-foam-coast-closeup.png` | 水岸泡沫：16×192² 跨格画布、1/3 相位密度、贴岸 |
| `island-shore-closeup.png` | 岛岸（四邻域全水的孤立陆地） |
| `lake-center-closeup.png` | 湖心开阔水面（无泡沫） |
| `narrow-waterway-closeup.png` | 窄水道（渠心保留水色） |
| `grass-forest-edge-closeup.png` | 草林交界（9 槽 NotThis 过渡） |
| `cliff-elevated-ground-closeup.png` | 崖段（ElevatedTerrain 160） |
| `shadow-overlap-closeup.png` | 树投影（画布下移一格） |
| `tree-closeup-conifer/birch.png` | 两种完整树木（根枢轴落地） |
| `tree-overlap-closeup.png` | 前后遮挡（南遮北） |
| `stump-ground-contact.png` | 树桩（资源耗尽态渲染路径验证；画面中心枢轴=格心） |
| `bush-ground-contact.png` / `ground-rock-placement.png` / `water-rock-placement.png` | 装饰接地/摆位 |
| `cloud-drift-a/b.png` + `cloud-animation-frames.png` | 云漂移两帧与并排合成（合成仅并排展示，未改像素） |
| `sprite-audit-terrain/water-foam/trees/decorations.png` | 素材核对图（Alpha 棋盘底 + 64px 网格线 + 枢轴十字 + 帧号；权威副本在 `../tiny-swords-asset-audit/`） |
| `capture-manifest.txt` | 全部截图元数据（实测分辨率/哈希/相机/对象坐标/Sprite 名/捕获方式） |
| `editmode/playmode-testresults.xml` | 全量测试结果（EditMode 133/133、PlayMode 22/22，零错误零警告） |

## 泡沫密度调优前后对比（项目约定，官方未定密度）

- TV-02-R2 首轮：全岸覆盖 → 泡沫连毯（证据：v03 目录 captures-before 及任务记录）
- TV-02-R3 初版：1/2 棋盘 → 破碎水域仍偏密（本轮 island-shore 首拍，记录于任务过程）
- **本轮定稿：1/3 相位 `(x+2y)≡0 (mod 3)`** → 任意两块间距 ≥2 格，水面主体清晰（本目录全部截图）
