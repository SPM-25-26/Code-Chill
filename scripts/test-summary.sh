#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TRX_PATH="$ROOT_DIR/src/backend/chill-code-backend.Tests/TestResults/TestResults.trx"
MD_PATH="$ROOT_DIR/src/backend/chill-code-backend.Tests/TestResults/TestSummary.md"
PREFIX="ChillCodeBackend.Tests."

dotnet test "$ROOT_DIR/src/backend/chill-code-backend.Tests" -v:n --logger "trx;LogFileName=TestResults.trx"

TRX_PATH_ESCAPED="$TRX_PATH"
MD_PATH_ESCAPED="$MD_PATH"

TRX_PATH="$TRX_PATH_ESCAPED" MD_PATH="$MD_PATH_ESCAPED" PREFIX="$PREFIX" python3 - <<'PY'
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path
import os

trx = Path(os.environ["TRX_PATH"])
md_path = Path(os.environ["MD_PATH"])
prefix = os.environ.get("PREFIX", "")
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

root = ET.parse(trx).getroot()

exec_to_class = {}
for ut in root.findall(".//t:UnitTest", ns):
    exec_el = ut.find(".//t:Execution", ns)
    tm = ut.find(".//t:TestMethod", ns)
    if exec_el is not None and tm is not None:
        exec_to_class[exec_el.get("id")] = tm.get("className")

stats = defaultdict(lambda: {"total": 0, "passed": 0, "failed": 0, "skipped": 0, "duration": 0.0})

def parse_duration(s: str) -> float:
    if not s:
        return 0.0
    h, m, rest = s.split(":")
    return int(h) * 3600 + int(m) * 60 + float(rest)

for r in root.findall(".//t:UnitTestResult", ns):
    exec_id = r.get("executionId")
    cls = exec_to_class.get(exec_id, "Unknown")
    outcome = (r.get("outcome") or "").lower()
    stats[cls]["total"] += 1
    stats[cls]["duration"] += parse_duration(r.get("duration"))
    if outcome == "passed":
        stats[cls]["passed"] += 1
    elif outcome == "failed":
        stats[cls]["failed"] += 1
    elif outcome == "skipped":
        stats[cls]["skipped"] += 1

def fmt_secs(s: float) -> str:
    return f"{s:.2f}"

def short_name(name: str) -> str:
    return name[len(prefix):] if name.startswith(prefix) else name

rows = []
totals = {"total": 0, "passed": 0, "failed": 0, "skipped": 0, "duration": 0.0}
for cls in sorted(stats):
    s = stats[cls]
    totals["total"] += s["total"]
    totals["passed"] += s["passed"]
    totals["failed"] += s["failed"]
    totals["skipped"] += s["skipped"]
    totals["duration"] += s["duration"]
    rows.append((
        short_name(cls),
        s["total"],
        s["failed"],
        s["passed"],
        s["skipped"],
        fmt_secs(s["duration"]),
    ))

header = "| Test Class | <span style=\"color:#1f77b4; font-weight:600\">Total</span> | <span style=\"color:#d62728; font-weight:600\">Failed</span> | <span style=\"color:#2ca02c; font-weight:600\">Passed</span> | <span style=\"color:#ff7f0e; font-weight:600\">Skipped</span> | Duration (s) |"
sep = "|---|---:|---:|---:|---:|---:|"
lines = [header, sep]
for r in rows:
    total_cell = f"<span style=\"color:#1f77b4; font-weight:600\">{r[1]}</span>"
    failed_cell = f"<span style=\"color:#d62728; font-weight:600\">{r[2]}</span>"
    passed_cell = f"<span style=\"color:#2ca02c; font-weight:600\">{r[3]}</span>"
    skipped_cell = f"<span style=\"color:#ff7f0e; font-weight:600\">{r[4]}</span>"
    lines.append(f"| {r[0]} | {total_cell} | {failed_cell} | {passed_cell} | {skipped_cell} | {r[5]} |")

total_row = (
    "<strong>Total</strong>",
    f"<span style=\"color:#1f77b4; font-weight:600\">{totals['total']}</span>",
    f"<span style=\"color:#d62728; font-weight:600\">{totals['failed']}</span>",
    f"<span style=\"color:#2ca02c; font-weight:600\">{totals['passed']}</span>",
    f"<span style=\"color:#ff7f0e; font-weight:600\">{totals['skipped']}</span>",
    fmt_secs(totals["duration"]),
)
lines.append(f"| {total_row[0]} | {total_row[1]} | {total_row[2]} | {total_row[3]} | {total_row[4]} | {total_row[5]} |")

md_path.write_text("\n".join([
    "# Test Summary (per Class)",
    "",
    *lines,
    "",
]), encoding="utf-8")

for r in rows:
    print(f"{r[0]}: insgesamt: {r[1]}; fehlgeschlagen: {r[2]}; erfolgreich: {r[3]}; übersprungen: {r[4]}; Dauer: {r[5]} Sek.")
print(f"Total: insgesamt: {totals['total']}; fehlgeschlagen: {totals['failed']}; erfolgreich: {totals['passed']}; übersprungen: {totals['skipped']}; Dauer: {fmt_secs(totals['duration'])} Sek.")
PY
