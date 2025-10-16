import pandas as pd
import os
import glob
import matplotlib.pyplot as plt
import matplotlib.image as mpimg


# Variables
REAL_SCALAR = 8
LEVEL_DATA = {
    "StartWalkRoomTutorial": {"img": "./Maps/Images/Tutorial.png", "offset": (-74, -68)},
    "JumpRoomTutorial": {"img": "./Maps/Images/Tutorial.png", "offset": (-74, -68)},
    "SecondRoomTutorial": {"img": "./Maps/Images/Tutorial.png", "offset": (-74, -68)},
    "FourthDashRoomTutorial": {"img": "./Maps/Images/Tutorial.png", "offset": (-74, -68)},
    "SpringRoom": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "SpringRoom2": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "Boosterroom1": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "Boosterroom2": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "Boosterroom3": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "CloudAndFeatherRoom": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "CloudAndFeatherRoom2": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
    "FinalRoom": {"img": "./Maps/Images/Final-Level.png", "offset": (-56, -167)},
}


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

        # Find EngagementBaiting-*.log
        log_files = glob.glob(os.path.join(folder_path, "EngagementBaiting-*.log"))
        log_path = log_files if log_files else None

        results.append((csv_path, log_path))
    return results

def plot_player_paths(csv_file, graph_offset=50):
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

        # Get image and offset info
        level_info = LEVEL_DATA.get(level_name, {"img": "./Maps/Images/Tutorial.png", "offset": None})
        img_path = level_info["img"]
        img = mpimg.imread(img_path)
        # img = img[::-1, :]  # Flip vertically
        if level_info["offset"]:
            x, y = level_info["offset"]
            x, y = x * REAL_SCALAR, y * REAL_SCALAR
            extent = [x, img.shape[1]+x, img.shape[0]+y, y]
        else:
            extent = [0, img.shape[1], img.shape[0], 0]

        plt.imshow(img, extent=extent, origin="upper")

        for death, attempt in group.groupby("Deaths"):
            x = attempt["X"].values
            y = attempt["Y"].values

            if len(x) < 2: # Not enough data to plot
                continue

            # If not first attempt, then not include first coordinate since it is from previous attempt
            if death != 0:
                x = x[1:]
                y = y[1:]

            # A line showing the full path with points
            color = plt.gca()._get_lines.get_next_color()
            plt.scatter(x, y, s=2, alpha=0.8, color=color)
            plt.plot(x, y, linewidth=0.5, alpha=0.5, color=color)

            # Add death markers except for when going to new level
            if attempt["SessionTime"].iloc[-1] == group["SessionTime"].max():
                plt.scatter(x[-1], y[-1], color="green", marker="o", s=40, label="Level End")
            else:
                # Only for the first death marker
                if "Death" not in [h.get_label() for h in plt.gca().get_legend_handles_labels()[0]]:
                    plt.scatter(x[-1], y[-1], color="red", marker="x", s=40, label="Death")
                else:
                    plt.scatter(x[-1], y[-1], color="red", marker="x", s=40)
                
        # Zoom in on the player area
        plt.xlim(group["X"].min() - graph_offset, group["X"].max() + graph_offset)
        plt.ylim(group["Y"].max() + graph_offset, group["Y"].min() - graph_offset)

        # Add title and labels
        plt.title(f"Player Movement in Level: {level_name}")
        plt.xlabel("X Position")
        plt.ylabel("Y Position")
        plt.grid(True)
        plt.legend()
        plt.tight_layout()
        plt.show()


if __name__ == "__main__":
    archive_path = "./Logs/Archived/"
    archives = extract_archives(archive_path)

    for csv_path, log_paths in archives:
        plot_player_paths(csv_path)