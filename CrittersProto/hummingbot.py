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
import time

from hummingloop import *
from boxworm import *

class MainWidget(BaseWidget) :
    def __init__(self):
        super(MainWidget, self).__init__()

        self.currentSeed = 1

        # Tracking the critters
        self.critters = []

        # Audio initialization
        self.audio = Audio()
        self.synth = Synth('./FluidR3_GM.sf2')
        self.audio.add_generator(self.synth)
        self.song = Song()
        self.song.cond.set_bpm(120)

        # well, here's a test
        #self.synths = {}


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
        #aNewSynth = Synth('./FluidR3_GM.sf2')
        #self.audio.add_generator(aNewSynth)
        self.spawn_boxworm(seed, self.synth)
        #self.synths[seed] = aNewSynth

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
       if keycode[1] is 'k':
            self.audio.start_recording()
            self.generate_next_humming()
            
    def generate_next_boxworm(self):
        self.audio.set_wav_file("wav/humoutput" + str(self.currentSeed) + ".wav")
        self.spawn_random_boxworm(seed=self.currentSeed)
        self.currentSeed += 1

    def generate_next_humming(self):
        self.audio.set_wav_file("wav/humoutput" + str(self.currentSeed) + ".wav")
        self.spawn_random_hummingloop(seed=self.currentSeed)
        self.currentSeed += 1

    def on_key_up(self, keycode):
        pass

    def get_focus(self, hummingloop):
        return 1

    def on_update(self):
       
        # Update song scheduler
        self.song.on_update()

        # Update scene
        kill_list = []
        for critter in self.critters:
            # Updating critters
            if critter.on_update(kivyClock.frametime):
                kill_list += [critter]
                self.generate_next_humming()

        # Kill loops
        for loop in kill_list:
            self.canvas.remove(loop)
            self.critters.remove(loop)
            self.song.remove_track(loop)
            #self.audio.remove_generator(self.synths[loop.seed])

run(MainWidget)