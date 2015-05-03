#####################################################################
#
# audio_player.py
# Derived from Eran's audio player
# Modifications made by Benjamin Chan
# MIT License
#
#####################################################################

import pyaudio
import numpy as np

kSamplingRate = 44100
kOutputChannels = 2

class AudioPlayer(object):
   def __init__(self):
      super(AudioPlayer, self).__init__()

      self.audio = pyaudio.PyAudio()
      dev_idx = self._find_best_output()

      self.stream = self.audio.open(format = pyaudio.paFloat32,
                                    channels = kOutputChannels,
                                    frames_per_buffer = 512,
                                    rate = kSamplingRate,
                                    output = True,
                                    input = False,
                                    output_device_index = dev_idx,
                                    stream_callback = self._callback)
      self.gain = 1
      self.frames = []

   def queueFramesForPlay(self, frames):
      self.frames = np.append(self.frames, frames)

   def close(self) :
      self.stream.stop_stream()
      self.stream.close()
      self.audio.terminate()

   def isActive(self):
      return self.stream.is_active()

   def _generateFrames(self, num_frames):
      if num_frames > len(self.frames):
         num_frames = len(self.frames)
      returnFrames = self.frames[0:num_frames]
      self.frames = self.frames[num_frames:]
      return returnFrames

   # return the best output index if found. Otherwise, return None
   # (which will choose the default)
   def _find_best_output(self):
      # for Windows, we want to find the ASIO host API and device
      cnt = self.audio.get_host_api_count()
      for i in range(cnt):
         api = self.audio.get_host_api_info_by_index(i)
         if api['type'] == pyaudio.paASIO:
            host_api_idx = i
            print 'Found ASIO', host_api_idx
            break
      else:
         # did not find desired API. Bail out
         return None

      cnt = self.audio.get_device_count()
      for i in range(cnt):
         dev = self.audio.get_device_info_by_index(i)
         if dev['hostApi'] == host_api_idx:
            print 'Found Device', i
            return i

      # did not find desired device.
      return None

   def _callback(self, in_data, num_frames, time_info, status):
      output = np.zeros(num_frames * kOutputChannels, dtype=np.float32)

      signal = self._generateFrames(num_frames * kOutputChannels)
      # works if returned signal is shorter than output as well.
      output[:len(signal)] += signal

      #output *= self.gain
      return (output.tostring(), pyaudio.paContinue)

