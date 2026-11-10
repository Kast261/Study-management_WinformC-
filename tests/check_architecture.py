"""Check the coursework architecture boundary and legacy project inclusion."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
errors = []
for source in (root / 'Forms').rglob('*.cs'):
    text = source.read_text(encoding='utf-8-sig')
    text = re.sub(r'/\*.*?\*/|//[^\n]*', '', text, flags=re.S)
    if re.search(r'BLT_Quan_Ly_Hoc_Tap\.(DAL|Database)|\bDatabaseHelper\b|\bNpgsql\w*\b|\bnew\s+\w+DAL\b', text):
        errors.append(f'{source.relative_to(root)} bypasses BLL')
project = ET.parse(root / 'BLT_Quan_Ly_Hoc_Tap.csproj')
ns = {'m': 'http://schemas.microsoft.com/developer/msbuild/2003'}
included = {n.attrib['Include'].replace('\\', '/') for n in project.findall('.//m:Compile', ns)}
for directory in ['BLL', 'Forms']:
    for source in (root / directory).rglob('*.cs'):
        if source.relative_to(root).as_posix() not in included:
            errors.append(f'{source.relative_to(root)} missing from csproj')
contracts = list((root / 'BLL/Interfaces').glob('I*Service.cs'))
if len(contracts) < 5:
    errors.append('Fewer than five application service interfaces')
for contract in contracts:
    name = contract.stem
    implementation = root / 'BLL/Services' / (name[1:] + '.cs')
    if not implementation.exists() or not re.search(r':\s*' + name + r'\b', implementation.read_text()):
        errors.append(f'{name} has no concrete implementation')
if errors:
    raise SystemExit('\n'.join(errors))
print(f'PASS: GUI uses BLL, {len(contracts)} service contracts implemented, source files included in csproj.')
