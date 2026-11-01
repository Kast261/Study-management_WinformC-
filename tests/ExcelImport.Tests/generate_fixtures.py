"""Regenerate test workbooks using openpyxl and xlwt (development tools only)."""
from pathlib import Path
from openpyxl import Workbook
import xlwt

root = Path(__file__).with_name("Fixtures")
root.mkdir(exist_ok=True)
headers = ["Nội dung", "Đáp án A", "Đáp án B", "Đáp án C", "Đáp án D", "Đáp án đúng"]
rows = [
    ["Trong C#, từ khóa nào khai báo một lớp?", "class", "struct", "interface", "enum", "A"],
    ["2 + 3 bằng bao nhiêu?", "3", "4", "5", "6", "C"],
]


def save(name, data):
    book = Workbook()
    sheet = book.active
    sheet.title = "CauHoi"
    for row in data:
        sheet.append(row)
    book.save(root / name)


book = xlwt.Workbook(encoding="utf-8")
sheet = book.add_sheet("CauHoi")
for i, row in enumerate([headers] + rows):
    for j, value in enumerate(row):
        sheet.write(i, j, value)
book.save(str(root / "valid.xls"))

save("reordered.xlsx", [
    ["STT", "Đáp án đúng", "D", "Nội dung câu hỏi", "B", "A", "C"],
    [1, " b ", 4, "Câu hỏi tiếng Việt", 2, 1, 3],
])
save("blank-rows.xlsx", [headers, rows[0], [], [None] * 6, rows[1]])
save("invalid.xlsx", [headers, ["Câu hỏi", "", "B", "C", "D", "E"],
                      ["Câu hỏi dài", "x" * 256, "B", "C", "D", "A"]])
save("missing-column.xlsx", [headers[:-1], rows[0][:-1]])
save("duplicate-column.xlsx", [headers + ["NoiDung"], rows[0] + ["Trùng"]])
save("empty.xlsx", [headers])
save("no-header.xlsx", [["Ghi chú"], ["Không phải bảng câu hỏi"]])
save("too-many.xlsx", [headers] + [rows[0]] * 5001)
book = Workbook()
book.active.title = "HuongDan"
book.active.append(["Chỉ là hướng dẫn"])
sheet = book.create_sheet("CauHoi")
for row in [["Bộ câu hỏi"], [], headers] + rows:
    sheet.append(row)
book.save(root / "notes-first.xlsx")
print("Generated", len(list(root.iterdir())), "Excel fixtures")
