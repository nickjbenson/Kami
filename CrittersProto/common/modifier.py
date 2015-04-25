#####################################################################
#
# modifier.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from kivy.core.window import Window
import numpy as np

# lets the client modify a value by holding down a particular keyboard key and
# moving the mouse up/down to change a value and apply that value to a function.
# Requires that on_key_down, on_key_up, and on_update be called.
# inputs are:
# key - keyboard key to activate (ie, 'a')
# name - name of the property
# values - list of values to choose from (ie, (1,2,3,4))
# func - function to call with one of the values given.
class Modifier(object):
   def __init__(self, key, name, values, func):
      super(Modifier, self).__init__()
      self.key = key
      self.name = name
      self.values = values
      self.func = func
      
      self.idx = 0
      self.pos = None

   def on_key_down(self, key) :
      if self.key == key:
         self.pos = Window.mouse_pos[1]

   def on_key_up(self, key) :
      if self.key == key:
         self.pos = None

   def on_update(self):
      if self.pos != None:
         p = Window.mouse_pos[1]
         delta = p - self.pos
         if delta > 5 or delta < -5:
            self._change_idx(delta/5)
            self.pos = p

   def get_txt(self) :
      active = '   ' if self.pos is None else ' >'
      return '%s[%s] %s: %s' % (active, self.key, self.name, str(self.values[self.idx]))

   def _change_idx(self, d):
      old_idx = self.idx
      self.idx = np.clip(self.idx + d, 0, len(self.values) - 1)
      if old_idx != self.idx:
         self.func(self.values[self.idx])
