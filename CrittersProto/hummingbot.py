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

        self.currentSeed = 0

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
        self.nextHummingTick = 0 # -1 if doesn't need one
        #self.audio.start_recording()


    def spawn_random_hummingloop(self, seed=-1):
        if seed is -1:
            seed = random.randint(1, 100000)
        self.spawn_hummingloop(seed, self.synth, 0, 0, 69) #69

    def spawn_hummingloop(self, seed, synth, channel, bank, preset):
        critter = Hummingloop(self, seed,\
            synth, channel, bank, preset, callback=self.critter_did_noteon)
        critter.set_loop_once(True)
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
        critter = Boxworm(self, seed, synth, callback=self.critter_did_noteon)
        critter.set_loop_once(True)
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
            #self.audio.start_recording()
            self.generate_next_boxworm()

    def critter_did_noteon(self, tick, pitch, velocity, duration):
        # start recording only on first "note_on" 
        # (which includes silent beats)
        self.audio.start_recording()
            
    def generate_next_boxworm(self):
        self.audio.stop_recording()
        self.audio.set_wav_file("wav/box_output" + str(self.currentSeed) + ".wav")
        if self.currentSeed < 1:
            # TODOVV HACK HACK HACK the first output is missycnrhonized
            # so generate it twice
            self.spawn_random_boxworm(seed=1)
        else:
            self.spawn_random_boxworm(seed=self.currentSeed)
        #self.audio.start_recording()
        self.currentSeed += 1

    def generate_next_humming(self):
        self.audio.stop_recording()
        self.audio.set_wav_file("wav/hum_output" + str(self.currentSeed) + ".wav")
        if self.currentSeed < 1:
            # TODOVV HACK HACK HACK the first output is missycnrhonized
            # so generate it twice
            self.spawn_random_hummingloop(seed=1)
        else:
            self.spawn_random_hummingloop(seed=self.currentSeed)
        #self.audio.start_recording()
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
                # stop all recordings until the next one
                self.audio.stop_recording()
                self.nextHummingTick = self.song.cond.get_tick() + 1000

        if self.nextHummingTick > 0 and self.song.cond.get_tick() > self.nextHummingTick:
            # create the next humming loop, (the dealy in creation is to deal with potential lag and what not)
            # is probably not necessary, but let's test it out.
            self.nextHummingTick = -1
            self.generate_next_boxworm()

        # Kill loops
        for loop in kill_list:
            self.canvas.remove(loop)
            self.critters.remove(loop)
            self.song.remove_track(loop)
            #self.audio.remove_generator(self.synths[loop.seed])

run(MainWidget)