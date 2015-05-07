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
from maracaws_algorithm import *

class Maracaws(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)

        # Key and chord
        self.toplay = 70
        self.notes = choose_pattern()
        for i in xrange(0, len(self.notes)):
            if self.notes[i] > 0:
                self.notes[i] = self.toplay
            else:
                self.notes[i] = -1

        self.note_velocity = 60 * 2 * NOTE_VELOCITY_MULT

        self.set_cpb(0, 128, 0)

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity


