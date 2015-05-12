##################################################################
# benchan
#
# MIT License (http://opensource.org/licenses/MIT)
#
##################################################################

from baseGenerator import BaseGenerator
import random

NOTES_PER_BEAT = 0.5

kPitches = (48, 60, 64, 67, 72,64, 67, 72,76,79,84, 88, 91, 96)

class Angel(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)
        # Notes
        self.notes = choose_notes()
        self.note_velocity = 60
        self.set_cpb(0, 0, 52)
        # Bignoise.sf2 - 0, 0, 1 -> thunder

        self.set_num_notes_per_beat(NOTES_PER_BEAT)

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity



def choose_notes():
    notesList = []
    probSustain = 1
    probNoteOn = 0.6
    noteOn = False
    maxIndex = 0
    # 16 beats
    for note in xrange(0, 8):
        if not noteOn and random.random() < probNoteOn:
            randIndex = random.randint(0, len(kPitches) - 2)
            if randIndex < maxIndex:
                notesList.append(0)
            else:
                notesList.append(kPitches[randIndex])
                maxIndex = randIndex
                probSustain = 1
                noteOn = True
        elif noteOn and random.random() < probSustain:
            notesList.append(0)
            probSustain -= 0.2
        else:
            notesList.append(-1)
            noteOn = False
    return notesList
