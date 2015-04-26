#####################################################################
#
# note.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from audio import kSamplingRate
import numpy as np

# Twelevth root of 2
kTRT = pow(2.0, 1.0/12.0) 

# convert midi pitch to frequency in Hz
# A440 = midi note 69
def midi_to_frequency(n) :
   return 440.0 * pow(kTRT, (n - 69))


class NoteGenerator(object):
   harmonics = {
      'sine' : (1.0, ),
      'square' : (1., 0, 1/3., 0, 1/5., 0, 1/7., 0, 1/9.), 
      'saw': (1., 1/2., 1/3., 1/4., 1/5., 1/6., 1/7., 1/8., 1/9.),
      'tri': (1., 0, -1/9., 0, 1/25., 0, -1/49.)
      }

   def __init__(self, pitch, gain, duration, type = 'sine') :
      super(NoteGenerator, self).__init__()

      self.freq = midi_to_frequency(pitch)
      self.gain = float(gain)
      self.dur = float(duration)
      self.frame = 0
      self.off_now = False

      self.attack_n = .5
      self.decay_n = .5

      attack_t = .02
      decay_t = duration - attack_t

      self.attack_f = round(attack_t * kSamplingRate)
      self.decay_f =  round(decay_t * kSamplingRate)

      self.harmonics = NoteGenerator.harmonics[type]

   # turn off the note immediately
   def off(self) :
      self.off_now = True

   def envelope(self, frames):

      # envelopes:
      attack = (frames / self.attack_f) ** (1/self.attack_n)
      decay = 1 - ((frames - self.attack_f) / self.decay_f) ** (1/self.decay_n)

      env = np.minimum(attack, decay)

      # if we need to cut off note now, ramp down to 0 immediately
      if self.off_now:
         start = env[0]
         env = np.linspace(start, -.01, len(frames))

      # clamp curve to 0
      env[env < 0] = 0
      return env

   def generate(self, num_frames) :
      # create range of frames to fill this buffer
      frames = np.arange(self.frame, self.frame + num_frames)

      # conversion factor: frequency -> cosine theta
      factor = self.freq * 2 * np.pi / kSamplingRate

      # envelope curve
      env = self.envelope(frames)

      # create output
      output = self.gain * env * additive_synth( factor * frames, self.harmonics )

      # advance frame counter
      self.frame += num_frames

      # we consider this note done when it's envelope reaches 0
      keep_going = env[-1] > 0

      #return (output, keep_going)


      # convert to interleaved stereo:
      stereo = np.empty(2 * num_frames, dtype=np.float32)
      
      stereo[0::2] = output # left channel
      stereo[1::2] = output # right channel
      return (stereo, keep_going)


def additive_synth(time, harmonics) :
   signal = harmonics[0] * np.sin( time )
   for (h, w) in enumerate( harmonics[1:] ):
      if w != 0:
         signal += w * np.sin( time * (h+2))
   return signal


