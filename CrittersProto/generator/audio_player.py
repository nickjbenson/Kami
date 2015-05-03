#####################################################################
#
# audio_player.py
# Derived from Eran's audio player
# MIT License
#
#####################################################################

import pyaudio

class Audio(object):
   def __init__(self, listener = None):
      super(Audio, self).__init__()

      self.audio = pyaudio.PyAudio()
      dev_idx = self._find_best_output()

      # write to wave files
      self.waver = None

      # write to a wave file per generator
      self.wavers = {}
      self.waverIndex = 1
      
      self.isRecording = False

      self.stream = self.audio.open(format = pyaudio.paFloat32,
                                    channels = kOutputChannels,
                                    frames_per_buffer = 512,
                                    rate = kSamplingRate,
                                    output = True,
                                    input = False,
                                    output_device_index = dev_idx,
                                    stream_callback = self._callback)
      self.gain = .5
      self.generators = []
