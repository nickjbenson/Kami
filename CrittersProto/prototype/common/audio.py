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
import wave
import numpy as np
import core
import struct

# remember that output is stereo here
kSamplingRate = 44100
kOutputChannels = 2

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
      self.listener = listener
      core.register_terminate_func(self.close)

   def start_recording(self):
      self.isRecording = True

   def stop_recording(self):
      self.isRecording = False

   # Set this to write to a global wav file. Else, audio.py
   # will write to a special wav file for each generator
   def set_wav_file(self, pathToWav):
      self.pathToWav = pathToWav

      wasRecording = self.isRecording
      self.isRecording = False

      if self.waver is not None:
         self.waver.close()
         self.waver = None

      self.waver = wave.open(self.pathToWav, 'wb')
      self.waver.setnchannels(kOutputChannels) #kOutputChannels
      self.waver.setsampwidth(2) # let's convert things into 16 bit integer format
      self.waver.setframerate(kSamplingRate)
      self.isRecording = wasRecording

   def get_waver_for_path(self, pathToWav):
      waver = wave.open(pathToWav, 'wb')
      waver.setnchannels(kOutputChannels) #kOutputChannels
      waver.setsampwidth(2) # let's convert things into 16 bit integer format
      waver.setframerate(kSamplingRate)
      return waver

   def get_new_waver(self):
      self.waverIndex += 1
      print "NEWWW"
      print self.waverIndex
      return self.get_waver_for_path("wav/output" + str(self.waverIndex - 1) + ".wav")

   def write_frames(self, data):
      self.write_frames_to_waver(data, self.waver)

   def write_frames_to_waver(self, data, waver):
      if waver is None:
         print "No wav file..."
         return
      fmt = 'h'*len(data)
      waver.writeframes(struct.pack(fmt, *data))

   def close(self) :
      self.stream.stop_stream()
      self.stream.close()
      self.audio.terminate()
      if self.waver is not None:
         self.waver.close()

   def add_generator(self, gen) :
      if gen not in self.generators: # add this for safety
         self.generators.append(gen)

   def remove_generator(self, gen) :
      if gen in self.generators:
         self.generators.remove(gen)
         awav = self.wavers[gen]
         awav.close()
         del self.wavers[gen]

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
         #signal *= self.gain

         # if self.waver is None:
         #    # write each generator to its own wave file
         #    if g not in self.wavers:
         #       awav = self.get_new_waver()
         #       self.wavers[g] = awav
         #    else:
         #       awav = self.wavers[g]
         #    intoutput = signal * np.iinfo(np.int16).max
         #    intoutput = signal.astype(np.int16)
         #    self.write_frames_to_waver(signal, awav)

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

      # don't stop recording in the middle of a generation
      if self.isRecording and self.waver is not None:
         intoutput = output * np.iinfo(np.int16).max
         intoutput = intoutput.astype(np.int16)
         self.write_frames(intoutput)
         
      return (output.tostring(), pyaudio.paContinue)


