// 离线复算 shim：与 Assets/_Project/Scripts/Core/Config/WorldGenConfig.cs 相关字段逐字一致
// （MapWidth/MapHeight/WorldEdgeThickness/WaterRatioMin/WaterRatioMax + 真类默认值）。
// 真类是 ScriptableObject（无 Unity 运行时时不可实例化），离线复算以纯 C# 同形类替代——仅复算用，不改项目源码。
namespace WorldSandbox.Core.Config
{
    public sealed class WorldGenConfig
    {
        public int MapWidth = 128;
        public int MapHeight = 128;
        public int WorldEdgeThickness = 1;
        public float WaterRatioMin = 0.20f;
        public float WaterRatioMax = 0.35f;
    }
}
