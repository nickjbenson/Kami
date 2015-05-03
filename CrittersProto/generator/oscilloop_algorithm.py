#####################################################################
#
# oscilloop_algorithm.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import random
import math

# MIDI values
C = 48

# Chord types
MAJOR_S = [0, 2, 4, 5, 7, 9, 11]
MINOR_S = [-3, -1, 0, 2, 4, 5, 8]
MAJOR = [0, 4, 7]
MINOR = [-3, 0, 4]
MAJOR_7 = [0, 4, 7, 11]
MINOR_7 = [-3, 0, 4, 8]
MAJOR_SUS4 = [0, 4, 5, 7]
MINOR_SUS4 = [-3, 0, 2, 4]
MAJOR_7_SUS4 = [0, 4, 5, 7, 11]
MINOR_7_SUS4 = [-3, 0, 2, 4, 8]
MAJOR_ADD9 = [0, 4, 7, 14]
MINOR_ADD9 = [-3, 1, 4, 11]
MAJOR_7_ADD9 = [0, 4, 7, 11, 14]
MINOR_7_ADD9 = [-3, 1, 4, 8, 11]
WHOLE_TONE = [0, 2, 4, 6, 8, 10]
PENTA = [0, 2, 4, 7, 9]

# Master chord list
CHORDS = [PENTA, MAJOR_7_ADD9]

def choose_notes(key, chord):
    # Set up arpeggiation MIDI notes

    notes_init = [key + chord[x]\
        for x in range(len(chord))]
    notes_span = [note - 24 for note in notes_init]\
        + [note - 12 for note in notes_init]\
        + notes_init\
        + [note + 12 for note in notes_init]\
        + [note + 24 for note in notes_init]\
        + [note + 36 for note in notes_init]
    span_length = len(notes_init)*3
    osc_amp = (span_length-1)/2.
    osc_ctr = (len(notes_span)-1)/2.
    osc_offset = random.choice(range(-7, 7, 1))

    if osc_ctr + osc_offset + osc_amp > len(notes_span)-1:
        osc_amp = len(notes_span)-1 - osc_ctr - osc_offset

    # Note generation functions

    def osciline(x, config=[(1, 1)]):
        y = 0
        for (freq, amp) in config:
            y += amp * math.sin(freq*math.pi*x)
        return y

    def normalize(values):
        max_val = 0
        for value in values:
            if value > max_val:
                max_val = value
        return [x/float(max_val) for x in values]

    def generate_random_oscillation_configuration(base_freq):
        config = []
        for i in range(1, 16, 1):
            freq = base_freq * i * random.choice([-1, 1])
            amp = random.random() * (1./i)
            config.append((freq, amp))
        return config

    # Actual note generation

    base_freq = 1/64.

    notes = []
    notes_idx = []
    osc_values = []
    norm_osc_values = []
    osc_config = generate_random_oscillation_configuration(base_freq)
    for x in range(int(round(8 / base_freq))):
        osc_values.append(osciline(x, config=osc_config))

    norm_osc_values = normalize(osc_values)
    print "center: %s" % (str(osc_ctr))
    print "offset: %s" % (str(osc_offset))
    print "amplit: %s" % (str(osc_amp))
    print "max val: %s" % (str(len(notes_span)-1))
    for x in range(int(round(8 / base_freq))):
        notes_idx.append(int(round(osc_ctr + osc_offset + osc_amp * norm_osc_values[x])))
        notes.append(notes_span[int(round(osc_ctr + osc_offset + osc_amp * norm_osc_values[x]))])
    print notes_idx

    # Note post-processing: sustain repetitive notes
    last_note = -1
    for i in range(len(notes)):
        if notes[i] == last_note:
            last_note = notes[i]
            notes[i] = 0
        else:
            last_note = notes[i]

    print notes
    return notes
