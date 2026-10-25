"""Desmonta um módulo .marshal (Python 3.11) com o xdis, sem executar nada dele.

Uso: python dis311.py <arquivo.marshal> [saida.txt]
"""
import io
import sys

import xdis
from xdis.unmarshal import load_code
from xdis.bytecode import Bytecode

MAGIC_311 = 3495  # 3.11 final


def dump(co, opc, out, indent=""):
    out.write(f"{indent}== code {co.co_name} line {co.co_firstlineno} args={co.co_varnames[:co.co_argcount]}\n")
    try:
        for ins in Bytecode(co, opc).get_instructions(co):
            out.write(f"{indent}{ins.offset:5d} {ins.opname:<26} {ins.argrepr or ''}\n")
    except Exception as ex:  # noqa: BLE001
        out.write(f"{indent}!! {ex!r}\n")
    for c in co.co_consts:
        if xdis.iscode(c):
            dump(c, opc, out, indent + "  ")


def main(path, dest=None):
    co = load_code(io.BytesIO(open(path, "rb").read()), MAGIC_311)
    opc = xdis.get_opcode_module((3, 11), "CPython")
    out = io.open(dest, "w", encoding="utf-8") if dest else sys.stdout
    dump(co, opc, out)


if __name__ == "__main__":
    main(*sys.argv[1:])
