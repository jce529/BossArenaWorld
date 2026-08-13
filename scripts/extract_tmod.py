import struct, zlib, os, sys

def read_7bit_int(f):
    result, shift = 0, 0
    while True:
        b = f.read(1)[0]
        result |= (b & 0x7f) << shift
        if not (b & 0x80): break
        shift += 7
    return result

def read_string(f):
    return f.read(read_7bit_int(f)).decode('utf-8')

def extract_tmod(tmod_path, out_dir):
    with open(tmod_path, 'rb') as f:
        assert f.read(4) == b'TMOD'
        tml_version = read_string(f)
        f.read(20)   # SHA1 hash
        f.read(256)  # signature
        struct.unpack('<i', f.read(4))[0]  # data length
        mod_name = read_string(f)
        mod_version = read_string(f)
        file_count = struct.unpack('<i', f.read(4))[0]
        entries = [(read_string(f), *struct.unpack('<ii', f.read(8))) for _ in range(file_count)]
        offset = f.tell()
        os.makedirs(out_dir, exist_ok=True)
        for name, ulen, clen in entries:
            f.seek(offset)
            raw = f.read(clen)
            data = zlib.decompress(raw, -15) if clen != ulen else raw
            path = os.path.join(out_dir, name.replace('/', os.sep))
            os.makedirs(os.path.dirname(path) or '.', exist_ok=True)
            open(path, 'wb').write(data)
            offset += clen

if __name__ == '__main__':
    extract_tmod(sys.argv[1], sys.argv[2])
