"""Junta os textos em chinês de strings.txt por função (para achar as listas de efeitos)."""
import re
import sys

src, target = sys.argv[1], sys.argv[2]
cur = ""
hits = {}
for line in open(src, encoding="utf-8").read().split("\n"):
    if line.startswith("## "):
        cur = line[3:].split(" (")[0].replace("<module>.", "")
        continue
    if re.search(r"[一-鿿]", line):
        hits.setdefault(cur, []).append(line.strip())
with open(target, "w", encoding="utf-8") as out:
    for k, v in hits.items():
        out.write(f"## {k} ({len(v)})\n")
        for x in v:
            out.write("    " + x[:200] + "\n")
