"""Descompila códigos marshal crus (Python 3.8) para .py, sem executar nada deles.

Uso: re-venv/Scripts/python.exe -I decomp38.py <arquivo.marshal ou pasta> <pasta de saída>
Cada .marshal ganha um cabeçalho de .pyc 3.8 e passa pelo decompyle3; se falhar, sai a desmontagem do xdis.
"""
import io
import os
import sys

from decompyle3.main import decompile
from xdis import load_module_from_file_object
from xdis.disasm import disassemble_file

HEADER = b"\x55\x0d\x0d\x0a" + b"\0" * 12


def one(src, out_dir):
    name = os.path.basename(src)[: -len(".marshal")]
    pyc = os.path.join(out_dir, name + ".pyc")
    with open(src, "rb") as f, open(pyc, "wb") as g:
        g.write(HEADER + f.read())
    target = os.path.join(out_dir, name + ".py")
    try:
        with open(pyc, "rb") as f:
            version, timestamp, magic, code, pypy, source_size, sip_hash = load_module_from_file_object(f, pyc)
        with open(target, "w", encoding="utf-8") as out:
            decompile(code, out=out, bytecode_version=version, showasm=False, showgrammar=False)
        print("ok", name)
    except Exception as ex:  # noqa: BLE001
        print("falhou", name, type(ex).__name__, str(ex)[:200])
        with open(os.path.join(out_dir, name + ".dis.txt"), "w", encoding="utf-8") as out:
            disassemble_file(pyc, outstream=out)


def main(src, out_dir):
    os.makedirs(out_dir, exist_ok=True)
    if os.path.isdir(src):
        for n in sorted(os.listdir(src)):
            if n.endswith(".marshal"):
                one(os.path.join(src, n), out_dir)
    else:
        one(src, out_dir)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
