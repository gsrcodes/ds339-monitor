"""Lista funções e textos de um código marshal 3.8 (sem executar): <nome da função>: textos.

Uso: re-venv/Scripts/python.exe -I strings38.py <arquivo.pyc> <saida.txt>
"""
import sys

from xdis import load_module


def walk(code, path, out):
    texts = [c for c in code.co_consts if isinstance(c, str) and c.strip()]
    out.write(f"## {path} (linha {code.co_firstlineno})\n")
    for t in texts:
        out.write("   " + repr(t)[:300] + "\n")
    for c in code.co_consts:
        if hasattr(c, "co_code"):
            walk(c, path + "." + c.co_name, out)


def main(pyc, target):
    version, timestamp, magic, code, pypy, source_size, sip_hash = load_module(pyc)
    with open(target, "w", encoding="utf-8") as out:
        walk(code, code.co_name, out)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
