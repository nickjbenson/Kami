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
from boxworm import *

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

        # Tracking the critters
        self.critters = []
        self.focus = None

        # Prompt label
        self.prompt = Label(text='foo', pos=(300, 400),\
            size=(200, 200), valign='top', font_size='20sp')
        self.prompt.text = "Input a number: "
        self.add_widget(self.prompt)

        # Last spawned critter label
        self.spawn_label = Label(text='foo', pos=(300, 300),\
            size=(200, 200), valign='top', font_size='10sp')
        self.add_widget(self.spawn_label)
        self.last_spawned = None

        # Debug info label
        self.debug_label = Label(text='foo', pos=(300, 200),\
            size=(200, 200), valign='top', font_size='20sp')
        self.add_widget(self.debug_label)

        # Other info label
        self.info_label = Label(text='foo', pos=(300, 100),\
            size=(200, 200), valign='top', font_size='20sp')
        self.info_label.text = "Press enter to send the input number.\nOr hold S to spawn randomly.\nHover over a critter to focus on it."
        self.add_widget(self.info_label)

        # width and height for convenience
        self.width = Window.width
        self.height = Window.height

        # Autospawn
        self.autospawn = False
        self.spawn_time = 0.25
        self.spawn_timer = self.spawn_time

    def spawn_random_hummingloop(self, seed=-1):
        if seed is -1:
            seed = random.randint(1, 100000)
        self.spawn_hummingloop(seed, self.synth, 0, 0, 69)

    def spawn_hummingloop(self, seed, synth, channel, bank, preset):
        critter = Hummingloop(self, seed,\
            synth, channel, bank, preset)
        self.song.add_track(critter)
        self.canvas.add(critter)
        self.critters.append(critter)
        self.song.start()

    def spawn_random_boxworm(self, seed=-1):
        if seed is -1:
            seed = random.randint(1, 100000)
        self.spawn_boxworm(seed, self.synth)

    def spawn_boxworm(self, seed, synth):
        critter = Boxworm(self, seed, synth)
        self.song.add_track(critter)
        self.canvas.add(critter)
        self.critters.append(critter)
        self.song.start()

    def remove_critter_audio(self, critter_track):
        self.song.remove_track(critter_track)

    def stop_humming(self):
        self.song.stop()

    def on_key_down(self, keycode, modifiers):
        
        if keycode[1] in '1234567890':
            self.prompt.text += keycode[1]

        elif keycode[1] is 'enter':
            if self.state is "INPUT_NUMBER":
                # Create boxworm and start playing.
                number = self.prompt.text[16:]
                self.spawn_random_boxworm(seed=number)
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

    def get_focus(self, hummingloop):
        if not self.focus:
            return 0
        elif self.focus is hummingloop:
            return 1
        else:
            return -1

    def on_update(self):
        # Update debug label
        self.debug_label.text = self.song.cond.now_str()
        self.debug_label.text += "\ntracks: "\
            + str(len(self.song.tracks))

        # Update focus label
        if self.focus:
            self.spawn_label.text = str(self.focus)
        else:
            self.spawn_label.text = "Hover over one."

        # auto-spawn with 's' key
        if self.autospawn:
            self.spawn_timer -= kivyClock.frametime
            if self.spawn_timer < 0:
                self.spawn_timer = self.spawn_time
                if random.choice([0, 1]) is 1:
                    self.spawn_random_boxworm()
                else:
                    self.spawn_random_hummingloop()

        # Update song scheduler
        self.song.on_update()

        # Update scene
        threshold = 20
        p = Window.mouse_pos
        kill_list = []
        for hummingloop in self.critters:
            # Updating focus info
            if abs(p[0] - hummingloop.pos[0]) <= threshold\
                and abs(p[1] - hummingloop.pos[1]) <= threshold:
                self.focus = hummingloop

            # Updating critters
            if hummingloop.on_update(kivyClock.frametime):
                kill_list += [hummingloop]

        # Removing focus
        if self.focus is not None\
            and abs(p[0] - self.focus.pos[0]) > threshold\
            and abs(p[1] - self.focus.pos[1]) > threshold:
            self.focus = None

        # Kill loops
        for loop in kill_list:
            self.canvas.remove(loop)
            self.critters.remove(loop)
            self.song.remove_track(loop)

run(MainWidget)