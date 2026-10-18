# r4 历史 72 失败逐条映射

> 来源：r3 tests/playmode-full-failures.md 的 72 条分类。
> 复核：tests/final-full-playmode.xml（最终树，109/110，1 skip）。
> 本表只做逐条映射，不修改断言、不关闭守卫、不把 skip 计为 pass。

## 汇总

- 共 72 条；在最终 XML 中定位 72/72 条；Passed 71、Failed 0、Skipped 1。
- 72/72 均能在最终 XML 中按测试名定位；无改名/删除/漏测。
- 唯一 skip：MVO_TPR3_StateContrast_SameFraming（r3 归入 legacy_capture_guard，现为模式约束）。

## 逐条

| # | r3 分类 | 测试 | 当前类 | 最终结果 | 处理/说明 |
|---|---|---|---|---|---|
| 1 | legacy_capture_guard | Buildings_PlacedRendered_WithFxLifecycle | BuildingFxShowcaseTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 2 | legacy_capture_guard | CaptureHudAtThreeResolutions | CameraFramingEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 3 | legacy_capture_guard | Showcase_SelectsCorrectFrameSetPerToolAndState | CreatureToolVariantTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 4 | legacy_capture_guard | CaptureAfter | FoamAfterCapture | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 5 | legacy_capture_guard | CaptureBaseline | FoamBaselineCapture | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 6 | legacy_capture_guard | CaptureFullSurvey | FullAssetSurveyTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 7 | legacy_capture_guard | LifeViews_SpawnSyncRemove_WithScreenshot | LifeViewIntegrationTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 8 | legacy_capture_guard | MVO_TPR3_StateContrast_SameFraming | MapVisualOverhaulCaptureTests | **Skipped** | 模式约束：要求 tpr3-*，本轮 tri-playmode-full；未计通过 |
| 9 | legacy_capture_guard | MVO1_FullMap_Overview | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 10 | legacy_capture_guard | MVO3_Shore_Grass_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 11 | legacy_capture_guard | MVO4_Decoration_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 12 | legacy_capture_guard | MVO5_CloudOverForest_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 13 | legacy_capture_guard | TPR2_Closeup_Repairs | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 14 | legacy_capture_guard | TS1_FullMap_TerrainStructure_Overview | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 15 | legacy_capture_guard | TS2_WallStructure_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 16 | legacy_capture_guard | TS3_OverlayFeatures_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 17 | legacy_capture_guard | TS4_BoundariesAndClearance_Closeups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 18 | legacy_capture_guard | TS6b_UnobstructedOverlayCloseups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 19 | legacy_capture_guard | TS6c_OfficialAssemblyEightCombos | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 20 | legacy_capture_guard | TS6d_ResourceSizeVariants | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 21 | legacy_capture_guard | TS7_Iter07DirectionalCloseups | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 22 | legacy_capture_guard | TS8_Iter08DiagnosticScenes | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 23 | legacy_capture_guard | TS8h_Iter08SlopeBottomCloseup | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 24 | legacy_capture_guard | TS9_Iter09DiagnosticScenes | MapVisualOverhaulCaptureTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 25 | legacy_capture_guard | CaptureNineSliceEvidence | NineSliceEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 26 | legacy_capture_guard | CaptureEvidence | TerrainCompositionEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 27 | legacy_capture_guard | CapturePlacementEvidence | TerrainPlacementEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 28 | legacy_capture_guard | CapturePlacementEvidenceV04 | TinySwordsPlacementEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 29 | legacy_capture_guard | DetailCard_AvatarsDrivenBySemantics_WithEvidence | UiAssetsEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 30 | legacy_capture_guard | CaptureGameHud_And_MainMenu | UIVisualSnapshotTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 31 | legacy_capture_guard | ButtonsPanelsPalettes_WiredWithEvidence | UiWiringEvidenceTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 32 | legacy_capture_guard | A_TerrainWaterFoam | VisualVerificationTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 33 | legacy_capture_guard | D_WaterRocksDuckClouds | VisualVerificationTests | **Passed** | 唯一临时捕获根 + AuthorizeLegacyOutDir；CaptureWriteGuard 未关闭 |
| 34 | capture_env_precondition | D067_CaptureAllScreens | D067CaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 35 | capture_env_precondition | D067_CaptureEnglishScreens | D067CaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 36 | capture_env_precondition | D068_Capture_ChineseFlow | D068EvidenceCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 37 | capture_env_precondition | D068_Capture_English | D068EvidenceCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 38 | capture_env_precondition | D068_Capture_TerrainThreeSeeds | D068EvidenceCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 39 | capture_env_precondition | Capture_CharactersCloseupMovementAndLoad | D068FinalCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 40 | capture_env_precondition | Capture_EnglishAnd_1280x720 | D068FinalCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 41 | capture_env_precondition | Capture_TerrainSeedsAndCoast | D068FinalCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 42 | capture_env_precondition | Capture_Zh_MenuPauseSettingsCivPanel | D068FinalCaptureTests | **Passed** | 测试夹具注入 D067/D068 唯一截图目录 |
| 43 | scene_setup_null | R1_ShadowRect_Repro | AiAMapEnvCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 44 | scene_setup_null | R2_StumpShadow_Repro | AiAMapEnvCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 45 | scene_setup_null | R3_FoamAnimation_SameFraming | AiAMapEnvCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 46 | scene_setup_null | R4_TerrainLayers | AiAMapEnvCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 47 | scene_setup_null | R5_CloudDrift_SameObject | AiAMapEnvCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 48 | scene_setup_null | MainScene_InitialCamera_FramedOnMapCentre | CameraInitialFramingTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 49 | scene_setup_null | RepeatedWireWorld_DoesNotOverrideAdjustedView | CameraInitialFramingTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 50 | scene_setup_null | C1_HorizontalRun_Closeup | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 51 | scene_setup_null | C2_VerticalRun_Closeup | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 52 | scene_setup_null | C3_Corner_Closeup | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 53 | scene_setup_null | C4_WaterFoot_Closeup | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 54 | scene_setup_null | C5_LandFoot_Closeup | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 55 | scene_setup_null | C6_Overview | CliffAuditCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 56 | scene_setup_null | HighGroundShadows_CountMatchesElevatedCells | FastNoiseRenderIntegrationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 57 | scene_setup_null | FullStack_GameBootstrap_AssemblesClockSnapshotAndUi | FinalIntegrationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 58 | scene_setup_null | WaterRejectFeedback_AndRemoveAt_TypedFxLifecycle | FxTriggerPathTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 59 | scene_setup_null | Iter11_GameTerrainShots | Iter11GameCaptureTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 60 | scene_setup_null | RunAndSheepStates_FrameAdvanceAndStateSwap | LifeAnimCoverageTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 61 | scene_setup_null | Bootstrap_FullAssembly_WorldAndRenderers_NoErrors | PlayModeSmokeTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 62 | scene_setup_null | AllCreatureToolVariants_SpawnExactTypeAndTool | ToolInputChainTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 63 | scene_setup_null | LeftClick_CreatureTool_FullChain_InputToDataEventView | ToolInputChainTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 64 | scene_setup_null | UiBlockedGuard_BlocksToolPointer_U74 | ToolInputChainTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 65 | scene_setup_null | TreeOverlap_RealRender_SouthInFront | TreeOverlapRenderTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 66 | scene_setup_null | B_CliffElevationShadow | VisualVerificationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 67 | scene_setup_null | C_TreesStumpsBushesRocks | VisualVerificationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 68 | scene_setup_null | E_CreaturesAnimation | VisualVerificationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 69 | scene_setup_null | G_UiPalettes | VisualVerificationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 70 | scene_setup_null | H_ToolInteractionReal | VisualVerificationTests | **Passed** | PlayModeTestWorld.EnsureWorld / 显式装配世界与快照 |
| 71 | test_order_isolation | R3_EnglishFlow_ToastAndHud_UseCurrentLanguage | D067R3LocalizationTests | **Passed** | R3 独立场景与顺序隔离；稳定 ID 收尾 |
| 72 | legacy_scene_state | F_BuildingsFx | VisualVerificationTests | **Passed** | EnsureWorld 后 4 建筑真实放置并截图 |
