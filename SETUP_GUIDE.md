# LSL Multi-Stream Synchronization Setup

This guide shows you how to synchronize your OpenViBE/OpenSignals sensor data with Unity eye tracking and event markers using Lab Streaming Layer (LSL).

## Overview

You now have a custom solution that synchronizes three data streams:

1. **OpenViBE/OpenSignals sensors** → HRV data (already streaming via LSL)
2. **Unity eye tracking** → Gaze position (new LSL stream)
3. **Unity event markers** → Game events like ball hits, scores (new LSL stream)

All streams use **LSL timestamps** for perfect synchronization, and are recorded to CSV files by a simple Python script.

---

## Setup Instructions

### 1. Unity Setup

#### A. Add the new scripts to your scene

1. **LSLEyeTrackingStreamer** (replaces the CSV-only logger)
   - Attach `LSLEyeTrackingStreamer.cs` to any GameObject in your scene
   - Configure settings:
     - `Stream Name`: "UnityEyeTracking" (or your preference)
     - `Stream Type`: "Gaze"
     - `Samples Per Second`: 90 (match your eye tracker's rate)

2. **LSLEventMarker** (sends game events)
   - Attach `LSLEventMarker.cs` to any GameObject in your scene
   - It will automatically create a singleton instance
   - No configuration needed!

3. **BallController** (updated with event markers)
   - Your existing `BallController.cs` has been updated to send markers
   - Events sent:
     - `GameStart` - When the game starts
     - `PlayerScore:<score>` - When player scores
     - `AIScore:<score>` - When AI scores
     - `BallReset` - When ball resets
     - `BallHit_<object>` - When ball hits a paddle

#### B. Optional: Keep the old CSV logger

You can keep `EyeDataLogger.cs` running alongside the LSL streamer for backup/local logs.

---

### 2. OpenViBE/OpenSignals Setup

Your sensor data is already streaming via LSL (stream type: "HRV"). No changes needed!

Verify it's working:
- Check that your `LSLStreaming.cs` script is active in Unity
- It should connect to the HRV stream automatically

---

### 3. Python Recorder Setup

#### A. Install Python dependencies

```bash
pip install -r requirements.txt
```

Or manually:
```bash
pip install pylsl
```

#### B. Run the recorder

1. Start your OpenViBE/OpenSignals sensor stream
2. Start Unity (which will now stream eye tracking + markers)
3. Run the Python recorder:

```bash
python lsl_recorder.py
```

The recorder will:
- Discover all LSL streams automatically
- Create a new folder with timestamp (e.g., `LSL_Recording_20260331_143022/`)
- Record each stream to a separate CSV file
- Synchronize all timestamps to LSL time

#### C. Stop recording

Press `Ctrl+C` to stop. Files are saved automatically.

---

## Output Files

After recording, you'll have a folder containing:

```
LSL_Recording_20260331_143022/
├── session_info.txt              # Recording metadata
├── HRV_HRV_20260331_143022.csv   # Sensor data from OpenViBE
├── UnityEyeTracking_Gaze_20260331_143022.csv  # Eye tracking data
└── UnityMarkers_Markers_20260331_143022.csv   # Event markers
```

### File Formats

**HRV data (sensor stream):**
```csv
LSL_Timestamp,Local_Timestamp,Channel_1
1234.56789,1711893022.123,75.2
```

**Eye tracking data:**
```csv
LSL_Timestamp,Local_Timestamp,Channel_1,Channel_2,Channel_3,Channel_4
1234.56789,1711893022.123,1024.5,768.3,1.234,1.0
# Channel_1 = GazeX (pixels)
# Channel_2 = GazeY (pixels)
# Channel_3 = LocalTime (Unity Time.time)
# Channel_4 = IsValid (1.0=valid, 0.0=invalid)
```

**Event markers:**
```csv
LSL_Timestamp,Local_Timestamp,Marker
1234.56789,1711893022.123,GameStart
1234.78901,1711893022.345,BallHit_PlayerPaddle
1235.01234,1711893022.567,PlayerScore:1
```

### Timestamp Synchronization

All streams use **LSL_Timestamp** for synchronization:
- Same clock across all streams
- Accurate to microseconds
- Corrects for network delays automatically

To align data in analysis:
1. Use `LSL_Timestamp` as the primary time reference
2. Merge data from different files based on these timestamps
3. Event markers can be matched exactly to sensor/eye tracking data

---

## Verification

### 1. Check Unity Console

When Unity starts, you should see:
```
LSL Eye Tracking Stream created: UnityEyeTracking (type: Gaze)
Streaming at 90 Hz with 4 channels: GazeX, GazeY, LocalTime, IsValid

LSL Event Marker Stream created: UnityMarkers (type: Markers)
Use LSLEventMarker.Instance.SendMarker("YourEvent") to send markers!

LSL HRV Stream found and connected!
```

### 2. Check Python Recorder Output

```
🔍 Searching for LSL streams (timeout: 5.0s)...

✅ Found 3 stream(s):
  📊 HRV (HRV)
     Channels: 1, Format: cf_float32
     Output: HRV_HRV_20260331_143022.csv
  📊 UnityEyeTracking (Gaze)
     Channels: 4, Format: cf_float32
     Output: UnityEyeTracking_Gaze_20260331_143022.csv
  📊 UnityMarkers (Markers)
     Channels: 1, Format: cf_string
     Output: UnityMarkers_Markers_20260331_143022.csv

🔴 Starting recording to: LSL_Recording_20260331_143022
Press Ctrl+C to stop recording
```

---

## Troubleshooting

### "No LSL streams found"

**Problem:** Python recorder can't find streams

**Solutions:**
1. Make sure OpenViBE/OpenSignals is running and broadcasting
2. Make sure Unity is running with the new scripts active
3. Check Windows Firewall isn't blocking LSL (port 16571 UDP)
4. Try increasing the timeout: edit `lsl_recorder.py` line with `timeout=5.0` to `timeout=10.0`

### "LSL Stream found but no data"

**Problem:** Stream connects but no samples recorded

**Solutions:**
1. Check Unity console for errors
2. Verify eye tracker is connected (Tobii software running)
3. Check that scripts are attached to active GameObjects

### Events not appearing

**Problem:** No markers in the marker stream

**Solutions:**
1. Verify `LSLEventMarker` script is in the scene
2. Check Unity console for "LSL Marker sent: ..." messages
3. Make sure game events are actually happening (play the game!)

---

## Advanced Usage

### Add Custom Event Markers

In any Unity script:

```csharp
// Simple marker
LSLEventMarker.Instance.SendMarker("MyEvent");

// Marker with value
LSLEventMarker.Instance.SendMarker("Difficulty", 5.2f);

// Numerical marker
LSLEventMarker.Instance.SendMarker(42);
```

### Adjust Eye Tracking Sample Rate

Edit `LSLEyeTrackingStreamer.cs`:
```csharp
public float samplesPerSecond = 90f;  // Change this value
```

Higher rates = more data, more CPU usage.

### Record to Custom Location

```bash
# Edit lsl_recorder.py main() function:
recorder = MultiStreamRecorder(output_dir="MyExperiment_Data")
```

---

## Comparison to LabRecorder

| Feature | LabRecorder | This Solution |
|---------|-------------|---------------|
| Setup | Complex GUI | Simple Python script |
| File format | XDF (binary) | CSV (easy to read) |
| Dependencies | Qt, GUI libraries | Just Python + pylsl |
| Customization | Limited | Full source code |
| Size | ~50 MB | ~5 KB |
| Speed | Fast | Fast enough |

**When to use LabRecorder:**
- You need XDF format
- You need very high-frequency streams (>1000 Hz)
- You need the GUI

**When to use this solution:**
- You want simple CSV output
- You want to understand/modify the code
- You want minimal dependencies

---

## Next Steps

### Data Analysis

Use Python/R/MATLAB to analyze the synchronized data:

```python
import pandas as pd

# Load data
hrv = pd.read_csv('HRV_HRV_20260331_143022.csv')
gaze = pd.read_csv('UnityEyeTracking_Gaze_20260331_143022.csv')
markers = pd.read_csv('UnityMarkers_Markers_20260331_143022.csv')

# Find events
ball_hits = markers[markers['Marker'].str.contains('BallHit')]

# Match HRV to events (within 50ms window)
for idx, event in ball_hits.iterrows():
    t = event['LSL_Timestamp']
    hrv_at_event = hrv[(hrv['LSL_Timestamp'] >= t-0.05) & 
                       (hrv['LSL_Timestamp'] <= t+0.05)]
    print(f"HRV during {event['Marker']}: {hrv_at_event['Channel_1'].mean()}")
```

### Visualization

Create synchronized plots showing sensor data + eye tracking + events on the same timeline.

---

## Questions?

If you run into issues:
1. Check the Unity console for error messages
2. Check the Python terminal output
3. Verify all streams show up in the "Found X stream(s)" message
4. Make sure timestamps are increasing (not all zeros)

Good luck with your experiment! 🚀
