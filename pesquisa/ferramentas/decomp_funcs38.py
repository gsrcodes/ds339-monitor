"""Descompila, função por função, as funções de um .pyc 3.8 cujo caminho bate com um filtro (sem executar nada).

Uso: re-venv/Scripts/python.exe -I decomp_funcs38.py <arquivo.pyc> <saida.py> <filtro regex>
Uma função que o decompyle3 não consegue vira a desmontagem do xdis no lugar (o resto continua).
"""
import io
import re
import sys
import threading

from decompyle3.semantics.pysource import code_deparse
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
            if not rx.search(path) or "<listcomp>" in path or "<genexpr>" in path or "<lambda>" in path:
                continue
            out.write(f"\n# ==== {path} (linha {co.co_firstlineno})\n")
            buf = io.StringIO()
            result = {}

            def run():
                try:
                    code_deparse(co, out=buf, version=version)
                    result["ok"] = True
                except Exception as ex:  # noqa: BLE001
                    result["err"] = f"{type(ex).__name__}: {str(ex)[:150]}"

            t = threading.Thread(target=run, daemon=True)
            t.start()
            t.join(60)
            if result.get("ok"):
                out.write(buf.getvalue())
            else:
                out.write(f"# descompilação falhou ({result.get('err', 'demorou demais')}); desmontagem:\n")
                dis = io.StringIO()
                try:
                    disco(version, co, timestamp, dis)
                except Exception as ex:  # noqa: BLE001
                    dis.write(f"# xdis falhou: {ex}\n")
                out.write("\n".join("# " + l for l in dis.getvalue().split("\n")))
            out.flush()
            print(path, "ok" if result.get("ok") else "FALHOU", flush=True)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3])
