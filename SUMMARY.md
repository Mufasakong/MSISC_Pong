# 📋 Project Summary

## What We Built

A complete data synchronization system for your multi-sensory interaction research, allowing you to record and align:

1. **OpenViBE/OpenSignals sensor data** (HRV)
2. **Tobii eye tracking data** (gaze position)  
3. **Unity game events** (ball hits, scores, etc.)

All streams are perfectly synchronized using Lab Streaming Layer (LSL) timestamps.

---

## 📁 Files Created

### Unity Scripts (Add to your scene)
- `Pong/Assets/Scripts/LSLEyeTrackingStreamer.cs` - Streams eye tracking via LSL
- `Pong/Assets/Scripts/LSLEventMarker.cs` - Sends game events as markers
- `Pong/Assets/Scripts/BallController.cs` - Updated with event markers

### Python Tools
- `lsl_recorder.py` - Records all LSL streams to CSV files
- `analyze_recording.py` - Example analysis script with plots
- `requirements.txt` - Python dependencies

### Documentation
- `README.md` - Main project overview
- `QUICKSTART.md` - Step-by-step setup checklist
- `SETUP_GUIDE.md` - Comprehensive guide with troubleshooting
- `ARCHITECTURE.md` - System design and data flow diagrams

---

## 🎯 How to Use

### One-Time Setup (10 minutes)

1. **Unity Setup**
   - Attach `LSLEyeTrackingStreamer.cs` to a GameObject
   - Attach `LSLEventMarker.cs` to a GameObject
   - Verify updated `BallController.cs` is in use

2. **Python Setup**
   ```bash
   pip install -r requirements.txt
   ```

### Each Recording Session (2 minutes)

1. Start OpenViBE/OpenSignals (sensors)
2. Start Unity and press Play
3. Run: `python lsl_recorder.py`
4. Play the game / run your experiment
5. Stop with `Ctrl+C`

### Data Analysis

```bash
python analyze_recording.py
```

Enter your recording folder name when prompted.

---

## 🎉 Advantages Over LabRecorder

| Feature | LabRecorder | Our Solution |
|---------|-------------|--------------|
| **Setup** | Complex GUI, dependencies | Simple Python script |
| **Output** | XDF (binary) | CSV (easy to read/analyze) |
| **File Size** | ~50 MB software | ~5 KB script |
| **Customization** | Limited | Full source code access |
| **Learning Curve** | Moderate | Minimal |
| **Dependencies** | Qt, GUI libraries | Just Python + pylsl |

---

## 🔬 Research Applications

Your synchronized data allows you to answer questions like:

- **How does HRV change during high-stress game moments?**
  → Match HRV data to `BallHit` event markers

- **Where do players look when they score?**
  → Match gaze position to `PlayerScore` markers

- **Does physiological arousal predict performance?**
  → Correlate HRV trends with score changes over time

- **How do event-related responses differ between players?**
  → Compare sensor data across multiple recording sessions

---

## 📊 Example Workflow

```
1. Design Experiment
   ├─ Define events of interest (e.g., ball hits, scores)
   └─ Set eye tracking sample rate (90 Hz recommended)

2. Run Session
   ├─ Start all components
   ├─ Record 10-20 minutes of gameplay
   └─ Stop and verify CSV files created

3. Analyze Data
   ├─ Load synchronized CSV files
   ├─ Extract events from markers
   ├─ Match sensor/gaze data to events
   └─ Statistical analysis + visualization

4. Results
   ├─ Timeline plots showing all streams
   ├─ Event-triggered averages
   └─ Correlation analysis
```

---

## 🔧 Customization Examples

### Add New Event Markers

In any Unity script:
```csharp
LSLEventMarker.Instance.SendMarker("CustomEvent");
LSLEventMarker.Instance.SendMarker("Difficulty", 7.5f);
```

### Change Eye Tracking Rate

In `LSLEyeTrackingStreamer.cs`:
```csharp
public float samplesPerSecond = 120f;  // Increase for higher rate
```

### Add More Sensor Channels

If your sensor sends multiple values, update `LSLStreaming.cs`:
```csharp
sample = new float[3];  // For 3 channels
```

---

## 📈 Next Steps

1. **Test the system** with a short recording session
2. **Verify** all three CSV files are created with data
3. **Run analysis script** to see the timeline visualization
4. **Customize** event markers for your specific research questions
5. **Scale up** to full experiment sessions

---

## 🆘 Support

If you encounter issues:

1. Check **Unity Console** for error messages
2. Check **Python terminal** output
3. Verify all three streams appear in recorder output
4. See `SETUP_GUIDE.md` for detailed troubleshooting

Common issues are usually:
- Firewall blocking LSL (port 16571 UDP)
- Forgetting to start one of the three components
- Eye tracker not connected/enabled

---

## 🙏 Credits

- **Lab Streaming Layer (LSL)**: https://github.com/sccn/labstreaminglayer
- **Tobii Gaming SDK**: https://developer.tobii.com/
- **OpenViBE**: http://openvibe.inria.fr/

---

## 📚 Further Reading

- LSL Protocol: https://labstreaminglayer.readthedocs.io/
- Tobii Unity SDK: https://developer.tobii.com/unity-sdk/
- OpenViBE Documentation: http://openvibe.inria.fr/documentation/

---

**Happy researching!** 🚀

For questions or issues, check the documentation files or examine the Unity console / Python output for detailed error messages.
