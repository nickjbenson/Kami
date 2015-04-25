#####################################################################
#
# metro.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from song import Track
from clock import kTicksPerQuarter

class Metronome(Track):
   def __init__(self, synth):
      super(Metronome, self).__init__()
      self.synth = synth

      self.beat_len = kTicksPerQuarter
      self.pitch = 60
      self.channel = 0

      # run-time variables
      self.on_cmd = None
      self.off_cmd = None

   def start(self):
      print 'metro start'
      self.synth.program(self.channel, 128, 0)

      now = self.song.cond.get_tick()
      next_beat = now - (now % self.beat_len) + self.beat_len
      self._post_at(next_beat)

   def stop(self):
      print 'metro stop'

      self.song.sched.remove(self.on_cmd)
      self.song.sched.remove(self.off_cmd)

      if self.off_cmd:
         self.off_cmd.execute()

      self.on_cmd = None
      self.off_cmd = None

   def _post_at(self, tick):
      self.on_cmd = self.song.sched.post_at_tick(tick, self._noteon)

   def _noteon(self, tick, ignore):
      # play the note right now:
      self.synth.noteon(self.channel, self.pitch, 100)
      
      # post the note off for later:
      self.off_cmd = self.song.sched.post_at_tick(tick + self.beat_len/2, self._noteoff, self.pitch)

      # schedule the next note on one beat later
      next_beat = tick + self.beat_len
      self._post_at(next_beat)

   def _noteoff(self, tick, pitch):
      self.synth.noteoff(self.channel, pitch)
