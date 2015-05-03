# oscilloop.py

# Basically just copied from hummingloop.py

from baseGenerator import BaseGenerator
from oscilloop_algorithm import *

# Synth track
class Oscilloop(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)

        # Key and chord
        key = C
        chord = random.choice(CHORDS)
        self.notes, self.config, self.norm_const = choose_notes(key, chord)

        self.note_velocity = 60 # NOTE velocity

        # MIDI parameters
        self.set_cpb(0, 0, 89)
        self.set_cc(0, 91, 124)
        self.set_cc(0, 93, 100)

    # override BaseGenerator note methods
    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity

    # Used by Main
    def get_config(self):
        return self.config

    def get_norm_const(self):
        return self.norm_const
