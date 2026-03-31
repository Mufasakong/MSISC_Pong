# MSISC Pong - Multi Sensory Interaction and Media Cognition

A research project integrating eye tracking (Tobii) and physiological sensors (OpenViBE/OpenSignals) with a Unity Pong game. All data streams are synchronized using Lab Streaming Layer (LSL) for precise temporal alignment.

## 🚀 Quick Start

**See [QUICKSTART.md](QUICKSTART.md) for a step-by-step checklist!**

## Features

- ✅ **Synchronized Data Streams**
  - OpenViBE/OpenSignals HRV sensor data
  - Tobii eye tracking (gaze position)
  - Unity game event markers (ball hits, scores, etc.)

- ✅ **Perfect Timestamp Alignment**
  - All streams use LSL's shared clock
  - Sub-millisecond synchronization
  - Automatic network delay correction

- ✅ **Easy Recording**
  - Simple Python script (alternative to LabRecorder)
  - CSV output (easy to analyze)
  - Automatic stream discovery

## System Overview

```
OpenViBE Sensors ──┐
                   ├──▶ LSL Network ──▶ Python Recorder ──▶ CSV Files
Tobii Eye Tracker ─┤
                   │
Unity Events ──────┘
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed system design.

## Documentation

- **[QUICKSTART.md](QUICKSTART.md)** - Step-by-step setup checklist
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Comprehensive setup and troubleshooting guide
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture and data flow

## Components

### Unity Scripts (Pong/Assets/Scripts/)

| Script | Purpose |
|--------|---------|
| `LSLEyeTrackingStreamer.cs` | Streams Tobii eye tracking via LSL |
| `LSLEventMarker.cs` | Sends game events as LSL markers |
| `BallController.cs` | Pong game logic with event markers |
| `LSLStreaming.cs` | Receives HRV data from OpenViBE |
| `EyeDataLogger.cs` | (Legacy) Local CSV backup |

### Python Scripts

| Script | Purpose |
|--------|---------|
| `lsl_recorder.py` | Records all LSL streams to synchronized CSV files |

## Requirements

### Hardware
- Tobii eye tracker (compatible with Tobii Gaming SDK)
- OpenViBE-compatible sensors (e.g., HRV monitor)

### Software
- Unity 2020.3+ (with Tobii SDK and LSL for Unity packages)
- Python 3.7+
- OpenViBE or OpenSignals (for sensor data)

### Python Dependencies
```bash
pip install pylsl
```

## Usage

1. **Start OpenViBE/OpenSignals** with your sensors
2. **Start Unity** and press Play
3. **Run Python recorder**:
   ```bash
   python lsl_recorder.py
   ```
4. **Play the game** - all data is recorded automatically
5. **Stop with Ctrl+C** - files are saved

## Output

Each recording session creates a folder with:
- `HRV_HRV_<timestamp>.csv` - Sensor data
- `UnityEyeTracking_Gaze_<timestamp>.csv` - Eye tracking data
- `UnityMarkers_Markers_<timestamp>.csv` - Event markers
- `session_info.txt` - Recording metadata

All files share synchronized `LSL_Timestamp` columns for alignment.

## Data Analysis Example

```python
import pandas as pd

# Load data
hrv = pd.read_csv('HRV_HRV_20260331_143022.csv')
gaze = pd.read_csv('UnityEyeTracking_Gaze_20260331_143022.csv')
markers = pd.read_csv('UnityMarkers_Markers_20260331_143022.csv')

# Find player score events
scores = markers[markers['Marker'].str.contains('PlayerScore')]

# Get HRV at each score event
for idx, event in scores.iterrows():
    t = event['LSL_Timestamp']
    hrv_at_score = hrv[
        (hrv['LSL_Timestamp'] >= t-0.1) & 
        (hrv['LSL_Timestamp'] <= t+0.1)
    ]['Channel_1'].mean()
    print(f"HRV during {event['Marker']}: {hrv_at_score:.2f}")
```

## Troubleshooting

See [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed troubleshooting.

**Quick fixes:**
- No streams found? Check all components are running
- Check Windows Firewall (LSL uses UDP port 16571)
- Increase timeout in `lsl_recorder.py` if streams are slow to discover

## License

Research project for Multi Sensory Interaction and Media Cognition studies.

## Contributors

- Research team at MSIMC

---

**Need help?** See the documentation files or check Unity console / Python output for error messages.
