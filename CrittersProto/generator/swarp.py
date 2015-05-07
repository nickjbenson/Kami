##################################################################
#
# swarp.py
# A harp like school of fish/creature/thing that makes shimmering
# noises
#
# benchan
#
# MIT License (http://opensource.org/licenses/MIT)
#
##################################################################

from baseGenerator import BaseGenerator

NOTES_PER_BEAT = 8

class Swarp(BaseGenerator):
    def __init__(self, seed):
        BaseGenerator.__init__(self, seed)
        # Notes
        self.notes = choose_notes()
        self.note_velocity = 60
        self.set_cpb(0, 0, seed)
        self.set_num_notes_per_beat(NOTES_PER_BEAT)
        print seed

    def get_notes_list(self):
        return self.notes

    def get_note_velocity(self):
        return self.note_velocity



def choose_notes():
    notesList = []
    for i in xrange(60, 85):
        notesList.append(i)
    return notesList


