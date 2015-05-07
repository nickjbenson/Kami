#####################################################################
#
# hummingloop.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from baseGenerator import BaseGenerator
from hummingloop_algorithm import *

class HummingLoop(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)

        # Key and chord
        key = C
        chord = random.choice(CHORDS)
        self.notes = choose_notes(key, chord)
        self.note_velocity = 60 * 2 * NOTE_VELOCITY_MULT

        self.set_cpb(0, 0, 69)

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity

    # Used by Main
    def get_config(self):
        return " ".join(str(note) for note in self.notes)




