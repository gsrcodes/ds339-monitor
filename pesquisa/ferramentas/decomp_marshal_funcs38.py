"""Decompile functions (matching a regex) from a raw marshalled 3.8 code object, without executing it.
Usage: python -I decomp_marshal_funcs38.py <in.marshal> <out.py> <regex>
"""
import io, re, sys, threading
from xdis.unmarshal import load_code
from xdis.magics import magic2int, by_version
from decompyle3.semantics.pysource import code_deparse
from xdis.disasm import disco

def walk(code, path, found):
    found.append((path, code))
    for c in code.co_consts:
        if hasattr(c, "co_code"):
            walk(c, path + "." + c.co_name, found)

def main(src, dst, pattern):
    mi = magic2int(by_version["3.8.0"] if "3.8.0" in by_version else by_version["3.8"])
    with open(src, "rb") as f:
        co = load_code(f, mi)
    found = []
    walk(co, "", found)
    rx = re.compile(pattern)
    with open(dst, "w", encoding="utf-8") as out:
        for path, c in found:
            if not rx.search(path) or "<listcomp>" in path or "<genexpr>" in path:
                continue
            out.write(f"\n# ==== {path} (line {c.co_firstlineno})\n")
            buf = io.StringIO(); res = {}
            def run():
                try:
                    code_deparse(c, out=buf, version=(3, 8)); res["ok"] = True
                except Exception as ex:
                    res["err"] = repr(ex)[:200]
            t = threading.Thread(target=run, daemon=True); t.start(); t.join(90)
            if res.get("ok"):
                out.write(buf.getvalue())
            else:
                out.write("# decompile failed (%s); disassembly:\n" % res.get("err", "timeout"))
                d = io.StringIO()
                try:
                    disco((3, 8), c, 0, d)
                except Exception as ex:
                    d.write("xdis failed %r" % ex)
                out.write("\n".join("# " + l for l in d.getvalue().split("\n")))
            out.flush()
            print(path, "ok" if res.get("ok") else "FAILED", flush=True)

if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3])
