"""
Simple LSL Recorder - Records all LSL streams with synchronized timestamps
This is a lightweight alternative to LabRecorder for your specific needs.

This script will:
1. Discover all available LSL streams
2. Record them to separate CSV files with synchronized timestamps
3. Create a master timestamp file for alignment
"""

import pylsl
import time
import csv
import os
from datetime import datetime
from pathlib import Path
import threading


class LSLStreamRecorder:
    """Records a single LSL stream to CSV"""
    
    def __init__(self, stream_info, output_dir):
        self.stream_info = stream_info
        self.output_dir = output_dir
        self.inlet = pylsl.StreamInlet(stream_info, max_buflen=360)
        self.is_recording = False
        self.thread = None
        
        # Get stream metadata
        self.stream_name = stream_info.name()
        self.stream_type = stream_info.type()
        self.channel_count = stream_info.channel_count()
        self.channel_format = stream_info.channel_format()
        
        # Generate filename
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        safe_name = "".join(c if c.isalnum() or c in ('-', '_') else '_' for c in self.stream_name)
        self.filename = f"{safe_name}_{self.stream_type}_{timestamp}.csv"
        self.filepath = os.path.join(output_dir, self.filename)
        
        print(f"  📊 {self.stream_name} ({self.stream_type})")
        print(f"     Channels: {self.channel_count}, Format: {self.channel_format}")
        print(f"     Output: {self.filename}")
    
    def start_recording(self):
        """Start recording in a separate thread"""
        self.is_recording = True
        self.thread = threading.Thread(target=self._record_loop)
        self.thread.daemon = True
        self.thread.start()
    
    def stop_recording(self):
        """Stop recording"""
        self.is_recording = False
        if self.thread:
            self.thread.join(timeout=2)
    
    def _record_loop(self):
        """Main recording loop (runs in separate thread)"""
        with open(self.filepath, 'w', newline='') as f:
            writer = csv.writer(f)
            
            # Write header based on channel format
            if self.channel_format == pylsl.cf_string:
                header = ['LSL_Timestamp', 'Local_Timestamp', 'Marker']
            else:
                header = ['LSL_Timestamp', 'Local_Timestamp'] + \
                        [f'Channel_{i+1}' for i in range(self.channel_count)]
            writer.writerow(header)
            
            # Record samples
            while self.is_recording:
                try:
                    if self.channel_format == pylsl.cf_string:
                        sample, timestamp = self.inlet.pull_sample(timeout=0.1)
                        if sample:
                            local_time = time.time()
                            writer.writerow([timestamp, local_time] + sample)
                    else:
                        sample, timestamp = self.inlet.pull_sample(timeout=0.1)
                        if sample:
                            local_time = time.time()
                            writer.writerow([timestamp, local_time] + sample)
                except Exception as e:
                    print(f"Error recording {self.stream_name}: {e}")
                    break


class MultiStreamRecorder:
    """Records multiple LSL streams simultaneously"""
    
    def __init__(self, output_dir=None):
        if output_dir is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_dir = f"LSL_Recording_{timestamp}"
        
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.recorders = []
        self.is_recording = False
        
    def discover_streams(self, timeout=5.0):
        """Discover all available LSL streams"""
        print(f"\n🔍 Searching for LSL streams (timeout: {timeout}s)...")
        streams = pylsl.resolve_streams(timeout)
        
        if not streams:
            print("❌ No LSL streams found!")
            return False
        
        print(f"\n✅ Found {len(streams)} stream(s):")
        
        for stream_info in streams:
            recorder = LSLStreamRecorder(stream_info, str(self.output_dir))
            self.recorders.append(recorder)
        
        return True
    
    def start_recording(self):
        """Start recording all streams"""
        if not self.recorders:
            print("❌ No streams to record!")
            return False
        
        print(f"\n🔴 Starting recording to: {self.output_dir}")
        print("Press Ctrl+C to stop recording\n")
        
        self.is_recording = True
        for recorder in self.recorders:
            recorder.start_recording()
        
        # Write session info
        self._write_session_info()
        
        return True
    
    def stop_recording(self):
        """Stop recording all streams"""
        print("\n🛑 Stopping recording...")
        self.is_recording = False
        
        for recorder in self.recorders:
            recorder.stop_recording()
        
        print(f"✅ Recording saved to: {self.output_dir}")
    
    def _write_session_info(self):
        """Write session metadata"""
        info_file = self.output_dir / "session_info.txt"
        with open(info_file, 'w') as f:
            f.write(f"LSL Recording Session\n")
            f.write(f"Start Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"\nRecorded Streams:\n")
            for recorder in self.recorders:
                f.write(f"  - {recorder.stream_name} ({recorder.stream_type})\n")
                f.write(f"    Channels: {recorder.channel_count}\n")
                f.write(f"    File: {recorder.filename}\n")


def main():
    """Main entry point"""
    print("=" * 60)
    print("  LSL Multi-Stream Recorder")
    print("  Simple alternative to LabRecorder")
    print("=" * 60)
    
    # Create recorder
    recorder = MultiStreamRecorder()
    
    # Discover streams
    if not recorder.discover_streams(timeout=5.0):
        print("\n⚠️  Make sure your LSL streams are running:")
        print("  1. OpenViBE/OpenSignals sensor stream")
        print("  2. Unity with LSLEyeTrackingStreamer")
        print("  3. Unity with LSLEventMarker")
        return
    
    # Start recording
    if not recorder.start_recording():
        return
    
    # Keep recording until interrupted
    try:
        while recorder.is_recording:
            time.sleep(0.1)
    except KeyboardInterrupt:
        pass
    finally:
        recorder.stop_recording()


if __name__ == "__main__":
    main()
