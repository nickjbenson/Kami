# humming.py

import sys
sys.path.append('./common')
from core import *
from audio import *
from clock import *
from song import *
from audiotrack import *
from wavegen import *
from graphics import *
from synth import *

from kivy.uix.label import Label
from kivy.clock import Clock as kivyClock

import random

from hummingloop import *

class MainWidget(BaseWidget) :
    def __init__(self):
        super(MainWidget, self).__init__()

        # Audio initialization
        self.audio = Audio()
        self.synth = Synth('./FluidR3_GM.sf2')
        self.audio.add_generator(self.synth)
        self.song = Song()
        self.song.cond.set_bpm(120)

        # State
        self.state = "INPUT_NUMBER"

        # Tracking the hummingloops
        self.hummingloops = []
        self.focused_this_update = False
        self.focus = None

        # Prompt label
        self.prompt = Label(text='foo', pos=(300, 400),\
            size=(200, 200), valign='top', font_size='20sp')
        self.prompt.text = "Input a number: "
        self.add_widget(self.prompt)

        # Other info label
        self.info_label = Label(text='foo', pos=(300, 100),\
            size=(200, 200), valign='top', font_size='20sp')
        self.info_label.text = "Press enter to send the input number.\nOr hold S to spawn randomly.\nHover over a hummingloop to focus on it."
        self.add_widget(self.info_label)

        # Debug info label
        self.debug_label = Label(text='foo', pos=(300, 200),\
            size=(200, 200), valign='top', font_size='20sp')
        self.add_widget(self.debug_label)

        # width and height for convenience
        self.width = Window.width
        self.height = Window.height

        # Autospawn
        self.autospawn = False
        self.spawn_time = 0.25
        self.spawn_timer = self.spawn_time

    def spawn_hummingloop(self, seed, synth, channel, bank, preset):
        hummingloop = Hummingloop(self, seed,\
            synth, channel, bank, preset)
        self.song.add_track(hummingloop)
        self.canvas.add(hummingloop)
        self.hummingloops.append(hummingloop)
        self.song.start()

    def remove_hummingloop_audio(self, hummingloop_track):
        self.song.remove_track(hummingloop_track)

    def stop_humming(self):
        self.song.stop()

    def on_key_down(self, keycode, modifiers):
        
        if keycode[1] in '1234567890':
            self.prompt.text += keycode[1]

        elif keycode[1] is 'enter':
            if self.state is "INPUT_NUMBER":
                # Create hummingloop and start playing.
                number = self.prompt.text[16:]
                self.spawn_hummingloop(number,\
                    self.synth, 0, 0, 47)
                self.prompt.text = "Input a number: "
            else:
                # Stop playing and go back.
                self.state = "INPUT_NUMBER"
                self.prompt.text = "Input a number: "
                self.stop_humming()

        elif keycode[1] is 's':
            self.autospawn = True

    def on_key_up(self, keycode):
        
        if keycode[1] is 's':
            self.autospawn = False

    def unfocus_other_hummingloops(self, focused_loop):
        for loop in self.hummingloops:
            if loop is not focused_loop:
                loop.set_focus(-1)

    def neutralize_focus(self):
        for loop in self.hummingloops:
            loop.set_focus(0)

    def on_update(self):
        # Update debug label
        self.debug_label.text = self.song.cond.now_str()

        # auto-spawn with 's' key
        if self.autospawn:
            self.spawn_timer -= kivyClock.frametime
            if self.spawn_timer < 0:
                self.spawn_timer = self.spawn_time
                self.spawn_hummingloop(random.randint(1, 1000000),\
                    self.synth, 0, 0, 39)

        # Update song scheduler
        self.song.on_update()

        # Update scene
        threshold = 20
        focused = False
        p = Window.mouse_pos
        kill_list = []
        for hummingloop in self.hummingloops:
            # Updating focus info
            if abs(p[0] - hummingloop.pos[0]) <= threshold\
                and abs(p[1] - hummingloop.pos[1]) <= threshold\
                and self.focus is not hummingloop:
                hummingloop.set_focus(1)
                self.focus = hummingloop
                focused = True

            # Updating hummingloops
            if hummingloop.on_update(kivyClock.frametime):
                kill_list += [hummingloop]
        if focused:
            self.unfocus_other_hummingloops(hummingloop)
        if self.focus is not None\
            and abs(p[0] - self.focus.pos[0]) > threshold\
            and abs(p[1] - self.focus.pos[1]) > threshold:
            self.neutralize_focus()
            self.focus = None

        # Kill loops
        for loop in kill_list:
            self.canvas.remove(loop)
            self.hummingloops.remove(loop)
            self.song.remove_track(loop)

run(MainWidget)