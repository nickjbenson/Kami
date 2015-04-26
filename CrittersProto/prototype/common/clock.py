#####################################################################
#
# clock.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import time
import numpy as np

# Simple time keeper object. It starts at 0 and knows how to pause
class Clock(object):
   def __init__(self):
      super(Clock, self).__init__()
      self.paused = True
      self.offset = 0

   def is_paused(self):
      return self.paused

   def get_time(self):
      if self.paused:
         return self.offset
      else:
         return self.offset + time.time()

   def start(self):
      if self.paused:
         self.paused = False
         self.offset -= time.time()

   def stop(self):
      if not self.paused:
         self.paused = True
         self.offset += time.time()

   def toggle(self):
      if self.paused:
         self.start()
      else:
         self.stop()


# The conductor object 
kTicksPerQuarter = 480


# data passed into tempo map is a list of points
# where each point is (time, tick)
# optionally pass in filepath instead which will
# read the file to create the list of (time, tick) points
# TempoMap will linearly interpolate this graph
class TempoMap(object):
   def __init__(self, data = None, filepath = None):
      super(TempoMap, self).__init__()

      if data == None:
         data = self._read_tempo_data(filepath)

      assert(data[0] == (0,0))
      assert(len(data) > 1)

      #self.times = np.array([x[0] for x in data])
      #self.ticks = np.array([x[1] for x in data])
      self.times, self.ticks = zip(*data)
      
   def time_to_tick(self, time) :
      tick = np.interp(time, self.times, self.ticks)
      return tick

   def tick_to_time(self, tick) :
      time = np.interp(tick, self.ticks, self.times)
      return time

   def _read_tempo_data(self, filepath):
      data = [(0,0)]

      for line in open(filepath).readlines():
         (time, beats) = line.strip().split('\t')
         time = float(time)

         delta_tick = float(beats) * kTicksPerQuarter
         last_tick = data[-1][1]
         data.append( (time, last_tick + delta_tick))

      return data


# Knows about a clock. Converts between time and ticks.
class Conductor(object):
   def __init__(self, clock) :
      super(Conductor, self).__init__()
      # stuff related to live-tempo setting
      self.bpm = 80
      self.offset = 0

      self.tempo_map = None
      self.clock = clock

   def set_bpm(self, bpm) :
      if self.tempo_map:
         print 'Cannot set bpm. Conductor is using a tempo_map.'
      else:
         # ensure that current time/tick remains the same before & after tempo change
         time = self.get_time()
         tick = self.time_to_tick(time)

         # using y = mx + b, y is tick, x = time, m is tempo slope, b is self.offset
         # so b = y - mx:
         self.bpm = bpm
         self.offset = tick - (self.bpm / 60) * kTicksPerQuarter * time

   def get_bpm(self) :
      if self.tempo_map:
         # get approximate tempo by asking tempo map for 2 points:
         time1 = self.get_time()
         time2 = time1 + 1
         tick1 = self.tempo_map.time_to_tick(time1)
         tick2 = self.tempo_map.time_to_tick(time2)

         slope = (tick2 - tick1) / (time2 - time1)
         bpm = slope * 60.0 / kTicksPerQuarter
         return bpm

      else:
         return self.bpm

   def set_tempo_map(self, tempo_map) :
      self.tempo_map = tempo_map

   def get_time(self) :
      return self.clock.get_time()

   def get_tick(self) :
      sec = self.get_time()
      return self.time_to_tick(sec)

   def time_to_tick(self, time):
      if self.tempo_map:
         return self.tempo_map.time_to_tick(time)
      else:
         return int( (self.bpm / 60) * kTicksPerQuarter * time + self.offset )

   def tick_to_time(self, tick):
      if self.tempo_map:
         return self.tempo_map.tick_to_time(tick)
      else:
         return 
      
   def now_str(self):
      time = self.get_time()
      tick = self.get_tick()
      beat = float(tick) / kTicksPerQuarter
      txt = "time:%.2f\ntick:%d\nbeat:%.2f" % (time, tick, beat)
      return txt


class Scheduler(object):
   def __init__(self, cond) :
      super(Scheduler, self).__init__()
      self.cond = cond
      self.commands = []

   # add a record for the function to call at the particular tick
   # keep the list of commands sorted from lowest to hightest tick
   # make sure tick is the first argument so sorting will work out
   # properly
   def post_at_tick(self, tick, func, arg = None) :
      now = self.cond.get_tick()

      if tick <= now:
         func(tick, arg)
         return None
      else:
         cmd = Command(tick, func, arg)
         self.commands.append(cmd)
         self.commands.sort(key = lambda x: x.tick)
         return cmd

   # attempt a removal. Does nothing if cmd is not found
   def remove(self, cmd):
      if cmd in self.commands:
         idx = self.commands.index(cmd)
         del self.commands[idx]

   # on_update should be called as often as possible.
   # the only trick here is to make sure we remove the command BEFORE
   # calling the command's function.
   def on_update(self):
      now = self.cond.get_tick()
      while self.commands:
         if self.commands[0].should_exec(now):
            command = self.commands.pop(0)
            command.execute()
         else:
            break

 
class Command(object):
   def __init__(self, tick, func, arg):
      super(Command, self).__init__()
      self.tick = int(tick)
      self.func = func
      self.arg = arg
      self.did_it = False
      
   def should_exec(self, tick) :
      return self.tick <= tick

   def execute(self):
      # ensure that execute only gets called once.
      if not self.did_it:
         self.did_it = True
         self.func( self.tick, self.arg )

   def __repr__(self):
      return 'cmd:%d' % self.tick
