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

    for death, attempt in group.groupby("Deaths"):
        x = attempt["X"].values
        y = attempt["Y"].values
        t = attempt["SessionTime"].values

        # If not first attempt, then not include first coordinate since it is from previous attempt
        if death != 0:
            x = x[1:]
            y = y[1:]
            t = t[1:]

        # Create scatter plot with color = time
        plt.scatter(x, y, s=8, alpha=0.8)

        # A line showing the full path (optional, makes trajectory clearer)
        plt.plot(x, y, linewidth=0.5, alpha=0.5, color="gray")

        # Add death markers
        plt.scatter(x[-1], y[-1], color="red", marker="x", s=40, label="Death" if death == 0 else "")

    plt.title(f"Player Movement in Level: {level_name}")
    plt.xlabel("X Position")
    plt.ylabel("Y Position")
    plt.gca().invert_yaxis()  # Celeste coordinate system: Y grows downward
    plt.grid(True)
    plt.legend()
    plt.tight_layout()
    plt.show()
