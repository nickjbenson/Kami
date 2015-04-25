#####################################################################
#
# audio.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import pyaudio
import numpy as np
import core

# remember that output is stereo here
kSamplingRate = 44100
kOutputChannels = 2


class Audio(object):
   def __init__(self, listener = None):
      super(Audio, self).__init__()

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
      self.gain = .5
      self.generators = []
      self.listener = listener
      core.register_terminate_func(self.close)

   def close(self) :
      self.stream.stop_stream()
      self.stream.close()
      self.audio.terminate()

   def add_generator(self, gen) :
      if gen not in self.generators: # add this for safety
         self.generators.append(gen)

   def remove_generator(self, gen) :
      self.generators.remove(gen)

   def set_gain(self, gain) :
      self.gain = np.clip(gain, 0, 1)

   def get_gain(self) :
      return self.gain

   def get_load(self) :
      return '%.1f%%. %d gens' % (100.0 * self.stream.get_cpu_load(), len(self.generators))

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

      # this calls generate() for each generator. generator must return:
      # (signal, keep_going). If keep_going is True, it means the generator
      # has more to generate. False means generator is done and will be
      # removed from the list. signal must be a numpay array of length
      # num_frames * kOutputChannels (or less)
      kill_list = []
      for g in self.generators:
         (signal, keep_going) = g.generate(num_frames)
         # works if returned signal is shorter than output as well.
         output[:len(signal)] += signal
         if not keep_going:
            kill_list.append(g)

      # remove generators that are done
      for g in kill_list:
         self.generators.remove(g)

      output *= self.gain
      if self.listener:
         self.listener.audio_cb(output)
      return (output.tostring(), pyaudio.paContinue)


