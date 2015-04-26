#####################################################################
#
# wavegen.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import numpy as np
import wave
from audio import *


# now, lets make a class that can hold samples in memory so that it
# can access them quickly and repeatedly. But, first, we just typed a bunch of code
# to read wave data and convert to mono-float. Let's abstract that and reuse it:
class WaveReader(object):
   def __init__(self, filepath) :
      super(WaveReader, self).__init__()

      self.wave = wave.open(filepath)
      self.channels, self.sampwidth, self.sr, self.end, \
         comptype, compname = self.wave.getparams()
      
      # for now, we will only accept stereo, 16 bit files      
      assert(self.channels == 2)
      assert(self.sampwidth == 2)

   def set_pos(self, pos) :
      self.wave.setpos(pos)

   def get_pos(self) :
      return self.wave.tell()

   # read an arbitrary chunk of an arbitrary length
   def read(self, num_frames) :
      # get the raw data from wave file as a byte string.
      # will return num_frames, or less if too close to end of file
      raw_bytes = self.wave.readframes(num_frames)

      frames_read = len(raw_bytes) / (self.sampwidth * self.channels)

      # convert to numpy array, where the dtype is int16 or int8
      samples = np.fromstring(raw_bytes, dtype = np.int16)

      # convert from integer type to floating point, and scale to [-1, 1]
      samples = samples.astype(np.float32)
      samples *= (1 / 32768.0)

      return samples


# now the wave file generator is much smaller. Most of the code is just dealing
# with the end condition
class WaveFileGenerator(object):
   def __init__(self, filepath) :
      self.reader = WaveReader(filepath)
      self.gain = 1
      self.paused = False

   def get_pos(self):
      return self.reader.get_pos()

   def set_pos(self, pos):
      self.reader.set_pos(pos)

   def set_gain(self, gain):
      self.gain = gain

   def reset(self):
      self.paused = True
      self.set_pos(0)

   def play_toggle(self):
      self.paused = not self.paused

   def start(self):
      self.paused = False

   def stop(self):
      self.paused = True

   def generate(self, num_frames) :
      keep_going = True

      if self.paused:
         output = np.zeros(num_frames * 2)
      else:
         output = self.reader.read(num_frames) * self.gain
         keep_going = len(output) == num_frames * 2

      return (output, keep_going)


# let's make a class that holds a small amount of wave data in memory
# and can play that like a sample, almost like a note.
class WaveSnippet(object):
   def __init__(self, wave_reader, start_frame, num_frames) :
      super(WaveSnippet, self).__init__()

      # get a local copy of the audio data from WaveReader
      wave_reader.set_pos(start_frame)
      self.data = wave_reader.read(num_frames)

   # the WaveSnippet itself should not play the note. It is not a generator.
   # Just a place to hold data. We create a generator with a reference to the
   # data and a way of keeping track of the current frame
   class Generator(object):
      def __init__(self, data, loop, speed) :
         super(WaveSnippet.Generator, self).__init__()
         self.data = data
         self.loop = loop
         self.speed = speed
         self.frame = 0
         self.end_frame = len (self.data) / 2
         self.release = False

      def set_speed(self, speed) :
         self.speed = speed

      def stop(self) :
         self.release = True

      def generate(self, num_frames) :
         # we need to grab a different # of frames, depending on speed:
         adj_frames = int(round(num_frames * self.speed))

         # get data - internal function takes care of looping if needed
         data, keep_going = self._get_data(adj_frames)

         # split L/R:
         data_l = data[0::2]
         data_r = data[1::2]

         # stretch or squash data to fit exactly into num_frames (ie 512)
         x = np.arange(adj_frames)
         x_resampled = np.linspace(0, adj_frames, num_frames)
         resampled_l = np.interp(x_resampled, x, data_l)
         resampled_r = np.interp(x_resampled, x, data_r)

         # handle release (fade out to avoid pop):
         if self.release:
            env = np.linspace(1.0,0.0, num_frames)
            resampled_l *= env
            resampled_r *= env
            keep_going = False

         # convert back to stereo
         output = np.empty(2 * num_frames, dtype=np.float32)
         output[0::2] = resampled_l
         output[1::2] = resampled_r

         return (output, keep_going)


      # return data and continue flag, but include looping provision
      def _get_data(self, num_frames) :
         keep_going = True

         # grab correct chunk of data
         start = self.frame * 2
         end = (self.frame + num_frames) * 2
         output = self.data[start : end]

         # advance current-frame
         self.frame += num_frames

         # check for end of data:
         remainder = self.frame - self.end_frame
         if remainder > 0:

            # in looping case, add a bit of data from the beginning
            if self.loop:
               output = np.append(output, self.data[0:remainder*2])
               self.frame = remainder

            # in non-looping case, append zeros to match expected buffer size
            else:
               output = np.append(output, np.zeros(remainder*2))
               keep_going = False

         # return output
         return (output, keep_going)

   # to play this audio, we need a generator class the plays the data. This is an
   def make_generator(self, loop, speed) :
      return WaveSnippet.Generator(self.data, loop, speed)
      

# simple class to hold a region and it's name
class AudioRegion(object):
   def __init__(self, name, start, len):
      super(AudioRegion, self).__init__()

      self.name = name
      self.start = start
      self.len = len

   def __repr__(self):
      return '[%s %d, %d]' % (self.name, self.start, self.len)


# a collection of regions read from a file
class SongRegions(object):
   def __init__(self, filepath):
      super(SongRegions, self).__init__()

      self.regions = []
      self.read_regions(filepath)

   def __repr__(self):
      out = ''
      for r in self.regions:
         out = out + str(r) + '\n'
      return out

   def read_regions(self, filepath) :
      lines = open(filepath).readlines()
   
      for line in lines:
         # each region is: start_time val len name, separated by tabs.
         # we don't care about val
         # time values are in seconds
         (start_sec, x, len_sec, name) = line.strip().split('\t')

         # convert time (in seconds) to frames. Assumes kSamplingRate
         start_f = int( float(start_sec) * kSamplingRate )
         len_f = int( float(len_sec) * kSamplingRate )

         self.regions.append(AudioRegion(name, start_f, len_f))


def make_snippets(regions, reader):
   snippets = {}
   for r in regions.regions:
      snippets[r.name] = WaveSnippet(reader, r.start, r.len)
   return snippets

