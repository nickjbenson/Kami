##################################################################
# benchan
#
# MIT License (http://opensource.org/licenses/MIT)
#
##################################################################

from baseGenerator import BaseGenerator
import random

NOTES_PER_BEAT = 1

kPitches = (60, 64, 67, 72,76,79,84, 88, 65, 60, 65)

class Clang(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed, synthName="../JR_male.sf2")
        # Notes
        self.notes = choose_notes()
        self.note_velocity = 60
        self.set_cpb(0, 0, 0)
        # Bignoise.sf2 - 0, 0, 1 -> thunder

        self.set_num_notes_per_beat(NOTES_PER_BEAT)

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity



def choose_notes():
    notesList = []
    probSustain = 1
    probNoteOn = 0.15
    noteOn = False
    # 16 beats
    for note in xrange(0, 16):
        if not noteOn and random.random() < probNoteOn:
            randIndex = random.randint(0, len(kPitches) - 2)
            notesList.append([kPitches[randIndex], kPitches[randIndex + 1]])
            probSustain = 0.7
            noteOn = True
        elif noteOn and random.random() < probSustain:
            notesList.append(0)
            probSustain -= 0.1
        else:
            notesList.append(-1)
            noteOn = False
    return notesList
