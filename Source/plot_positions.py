import pandas as pd
import os
import glob
import matplotlib.pyplot as plt
import matplotlib.cm as cm
import matplotlib.colors as mcolors


def extract_archives(archive_path):
    """
    Extract all PlayerPositions.csv and EngagementBaiting-*.log files from every folder in the archive_path.
    Returns a list of tuples: (csv_path, log_path) for each folder found.
    """
    results = []
    # Each subfolder is named after the date and time it was saved
    for subdir in os.listdir(archive_path):
        folder_path = os.path.join(archive_path, subdir)
        if not os.path.isdir(folder_path):
            continue

        # Find PlayerPositions.csv
        csv_path = os.path.join(folder_path, "PlayerPositions.csv")
        if not os.path.isfile(csv_path):
            continue

        # Find EngagementBaiting-*.log (there may be multiple, pick the first)
        log_files = glob.glob(os.path.join(folder_path, "EngagementBaiting-*.log"))
        log_path = log_files[0] if log_files else None

        results.append((csv_path, log_path))
    return results

def plot_player_paths(csv_file):
    """
    Plot player path from the given CSV file.
    """
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

            if len(x) < 2: # Not enough data to plot
                continue

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

if __name__ == "__main__":
    archive_path = "./Logs/Archived/"
    archives = extract_archives(archive_path)
    print(archives)

    for csv_path, log_path in archives:
        plot_player_paths(csv_path)