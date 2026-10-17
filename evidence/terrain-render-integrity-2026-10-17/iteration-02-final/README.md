# iteration-02-final —— D-065 全地图地表覆盖与地形素材拼接综合整改（最终批）

> 采集：2026-10-17｜Unity 6000.6.5f1 / URP 2D｜窗口化真实渲染｜1280×720｜三种子 seed-20261013/14/15
> **修复**：P1（陆地内部水色缝隙）——唯一生产修复；P2/P3/P4 经三方审计判定（证据支持的非缺陷/无缺陷）。
> 唯一生产改动文件：`TerrainTilemapRenderer.cs`（底色层"显式完整片"垫底 + 契约测试 1 例）。

## 一、十项结论速览（对应任务书 §十）

1. **水色缝隙根因（已修复）**：主层"林界流苏/自身边片"（槽位实测透明 3.1%~49.8%）的**透明像素
   露出下层**——下层=WaterBackground 层（陆地格原为 null）→ **相机背景水色**。**非**"件的图形含水色"
   （全素材水色像素实测=0，唯一水色源为水背景/clear color）。**任务书假设（边缘素材透明区露出底层）
   成立**，修复=陆地格底色铺"显式完整片"（0% 透明）。
   复核：**226/146 → 103/57 → 48/29（每格 ≤3px，占像素 <0.01%）**——残余为装饰边缘图案与水色值的
   巧合（非露底，通道已堵死）。
2. **深绿色地块**：= elevation=2 Mount 顶面（color3 色板 (95,171,98)；数据/素材/文档"五色表"三方互证）；
   "突兀"=色阶硬切（素材包无"高地间边缘件"）+ 色相跳变——**证据支持的非缺陷（交验）**。见 `analysis/p2-mount-verdict.md`。
3. **三角/坡面/台阶**：计划（离线级联复算与 Unity 逐位互证：walls/slopes/stairs 三种子全对）→
   语义链（枚举注释/渲染映射/素材文档）三方自洽 + 切片叠加画面实证；**无缺陷**。见 `analysis/p3p4-verification.md`。
4. **其他覆盖层（P4）**：阴影/泡沫/林缘/云逐项复核无新缺陷；刷新链（FxCommandBus+8 邻口径）完备，
   当前无地形编辑入口（"笔刷未同步"无可达路径）。
5. 代码差异：`run-verification.txt`（含理由与回退保护）。
6. 全图扫描与规则矩阵：见 `analysis/`（mask/terrain/elev/plan 全链数据 + stats2/stats3 + 色阶表）。
7. 测试与日志：`test-evidence/`（EditMode 304/304 final；PlayMode 三种子 XML/log 在各 seed 目录）。
8. D-061~64 回归：EditMode 304/304 含全部既有用例；生产文件相对 iter-00/01 仅 TerrainTilemapRenderer
   的上述改动（git-state 指纹对照）。
9. 历史保护：采前/采后 **2312/2312 零覆盖**；主仓库零 git 写（见 `git-state.txt`）。
10. 分类：**已修复**=P1；**证据支持的非缺陷**=P2（Mount 色阶）、P3 波浪描边低缩放观感、P4 全部；
    **未修复**= 无（P1 残余 <0.01% 像素为不可见级，如需彻底归零见 continuation 说明）；**证据不足**=无。

## 二、修复前后关键对照（同机位）

- `analysis/beforeafter-P1-seed14-31-53.png`：seed14 草林边界带——v0 青色块多处 vs v2 全部消失
  （仅余白/深色流苏）。
- 全图：`seed-*/TS1a/TS1b`（v2）与 `../iteration-00-baseline/` 对照（差异=修复处+动画相位）。

## 三、分析材料（`analysis/`）

`p1-root-cause.md`（根因+v0/v1/v2 迭代）、`p2-mount-verdict.md`、`p3p4-verification.md`、
`p1-verification-scan.txt`（三版扫描对照）、`terrain-palette-map.txt`（色阶表）、
全链数据（`terrain-/elev-/plan-seed-*.txt` + `stats2/stats3-seed-*.txt`）、
`tools/offline-recompute/`（离线级联复算源码——与 Unity 实跑逐位互证：water/hill/mount/forest/walls/slopes/stairs 全对）。

## 四、局限性（如实声明）

1. P1 残余 48/35/63 格（单格 ≤3px、深处内陆 29/29/23）——形态=装饰素材边缘图案像素与水色值的巧合，
   视觉不可见级；如需 0 归零可另开"素材精确匹配"专项。
2. 格级/像素级扫描口径的敏感度：覆盖物边缘 ±1 像素。
3. A/B 多 Agent 并行调查（素材/调用链）因运行超时未产出完整报告，其问题范围的结论由主 Agent
   直接完成并留证（素材逐片实测、调用链逐行定位、离线级联复算）。
