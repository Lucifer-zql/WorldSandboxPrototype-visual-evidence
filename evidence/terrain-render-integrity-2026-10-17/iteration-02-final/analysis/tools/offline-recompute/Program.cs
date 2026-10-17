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
    static TerrainType[] terrain;
    static byte[] elev;

    public static void Main()
    {
        int[] seeds = { 20261013, 20261014, 20261015 };
        // 基线 manifest ts1 统计（Unity 实跑）：water / hill / mount
        int[] expWater = { 5388, 3605, 4324 };
        int[] expHill = { 2296, 2294, 2319 };
        int[] expMount = { 648, 652, 655 };
        string outDir = Environment.GetEnvironmentVariable("OFFLINE_OUT");

        for (var s = 0; s < 3; s++)
        {
            var seed = seeds[s];
            var store = new TerrainStore(W, H);
            var bounds = new MapBounds(W, H, 1);
            var cfg = new WorldGenConfig();
            var attemptRng = DeterministicRandom.FromCombinedSeed(DeterministicRandom.CombineSeed(seed, 1));
            WaterLandStep.Apply(store, bounds, cfg, attemptRng.Fork(1));
            var elevation = ElevationStep.Apply(store, bounds, attemptRng.Fork(5));
            GrassForestStep.Apply(store, bounds, cfg, attemptRng.Fork(3));

            terrain = store.Raw;
            elev = elevation;

            int water = 0, hill = 0, mount = 0, forest = 0;
            for (var i = 0; i < W * H; i++)
            {
                if (terrain[i] == TerrainType.Water) water++;
                else if (terrain[i] == TerrainType.Forest) forest++;
                if (elevation[i] == 1) hill++;
                else if (elevation[i] == 2) mount++;
            }
            Console.WriteLine($"seed={seed} water={water}(exp {expWater[s]}) hill={hill}(exp {expHill[s]}) mount={mount}(exp {expMount[s]}) forest={forest}");

            // ---- 导出 ----
            var t = new System.Text.StringBuilder();
            for (var y = H - 1; y >= 0; y--)
            {
                for (var x = 0; x < W; x++)
                {
                    var tt = terrain[y * W + x];
                    t.Append(tt == TerrainType.Water ? '~' : tt == TerrainType.Forest ? 'F' : '.');
                }
                t.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "terrain-seed-" + seed + ".txt"), t.ToString());

            var e = new System.Text.StringBuilder();
            for (var y = H - 1; y >= 0; y--)
            {
                for (var x = 0; x < W; x++) e.Append((char)('0' + elevation[y * W + x]));
                e.Append('\n');
            }
            File.WriteAllText(Path.Combine(outDir, "elev-seed-" + seed + ".txt"), e.ToString());

            // ---- 小高地分量清单（elev≥1 的 4 连通分量，size≤12；bbox/含Mount/临水性） ----
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("## seed=" + seed + " 小高地分量（elev>=1, 4连通, size<=12）");
            var seen = new bool[W * H];
            var q = new Queue<int>();
            for (var i = 0; i < W * H; i++)
            {
                if (seen[i] || elevation[i] < 1) continue;
                var cells = new List<int>();
                q.Enqueue(i); seen[i] = true;
                while (q.Count > 0)
                {
                    var c = q.Dequeue(); cells.Add(c);
                    var cx = c % W; var cy = c / W;
                    Try(q, seen, cx + 1, cy); Try(q, seen, cx - 1, cy); Try(q, seen, cx, cy + 1); Try(q, seen, cx, cy - 1);
                }
                if (cells.Count > 12) continue;
                int minx = W, maxx = -1, miny = H, maxy = -1; bool hasMount = false, adjWater = false;
                foreach (var c in cells)
                {
                    var cx = c % W; var cy = c / W;
                    if (cx < minx) minx = cx; if (cx > maxx) maxx = cx;
                    if (cy < miny) miny = cy; if (cy > maxy) maxy = cy;
                    if (elevation[c] >= 2) hasMount = true;
                    if (IsWater(cx + 1, cy) || IsWater(cx - 1, cy) || IsWater(cx, cy + 1) || IsWater(cx, cy - 1)) adjWater = true;
                }
                sb.AppendLine($"  size={cells.Count} bbox=({minx},{miny})-({maxx},{maxy}) hasMount={hasMount} 临水={adjWater}");
            }

            // ---- "高地|低地草"邻接对（扫描序前 20；相机 TS4d 即瞄准其中某对的中点） ----
            sb.AppendLine("## 高地(elev>=1) 邻 低地草(elev==0且Grass) 首 20 对（y 降序扫描=游戏世界 y 大在前）");
            var cnt = 0;
            for (var y = H - 1; y >= 0 && cnt < 20; y--)
                for (var x = 0; x < W && cnt < 20; x++)
                {
                    if (elevation[y * W + x] < 1 || terrain[y * W + x] == TerrainType.Water) continue;
                    int[] dxs = { 0, 0, 1, -1 }, dys = { 1, -1, 0, 0 };
                    for (var k = 0; k < 4 && cnt < 20; k++)
                    {
                        var nx = x + dxs[k]; var ny = y + dys[k];
                        if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
                        if (elevation[ny * W + nx] == 0 && terrain[ny * W + nx] == TerrainType.Grass)
                        {
                            sb.AppendLine($"  高地({x},{y}) elev={elevation[y * W + x]} 邻低地草({nx},{ny})");
                            cnt++;
                        }
                    }
                }
            // ---- 该种子"高地邻草"总对数（供判断覆盖面） ----
            var totalPairs = 0;
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++)
                {
                    if (elevation[y * W + x] < 1 || terrain[y * W + x] == TerrainType.Water) continue;
                    if (x + 1 < W && elevation[y * W + x + 1] == 0 && terrain[y * W + x + 1] == TerrainType.Grass) totalPairs++;
                    if (y + 1 < H && elevation[(y + 1) * W + x] == 0 && terrain[(y + 1) * W + x] == TerrainType.Grass) totalPairs++;
                }
            sb.AppendLine("## 高-低草邻接对总数（东+北向计数）：" + totalPairs);
            File.WriteAllText(Path.Combine(outDir, "stats2-seed-" + seed + ".txt"), sb.ToString());
        }
        Program4.Run(outDir);
    }

    static void Try(Queue<int> q, bool[] seen, int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return;
        var i = y * W + x;
        if (seen[i] || elev[i] < 1) return;
        seen[i] = true; q.Enqueue(i);
    }

    static bool IsWater(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return false;
        return terrain[y * W + x] == TerrainType.Water;
    }
}
