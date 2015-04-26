from synth.py import *

class Looper(Track):
    def __init_(self, notes, cbp=(0, 0, 1), synth=None):
        self.notes = notes
        self.idx = 0
        self.cbp = cbp
        self.playing = False

        # Initialize synth if none provided
        if synth is None:
            self.synth = Synth()

    def set_notes(self, notes):
        self.notes = notes
        self.idx = min(len(notes), self.idx)

    def play(self):
        self.playing = True

    def stop(self):
        self.playing = False