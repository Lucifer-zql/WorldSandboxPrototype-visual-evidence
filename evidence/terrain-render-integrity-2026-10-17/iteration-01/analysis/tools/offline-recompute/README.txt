# 离线逐位复算工具（D-065 iter-01 审计）

用途与结论见 ../../offline-recompute-verification.txt。此处为源码与复现步骤。

## 文件
- `Shim.cs`：WorldGenConfig 的纯 C# 同形替代（真类为 ScriptableObject，离线不可实例化；
  仅含 5 个消费字段、默认值与真类逐字一致；**未复制任何业务方法**）。
- `Program.cs`：统计/导出程序——调用**真实源文件**（WaterLandStep 等 7 文件，见下）生成三种子世界，
  输出 `stats-seed-*.txt`（全量审计统计）与 `mask-seed-*.txt`（128×128 数据水格 mask）。

## 复现步骤（Windows；Git Bash）
    R="<主仓库>/Assets/_Project/Scripts"
    CSC="/c/Program Files/dotnet/sdk/<ver>/Roslyn/bincore/csc.dll"
    MONO47="<Unity>/Editor/Data/MonoBleedingEdge/lib/mono/4.7.1-api"
    dotnet "$CSC" -nologo -target:exe -langversion:9.0 -nostdlib+ -noconfig -out:wlhost2.exe \
      -r:"$MONO47/mscorlib.dll" -r:"$MONO47/System.dll" -r:"$MONO47/System.Core.dll" \
      -r:"$MONO47/Facades/netstandard.dll" \
      Shim.cs Program.cs \
      "$R/Core/Enums/TerrainType.cs" "$R/Core/World/MapIndex.cs" "$R/Core/Config/IntRange.cs" \
      "$R/World/DeterministicRandom.cs" "$R/World/MapBounds.cs" "$R/World/TerrainStore.cs" \
      "$R/World/GenSteps/WaterLandStep.cs"
    OFFLINE_OUT=<输出目录> "<Unity>/Editor/Data/MonoBleedingEdge/bin/mono.exe" wlhost2.exe

## 校核点
三种子水格数应为 5388 / 3605 / 4324（与 Unity 实跑一致，见 verification 文本）；
不同则说明源文件或 shim 与出具结果时点不一致，结果作废重算。
