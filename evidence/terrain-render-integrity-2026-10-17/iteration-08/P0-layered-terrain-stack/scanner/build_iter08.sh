#!/bin/bash
# D-065 iter-07 离线复算：当前工作区生产代码 → plan7 明细导出器。
set -e
CSC="/c/Program Files/dotnet/sdk/10.0.401/Roslyn/bincore/csc.dll"
UE="/d/software/Unity Hub/Editor/6000.6.5f1/Editor/Data"
MONO="$UE/MonoBleedingEdge/lib/mono/4.7.1-api"
NM="$UE/Managed/UnityEngine"
R="D:/data/UnityProjects/WorldSandboxPrototype"
O="$TEMP/iter01-scan/offline2"

COMMON=(
  "$O/Shim.cs" "$O/Program8.cs"
  "$R/Assets/_Project/Scripts/Core/Enums/TerrainType.cs"
  "$R/Assets/_Project/Scripts/Core/Config/IntRange.cs"
  "$R/Assets/_Project/Scripts/Core/Diagnostics/ILogger.cs"
  "$R/Assets/_Project/Scripts/Core/Diagnostics/Log.cs"
  "$R/Assets/_Project/Scripts/Core/World/MapIndex.cs"
  "$R/Assets/_Project/Scripts/World/DeterministicRandom.cs"
  "$R/Assets/_Project/Scripts/World/TerrainStore.cs"
  "$R/Assets/_Project/Scripts/World/MapBounds.cs"
  "$R/Assets/_Project/Scripts/World/ElevationStore.cs"
  "$R/Assets/_Project/Scripts/World/GenSteps/WaterLandStep.cs"
  "$R/Assets/_Project/Scripts/World/GenSteps/PlateauStep.cs"
  "$R/Assets/_Project/Scripts/World/GenSteps/GrassForestStep.cs"
  "$R/Assets/_Project/Scripts/World/GenSteps/TerrainOverlayPlan.cs"
  "$R/Assets/_Project/ThirdParty/FastNoiseLite/FastNoiseLite.cs"
)

dotnet "$CSC" -nologo -target:exe -langversion:9.0 -nostdlib+ -noconfig \
  -out:"$O/wlhost8.exe" -r:"$MONO/mscorlib.dll" -r:"$MONO/System.dll" -r:"$MONO/System.Core.dll" \
  -r:"$MONO/Facades/netstandard.dll" -r:"$NM/UnityEngine.CoreModule.dll" \
  "${COMMON[@]}"
echo "== wlhost8 编译通过 =="
