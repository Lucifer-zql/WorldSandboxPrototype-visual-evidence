using System;
using System.IO;
using System.Text;
using WorldSandbox.Core.Config;
using WorldSandbox.Core.Enums;
using WorldSandbox.World;
using WorldSandbox.World.GenSteps;

public static class Program4
{
    const int W = 128, H = 128;

    public static void Run(string outDir)
    {
        int[] seeds = { 20261013, 20261014, 20261015 };
        foreach (var seed in seeds)
        {
            var store = new TerrainStore(W, H);
            var bounds = new MapBounds(W, H, 1);
            var cfg = new WorldGenConfig();
            var attemptRng = DeterministicRandom.FromCombinedSeed(DeterministicRandom.CombineSeed(seed, 1));
            WaterLandStep.Apply(store, bounds, cfg, attemptRng.Fork(1));
            var elevation = ElevationStep.Apply(store, bounds, attemptRng.Fork(5));
            GrassForestStep.Apply(store, bounds, cfg, attemptRng.Fork(3));
            var plan = TerrainOverlayPlan.Compute(store.Raw, elevation, W, H);

            var grid = new StringBuilder();
            var cells = new char[W * H];
            for (var i = 0; i < W * H; i++) cells[i] = '.';
            foreach (var c in plan.CliffCells) cells[c.Y * W + c.X] = 'W';
            foreach (var s in plan.SlopeCells) cells[s.Y * W + s.X] = 'S';
            foreach (var st in plan.Stairs)
            {
                cells[st.UpperY * W + st.UpperX] = 'U';
                cells[st.LowerY * W + st.LowerX] = 'L';
            }
            for (var y = H - 1; y >= 0; y--)
            {
                for (var x = 0; x < W; x++) grid.Append(cells[y * W + x]);
                grid.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "plan-seed-" + seed + ".txt"), grid.ToString());

            var sb = new StringBuilder();
            sb.AppendLine($"## seed={seed} CliffCells={plan.CliffCells.Count} SlopeCells={plan.SlopeCells.Count} Stairs={plan.Stairs.Count}");
            sb.AppendLine("## Stairs 清单（upper/lower/side）:");
            foreach (var st in plan.Stairs)
                sb.AppendLine($"  U({st.UpperX},{st.UpperY}) L({st.LowerX},{st.LowerY}) side={st.Side}");
            sb.AppendLine("## SlopeCells 清单:");
            foreach (var s in plan.SlopeCells)
                sb.AppendLine($"  ({s.X},{s.Y}) side={s.Side} panelLevel={s.PanelLevel}");
            File.WriteAllText(Path.Combine(outDir, "stats3-seed-" + seed + ".txt"), sb.ToString());
            Console.WriteLine($"seed={seed} plan: walls={plan.CliffCells.Count} slopes={plan.SlopeCells.Count} stairs={plan.Stairs.Count}");
        }
    }
}
