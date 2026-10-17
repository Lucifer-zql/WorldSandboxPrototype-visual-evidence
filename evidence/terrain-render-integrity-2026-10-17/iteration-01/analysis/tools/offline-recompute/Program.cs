using System;
using System.Collections.Generic;
using System.IO;
using WorldSandbox.Core.Config;
using WorldSandbox.Core.Enums;
using WorldSandbox.World;
using WorldSandbox.World.GenSteps;

public static class Program
{
    const int W = 128, H = 128;
    static bool[,] water;

    public static void Main()
    {
        int[] seeds = { 20261013, 20261014, 20261015 };
        string outDir = Environment.GetEnvironmentVariable("OFFLINE_OUT");
        foreach (var seed in seeds)
        {
            var terrain = new TerrainStore(W, H);
            var bounds = new MapBounds(W, H, 1);
            var cfg = new WorldGenConfig();
            var rng = DeterministicRandom.FromCombinedSeed(DeterministicRandom.CombineSeed(seed, 1));
            WaterLandStep.Apply(terrain, bounds, cfg, rng.Fork(1));

            water = new bool[W, H];
            int waterCells = 0;
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++)
                    if (terrain.Get(x, y) == TerrainType.Water) { water[x, y] = true; waterCells++; }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("## seed=" + seed + " waterCells=" + waterCells + " landCells=" + (W * H - waterCells) +
                          " (" + (100.0 * waterCells / (W * H)).ToString("F4") + "%)");

            // ---- 4 连通分量 ----
            var compId = new int[W, H];
            for (var i = 0; i < W; i++) for (var j = 0; j < H; j++) compId[i, j] = -1;
            var comps = new List<List<int[]>>();
            var q = new Queue<int[]>();
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++)
                {
                    if (!water[x, y] || compId[x, y] >= 0) continue;
                    var comp = new List<int[]>();
                    var id = comps.Count;
                    q.Enqueue(new[] { x, y }); compId[x, y] = id;
                    while (q.Count > 0)
                    {
                        var c = q.Dequeue(); comp.Add(c);
                        int[] dxs = { 1, -1, 0, 0 };
                        int[] dys = { 0, 0, 1, -1 };
                        for (var k = 0; k < 4; k++)
                        {
                            var nx = c[0] + dxs[k]; var ny = c[1] + dys[k];
                            if (nx < 0 || ny < 0 || nx >= W || ny >= H || !water[nx, ny] || compId[nx, ny] >= 0) continue;
                            compId[nx, ny] = id; q.Enqueue(new[] { nx, ny });
                        }
                    }
                    comps.Add(comp);
                }

            sb.AppendLine("components=" + comps.Count);
            var inlandList = new List<int>();
            for (var i = 0; i < comps.Count; i++)
            {
                var c = comps[i];
                int minx = W, maxx = -1, miny = H, maxy = -1; bool edge = false;
                foreach (var p in c)
                {
                    if (p[0] < minx) minx = p[0];
                    if (p[0] > maxx) maxx = p[0];
                    if (p[1] < miny) miny = p[1];
                    if (p[1] > maxy) maxy = p[1];
                    if (p[0] == 0 || p[1] == 0 || p[0] == W - 1 || p[1] == H - 1) edge = true;
                }
                if (!edge) inlandList.Add(i);
                sb.AppendLine("  comp#" + i + " size=" + c.Count + " bbox=(" + minx + "," + miny + ")-(" + maxx + "," + maxy + ") " + (edge ? "EDGE" : "INLAND"));
            }

            // ---- 内陆分量形状细节 ----
            foreach (var i in inlandList)
            {
                var c = comps[i];
                var cells = new HashSet<long>();
                int minx = W, maxx = -1, miny = H, maxy = -1;
                foreach (var p in c)
                {
                    cells.Add((long)p[1] * W + p[0]);
                    if (p[0] < minx) minx = p[0];
                    if (p[0] > maxx) maxx = p[0];
                    if (p[1] < miny) miny = p[1];
                    if (p[1] > maxy) maxy = p[1];
                }
                bool In(int x, int y) { return x >= 0 && y >= 0 && x < W && y < H && cells.Contains((long)y * W + x); }
                sb.AppendLine("  INLAND comp#" + i + " size=" + c.Count + " bbox=(" + minx + "," + miny + ")-(" + maxx + "," + maxy +
                              ") 宽" + (maxx - minx + 1) + "×高" + (maxy - miny + 1));
                for (var y = miny; y <= maxy; y++)
                {
                    int x0 = -1, x1 = -1;
                    for (var x = minx; x <= maxx; x++) if (In(x, y)) { if (x0 < 0) x0 = x; x1 = x; }
                    if (x0 >= 0) sb.AppendLine("    row y=" + y + ": x[" + x0 + "," + x1 + "] w=" + (x1 - x0 + 1));
                }
                int runL = 0, runR = 0, maxL = 0, maxR = 0;
                int prevL = int.MinValue, prevR = int.MinValue;
                for (var y = miny; y <= maxy; y++)
                {
                    int x0 = -1, x1 = -1;
                    for (var x = minx; x <= maxx; x++) if (In(x, y)) { if (x0 < 0) x0 = x; x1 = x; }
                    if (x0 < 0) { runL = runR = 0; prevL = prevR = int.MinValue; continue; }
                    if (x0 == prevL) { runL++; if (runL > maxL) maxL = runL; } else runL = 1;
                    if (x1 == prevR) { runR++; if (runR > maxR) maxR = runR; } else runR = 1;
                    prevL = x0; prevR = x1;
                }
                int runT = 0, runB = 0, maxT = 0, maxB = 0, prevT = int.MinValue, prevB = int.MinValue;
                for (var x = minx; x <= maxx; x++)
                {
                    int y0 = -1, y1 = -1;
                    for (var y = miny; y <= maxy; y++) if (In(x, y)) { if (y0 < 0) y0 = y; y1 = y; }
                    if (y0 < 0) { runT = runB = 0; prevT = prevB = int.MinValue; continue; }
                    if (y0 == prevT) { runT++; if (runT > maxT) maxT = runT; } else runT = 1;
                    if (y1 == prevB) { runB++; if (runB > maxB) maxB = runB; } else runB = 1;
                    prevT = y0; prevB = y1;
                }
                sb.AppendLine("    straight-edges: maxLeft=" + maxL + " maxRight=" + maxR + " maxTop=" + maxT + " maxBottom=" + maxB);
            }

            // ---- 窄点（宽度 1 水格） ----
            var pinchList = new List<string>();
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++)
                {
                    if (!water[x, y]) continue;
                    bool lr = !(x > 0 && water[x - 1, y]) && !(x < W - 1 && water[x + 1, y]);
                    bool tb = !(y > 0 && water[x, y - 1]) && !(y < H - 1 && water[x, y + 1]);
                    if (lr || tb) pinchList.Add("(" + x + "," + y + ")" + (lr ? "H" : "") + (tb ? "V" : ""));
                }
            sb.AppendLine("pinchCells(宽度1处) count=" + pinchList.Count);
            sb.AppendLine("  " + string.Join(" ", pinchList));

            var sizes = new List<int>();
            foreach (var c in comps) sizes.Add(c.Count);
            sizes.Sort(); sizes.Reverse();
            sb.AppendLine("water component sizes: " + string.Join(",", sizes));

            // ---- mask 文件（顶部 y=127 先输出） ----
            var maskSb = new System.Text.StringBuilder();
            for (var y = H - 1; y >= 0; y--)
            {
                for (var x = 0; x < W; x++) maskSb.Append(water[x, y] ? '~' : '.');
                maskSb.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "mask-seed-" + seed + ".txt"), maskSb.ToString());
            File.WriteAllText(Path.Combine(outDir, "stats-seed-" + seed + ".txt"), sb.ToString());
            Console.WriteLine("seed=" + seed + " done: waterCells=" + waterCells + " comps=" + comps.Count +
                              " inland=" + inlandList.Count + " pinch=" + pinchList.Count);
        }
    }
}
