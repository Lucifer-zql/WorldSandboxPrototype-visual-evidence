# iteration-06 —— D-065 内陆接缝/高地矩形/官方组合/资源大小 验收整改

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染 1280×720｜三种子 20261013/14/15
> 本轮依据官方素材示意图与 Tilemap Guide 逐项核查；**核心新发现与修复**：高地顶面的"矩形色块"
> 经数据实证为**真实 Mount(elev2) 区**（A 情形），而它与 Hill(elev1) 的 **1 级高差在三面只有
> 色板切换、没有任何连接结构**——已修复生产代码。

## 一、目录

```
iteration-06/
  P1-inland-color-boundaries/      # 内陆水色系扫描（0/0/0）+ 前后图 + 逐类边界判定表
  P2-mount-rectangle-investigation/# 矩形来源数据实证（A 情形）+ 修复前后同机位对照 + analysis
  P3-official-terrain-assembly/    # 官方规则→代码映射表 + 8 类组合近景(TS6c) + 数据验证
  P4-water-rock-regression/        # 禁石回归（数据+断言+扫描）
  P5-resource-size-variants/       # 清单双口径分级 + 变体分布 + TS6d 真实画面（9 图×3 种子）
  regression-final/
    seed-20261013|14|15/           # 本批全量截图（39/51/51 张）+XML/log
    test-evidence/                 # EditMode 328/328（r1 终版）
  README.md / SHA256SUMS.txt
```

## 二、结论速览

1. **P2（已修复——本轮核心）**：矩形色块来源=**A 情形**（seed14 高程实测：x24-26,y4-5=elev2
   Mount 区；x22-28,y6-7=elev1 Hill；y3=elev0 低地）。**缺陷**：Mount↔Hill 的 1 级高差在
   北/东/西三面只有 color3/color1 色板切换、**无任何顶面边缘/连接结构**（仅南向 2 级直落有墙）。
   **修复**（`TerrainTilemapRenderer.cs`）：顶面暴露判定由"邻位 elev ≥ 1"改为"**邻位 elev ≥
   本格 elev**"（任何降级边缘绘制顶面边缘件；选片族规则不变；未引入任何贴花；色阶保留）。
   **证据**：同机位前后对照（`before-iter05-rectangle-seed14.png` 纯色直边 →
   `after-iter06-edge-connected-seed14.png` 完整右族草缘边件）；新契约
   `Render_Elevated_OneLevelDropShowsEdgeTiles_Iter06P2`。
2. **P1（三层验证）**：修 P2 后"不同颜色地表交界"（Hill↔Mount）均具备官方连接结构；草↔林由
   右族流苏（0% 浅青白）承担；**iter-06 内陆水色系扫描 0/0/0**（纯水色+浅青白双口径）；
   逐类边界判定表见 analysis（含"同高度不同颜色"的实际配置说明：当前统一色板，无同高异色块）。
3. **P3（映射表+8 组合近景）**：官方规则→项目实现的逐项映射表（analysis）；新增 `TS6c` 8 类
   组合近景（左/右坡、台阶、台阶邻墙、墙内外角、水脚、陆脚——seed14 全 8 张）；数据验证：
   墙脚族↔南邻 0 不一致、坡向 0 违规、地形合法性 0 违规、槽位对账全过。P3 方向**无生产改动**
   （本轮相关修复即 P2 顶面规则——如实）。
4. **P4（保持通过）**：未改 ScatterStep；离线 waterRock=0、陆岩落水=0、meat=0（duck 色=金矿误报）；
   断言全过；地形 Tile 内嵌石纹保留。
5. **P5（清单校准+真实画面）**：统一绝对阈值（小<40px/中40-79/大≥80）+ 类内相对双口径；金矿
   1-6=小/小/中/中/大/大（修正原"58×52 标小"）；树 190/122；岩石 26-42（3 小 1 中 + 类内分级）。
   **TS6d 真实画面**（三种子各 9 图）：金矿三档特写（0.5 格 vs 1.5 格差异清晰）、两树种、四岩石。

## 三、修复代码差异（相对 iteration-05）

| 文件 | 改动 |
| --- | --- |
| `Assets/_Project/Scripts/Presentation/TerrainTilemapRenderer.cs` | P2：`IsElevatedAtLeast` + `ResolveElevatedTopTile` 暴露判定改"邻位 elev < 本格 elev"；删死的 `IsElevatedAt` |
| `Assets/Tests/EditMode/CliffTileSelectionTests.cs` | 新增 iter-06 P2 契约（1 级降边） |
| `Assets/Tests/PlayMode/MapVisualOverhaulCaptureTests.cs` | 新增 TS6c（8 组合近景）/TS6d（资源大小验收） |

（其余全部为证据/文档；P4 零改动；历史批与 Derived 素材原样保留。）

## 四、回归与保护
- **EditMode 328/328 全过**；PlayMode 截图套件三种子 12/13/13 total、11/12/12 passed（1 skipped=机位未命中）；
  TS6d 补采三种子各 9 图（独立目录 `P5-resource-size-variants/TS6d-fixed/`，含过程批次 TS6d-run1 留档）。
- 历史证据（iter-00~05）采前/采后 `sha256sum -c` 零覆盖；本批写盘全经 CaptureWriteGuard；
  主仓库零 git 写。

## 五、局限与如实声明
1. P1 的"无法复现水色缝隙"以三层验证为据——不否定制作人观察，请求具体截图/坐标；
2. P3 未改生产（四层验证含官方映射表未发现需修复项）；
3. P5 的大小分级按 alpha bbox 实测与统一阈值，不虚构清单外变体；
4. TS6d 首次运行（修复前版本）因 sprite 名不匹配（Tree1 vs Tree1_0）仅产出 7 图并因 guard
   拒绝覆盖而失败——过程留档 `TS6d-run1/`；修复版（含 inactive 池+前缀匹配）三种子全过。
