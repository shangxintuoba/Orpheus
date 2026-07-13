"""
Excel -> JSON converter for the Unity DialogueManager, with a simple GUI.

Lets you pick an input .xlsx file and an output .json location with file
dialogs, then converts on button click. No command line needed once this
is packaged into an .exe (see build instructions at the bottom of the chat).

Expected Excel columns (exact names, case-sensitive):
    ID | Character | Text | nextID | isAChoice | nextID_true | nextID_false | nextID_notAnswered

Output is a plain JSON array (DialogueManager.cs wraps it itself), e.g.:
    [
      { "ID": 0, "Character": "Alice", "Text": "Hello there!", "nextID": 1,
        "isAChoice": false, "nextID_true": 0, "nextID_false": 0, "nextID_notAnswered": 0 },
      ...
    ]
"""

import json
import tkinter as tk
from tkinter import filedialog, messagebox
from pathlib import Path

try:
    import openpyxl
except ImportError:
    openpyxl = None

BOOL_COLUMNS = {"isAChoice", "isAutoPlay"}
INT_COLUMNS = {"ID", "nextID", "nextID_true", "nextID_false", "nextID_notAnswered"}
FLOAT_COLUMNS = {"playtime"}
STRING_COLUMNS = {"Character", "Text"}
REQUIRED_COLUMNS = BOOL_COLUMNS | INT_COLUMNS | FLOAT_COLUMNS | STRING_COLUMNS


def to_bool(value):
    if isinstance(value, bool):
        return value
    if value is None:
        return False
    return str(value).strip().lower() in ("true", "1", "yes")


def to_int(value):
    if value is None or str(value).strip() == "":
        return 0
    return int(value)


def to_float(value):
    if value is None or str(value).strip() == "":
        return 0.0
    return float(value)


def to_str(value):
    return "" if value is None else str(value)


def find_header_row(rows):
    """Scans rows top-down for the first one that contains most/all of the
    required column names (case-insensitively), instead of assuming row 1
    is always the header. Returns (header_row_index, col_index_map)."""
    required_lower = {name.lower() for name in REQUIRED_COLUMNS}

    best_row_idx = None
    best_col_index = None
    best_match_count = 0

    for row_idx, row in enumerate(rows):
        headers_lower = [str(c).strip().lower() if c is not None else "" for c in row]
        match_count = sum(1 for h in headers_lower if h in required_lower)
        if match_count > best_match_count:
            best_match_count = match_count
            best_row_idx = row_idx
            # Map each REQUIRED_COLUMNS canonical name -> its column index,
            # by matching case-insensitively against this row's headers.
            best_col_index = {}
            for name in REQUIRED_COLUMNS:
                if name.lower() in headers_lower:
                    best_col_index[name] = headers_lower.index(name.lower())

    if best_row_idx is None or best_match_count < len(REQUIRED_COLUMNS):
        found = set(best_col_index.keys()) if best_col_index else set()
        missing = REQUIRED_COLUMNS - found
        raise ValueError(
            f"Could not find a header row containing all required columns.\n"
            f"Missing (or misnamed): {', '.join(sorted(missing))}\n\n"
            f"Required column names (case doesn't matter):\n"
            f"{', '.join(sorted(REQUIRED_COLUMNS))}"
        )

    return best_row_idx, best_col_index


def convert(excel_path: Path, json_path: Path):
    """Does the actual conversion. Raises ValueError with a readable
    message on bad input, so the GUI can show it in a message box."""
    workbook = openpyxl.load_workbook(excel_path, data_only=True)
    sheet = workbook.active

    rows = list(sheet.iter_rows(values_only=True))
    if not rows:
        raise ValueError("The selected sheet is empty.")

    header_row_idx, col_index = find_header_row(rows)
    data_rows = rows[header_row_idx + 1:]

    entries = []
    for row_num, row in enumerate(data_rows, start=header_row_idx + 2):
        if all(cell is None for cell in row):
            continue

        def cell(column_name):
            idx = col_index[column_name]
            return row[idx] if idx < len(row) else None

        try:
            entry = {
                "ID": to_int(cell("ID")),
                "Character": to_str(cell("Character")),
                "Text": to_str(cell("Text")),
                "nextID": to_int(cell("nextID")),
                "isAChoice": to_bool(cell("isAChoice")),
                "nextID_true": to_int(cell("nextID_true")),
                "nextID_false": to_int(cell("nextID_false")),
                "nextID_notAnswered": to_int(cell("nextID_notAnswered")),
                "playtime": to_float(cell("playtime")),
                "isAutoPlay": to_bool(cell("isAutoPlay")),
            }
        except (ValueError, TypeError) as e:
            raise ValueError(f"Row {row_num}: {e}")

        entries.append(entry)

    json_path.write_text(json.dumps(entries, indent=2, ensure_ascii=False), encoding="utf-8")
    return len(entries)


class ConverterApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Dialogue Excel -> JSON Converter")
        self.geometry("520x220")
        self.resizable(False, False)

        self.excel_path = tk.StringVar()
        self.json_path = tk.StringVar()

        padding = {"padx": 10, "pady": 8}

        # --- Input file row ---
        tk.Label(self, text="Excel file (.xlsx):").grid(row=0, column=0, sticky="w", **padding)
        tk.Entry(self, textvariable=self.excel_path, width=45).grid(row=1, column=0, columnspan=2, sticky="w", padx=10)
        tk.Button(self, text="Browse...", command=self.pick_excel).grid(row=1, column=2, padx=10)

        # --- Output file row ---
        tk.Label(self, text="Output JSON location:").grid(row=2, column=0, sticky="w", **padding)
        tk.Entry(self, textvariable=self.json_path, width=45).grid(row=3, column=0, columnspan=2, sticky="w", padx=10)
        tk.Button(self, text="Browse...", command=self.pick_output).grid(row=3, column=2, padx=10)

        # --- Convert button ---
        tk.Button(
            self, text="Convert", command=self.run_conversion,
            width=20, height=2, bg="#4CAF50", fg="white"
        ).grid(row=4, column=0, columnspan=3, pady=20)

        # --- Status label ---
        self.status = tk.Label(self, text="", fg="gray")
        self.status.grid(row=5, column=0, columnspan=3)

        if openpyxl is None:
            messagebox.showerror(
                "Missing dependency",
                "The 'openpyxl' library is not installed.\n\n"
                "If running from source: pip install openpyxl\n"
                "If running the .exe: this should be bundled already — "
                "please rebuild it (see setup instructions)."
            )

    def pick_excel(self):
        path = filedialog.askopenfilename(
            title="Select Excel file",
            filetypes=[("Excel files", "*.xlsx")]
        )
        if path:
            self.excel_path.set(path)
            # Auto-fill a sensible default output path next to the input file.
            if not self.json_path.get():
                default_out = str(Path(path).with_suffix(".json"))
                self.json_path.set(default_out)

    def pick_output(self):
        path = filedialog.asksaveasfilename(
            title="Choose output location",
            defaultextension=".json",
            filetypes=[("JSON files", "*.json")]
        )
        if path:
            self.json_path.set(path)

    def run_conversion(self):
        excel_str = self.excel_path.get().strip()
        json_str = self.json_path.get().strip()

        if not excel_str:
            messagebox.showwarning("Missing input", "Please select an Excel file first.")
            return
        if not json_str:
            messagebox.showwarning("Missing output", "Please choose an output location first.")
            return
        if openpyxl is None:
            messagebox.showerror("Missing dependency", "openpyxl is not installed.")
            return

        excel_path = Path(excel_str)
        json_path = Path(json_str)

        if not excel_path.exists():
            messagebox.showerror("File not found", f"Could not find:\n{excel_path}")
            return

        self.status.config(text="Converting...", fg="gray")
        self.update_idletasks()

        try:
            count = convert(excel_path, json_path)
        except Exception as e:
            self.status.config(text="Conversion failed.", fg="red")
            messagebox.showerror("Conversion failed", str(e))
            return

        self.status.config(text=f"Done — {count} rows converted.", fg="green")
        messagebox.showinfo("Success", f"Converted {count} rows.\nSaved to:\n{json_path}")


if __name__ == "__main__":
    app = ConverterApp()
    app.mainloop()
