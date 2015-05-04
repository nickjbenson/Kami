#####################################################################
#
# boxworm.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from baseGenerator import BaseGenerator
from boxworm_algorithm import *


class BoxWorm(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)
        # Notes
        self.notes = choose_notes()
        self.note_velocity = 60 * 2 * NOTE_VELOCITY_MULT
        self.set_cpb(10, 128, 0)

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity






