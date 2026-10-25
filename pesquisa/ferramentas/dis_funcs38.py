"""Disassemble (never execute) functions of a 3.8 .pyc whose path matches a regex.
Usage: python -I dis_funcs38.py <file.pyc> <out.txt> <regex>"""
import io, re, sys
from xdis import load_module
from xdis.disasm import disco

def walk(code, path, found):
    found.append((path, code))
    for c in code.co_consts:
        if hasattr(c, "co_code"):
            walk(c, path + "." + c.co_name, found)

def main(pyc, target, pattern):
    version, timestamp, magic, code, pypy, source_size, sip_hash = load_module(pyc)
    found = []
    walk(code, "", found)
    rx = re.compile(pattern)
    with open(target, "w", encoding="utf-8") as out:
        for path, co in found:
            if not rx.search(path):
                continue
            out.write(f"\n# ==== {path} (linha {co.co_firstlineno})\n")
            dis = io.StringIO()
            disco(version, co, timestamp, dis)
            out.write("\n".join("# " + l for l in dis.getvalue().split("\n")))
            print(path, flush=True)

if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3])
