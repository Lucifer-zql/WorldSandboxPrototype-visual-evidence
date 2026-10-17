// D-065 iter-08 全图扫描器：生产 plan（当前工作区代码）→ P0 逐层连接 / P1 内陆暴露边 / P2 坡道合法性 CSV。
// 判定为**独立实现**（按任务书规则；不调用渲染器）。
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorldSandbox.Core.Config;
using WorldSandbox.Core.Enums;
using WorldSandbox.World;
using WorldSandbox.World.GenSteps;

public static class Program8
{
    const int W = 128, H = 128;

    public static void Main()
    {
        int[] seeds = { 20261013, 20261014, 20261015 };
        string outDir = Environment.GetEnvironmentVariable("OFFLINE_OUT");

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

            var walls = new Dictionary<int, CliffWallCell>();
            foreach (var c in plan.CliffCells) walls[c.Y * W + c.X] = c;
            var slopeUpper = new Dictionary<int, SlopeCell>();
            var slopeLower = new Dictionary<int, int>();   // lowerIndex -> upperIndex
            foreach (var s in plan.SlopeCells) { slopeUpper[s.Y * W + s.X] = s; slopeLower[s.LowerY * W + s.X] = s.Y * W + s.X; }
            var stairCells = new HashSet<int>();
            foreach (var st in plan.Stairs) { stairCells.Add(st.UpperY * W + st.UpperX); stairCells.Add(st.LowerY * W + st.LowerX); stairsUpper[st.UpperY * W + st.UpperX] = st; }

            // ---------- P0：高差连接（每高格 -> 四邻更低格） ----------
            var p0 = new StringBuilder();
            p0.AppendLine("seed,x,y,terrain,elev,dir,nt_terrain,nt_elev,source_rule,wall_row,wall_variant,is_mid_bench,flat_below,p0_verdict");
            int connTotal = 0, connOk = 0, missingWall = 0, missingGround = 0, missingMid = 0, illegalWaterFoot = 0, waterBgBreach = 0, unexplained = 0, northEdges = 0, narrowDitch = 0;
            for (var y = 0; y < H; y++)
            {
                for (var x = 0; x < W; x++)
                {
                    var e0 = elevation[y * W + x];
                    var t0 = store.Get(x, y);
                    if (e0 < 1 || t0 == TerrainType.Cliff) continue;
                    for (var d = 0; d < 4; d++)
                    {
                        int nx = x + (d == 0 ? 0 : d == 1 ? 1 : d == 2 ? 0 : -1);
                        int ny = y + (d == 0 ? 1 : d == 1 ? 0 : d == 2 ? -1 : 0);
                        if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
                        var e1 = elevation[ny * W + nx];
                        if (e1 >= e0) continue;
                        connTotal++;
                        var t1 = store.Get(nx, ny);
                        var dir = d == 0 ? "N" : d == 1 ? "E" : d == 2 ? "S" : "W";
                        var idx = ny * W + nx;
                        var hasWall = walls.TryGetValue(idx, out var wall);
                        var hasSlope = slopeUpper.ContainsKey(idx) || slopeLower.ContainsKey(idx);
                        var hasStair = stairCells.Contains(idx);
                        var isMid = e0 >= ElevationStore.MountLevel && e1 == ElevationStore.HighGround;   // Mount->Hill 中间台面
                        string source, verdict;
                        if (dir == "N") { northEdges++; connOk++; p0.AppendLine($"{seed},{x},{y},{t0},{e0},{dir},{t1},{e1},north-top-edge,{(hasWall ? wall.Row : -1)},{(hasWall ? wall.Variant : -1)},0,{(t1 == TerrainType.Water ? "water-bg" : "flat(land)")},OK:北缘顶面过渡(官方无墙)"); continue; }
                        if (t1 == TerrainType.Water && !hasWall) { narrowDitch++; connOk++; p0.AppendLine($"{seed},{x},{y},{t0},{e0},{dir},{t1},{e1},narrow-ditch-no-wall,-1,-1,0,water-bg,OK:窄水沟不写水格(官方保留形态)"); continue; }
                        if (isMid)
                        {
                            source = "mid-bench(P0 不写墙)";
                            verdict = (hasWall || hasSlope || hasStair) ? "FAIL:中间台面被覆盖" : "OK:中间台面完整";
                            if (hasWall || hasSlope || hasStair) { missingMid++; unexplained++; } else connOk++;
                        }
                        else if (hasWall)
                        {
                            source = wall.Row >= 5 ? "wall-water(r5)" : "wall-land(r4)";
                            var wallSelfWater = t1 == TerrainType.Water;
                            var belowIsWater = wallSelfWater || (ny - 1 >= 0 && store.Get(nx, ny - 1) == TerrainType.Water);
                            var belowHasGround = t1 == TerrainType.Grass || t1 == TerrainType.Forest;
                            if (wall.Row >= 5 && !belowIsWater) { illegalWaterFoot++; verdict = "FAIL:非法水脚(内陆)"; }
                            else if (!belowIsWater && !belowHasGround) { missingGround++; verdict = "FAIL:墙下无低层地表"; }
                            else { verdict = belowIsWater ? "OK:岩壁入水(合法)" : "OK:陆脚+低层地表"; connOk++; }
                            if (belowIsWater) waterBgBreach++;   // 记录"墙格为水"的合法入水数量（水背景可见=官方语义）
                        }
                        else if (hasSlope || hasStair)
                        {
                            source = hasStair ? "stairs" : "slope";
                            verdict = "OK:豁口替代墙段"; connOk++;
                        }
                        else
                        {
                            source = "none";
                            missingWall++; verdict = "FAIL:缺失墙体/连接结构";
                        }
                        var flatBelow = t1 == TerrainType.Water ? "water-bg" : (t1 == TerrainType.Grass || t1 == TerrainType.Forest) ? "flat(land)" : "NONE!";
                        p0.AppendLine($"{seed},{x},{y},{t0},{e0},{dir},{t1},{e1},{source},{(hasWall ? wall.Row : -1)},{(hasWall ? wall.Variant : -1)},{(isMid ? 1 : 0)},{flatBelow},{verdict}");
                    }
                }
            }
            File.WriteAllText(Path.Combine(outDir, "p0-connections-seed-" + seed + ".csv"), p0.ToString());

            // ---------- P1：内陆暴露边（非水格的每个暴露正交边） ----------
            var p1 = new StringBuilder();
            p1.AppendLine("seed,x,y,terrain,elev,dir,nt_terrain,exposure_type,is_water_edge,flat_family,band_mask,verdict");
            int inlandEdges = 0, waterEdges = 0, inlandCoastal = 0;
            for (var y = 0; y < H; y++)
            {
                for (var x = 0; x < W; x++)
                {
                    var t = store.Get(x, y);
                    if (t != TerrainType.Grass && t != TerrainType.Forest) continue;
                    var ownIsForest = t == TerrainType.Forest;
                    for (var d = 0; d < 4; d++)
                    {
                        int nx = x + (d == 0 ? 0 : d == 1 ? 1 : d == 2 ? 0 : -1);
                        int ny = y + (d == 0 ? 1 : d == 1 ? 0 : d == 2 ? -1 : 0);
                        bool inb = nx >= 0 && ny >= 0 && nx < W && ny < H;
                        var nt = inb ? store.Get(nx, ny) : TerrainType.None;
                        if (inb && nt == t) continue;   // 同地形：不暴露
                        var dir = d == 0 ? "N" : d == 1 ? "E" : d == 2 ? "S" : "W";
                        var isWater = inb && nt == TerrainType.Water;
                        // 族判定（与渲染器同规则）：同格任一边暴露为水 → 该格水缘；混合缘 → 右族 + band
                        var (wm, lm) = ExposureMasks(store, elevation, x, y);
                        var mixed = wm > 0 && lm > 0;
                        var flatFamily = (wm > 0 && !mixed) ? "shore(左族)" : "land(右族)";
                        var band = mixed ? wm : 0;
                        string verdict;
                        if (isWater) { waterEdges++; verdict = "OK:真实水缘"; }
                        else
                        {
                            inlandEdges++;
                            if (wm > 0 && !mixed) { inlandCoastal++; verdict = "FAIL:内陆边用左族整片(水岸风)"; }
                            else verdict = "OK:右族/中心片(无海岸白)";
                            if (!inb) verdict = "OK:越界(右族)";
                        }
                        if (!inb) verdict = "OK:越界(右族)";
                        p1.AppendLine($"{seed},{x},{y},{t},{elevation[y * W + x]},{dir},{nt},{(isWater ? "water" : "land")},{(isWater ? 1 : 0)},{flatFamily},{band},{verdict}");
                    }
                }
            }
            File.WriteAllText(Path.Combine(outDir, "p1-edges-seed-" + seed + ".csv"), p1.ToString());

            // ---------- P2：坡道上下端 ----------
            var p2 = new StringBuilder();
            p2.AppendLine("seed,x,y,spr,dir,terrain,elev,east_terrain,east_elev,west_terrain,west_elev,upper_ok,lower_y,lower_terrain,lower_elev,lower_ok,nb_wall_s,nb_wall_n,top_elevated,verdict");
            int slopeTotal = 0, slopeOk = 0, slopeBadUpper = 0, slopeBadLower = 0, slopeConflictWall = 0;
            foreach (var s in plan.SlopeCells)
            {
                slopeTotal++;
                var idx = s.Y * W + s.X;
                var t = store.Get(s.X, s.Y);
                var e = elevation[idx];
                var eE = s.X + 1 < W ? elevation[s.Y * W + s.X + 1] : 0;
                var eW = s.X - 1 >= 0 ? elevation[s.Y * W + s.X - 1] : 0;
                var tE = s.X + 1 < W ? store.Get(s.X + 1, s.Y) : TerrainType.None;
                var tW = s.X - 1 >= 0 ? store.Get(s.X - 1, s.Y) : TerrainType.None;
                var upperOk = (s.Side == OverlaySlopeSide.Left ? eE : eW) == ElevationStore.HighGround;
                var lowerY = s.LowerY;
                var lowerT = lowerY >= 0 ? store.Get(s.X, lowerY) : TerrainType.None;
                var lowerE = lowerY >= 0 ? elevation[lowerY * W + s.X] : -1;
                var lowerOk = lowerY >= 0 && (lowerT == TerrainType.Grass || lowerT == TerrainType.Forest) && lowerE == 0;
                var nbWallS = lowerY >= 0 && walls.ContainsKey(lowerY * W + s.X) ? 1 : 0;
                var nbWallN = walls.ContainsKey((s.Y + 1) * W + s.X) ? 1 : 0;
                var topElevated = elevation[s.Y * W + s.X] < 1 && ((s.Side == OverlaySlopeSide.Left ? eE : eW) >= 1);
                string verdict;
                if (!upperOk) { slopeBadUpper++; verdict = "FAIL:上端高格方向不符"; }
                else if (!lowerOk) { slopeBadLower++; verdict = "FAIL:下端无低层陆地"; }
                else if (slopeUpper.ContainsKey(lowerY >= 0 ? lowerY * W + s.X : -1)) { slopeOk++; verdict = "OK:坡列(下段由下方坡上段承接)"; }
                else { slopeOk++; verdict = "OK:上端高台+下端低层陆地"; }
                p2.AppendLine($"{seed},{s.X},{s.Y},{(s.Side == OverlaySlopeSide.Left ? "_36" : "_39")},{s.Side},{t},{e},{tE},{eE},{tW},{eW},{(upperOk ? 1 : 0)},{lowerY},{lowerT},{lowerE},{(lowerOk ? 1 : 0)},{nbWallS},{nbWallN},{(topElevated ? 1 : 0)},{verdict}");
            }
            File.WriteAllText(Path.Combine(outDir, "p2-slopes-seed-" + seed + ".csv"), p2.ToString());

            var summary = new StringBuilder();
            summary.AppendLine($"## seed={seed}");
            summary.AppendLine($"P0: connections={connTotal} ok={connOk} missingWall={missingWall} missingGround={missingGround} missingMid={missingMid} illegalWaterFoot={illegalWaterFoot} waterCellWalls(legal)={waterBgBreach} northEdges={northEdges} narrowDitch={narrowDitch} unexplained={unexplained}");
            summary.AppendLine($"P1: inlandEdges={inlandEdges} waterEdges={waterEdges} inlandCoastalMisuse={inlandCoastal}");
            summary.AppendLine($"P2: slopes={slopeTotal} ok={slopeOk} badUpper={slopeBadUpper} badLower={slopeBadLower} overlapWarn={slopeConflictWall} ");
            File.WriteAllText(Path.Combine(outDir, "scan-summary-seed-" + seed + ".txt"), summary.ToString());
            Console.WriteLine(summary.ToString());
        }
    }

    private static readonly Dictionary<int, StairPlacement> stairsUpper = new Dictionary<int, StairPlacement>();



    /// <summary>Flat 层逐方向掩码（与渲染器 AccumulateFlatEdge 同规则：暴露=邻位地形不同；越界=陆暴露）。</summary>
    private static (int waterMask, int landMask) ExposureMasks(TerrainStore store, byte[] elevation, int x, int y)
    {
        var own = store.Get(x, y);
        int wm = 0, lm = 0;
        for (var d = 0; d < 4; d++)
        {
            int nx = x + (d == 0 ? 0 : d == 1 ? 1 : d == 2 ? 0 : -1);
            int ny = y + (d == 0 ? 1 : d == 1 ? 0 : d == 2 ? -1 : 0);
            var bit = d == 0 ? 1 : d == 1 ? 2 : d == 2 ? 4 : 8;
            if (nx < 0 || ny < 0 || nx >= W || ny >= H) { lm |= bit; continue; }
            var t = store.Get(nx, ny);
            if (t == own) continue;
            if (t == TerrainType.Water) wm |= bit; else lm |= bit;
        }
        return (wm, lm);
    }
}
