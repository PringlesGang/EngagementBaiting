import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.cm as cm
import matplotlib.colors as mcolors

csv_file = "./../PlayerPositions.csv"

# Load CSV
df = pd.read_csv(csv_file, parse_dates=["Timestamp"])

# Ensure sorting by session time
df = df.sort_values(by=["Level", "SessionTime"]).reset_index(drop=True)

# Group by Level
for level_name, group in reversed(list(df.groupby("Level"))):
    if group.empty:
        continue

    plt.figure(figsize=(8, 6))

    # Get last session time to identify level transitions
    last_time = group["SessionTime"].max()

    for death, attempt in group.groupby("Deaths"):
        x = attempt["X"].values
        y = attempt["Y"].values
        t = attempt["SessionTime"].values

        # If not first attempt, then not include first coordinate since it is from previous attempt
        if death != 0:
            x = x[1:]
            y = y[1:]
            t = t[1:]

        # A line showing the full path with points
        color = plt.gca()._get_lines.get_next_color()
        plt.scatter(x, y, s=2, alpha=0.8, color=color)
        plt.plot(x, y, linewidth=0.5, alpha=0.5, color=color)

        # Add death markers except for when going to new level
        if t[-1] != last_time:
            # Only for the first death marker
            if "Death" not in [h.get_label() for h in plt.gca().get_legend_handles_labels()[0]]:
                plt.scatter(x[-1], y[-1], color="red", marker="x", s=40, label="Death")
            else:
                plt.scatter(x[-1], y[-1], color="red", marker="x", s=40)
        else:
            plt.scatter(x[-1], y[-1], color="green", marker="o", s=40, label="Level End")

    plt.title(f"Player Movement in Level: {level_name}")
    plt.xlabel("X Position")
    plt.ylabel("Y Position")
    plt.gca().invert_yaxis()  # Celeste coordinate system: Y grows downward
    plt.grid(True)
    plt.legend()
    plt.tight_layout()
    plt.show()
