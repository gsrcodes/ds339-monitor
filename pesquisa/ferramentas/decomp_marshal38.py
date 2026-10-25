"""Decompile a raw marshalled Python 3.8 code object (PyInstaller pyz entry) without executing it.
Usage: python -I decomp_marshal38.py <in.marshal> <out.py>
"""
import io, sys, threading
from xdis.unmarshal import load_code
from xdis.magics import magic2int, by_version
from decompyle3.semantics.pysource import code_deparse
from xdis.disasm import disco

def main(src, dst):
    magic = by_version["3.8.0"] if "3.8.0" in by_version else by_version["3.8"]
    mi = magic2int(magic)
    with open(src, "rb") as f:
        co = load_code(f, mi)
    buf = io.StringIO()
    res = {}
    def run():
        try:
            code_deparse(co, out=buf, version=(3, 8))
            res["ok"] = True
        except Exception as ex:
            res["err"] = repr(ex)[:300]
    t = threading.Thread(target=run, daemon=True); t.start(); t.join(300)
    with open(dst, "w", encoding="utf-8") as out:
        if res.get("ok"):
            out.write(buf.getvalue())
        else:
            out.write("# decompile failed: %s\n" % res.get("err", "timeout"))
            d = io.StringIO()
            disco((3, 8), co, 0, d)
            out.write(d.getvalue())
    print("ok" if res.get("ok") else "FAILED", res.get("err"))

if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
