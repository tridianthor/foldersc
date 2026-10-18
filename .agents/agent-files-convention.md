# Agent Files Organization & Sorting Conventions

This document outlines the directory structure and sorting conventions for all agent-generated documentation in the `.agents/` directory (e.g., `issue/`, `feature/`, `plan/`, `changelogs/`, `report/`).

---

## 1. Directory Structure

All files within agent documentation categories must be organized chronologically by their **creation date** into year, month, and week subdirectories:

```text
.agents/<category>/<year>/<month>/week<N>/<file-name>.md
```

### Path Components
- **`<category>`**: The functional category (e.g., `issue`, `feature`, `plan`, `changelogs`, `report`).
- **`<year>`**: 4-digit year folder (e.g., `2026`, `2027`).
- **`<month>`**: Full lowercase English month name (e.g., `january`, `february`, `august`, `september`, `october`).
- **`week<N>`**: Week folder formatted as `week1`, `week2`, `week3`, `week4`, `week5`, etc.
- **`<file-name>.md>`**: Descriptive Markdown file name.

---

## 2. Calendar Week Calculation Rule

Week numbering follows the **Monday–Sunday Calendar Week** model:

1. **`week1`** always begins on the **1st day of the month** and ends on the **first Sunday** of that month (even if it spans only 1 or 2 days).
2. Each subsequent **Monday** begins a new week (`week2`, `week3`, etc.).
3. The last week continues through the end of the month.

### Mathematical Formula
For a given date with day $D$ in a month where the first day has weekday $W_1$ ($0 = \text{Monday}, \dots, 6 = \text{Sunday}$):
- First Sunday occurs on day: $D_{\text{first\_sunday}} = 1 + (6 - W_1)$
- If $D \le D_{\text{first\_sunday}}$, then **Week = 1**
- If $D > D_{\text{first\_sunday}}$, then **Week = $1 + \lfloor \frac{D - D_{\text{first\_sunday}} - 1}{7} \rfloor + 1$**

---

## 3. Python Reference Implementation

When creating or sorting files programmatically, use the following helper function:

```python
import datetime
import os

def get_week_of_month(dt: datetime.date) -> int:
    """
    Returns the week of the month (1-indexed) based on Monday–Sunday calendar weeks.
    Week 1 begins on day 1 and ends on the first Sunday of the month.
    Every subsequent Monday begins a new week.
    """
    first_day = dt.replace(day=1)
    first_sunday_day = 1 + (6 - first_day.weekday())
    if dt.day <= first_sunday_day:
        return 1
    days_after_first_sunday = dt.day - first_sunday_day
    return 1 + (days_after_first_sunday - 1) // 7 + 1


def get_agent_file_dir(category: str, dt: datetime.date, base_dir: str = ".agents") -> str:
    """
    Returns the destination directory path for an agent file.
    Example: .agents/issue/2026/august/week3
    """
    year = str(dt.year)
    month_name = dt.strftime("%B").lower()
    week_num = get_week_of_month(dt)
    return os.path.join(base_dir, category, year, month_name, f"week{week_num}")
```

---

## 4. Calendar Mapping Examples

### August 2026
- **Aug 1 – Aug 2** (Sat–Sun): `week1`
- **Aug 3 – Aug 9** (Mon–Sun): `week2`
- **Aug 10 – Aug 16** (Mon–Sun): `week3` (e.g., Aug 14)
- **Aug 17 – Aug 23** (Mon–Sun): `week4` (e.g., Aug 18–21)
- **Aug 24 – Aug 30** (Mon–Sun): `week5` (e.g., Aug 24, 27, 28)
- **Aug 31** (Mon): `week6`

### September 2026
- **Sep 1 – Sep 6** (Tue–Sun): `week1` (e.g., Sep 1)
- **Sep 7 – Sep 13** (Mon–Sun): `week2` (e.g., Sep 7, 8)
- **Sep 14 – Sep 20** (Mon–Sun): `week3`
- **Sep 21 – Sep 27** (Mon–Sun): `week4`
- **Sep 28 – Sep 30** (Mon–Wed): `week5` (e.g., Sep 30)

### October 2026
- **Oct 1 – Oct 4** (Thu–Sun): `week1` (e.g., Oct 1)
- **Oct 5 – Oct 11** (Mon–Sun): `week2`

---

## 5. Workflow for New Files

When creating new issue reports, feature specs, plans, changelogs, or report files:
1. Determine today's date.
2. Calculate the corresponding `<year>`, `<month>`, and `week<N>` folder using the convention above.
3. Place the file directly in `.agents/<category>/<year>/<month>/week<N>/`.
4. Avoid leaving loose `.md` files at the root of `.agents/<category>/` or `.agents/<category>/<year>/`.