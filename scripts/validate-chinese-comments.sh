#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -gt 0 ]; then
  targets=("$@")
else
  mapfile -t targets < <(git diff --cached --name-only --diff-filter=ACM)
fi

if [ "${#targets[@]}" -eq 0 ]; then
  echo "未检测到需要校验的文件。"
  exit 0
fi

python3 - <<'PY'
import re
import sys
from pathlib import Path

files = [Path(p) for p in sys.argv[1:]]
pattern = re.compile(r'[\u4e00-\u9fff]')
fail = False

for path in files:
    if not path.exists():
        continue
    if path.suffix.lower() not in {'.cs', '.ts', '.tsx', '.js'}:
        continue
    try:
        text = path.read_text(encoding='utf-8')
    except UnicodeDecodeError:
        text = path.read_text(encoding='utf-8', errors='ignore')

    has_cn = False

    for line in text.splitlines():
        idx = line.find('//')
        if idx != -1 and pattern.search(line[idx:]):
            has_cn = True
            break
        idx3 = line.find('///')
        if idx3 != -1 and pattern.search(line[idx3:]):
            has_cn = True
            break

    if not has_cn:
        for match in re.finditer(r'/\*.*?\*/', text, flags=re.S):
            if pattern.search(match.group(0)):
                has_cn = True
                break

    if not has_cn:
        print(f"缺少中文注释: {path}")
        fail = True

if fail:
    print("请为上述文件补充中文注释（仓库硬性要求）。")
    sys.exit(1)
else:
    print("中文注释校验通过。")
PY
