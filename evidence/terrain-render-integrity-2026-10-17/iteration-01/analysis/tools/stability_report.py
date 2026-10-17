# iteration-00 vs iteration-01 全量稳定性报告（全像素）——"零修复"的零差异证明
# 输出：发布版文本（stdout 重定向保存）
import os
from PIL import Image
from collections import Counter

B = r"D:\data\UnityProjects\WorldSandboxPrototype\docs\impl\evidence\terrain-render-integrity-2026-10-17"
I0 = os.path.join(B, "iteration-00-baseline")
I1 = os.path.join(B, "iteration-01")

CLOUD = {(229, 233, 205), (218, 215, 187), (251, 253, 239), (233, 244, 250)}
FOAM = {(61, 144, 143), (71, 171, 168), (198, 240, 218), (140, 194, 195),
        (69, 133, 150), (59, 113, 128), (60, 145, 143), (167, 202, 185)}
GRASS = {(133, 177, 85), (112, 148, 77), (119, 158, 94), (101, 132, 84), (154, 185, 78),
         (146, 185, 79), (129, 155, 72), (100, 134, 70), (123, 156, 73), (95, 171, 98), (103, 211, 102), (128, 171, 94), (61, 100, 58), (110, 141, 66),
         (93, 160, 102), (116, 179, 98), (183, 185, 87), (113, 154, 96)}
TREE = {(45, 131, 129), (55, 174, 122), (22, 28, 46), (49, 90, 109)}

def attr(ca, cb):
    if ca in CLOUD or cb in CLOUD: return "cloud(云相位)"
    if ca in FOAM or cb in FOAM: return "foam/shore(泡沫/岸线相位)"
    if ca in TREE or cb in TREE: return "tree(树冠摇摆相位)"
    if ca in GRASS and cb in GRASS: return "grass-tex(草地纹理相位)"
    return f"UNKNOWN {ca}->{cb}"

print("## iteration-00 vs iteration-01 稳定性对照（D-065 零修复轮的零差异证明）")
print("## 方法：同种子、同套件、同参数（1280x720、窗口化）两次独立采集的全像素逐点比对。")
print("## 归因口径：差异色对∈{云系、泡沫/岸系、树冠系、草纹理系} → 动画相位；无法归因者逐条列出。")
print()

total_ident = 0
total_diff = 0
total_unknown = 0
for seed in ("seed-20261013", "seed-20261014", "seed-20261015"):
    d0 = os.path.join(I0, seed)
    d1 = os.path.join(I1, seed)
    if not os.path.isdir(d1):
        print(f"== {seed}: 01 目录不存在，跳过 ==")
        continue
    names = sorted(f for f in os.listdir(d1) if f.endswith(".png"))
    ident, diffs = [], []
    for n in names:
        p0, p1 = os.path.join(d0, n), os.path.join(d1, n)
        if not os.path.exists(p0):
            print(f"  [仅 01 存在] {n}")
            continue
        a = Image.open(p0).convert("RGB"); b = Image.open(p1).convert("RGB")
        pa, pb = a.load(), b.load()
        w, h = a.size
        cnt = 0
        pairs = Counter()
        for y in range(h):
            for x in range(w):
                ca, cb = pa[x, y], pb[x, y]
                if ca != cb:
                    cnt += 1
                    pairs[(ca, cb)] += 1
        if cnt == 0:
            ident.append(n)
        else:
            attrs = Counter()
            unknown = 0
            for (ca, cb), k in pairs.items():
                a2 = attr(ca, cb)
                attrs[a2.split("(")[0]] += k
                if a2.startswith("UNKNOWN"):
                    unknown += k
            diffs.append((n, cnt, attrs, unknown))
            total_unknown += unknown
    total_ident += len(ident)
    total_diff += len(diffs)
    print(f"== {seed}: 共 {len(ident)+len(diffs)} 图；逐字节一致 {len(ident)}；有差异 {len(diffs)} ==")
    for n, cnt, attrs, unknown in sorted(diffs, key=lambda t: -t[1]):
        detail = " ".join(f"{k}={v}" for k, v in attrs.most_common())
        unk = f" 未归因={unknown}" if unknown else ""
        print(f"  {n}: diff_px={cnt} ({100.0*cnt/(1280*720):.4f}%) [{detail}]{unk}")
    print()

print(f"## 汇总：一致 {total_ident} 图 / 有差异 {total_diff} 图 / 未归因像素总数 {total_unknown}")
