# TV-02 证据包：地形叠层、树木渲染与整体画面修复

- **任务**：`docs/impl/TV-02-地形叠层与树木渲染修复.md`
- **场景**：`Assets/_Project/Scenes/Main.unity`（真实 Play Mode，非 Prefab 摆拍）
- **种子**：`20261013`（场景内 `worldSeed` 固定值）；**地图**：128×128
- **捕获方式**：PlayMode 测试（`Assets/Tests/PlayMode/TerrainCompositionEvidenceTests.cs`）内
  `ScreenCapture.CaptureScreenshot`，Game View 设为真实分辨率；manifest 记录**实测**
  `Screen.width/height`、相机位置与 orthoSize。近景 = 把真实相机移到真实地图坐标（数据驱动：
  树位来自真实 `ResourceNodes` 池，岸线/湖心来自真实地形数据）。
- **快速审阅**：`review-contact-sheet-terrain.svg`（地形与水面）与 `review-contact-sheet-trees.svg`
  （树木渲染与遮挡，TV-02-R1 修正后）——两份均为自包含 JPEG 内嵌（各 ≤700KB，外部工具可直接读取）；
  旧版全量联系表 `review-contact-sheet.svg`（~2.2MB）保留作存档，外部读取建议用拆分版。

## 文件清单

| 文件 | 内容 |
| --- | --- |
| `hud_1280x720.png` / `hud_1920x1080.png` / `hud_2560x1440.png` | 三档分辨率 HUD 全景（文件名 = 实测 Screen 尺寸） |
| `tree-closeup-conifer.png` | 深绿针叶树近景（`Tree1_0`，ortho 4）：完整树冠与树干，根部落在所属格 |
| `tree-closeup-birch.png` | 黄桦近景（`Tree4_0`，ortho 4） |
| `tree-overlap-closeup.png` | 前后遮挡近景：相邻两行针叶树，**南树（下方）正确遮挡北树干根**（TV-02-R1 修正后重拍；排序方向由 `TreeOverlapRenderTests` 真实渲染像素对比回归锁定） |
| `coast-transition-closeup.png` | 水岸过渡：泡沫贴岸、草地/森林九宫格绒毛、岩壁带（含烘焙水影）沿岸成段 |
| `foam-anim-a.png` / `foam-anim-b.png` | 同机位间隔 2 秒连拍：泡沫方块"呼吸"（出现/消散），证明动画在播且 4 相位错帧 |
| `lake-center-closeup.png` | 湖心与开阔水面：主体为连续纯色水面，无泡沫方块 |
| `event-panel-empty.png` | 事件面板空状态「暂无事件」（`log.empty` 键，1:1 像素裁剪自 `hud_1920x1080.png`，未缩放） |
| `sprite-audit-trees.png` | 树木子精灵核对图：重切后 16 片全部为**完整树**（Tree1 = 针叶树 8×192×256 根枢轴 0.0625；Tree4 = 黄桦 8×192×192 根枢轴 0.1302），逐片来自 `sprite-dump/`（GPU blit 1:1 导出） |
| `sprite-dump/` | 16 个子精灵 1:1 导出 + `capture-manifest.txt` 尾部含逐片 rect/pivot 元数据 |
| `capture-manifest.txt` | 逐张截图元数据（实测分辨率 / 种子 / 地图 / 相机 / 涉及 Sprite / 捕获方式） |
| `captures-before/` | **修复前/过程中**证据：TV-02 基线截图（`../../IMPL-14/shot-gamehud-1920.png`，本地文件，已内嵌于联系表末行）的放大裁剪（泡沫点阵、树残片、空面板）与泡沫帧逐帧分析图 |
| `review-contact-sheet.svg` | 自包含审阅联系表（内嵌全部截图缩略图） |
| `*.log` / `*-testresults.xml` | 本轮 Unity 批处理日志与测试结果（EditMode 124/124、PlayMode 19/19 通过；evidence 捕获运行日志） |

## 修复后画面验收口径（对照任务 §六）

1. 湖泊/开阔水面主体连续清晰，无独立浅色方块；泡沫仅出现在紧贴岸线的水格带，且随动画呼吸断续。
2. 地图内部无树木残片；两种完整树木可见，树根落在所属格地面，树冠自然上展 2~3 格。
3. 相邻行树木呈"南遮北"自然遮挡；树与地面/岩壁/阴影叠放关系正确。
4. 事件面板空态显示「暂无事件」，随语言切换（EditMode 测试覆盖中英文与真实事件三种情况）。

## 复现

```bash
"D:/software/Unity Hub/Editor/6000.6.5f1/Editor/Unity.exe" -batchmode \
  -projectPath <项目根> -runTests -testPlatform PlayMode \
  -testFilter WorldSandbox.Tests.PlayMode.TerrainCompositionEvidenceTests \
  -testResults <out>.xml -logFile <out>.log
```

捕获后需以 `sprite-audit-trees.png` 一节脚本方式重建核对图/联系表（见任务实施文档 §7）。
