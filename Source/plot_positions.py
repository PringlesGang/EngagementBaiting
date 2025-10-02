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
for level_name, group in df.groupby("Level"):
    if group.empty:
        continue

    x = group["X"].values
    y = group["Y"].values
    t = group["SessionTime"].values

    # Normalize time for colormap
    norm = mcolors.Normalize(vmin=t.min(), vmax=t.max())
    cmap = cm.viridis

    # Create scatter plot with color = time
    plt.figure(figsize=(8, 6))
    sc = plt.scatter(x, y, c=t, cmap=cmap, s=8, alpha=0.8)

    # A line showing the full path (optional, makes trajectory clearer)
    plt.plot(x, y, linewidth=0.5, alpha=0.5, color="gray")

    plt.title(f"Player Movement in Level: {level_name}")
    plt.xlabel("X Position")
    plt.ylabel("Y Position")
    plt.gca().invert_yaxis()  # Celeste coordinate system: Y grows downward
    plt.colorbar(sc, label="Session Time (s)")
    plt.grid(True)

    plt.tight_layout()
    plt.show()
