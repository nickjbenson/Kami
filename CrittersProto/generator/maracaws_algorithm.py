#####################################################################
#
# maracas_algorithm.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import random

# Silence probability lists
HALF_SILENCE = [0, 1]
THIRD_SILENCE = [0, 0, 1]
TWOTHIRD_SILENCE = [0, 1, 1]
# Master silence probability list
SILENCE_PROB_LISTS = [HALF_SILENCE, THIRD_SILENCE,\
TWOTHIRD_SILENCE]

# Sustain probability lists
HALF_SUSTAIN = [0, 1]
THIRD_SUSTAIN = [0, 0, 1]
TWOTHIRD_SUSTAIN = [0, 1, 1]
# Master sustain probability list
SUSTAIN_PROB_LISTS = [HALF_SUSTAIN,  THIRD_SUSTAIN,\
TWOTHIRD_SUSTAIN]

# Patterns
PATTERN_1 = [("A", 8), ("B", 8), ("A", 8), ("C", 8)]
PATTERN_2 = [("A", 8), ("A", 8), ("B", 8), ("B", 8)]
PATTERN_3 = [("A", 8), ("B", 8), ("C", 8), ("B", 8)]
PATTERN_4 = [("A", 8), ("A", 8), ("B", 8), ("C", 8)]
PATTERN_5 = [("A", 8), ("B", 8), ("B", 8), ("A", 8)]
PATTERN_6 =\
[("A", 4), ("A", 4), ("B", 8), ("C", 8), ("A", 4), ("C", 4)]
PATTERN_7 =\
[("A", 4), ("B", 4), ("A", 4), ("B", 4),\
("A", 4), ("C", 4), ("A", 4), ("C", 4)]

# Master pattern list
PATTERNS = [PATTERN_1,\
PATTERN_2, PATTERN_3,\
PATTERN_4, PATTERN_5,\
PATTERN_6, PATTERN_7]

# Interaction configuration
FOCUS_SPEED = 100 # velocity ints / second
MAX_VEL = 50
MIN_VEL = 50
NOTE_VELOCITY_MULT = 0.5

def choose_pattern():

    # Generate notes
    notes = []
    subpat_dict = {}
    pattern = random.choice(PATTERNS)
    silence_prob_list = random.choice(SILENCE_PROB_LISTS)
    for subpat in pattern:
        if subpat[0] not in subpat_dict:
            # Generate new subpattern
            new_subpat = []
            for i in range(subpat[1]):
                new_subpat += [1]
                # Silence
                if random.choice(silence_prob_list) is 1:
                    new_subpat[-1] = -1
            subpat_dict[subpat[0]] = new_subpat
        # Add each subpattern's notes according to pattern
        notes += subpat_dict[subpat[0]]

    # Sustain processing of notes.
    # Add potential sustains instead of silences
    # (0 instead of -1)
    sustain_possible = False
    sustain_prob_list = random.choice(SUSTAIN_PROB_LISTS)
    for i in range(len(notes)):
        if notes[i] is not -1:
            # A note can be sustained.
            sustain_possible = True
        if notes[i] is -1 and sustain_possible:
            if random.choice(sustain_prob_list) is 1:
                notes[i] = 0 if i < len(notes)-1 else -1
            else:
                # A note-off event will happen,
                # sustain is no longer possible
                sustain_possible = False
                
    return notes
