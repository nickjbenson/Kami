#####################################################################
#
# graphics.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

# some graphics utilities

from kivy.uix.label import Label
from kivy.uix.widget import Widget
from kivy.clock import Clock as kivyClock
from kivy.graphics.instructions import InstructionGroup
from kivy.graphics import Color, Ellipse, Line

import numpy as np


# Scene Widget is just a simple manager of InstructionGroup-style objects that get updated with
# time, and removed when they are done
class Scene(Widget) :
   def __init__(self):
      super(Scene, self).__init__()
      self.objects = []

   # add an object. The object must be an InstructionGroup (ie, can be added to canvas) and 
   # it must have an on_update(self, dt) method that returns True to keep going or False to end
   def add_object(self, obj):
      self.canvas.add(obj)
      self.objects.append(obj)
   
   def on_update(self):
      dt = kivyClock.frametime
      kill_list = [o for o in self.objects if o.on_update(dt) == False]

      for o in kill_list:
         self.objects.remove(o)
         self.canvas.remove(o)


# KeyFrame Animation class - ys (the key values) can also be lists of values
# c/o Danny Manesh
# Example: a = KFAnim([0, 2], [20, 40]) - will animate a value over two seconds from 20 to 40
# Example: a = KFAnim([0, 1, 2], ([1, 0, 0], [0, 1, 0], [0, 0, 1])) - will animate 3 values. In this example,
# the values are colors, so this will animate smoothly from red to green to blue over 2 seconds.
class KFAnim(object):
   def __init__(self, xs, ys):
      super(KFAnim, self).__init__()
      self.xs = xs
      self.ys = np.array(ys)

   def eval(self, t):
      if len(self.ys.shape) == 1:
         return np.interp(t, self.xs, self.ys)
      else:
         return np.array([np.interp(t, self.xs, y_1d) for y_1d in zip(*self.ys) ])

   # return true if given time is within keyframe range. Otherwise, false.
   def is_active(self, t) :
      return t < self.xs[-1]


# A graphics object for displaying a point moving in a pre-defined 3D space
# the 3D point must be in the range [0,1] for all 3 coordinates.
# depth is rendered as the size of the circle.
class Cursor3D(InstructionGroup):
   def __init__(self, area_size, area_pos, rgb, border = True):
      super(Cursor3D, self).__init__()
      self.area_size = area_size
      self.area_pos = area_pos
      self.xy = np.array((0,0))
      
      if border:
         self.add(Color(1, 0, 0))
         self.add(Line(rectangle= area_pos + area_size))

      self.color = Color(*rgb)
      self.add(self.color)

      self.cursor = Ellipse(segments = 40)
      self.cursor.size = (30,30)
      self.add(self.cursor)

   # position is a 3D point with all values from 0 to 1
   def set_pos(self, pos):
      radius = 10 + pos[2] * 40
      self.xy = pos[0:2] * self.area_size + self.area_pos
      self.cursor.pos =  self.xy - radius
      self.cursor.size = radius*2, radius*2

   def set_color(self, rgb):
      self.color.rgb = rgb

   def get_screen_xy(self) :
      return self.xy

