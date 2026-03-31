"""
Example data analysis script for synchronized LSL recordings
Shows how to align and analyze the three data streams together
"""

import pandas as pd
import numpy as np
from pathlib import Path
import matplotlib.pyplot as plt


def load_recording(folder_path):
    """Load all CSV files from a recording session"""
    folder = Path(folder_path)
    
    # Find the CSV files
    files = list(folder.glob("*.csv"))
    
    data = {}
    for file in files:
        if "HRV" in file.name:
            data['hrv'] = pd.read_csv(file)
        elif "Gaze" in file.name:
            data['gaze'] = pd.read_csv(file)
        elif "Markers" in file.name:
            data['markers'] = pd.read_csv(file)
    
    return data


def find_nearest_sample(df, timestamp, time_column='LSL_Timestamp', window=0.1):
    """Find the nearest sample in a dataframe to a given timestamp"""
    mask = (df[time_column] >= timestamp - window) & (df[time_column] <= timestamp + window)
    nearby = df[mask]
    
    if len(nearby) == 0:
        return None
    
    # Return the closest sample
    idx = (nearby[time_column] - timestamp).abs().idxmin()
    return nearby.loc[idx]


def analyze_events(data):
    """Analyze sensor data around game events"""
    
    markers = data['markers']
    hrv = data['hrv']
    gaze = data['gaze']
    
    print("\n" + "="*60)
    print("EVENT ANALYSIS")
    print("="*60)
    
    # Analyze each type of event
    event_types = ['PlayerScore', 'AIScore', 'BallHit', 'BallReset']
    
    for event_type in event_types:
        event_markers = markers[markers['Marker'].str.contains(event_type, na=False)]
        
        if len(event_markers) == 0:
            continue
        
        print(f"\n{event_type}: {len(event_markers)} occurrences")
        print("-" * 40)
        
        hrv_values = []
        gaze_x_values = []
        gaze_y_values = []
        
        for idx, event in event_markers.iterrows():
            t = event['LSL_Timestamp']
            
            # Find HRV at this time
            hrv_sample = find_nearest_sample(hrv, t)
            if hrv_sample is not None:
                hrv_values.append(hrv_sample['Channel_1'])
            
            # Find gaze at this time
            gaze_sample = find_nearest_sample(gaze, t)
            if gaze_sample is not None:
                if gaze_sample['Channel_4'] == 1.0:  # Valid gaze
                    gaze_x_values.append(gaze_sample['Channel_1'])
                    gaze_y_values.append(gaze_sample['Channel_2'])
        
        # Print statistics
        if hrv_values:
            print(f"  HRV: mean={np.mean(hrv_values):.2f}, std={np.std(hrv_values):.2f}")
        
        if gaze_x_values:
            print(f"  Gaze X: mean={np.mean(gaze_x_values):.1f}px, std={np.std(gaze_x_values):.1f}px")
            print(f"  Gaze Y: mean={np.mean(gaze_y_values):.1f}px, std={np.std(gaze_y_values):.1f}px")


def plot_timeline(data):
    """Create a timeline plot showing all data streams"""
    
    hrv = data['hrv']
    gaze = data['gaze']
    markers = data['markers']
    
    # Create figure with subplots
    fig, axes = plt.subplots(3, 1, figsize=(12, 8), sharex=True)
    
    # Plot HRV
    axes[0].plot(hrv['LSL_Timestamp'], hrv['Channel_1'], 'b-', linewidth=0.5)
    axes[0].set_ylabel('HRV Value')
    axes[0].set_title('Heart Rate Variability')
    axes[0].grid(True, alpha=0.3)
    
    # Plot Gaze position
    axes[1].plot(gaze['LSL_Timestamp'], gaze['Channel_1'], 'r-', label='Gaze X', linewidth=0.5)
    axes[1].plot(gaze['LSL_Timestamp'], gaze['Channel_2'], 'g-', label='Gaze Y', linewidth=0.5)
    axes[1].set_ylabel('Screen Position (px)')
    axes[1].set_title('Eye Tracking')
    axes[1].legend()
    axes[1].grid(True, alpha=0.3)
    
    # Plot event markers
    axes[2].scatter(markers['LSL_Timestamp'], range(len(markers)), c='black', marker='|', s=100)
    axes[2].set_ylabel('Event #')
    axes[2].set_xlabel('LSL Timestamp (s)')
    axes[2].set_title('Game Events')
    axes[2].grid(True, alpha=0.3)
    
    # Add event labels
    for idx, event in markers.iterrows():
        if idx % 5 == 0:  # Only label every 5th event to avoid clutter
            axes[2].text(event['LSL_Timestamp'], idx, event['Marker'], 
                        rotation=45, fontsize=6, ha='right')
    
    plt.tight_layout()
    plt.savefig('timeline_plot.png', dpi=150)
    print("\n✅ Timeline plot saved to: timeline_plot.png")
    plt.show()


def main():
    """Main analysis function"""
    
    print("="*60)
    print("LSL RECORDING ANALYSIS")
    print("="*60)
    
    # Ask user for recording folder
    print("\nEnter the path to your recording folder:")
    print("Example: LSL_Recording_20260331_143022")
    folder = input("> ").strip()
    
    if not Path(folder).exists():
        print(f"❌ Folder not found: {folder}")
        return
    
    # Load data
    print("\n📂 Loading data...")
    data = load_recording(folder)
    
    # Check what we found
    if 'hrv' in data:
        print(f"  ✅ HRV data: {len(data['hrv'])} samples")
    if 'gaze' in data:
        print(f"  ✅ Gaze data: {len(data['gaze'])} samples")
    if 'markers' in data:
        print(f"  ✅ Markers: {len(data['markers'])} events")
    
    # Analyze events
    if all(k in data for k in ['hrv', 'gaze', 'markers']):
        analyze_events(data)
        
        # Ask if user wants plots
        print("\n" + "="*60)
        response = input("\nGenerate timeline plot? (y/n): ").strip().lower()
        if response == 'y':
            plot_timeline(data)
    else:
        print("\n⚠️  Not all data streams found. Make sure recording has all CSV files.")


if __name__ == "__main__":
    main()
