# oscilloop_algorithm.py

import math

# "Keys" (MIDI value root notes)
C = 48

# Chords
MAJOR_7 = [0, 4, 7, 11]

# [0, 4, 7,11,12,16,19,23]
# [0, 1, 2, 3, 4, 5, 6, 7]

def choose_notes(key, chord):
    # Set up arpeggiation MIDI notes
    notes_init = [key + chord[x]\
        for x in range(len(chord))]
    notes_span = notes_init + [note + 12 for note in notes_init]

    osc_amp = (len(notes_span)-1)/2.
    osc_ctr = (len(notes_span)-1)/2.

    osc_freq_coeff = 0.25

    notes = []
    notes_idx = []
    for x in range(32):
        notes_idx.append(int(round(osc_ctr + osc_amp * math.sin(osc_freq_coeff*math.pi*x))))
        notes.append(notes_span[int(round(osc_ctr + osc_amp * math.sin(osc_freq_coeff*math.pi*x)))])
    print notes_idx
    print notes

    return notes

choose_notes(C, MAJOR_7)