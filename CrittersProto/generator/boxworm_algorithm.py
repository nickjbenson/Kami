#####################################################################
#
# hummingloop_algorithm.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import random as r

def choose_notes():
    notes = []

    pattern = r.choice(PATTERNS)
    subpat_dict = {}
    # Generate subpattern dictionary
    for mapping in pattern[1]:
        # Generate subpattern
        new_subpat = []
        subpat_probs = r.choice(HIT_PROBS)
        for i in range(mapping[1]):
            if r.random() < subpat_probs[i]:
                new_subpat.append(r.choice(HITS))
            else:
                new_subpat.append(-1)
        subpat_dict[mapping[0]] = new_subpat
    # Generate notes based on pattern
    for char in pattern[0]:
        notes += subpat_dict[char]

    # Late-pass mutation: Ensure first-note hit
    notes[0] = r.choice(HITS)

    # Late-pass mutation: Alternate rapid sequence hits
    cur_hit = -1
    for i in range(len(notes)):
        if notes[i] == cur_hit:
            notes[i] = ALT_HITS[notes[i]]
        cur_hit = notes[i]

    print "Notes: " + str(notes)
    return notes

# Rhythm patterns
PATTERN_1 = ("ABABABAC", [("A", 8), ("B", 8), ("C", 8)])
PATTERN_2 = ("AABAABAABAAC", [("A", 4), ("B", 8), ("C", 8)])
PATTERN_3 = ("ABABABACC", [("A", 8), ("B", 8), ("C", 4)])
PATTERNS = [PATTERN_1, PATTERN_2, PATTERN_3]

# 16th slot hit probabilities
HIT_PROB_1 = [0.6, 0.4, 0.5, 0.4]*4
HIT_PROB_2 = [0.8, 0.3, 0.7, 0.3]*4
HIT_PROB_3 = [0.3, 0.8, 0.5, 0.6]*4
HIT_PROBS = [HIT_PROB_1, HIT_PROB_2, HIT_PROB_3]

# Possible hits
HITS = [48, 45, 42, 35]
ALT_HITS = {-1:-1, 48:50, 45:47, 42:-1, 35:36}

# Interaction configuration
FOCUS_SPEED = 100 # velocity ints / second
MAX_VEL = 50
MIN_VEL = 50
NOTE_VELOCITY_MULT = 0.5