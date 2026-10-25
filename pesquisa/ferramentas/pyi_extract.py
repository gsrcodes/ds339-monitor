"""Extrai o arquivo do PyInstaller (CArchive + PYZ) de um executável, sem executar nada dele.

Uso: python -I pyi_extract.py <exe> <pasta de saída>
Grava cada módulo do PYZ como <nome>.marshal (código marshal cru, sem cabeçalho de .pyc) e os scripts de entrada.
"""
import os
import struct
import sys
import zlib

MAGIC = b"MEI\014\013\012\013\016"


def main(exe, out):
    data = open(exe, "rb").read()
    pos = data.rfind(MAGIC)
    if pos < 0:
        sys.exit("cookie do PyInstaller não encontrado")
    # Cookie (PyInstaller >= 2.1): magic(8) len(4) toc(4) toclen(4) pyver(4) pylibname(64)
    length, toc, toclen, pyver = struct.unpack("!IIII", data[pos + 8:pos + 24])
    cookie_size = 24 + 64
    start = pos + cookie_size - length
    print(f"pacote em {start}, toc {toc}+{toclen}, python {pyver}")
    os.makedirs(out, exist_ok=True)
    entries = []
    p = start + toc
    end = p + toclen
    while p < end:
        size, epos, csize, usize, cflag, typ = struct.unpack("!IIIIBc", data[p:p + 18])
        name = data[p + 18:p + size].rstrip(b"\0").decode("utf-8", "replace")
        entries.append((name, epos, csize, usize, cflag, typ.decode()))
        p += size
    for name, epos, csize, usize, cflag, typ in entries:
        blob = data[start + epos:start + epos + csize]
        if cflag:
            blob = zlib.decompress(blob)
        if typ in ("s", "m", "M"):
            path = os.path.join(out, "_entry", name.replace("/", "_") + ".marshal")
            os.makedirs(os.path.dirname(path), exist_ok=True)
            open(path, "wb").write(blob)
            print("script", name, typ, len(blob))
        elif typ == "z":
            extract_pyz(blob, os.path.join(out, "pyz"))
        else:
            pass  # binários e dados: já estão na pasta _internal


def extract_pyz(blob, out):
    import marshal
    assert blob[:4] == b"PYZ\0", blob[:4]
    toc_pos = struct.unpack("!I", blob[8:12])[0]
    toc = marshal.loads(blob[toc_pos:])
    if isinstance(toc, dict):
        toc = list(toc.items())
    n = 0
    for name, info in toc:
        if isinstance(name, bytes):
            name = name.decode()
        typ, pos, length = info
        raw = blob[pos:pos + length]
        try:
            code = zlib.decompress(raw)
        except zlib.error:
            print("não descomprimiu (cifrado?)", name)
            continue
        path = os.path.join(out, name.replace(".", os.sep) + (os.sep + "__init__" if typ == 1 else "") + ".marshal")
        os.makedirs(os.path.dirname(path), exist_ok=True)
        open(path, "wb").write(code)
        n += 1
    print("pyz:", n, "módulos")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
