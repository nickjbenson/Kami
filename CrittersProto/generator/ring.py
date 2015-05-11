##################################################################
#
# A simple school bell
#
# benchan
#
# MIT License (http://opensource.org/licenses/MIT)
#
##################################################################

from baseGenerator import BaseGenerator

NOTES_PER_BEAT = 8

class Ring(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)
        # Notes
        self.notes = choose_notes()
        self.note_velocity = 60
        self.set_cpb(0, 128, 0)

        self.set_num_notes_per_beat(NOTES_PER_BEAT)
        print seed

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity



def choose_notes():
    notesList = []
    for i in xrange(0, 16):
        notesList.append(81)
    for i in xrange(0, 48):
        notesList.append(0)
    # Super sparse
    for i in xrange(0, 8*16):
        notesList.append(0)
    return notesList


