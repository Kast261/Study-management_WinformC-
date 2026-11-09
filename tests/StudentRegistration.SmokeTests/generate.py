"""Extract production event handlers so the smoke test cannot drift from the form."""
import argparse
import re
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--source', type=Path)
args = parser.parse_args()
here = Path(__file__).resolve().parent
source = (args.source or here.parents[1] / 'Forms/HocSinh/Frm_HocVien.cs').read_text(encoding='utf-8-sig')
names = ['btnDangKyLop_Click', 'btnHV_HuyDK_Click', 'OnActivated',
         'LamMoiTrangDangXem', 'LoadTabLopHocCuaToi']
methods = []
for name in names:
    # These methods contain no braces in string literals except balanced interpolation.
    declaration = re.search(r'(?m)^        (?:private|protected override) [^\n]+\b' + re.escape(name) + r'\(', source)
    if declaration is None:
        raise RuntimeError('Production handler not found: ' + name)
    start = declaration.start()
    opening = source.index('{', start)
    depth = 1
    end = opening + 1
    while depth:
        depth += (source[end] == '{') - (source[end] == '}')
        end += 1
    methods.append(source[start:end])
output = here / 'obj'
output.mkdir(exist_ok=True)
(output / 'ProductionHandlers.cs').write_text('''
using System;
using System.Data;
using System.Windows.Forms;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using MessageBox = DialogStub;
partial class RegistrationHost
{
''' + '\n'.join(methods) + '\n}\n')
