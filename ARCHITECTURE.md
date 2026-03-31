# System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     DATA SYNCHRONIZATION FLOW                       │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────┐
│  OpenViBE/OpenSignals│
│   Sensor Hardware    │
│   (HRV Monitor)      │
└──────────┬───────────┘
           │ USB/Bluetooth
           ▼
┌──────────────────────┐         LSL Network Protocol (UDP)
│  LSLStreaming.cs     │         ┌────────────────────────┐
│  (Unity Component)   │────────▶│  Stream: "HRV"         │
│  Reads: HRV data     │         │  Type: float32         │
└──────────────────────┘         │  Rate: Variable        │
                                 │  Channels: 1           │
                                 └────────┬───────────────┘
                                          │
┌──────────────────────┐                  │
│   Tobii Eye Tracker  │                  │
│   (Hardware)         │                  │
└──────────┬───────────┘                  │
           │ Tobii API                    │
           ▼                              │
┌──────────────────────┐                  │
│ LSLEyeTrackingStream │         ┌────────▼───────────────┐
│  (Unity Component)   │────────▶│  Stream: "UnityEyeTrack"│
│  Reads: Gaze X, Y    │         │  Type: float32         │
└──────────────────────┘         │  Rate: 90 Hz           │
                                 │  Channels: 4           │
                                 │  (X, Y, Time, Valid)   │
                                 └────────┬───────────────┘
                                          │
┌──────────────────────┐                  │
│   Unity Game Events  │                  │
│  (Ball hits, scores) │                  │
└──────────┬───────────┘                  │
           │ C# method calls              │
           ▼                              │
┌──────────────────────┐         ┌────────▼───────────────┐
│  LSLEventMarker.cs   │────────▶│  Stream: "UnityMarkers"│
│  (Unity Component)   │         │  Type: string          │
│  Sends: Event names  │         │  Rate: Irregular       │
└──────────────────────┘         │  Channels: 1           │
                                 └────────┬───────────────┘
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    │         LSL NETWORK (Local)              │
                    │    All streams share synchronized clock  │
                    └─────────────────────┬─────────────────────┘
                                          │
                                          ▼
                    ┌─────────────────────────────────────────┐
                    │      lsl_recorder.py (Python)           │
                    │  - Discovers all streams automatically  │
                    │  - Records to synchronized CSV files    │
                    │  - Timestamp alignment built-in         │
                    └─────────────────────┬─────────────────────┘
                                          │
                    ┌─────────────────────▼─────────────────────┐
                    │   Output Folder: LSL_Recording_<time>/    │
                    ├───────────────────────────────────────────┤
                    │  HRV_HRV_<timestamp>.csv                  │
                    │    ├─ LSL_Timestamp                       │
                    │    ├─ Local_Timestamp                     │
                    │    └─ Channel_1 (HRV value)               │
                    │                                           │
                    │  UnityEyeTracking_Gaze_<timestamp>.csv    │
                    │    ├─ LSL_Timestamp                       │
                    │    ├─ Local_Timestamp                     │
                    │    ├─ Channel_1 (Gaze X)                  │
                    │    ├─ Channel_2 (Gaze Y)                  │
                    │    ├─ Channel_3 (Unity Time)              │
                    │    └─ Channel_4 (Valid flag)              │
                    │                                           │
                    │  UnityMarkers_Markers_<timestamp>.csv     │
                    │    ├─ LSL_Timestamp                       │
                    │    ├─ Local_Timestamp                     │
                    │    └─ Marker (event string)               │
                    │                                           │
                    │  session_info.txt                         │
                    │    └─ Metadata about recording            │
                    └───────────────────────────────────────────┘
```

## Key Concepts

### 1. LSL Network
- All streams use the same synchronized clock
- Automatic timestamp correction for network delays
- Streams discovered automatically (no manual IP configuration)

### 2. Timestamp Synchronization
- **LSL_Timestamp**: Master clock shared across all streams
- **Local_Timestamp**: Local computer time (for backup)
- Use LSL_Timestamp for all data alignment!

### 3. Data Flow
```
Hardware → Unity/Software → LSL Stream → Python Recorder → CSV Files
```

### 4. Event Synchronization Example
```
Time (LSL)    | HRV Stream | Gaze Stream      | Marker Stream
──────────────┼────────────┼──────────────────┼──────────────────
1234.567      | 75.2       | (1024, 768, 1)   | -
1234.578      | 75.3       | (1028, 770, 1)   | -
1234.589      | 75.1       | (1030, 771, 1)   | "BallHit_Player"  ← Event!
1234.600      | 76.5       | (1035, 772, 1)   | -
1234.611      | 78.2       | (1038, 773, 1)   | -
```

When you see "BallHit_Player" at time 1234.589:
- You can look up HRV = 75.1 at that exact moment
- You can look up Gaze = (1030, 771) at that exact moment
- All perfectly synchronized!

## Why This Works Better Than CSV-Only Approach

### ❌ Old Approach (CSV files)
```
Unity: EyeData_143022.csv (uses Time.time)
Unity: HRV_143022.csv (uses Time.time)
Problem: Time.time starts at 0 each time Unity runs!
Problem: No shared clock between recordings
Problem: Manual alignment needed (error-prone)
```

### ✅ New Approach (LSL)
```
All streams use LSL's shared clock
Automatic sub-millisecond synchronization
Works even if you restart Unity mid-session
No manual alignment needed!
```
