# D-068 r3 全量 PlayMode 失败逐条分类

> 原始 XML：playmode-full.xml；被测工作树：主仓库未提交工作树（Unity 6000.6.5f1）。
> 本文件只做分类，**不修改任何断言、不关闭任何守卫、不把失败写成通过**。

## 汇总

- 总数 **109**，通过 **37**，失败 **72**。
- legacy_capture_guard：**33**
- capture_env_precondition：**9**
- scene_setup_null：**28**
- test_order_isolation：**1**
- legacy_scene_state：**1**

## 分类说明

- legacy_capture_guard：历史取证套件写死 docs/impl/evidence/... 或未设置 MVO_CAPTURE_MODE，被 CaptureWriteGuard 失败关闭；属于环境前置，不是产品功能失败。
- capture_env_precondition：D067_SHOT_DIR / D068_SHOT_DIR / D068_FINAL_SHOT_DIR 未设置，OneTimeSetUp 明确拒绝。
- scene_setup_null：旧用例假设 GameBootstrap.WireWorld / MapReader / SimClock / TerrainTilemapRenderer.CurrentPlan 已由前置用例装配；全量批次顺序下为空。单独运行本类 D-068 用例不受影响。
- d067_stale_selector：D067_CaptureEnglishScreens 在写完 8 张英文截图后，仍按旧控件名 Btn_Done 找关闭按钮；当前设置页关闭按钮的 GameObject 名为本地化标签（Btn_完成），故用例在收尾处失败。截图 38 已生成，产品按钮可点；未改旧用例。
- test_order_isolation：R3_EnglishFlow... 单独运行 1/1 通过，全量批次中场景未在 3000 帧内就绪而取到 null；属用例隔离问题。
- batchmode_screencapture：D067_FullFlow... 的 640×480 ScreenCapture 在 batchmode 不落盘；D-067 §8 已登记。
- legacy_scene_state：F_BuildingsFx 期望 4 建筑但得到 0；该旧套件依赖自建场景状态，本批未单独复现出产品建筑缺失（D-068 角色/建筑用例与截图均可见房屋）。**待逐条单独复核，不计入通过**。
- other_pending_triage：其余未归类断言，逐条列在下方，未做任何“通过”声明。

## 逐条失败

### legacy_capture_guard（33）

- **Buildings_PlacedRendered_WithFxLifecycle** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/buildings-fx" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CaptureHudAtThreeResolutions** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-terrain-integration/camera-initial-framing" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重
- **Showcase_SelectsCorrectFrameSetPerToolAndState** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/creature-tool-variants" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CaptureAfter** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/foam-alignment-fix/after" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CaptureBaseline** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/foam-alignment-fix/before" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CaptureFullSurvey** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/full-audit-v05" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **LifeViews_SpawnSyncRemove_WithScreenshot** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/life-views-v01" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **MVO_TPR3_StateContrast_SameFraming** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **MVO1_FullMap_Overview** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **MVO3_Shore_Grass_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **MVO4_Decoration_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **MVO5_CloudOverForest_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TPR2_Closeup_Repairs** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS1_FullMap_TerrainStructure_Overview** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS2_WallStructure_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS3_OverlayFeatures_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS4_BoundariesAndClearance_Closeups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS6b_UnobstructedOverlayCloseups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS6c_OfficialAssemblyEightCombos** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS6d_ResourceSizeVariants** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS7_Iter07DirectionalCloseups** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS8_Iter08DiagnosticScenes** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS8h_Iter08SlopeBottomCloseup** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **TS9_Iter09DiagnosticScenes** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 环境变量 MVO_CAPTURE_MODE 未设置——D-063-A 失败关闭：捕获套件不再缺省回落历史模式/历史目录。请显式设置白名单模式（允许前缀：tri-）后重跑。
- **CaptureNineSliceEvidence** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-terrain-integration/post-fix-nine-slice" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，
- **CaptureEvidence** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-terrain-composition-v02" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CapturePlacementEvidence** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-terrain-placement-v03" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CapturePlacementEvidenceV04** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/tiny-swords-placement-v04" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **DetailCard_AvatarsDrivenBySemantics_WithEvidence** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-assets-v01" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **CaptureGameHud_And_MainMenu** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/IMPL-14" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **ButtonsPanelsPalettes_WiredWithEvidence** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/ui-wiring-v01" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历史目录）。
- **A_TerrainWaterFoam** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/parallel-repair-b/visual-verification" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历
- **D_WaterRocksDuckClouds** — WorldSandbox.Tests.Common.CaptureRejectedException : [D-063-A CaptureWriteGuard] 历史硬编码输出目录 "docs/impl/evidence/parallel-repair-b/visual-verification" 的写盘在 D-063-A 下默认拒绝（失败关闭）。如需重跑该套件：设置 WS_ALLOW_LEGACY_CAPTURE=1 且 WS_LEGACY_CAPTURE_OUT_DIR=<非历史新目录>（输出重定向，绝不写回历

### capture_env_precondition（9）

- **D067_CaptureAllScreens** — OneTimeSetUp: 必须设置 D067_SHOT_DIR（证据目录） Expected: False But was: True
- **D067_CaptureEnglishScreens** — OneTimeSetUp: 必须设置 D067_SHOT_DIR（证据目录） Expected: False But was: True
- **D068_Capture_ChineseFlow** — OneTimeSetUp: 未设置 D068_SHOT_DIR（证据截图目录） Expected: False But was: True
- **D068_Capture_English** — OneTimeSetUp: 未设置 D068_SHOT_DIR（证据截图目录） Expected: False But was: True
- **D068_Capture_TerrainThreeSeeds** — OneTimeSetUp: 未设置 D068_SHOT_DIR（证据截图目录） Expected: False But was: True
- **Capture_CharactersCloseupMovementAndLoad** — OneTimeSetUp: 未设置 D068_FINAL_SHOT_DIR（r3 证据截图目录） Expected: False But was: True
- **Capture_EnglishAnd_1280x720** — OneTimeSetUp: 未设置 D068_FINAL_SHOT_DIR（r3 证据截图目录） Expected: False But was: True
- **Capture_TerrainSeedsAndCoast** — OneTimeSetUp: 未设置 D068_FINAL_SHOT_DIR（r3 证据截图目录） Expected: False But was: True
- **Capture_Zh_MenuPauseSettingsCivPanel** — OneTimeSetUp: 未设置 D068_FINAL_SHOT_DIR（r3 证据截图目录） Expected: False But was: True

### scene_setup_null（28）

- **R1_ShadowRect_Repro** — System.NullReferenceException : Object reference not set to an instance of an object
- **R2_StumpShadow_Repro** — System.NullReferenceException : Object reference not set to an instance of an object
- **R3_FoamAnimation_SameFraming** — System.NullReferenceException : Object reference not set to an instance of an object
- **R4_TerrainLayers** — System.NullReferenceException : Object reference not set to an instance of an object
- **R5_CloudDrift_SameObject** — System.NullReferenceException : Object reference not set to an instance of an object
- **MainScene_InitialCamera_FramedOnMapCentre** — GameBootstrap 未完成 WireWorld（MapReader 为 null） Expected: not null But was: null
- **RepeatedWireWorld_DoesNotOverrideAdjustedView** — GameBootstrap 未完成 WireWorld（MapReader 为 null） Expected: not null But was: null
- **C1_HorizontalRun_Closeup** — System.NullReferenceException : Object reference not set to an instance of an object
- **C2_VerticalRun_Closeup** — System.NullReferenceException : Object reference not set to an instance of an object
- **C3_Corner_Closeup** — System.NullReferenceException : Object reference not set to an instance of an object
- **C4_WaterFoot_Closeup** — System.NullReferenceException : Object reference not set to an instance of an object
- **C5_LandFoot_Closeup** — System.NullReferenceException : Object reference not set to an instance of an object
- **C6_Overview** — System.NullReferenceException : Object reference not set to an instance of an object
- **HighGroundShadows_CountMatchesElevatedCells** — GameBootstrap 未完成 WireWorld（MapReader 为空） Expected: not null But was: null
- **FullStack_GameBootstrap_AssemblesClockSnapshotAndUi** — SimClock 未装配 Expected: not null But was: null
- **WaterRejectFeedback_AndRemoveAt_TypedFxLifecycle** — System.NullReferenceException : Object reference not set to an instance of an object
- **Iter11_GameTerrainShots** — 真实渲染链应已计算覆盖计划 Expected: not null But was: null
- **RunAndSheepStates_FrameAdvanceAndStateSwap** — System.NullReferenceException : Object reference not set to an instance of an object
- **Bootstrap_FullAssembly_WorldAndRenderers_NoErrors** — GameBootstrap.MapReader 未装配（WireWorld 未执行？） Expected: not null But was: null
- **AllCreatureToolVariants_SpawnExactTypeAndTool** — System.NullReferenceException : Object reference not set to an instance of an object
- **LeftClick_CreatureTool_FullChain_InputToDataEventView** — System.NullReferenceException : Object reference not set to an instance of an object
- **UiBlockedGuard_BlocksToolPointer_U74** — System.NullReferenceException : Object reference not set to an instance of an object
- **TreeOverlap_RealRender_SouthInFront** — GameBootstrap 未完成 WireWorld Expected: not null But was: null
- **B_CliffElevationShadow** — TerrainTilemapRenderer.CurrentPlan 应已装配（D-059 渲染链） Expected: not null But was: null
- **C_TreesStumpsBushesRocks** — System.NullReferenceException : Object reference not set to an instance of an object
- **E_CreaturesAnimation** — System.NullReferenceException : Object reference not set to an instance of an object
- **G_UiPalettes** — System.NullReferenceException : Object reference not set to an instance of an object
- **H_ToolInteractionReal** — System.NullReferenceException : Object reference not set to an instance of an object

### test_order_isolation（1）

- **R3_EnglishFlow_ToastAndHud_UseCurrentLanguage** — Expected: not null But was: null

### legacy_scene_state（1）

- **F_BuildingsFx** — 4 建筑放置 Expected: 4 But was: 0

## 已修复并单独复跑的用例

- HudEqualsSnapshot：全量批次首次失败（食物期望 45、实际 0），根因是 TopBarView 在“文明列表非 null 但为空”时不再回退聚落库存；已修为 civs != null && civs.Count > 0 才走文明求和，否则回退第一聚落。单独复跑 **1/1 通过**（hud-targeted.xml）。

## 未改动

- 未改任何测试断言；未关闭 CaptureWriteGuard；未把任何失败标记为 Ignore/Skip。
- D-068 新增的 4 个截图用例在设置 D068_FINAL_SHOT_DIR 时 4/4 通过，26/26 PNG 落盘。
