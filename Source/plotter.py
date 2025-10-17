import os
import glob
import re
import pandas as pd
from dataclasses import dataclass
from typing import List, Dict
import matplotlib.pyplot as plt
import matplotlib.image as mpimg


# Constants
REAL_SCALAR = 8

@dataclass(frozen=True)
class LevelInfo:
    name: str
    img_path: str
    offset: tuple
    rooms: List[str]

LEVEL_DATA: Dict[str, LevelInfo] = {
    "Tutorial": LevelInfo(
        name = "Tutorial",
        img_path="./Maps/Images/Tutorial.png",
        offset=(-74, -68),
        rooms=[
            "StartWalkRoomTutorial",
            "JumpRoomTutorial",
            "SecondRoomTutorial",
            "FourthDashRoomTutorial",
        ],
    ),
    "Test_level": LevelInfo(
        name = "Test_level",
        img_path = "./Maps/Images/Final-Level.png",
        offset = (-56, -167),
        rooms = [
            "SpringRoom",
            "SpringRoom2",
            "Boosterroom1",
            "Boosterroom2",
            "Boosterroom3",
            "CloudAndFeatherRoom",
            "CloudAndFeatherRoom2",
            "FinalRoom",
        ],
    ),
}

# Reverse index for fast lookups by room name
ROOM_TO_LEVEL: Dict[str, LevelInfo] = {}
for level in LEVEL_DATA.values():
    for room in level.rooms:
        if room in ROOM_TO_LEVEL:
            raise ValueError(f"Duplicate room '{room}' assigned to multiple levels")
        ROOM_TO_LEVEL[room] = level

def get_level_info_by_room(room_name: str) -> LevelInfo | None:
    """
    Return LevelInfo for a given room name, or None if unknown.
    """
    return ROOM_TO_LEVEL.get(room_name)

def extract_archives(archive_path: str) -> dict:
    """
    Extract all PlayerPositions.csv and EngagementBaiting-*.log files from every folder in the archive_path.
    Returns a list of tuples: (csv_path, log_path) for each folder found.
    """
    user_id = 0 # TEMP until user ID is added to logs
    results = dict()
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

        # TODO: Input user id logic here if needed
        results[user_id] = (csv_path, log_path)
        user_id += 1 
    return results

def extract_logdata(log_path: str) -> pd.DataFrame:
    """
    Extract log data from a single log file into a DataFrame.
    returns: ['timestamp', 'room', 'x', 'y', 'sentiment', 'message']
    """
    if not os.path.isfile(log_path):
        return pd.DataFrame()

    # Regex patterns
    room_re = re.compile(r'Entering screen "([^"]+)"')
    message_re = re.compile(r'Showing (\w+) death screen message "([^"]+)"')
    death_re = re.compile(r'The player died at {X:(-?\d+)\s+Y:(-?\d+)}')
    timestamp_re = re.compile(r'\[(.*?)\]')

    data = []
    current_room = None
    current_event = {}

    with open(log_path, 'r', encoding='utf-8') as f:
        for line in f:
            # Extract timestamp
            timestamp_match = timestamp_re.search(line)
            timestamp = timestamp_match.group(1) if timestamp_match else None

            # Detect entering a new room
            room_match = room_re.search(line)
            if room_match:
                current_room = room_match.group(1)
                continue

            # Detect death message and sentiment
            msg_match = message_re.search(line)
            if msg_match:
                sentiment, message = msg_match.groups()
                current_event = {
                    "timestamp": timestamp,
                    "room": current_room,
                    "x": None,
                    "y": None,
                    "sentiment": sentiment,
                    "message": message
                }
                continue

            # Detect coordinates of death
            death_match = death_re.search(line)
            if death_match and current_event:
                x, y = map(int, death_match.groups())
                current_event["x"] = x
                current_event["y"] = y
                data.append(current_event)
                current_event = {}

    log_df = pd.DataFrame(data, columns=['timestamp', 'room', 'x', 'y', 'sentiment', 'message'])
    return log_df

def combine_log_user(log_paths: list) -> pd.DataFrame:
    """
    Combine multiple log files for a single user into one DataFrame.
    """
    user_logs = []
    for log_path in log_paths:
        if log_path:
            log_df = extract_logdata(log_path)
            user_logs.append(log_df)

    if user_logs:
        return pd.concat(user_logs, ignore_index=True)
    return user_logs

def get_img_level(level_name: str):
    """
    create image and extent for plotting based on level name
    """
    level_info = get_level_info_by_room(level_name)
    if level_info is None:
        # default fallback to Tutorial
        level_info = LEVEL_DATA["Tutorial"]

    img = mpimg.imread(level_info.img_path)

    # Offsets in the dataclass are in small coordinates; scale to game units
    x = level_info.offset[0] * REAL_SCALAR
    y = level_info.offset[1] * REAL_SCALAR

    extent = [x, x + img.shape[1], y + img.shape[0], y]
    return level_info, img, extent

def plot_player_paths(csv_file: str, graph_path: str, graph_offset: int = 50, show_plot: bool = False) -> None:
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

        # Get image and offset info (maps image pixels into game coordinates)
        level_info, img, extent = get_img_level(level_name)
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
        if show_plot:
            plt.show()
        if graph_path:
            # ensure the target directory exists
            os.makedirs(graph_path, exist_ok=True)
            plt.savefig(os.path.join(graph_path, f"PlayerPath_{level_name}.pdf"))
        plt.close()

def total_death_bar_plot(log_dfs: list, graph_path: str, show_plot: bool = False) -> None:
    """
    Create bar plot of total deaths per room from multiple log DataFrames.
    """
    combined_df = pd.concat(log_dfs, ignore_index=True)

    # plot each level separately showing deaths per room in that level.
    for level_name, level_info in LEVEL_DATA.items():
        level_rooms = level_info.rooms
        level_df = combined_df[combined_df['room'].isin(level_rooms)]

        if level_df.empty:
            # skip levels with no data
            continue

        # Keep the order of rooms as defined in LEVEL_DATA and fill zeros where missing
        counts = level_df['room'].value_counts().reindex(level_rooms, fill_value=0)

        plt.figure(figsize=(8, 4))
        counts.plot(kind='bar', color=plt.cm.tab20.colors)
        plt.title(f"Total Deaths per Room in Level: {level_name}")
        plt.xlabel("Room")
        plt.ylabel("Number of Deaths")
        plt.xticks(rotation=45, ha='right') # Rotate x labels for better readability
        plt.tight_layout()
        if show_plot:
            plt.show()
        if graph_path:
            plt.savefig(os.path.join(graph_path, f"TotalDeaths_{level_name}.pdf"))
        plt.close()

def generate_reports(archive_path: str, graph_path: str, show_bar_plot: bool = False, show_path_plot: bool = False) -> None:
    """
    Generate player path plots and total death plots from archived data.
    """
    # Extract data from archives
    archives = extract_archives(archive_path)

    # Plot player paths and collect log data
    log_dfs = []
    for user_id, (csv_path, log_paths) in archives.items():
        # create per-user graph directory
        user_graph_dir = os.path.join(graph_path, f"user_{user_id}")
        plot_player_paths(csv_path, graph_path=user_graph_dir, show_plot=show_path_plot)
        log_df = combine_log_user(log_paths)
        log_dfs.append(log_df)

    total_death_bar_plot(log_dfs, graph_path=graph_path, show_plot=show_bar_plot)


if __name__ == "__main__":
    archive_path = "./Logs/Archived/"
    graph_path = "./Logs/Graphs/"
    show_bar_plot = True
    show_path_plot= False

    generate_reports(archive_path, graph_path, show_bar_plot=show_bar_plot, show_path_plot=show_path_plot)
