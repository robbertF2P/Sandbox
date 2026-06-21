#!/usr/bin/env python3
"""Generate PDF from the team summary markdown using fpdf2."""

import re
from pathlib import Path

from fpdf import FPDF

DOCS = Path(__file__).resolve().parent
MD_PATH = DOCS / "copilot-ado-jira-review-summary.md"
PDF_PATH = DOCS / "copilot-ado-jira-review-summary.pdf"


FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
FONT_MONO = "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf"


class SummaryPDF(FPDF):
    def __init__(self) -> None:
        super().__init__()
        self.add_font("DejaVu", "", FONT)
        self.add_font("DejaVu", "B", FONT_BOLD)
        self.add_font("DejaVu", "I", FONT)
        self.add_font("DejaVuMono", "", FONT_MONO)

    def header(self) -> None:
        self.set_font("DejaVu", "I", 8)
        self.set_text_color(120, 120, 120)
        self.cell(0, 8, "Floorganise — Copilot + ADO + Jira + Slack Summary", align="R")
        self.ln(4)

    def footer(self) -> None:
        self.set_y(-12)
        self.set_font("DejaVu", "I", 8)
        self.set_text_color(120, 120, 120)
        self.cell(0, 8, f"Page {self.page_no()}/{{nb}}", align="C")


def write_wrapped(pdf: SummaryPDF, text: str, size: int = 10, style: str = "") -> None:
    pdf.set_font("DejaVu", style, size)
    pdf.set_text_color(30, 30, 30)
    pdf.multi_cell(0, 5, text)
    pdf.ln(1)


def parse_table(lines: list[str]) -> tuple[list[str], list[list[str]]] | None:
    if len(lines) < 2 or "|" not in lines[0]:
        return None
    rows = []
    for line in lines:
        if not line.strip().startswith("|"):
            break
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if all(set(c) <= {"-", ":", " "} for c in cells):
            continue
        rows.append(cells)
    if len(rows) < 1:
        return None
    return rows[0], rows[1:]


def render_table(pdf: SummaryPDF, headers: list[str], rows: list[list[str]]) -> None:
    col_count = len(headers)
    width = (pdf.w - pdf.l_margin - pdf.r_margin) / col_count
    pdf.set_font("DejaVu", "B", 8)
    pdf.set_fill_color(245, 245, 245)
    for h in headers:
        pdf.cell(width, 6, h[:40], border=1, fill=True)
    pdf.ln()
    pdf.set_font("DejaVu", "", 8)
    for row in rows:
        if len(row) < col_count:
            row = row + [""] * (col_count - len(row))
        for cell in row[:col_count]:
            pdf.cell(width, 6, cell[:50], border=1)
        pdf.ln()
    pdf.ln(2)


def main() -> None:
    pdf = SummaryPDF()
    pdf.alias_nb_pages()
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.add_page()

    lines = MD_PATH.read_text(encoding="utf-8").splitlines()
    i = 0
    in_code = False
    code_buf: list[str] = []

    while i < len(lines):
        line = lines[i]

        if line.strip().startswith("```"):
            if in_code:
                pdf.set_font("DejaVuMono", "", 8)
                pdf.set_fill_color(244, 244, 244)
                pdf.multi_cell(0, 4, "\n".join(code_buf), fill=True)
                pdf.ln(2)
                code_buf = []
                in_code = False
            else:
                in_code = True
            i += 1
            continue

        if in_code:
            code_buf.append(line)
            i += 1
            continue

        if line.strip() == "---":
            pdf.ln(2)
            i += 1
            continue

        if line.startswith("# "):
            pdf.set_font("DejaVu", "B", 16)
            pdf.set_text_color(20, 20, 20)
            pdf.multi_cell(0, 8, line[2:].strip())
            pdf.ln(2)
            i += 1
            continue

        if line.startswith("## "):
            pdf.ln(3)
            pdf.set_font("DejaVu", "B", 13)
            pdf.set_text_color(30, 30, 30)
            pdf.multi_cell(0, 7, line[3:].strip())
            pdf.ln(1)
            i += 1
            continue

        if line.startswith("### "):
            pdf.ln(2)
            pdf.set_font("DejaVu", "B", 11)
            pdf.multi_cell(0, 6, line[4:].strip())
            pdf.ln(1)
            i += 1
            continue

        if line.strip().startswith("|"):
            table_lines = []
            while i < len(lines) and lines[i].strip().startswith("|"):
                table_lines.append(lines[i])
                i += 1
            parsed = parse_table(table_lines)
            if parsed:
                render_table(pdf, parsed[0], parsed[1])
            continue

        if line.strip().startswith("- "):
            text = re.sub(r"\*\*(.+?)\*\*", r"\1", line.strip()[2:])
            write_wrapped(pdf, f"  • {text}", size=10)
            i += 1
            continue

        if re.match(r"^\d+\.\s", line.strip()):
            text = re.sub(r"\*\*(.+?)\*\*", r"\1", line.strip())
            write_wrapped(pdf, f"  {text}", size=10)
            i += 1
            continue

        if line.strip():
            text = re.sub(r"\*\*(.+?)\*\*", r"\1", line.strip())
            text = re.sub(r"`([^`]+)`", r"\1", text)
            text = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", text)
            write_wrapped(pdf, text, size=10)
        i += 1

    pdf.output(str(PDF_PATH))
    print(f"Created {PDF_PATH} ({PDF_PATH.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
