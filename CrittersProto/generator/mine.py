#####################################################################
#
# puffer.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan, mnjy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from baseGenerator import BaseGenerator


# Chord types
I = [0, 4, 7]
ii = [1, 4, 8]
iii = [2, 5, 9]
IV = [3, 7, 10]
V = [4, 8, 11]
vi = [5, 8, 12]
vii0 = [6, 9, 12]
VIII = [7, 11, 14]

# Progressions
PROG_1 = [I, V, I, V]
PROG_2 = [I, ii, V, I]
PROG_3 = [I, iii, vi, V]
PROG_4 = [I, V, vii0, VIII]
PROG_5 = [iii, vi, IV, I]
PROG_6 = [vi, IV, V, I]
PROG_7 = [vi, ii, V, I]
PROG_8 = [VIII, IV, V, I]

# Master progressions list
PROGRESSIONS = [PROG_1, PROG_2, PROG_3, PROG_4, PROG_5, PROG_6, PROG_7, PROG_8]

NOTES_PER_BEAT = 1

class Mine(BaseGenerator):
    def __init__(self, idx):
        BaseGenerator.__init__(self, 1)

        idx = (idx-1)%len(PROGRESSIONS)
        # Key and chords
        self.key = 36
        self.chords = PROGRESSIONS[idx]
        self.note_velocity = 60

        self.set_cpb(0, 0, 58) #tuba
        self.set_num_notes_per_beat(NOTES_PER_BEAT)

    def get_notes_list(self):
        return self.chords

    def get_note_velocity(self):
        return self.note_velocity

        
