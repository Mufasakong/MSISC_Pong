# Quick Start Checklist

Follow these steps to get your synchronized data recording working:

## ✅ Unity Setup (5 minutes)

- [ ] 1. Open your Unity project
- [ ] 2. Add `LSLEyeTrackingStreamer.cs` to a GameObject in your scene
- [ ] 3. Add `LSLEventMarker.cs` to a GameObject in your scene
- [ ] 4. Verify `BallController.cs` was updated (check for LSLEventMarker references)
- [ ] 5. Verify `LSLStreaming.cs` is still attached and configured for HRV
- [ ] 6. Press Play in Unity - check console for "LSL ... Stream created" messages

## ✅ Python Setup (2 minutes)

- [ ] 1. Open terminal/command prompt
- [ ] 2. Navigate to project folder: `cd C:\Users\gulfu\Documents\MSIMC\MSISC_Pong`
- [ ] 3. Install dependencies: `pip install pylsl`
- [ ] 4. Test the recorder: `python lsl_recorder.py`
- [ ] 5. You should see "Searching for LSL streams..."

## ✅ Recording Workflow

### Each Recording Session:

1. **Start OpenViBE/OpenSignals** 
   - Make sure HRV sensor is streaming
   - Should show "LSL HRV Stream found and connected!" in Unity console

2. **Start Unity**
   - Press Play
   - Wait for all 3 "LSL ... Stream created" messages

3. **Start Python Recorder**
   ```bash
   python lsl_recorder.py
   ```
   - Should find 3 streams: HRV, UnityEyeTracking, UnityMarkers

4. **Run your experiment**
   - Play the Pong game
   - Events will be automatically recorded

5. **Stop recording**
   - Press `Ctrl+C` in Python terminal
   - Stop Unity
   - Stop OpenViBE/OpenSignals

6. **Check output**
   - Look for folder like `LSL_Recording_YYYYMMDD_HHMMSS/`
   - Should contain 3 CSV files + session_info.txt

## 🔍 Verification

Run this quick test:

1. Start all three components (OpenViBE, Unity, Python recorder)
2. Play one round of Pong (score at least one point)
3. Stop recording
4. Open the marker CSV file
5. You should see events like:
   - `GameStart`
   - `BallReset`
   - `BallHit_PlayerPaddle` or `BallHit_AIPaddle`
   - `PlayerScore:1` or `AIScore:1`

If you see these events with timestamps, **everything is working!** ✨

## 🆘 Having Issues?

See `SETUP_GUIDE.md` for detailed troubleshooting.

Common quick fixes:
- **No streams found:** Make sure all three components are running
- **Only 1-2 streams found:** Check Unity console for errors
- **Empty CSV files:** Make sure you actually played the game after starting recording
