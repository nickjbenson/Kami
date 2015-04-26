#####################################################################
#
# hummingloop.py
#
# Copyright (c) 2015, Nick Benson
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from song import Track
from clock import kTicksPerQuarter

from kivy.graphics.instructions import InstructionGroup
from kivy.graphics import Color, Ellipse, Line, Rectangle
from kivy.graphics import PushMatrix, PopMatrix, Translate, Scale, Rotate

import random

# MIDI values
C = 60

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

# Master chord list
CHORDS = [MAJOR_S, MINOR_S,\
MAJOR, MINOR,\
MAJOR_7, MINOR_7,\
MAJOR_SUS4, MINOR_SUS4,\
MAJOR_7_SUS4, MINOR_7_SUS4,\
MAJOR_ADD9, MINOR_ADD9,\
MAJOR_7_ADD9, MINOR_7_ADD9]

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

# Utility functions
def make_ellipse(pos, rad, segments=20):
    return Ellipse(pos=(pos[0]-rad, pos[1]-rad), size=(rad*2, rad*2),\
        segments=segments)

def move_ellipse(ellipse, rad, newpos):
    ellipse.pos = (newpos[0]-rad, newpos[1]-rad)

# Synth track
class Hummingloop(InstructionGroup, Track):
    def __init__(self, widget, seed, synth, channel, bank, preset, callback=None):
        super(Hummingloop, self).__init__()
        self.playing = False
        self.callback = callback
        self.widget = widget
        self.seed = seed
        random.seed(seed)

        # MIDI parameters
        self.synth = synth
        self.channel = channel
        self.cbp = (channel, bank, preset)
        #self.synth.cc(self.channel, 1, 40)

        # Performance parameters
        self.note_grid = kTicksPerQuarter / 4
        self.note_len_ratio = 1

        # Key and chord
        self.key = C
        self.chord = random.choice(CHORDS)
        self.notes = self.choose_notes()

        # Run-time variables
        self.cur_idx = 0
        self.idx_inc = 1
        self.on_cmd = None
        self.off_cmd = None
        self.note_velocity = 60 * NOTE_VELOCITY_MULT # NOTE velocity
        self.focused = 0

        # Physical run-time variables
        self.pos = (700,\
            random.randint(100, 500))
        self.vel = ((-1 if self.pos > self.widget.width/2. else 1) * \
            random.randint(MIN_VEL, MAX_VEL), 0)

        # InstructionGroup init (visuals)
        self.circle_color = Color(random.random(),\
            random.random(), random.random())
        self.circle_radius = random.randint(20, 30)
        self.circle = make_ellipse(self.pos, self.circle_radius)
        self.add(self.circle_color)
        self.add(self.circle)

        # Loop tracker
        self.numLoops = 0
        self.loopOnce = False

    # Whether this hummingloop should terminate itself after a single loop
    def set_loop_once(self, loopOnce):
        self.loopOnce = loopOnce

    def choose_notes(self):
        notes_init = [self.key + self.chord[x]\
            for x in range(len(self.chord))]

        notes_span = notes_init + [note + 12 for note in notes_init]

        # Generate notes
        notes = []
        subpat_dict = {}
        self.pattern = random.choice(PATTERNS)
        self.silence_prob_list = random.choice(SILENCE_PROB_LISTS)
        for subpat in self.pattern:
            if subpat[0] not in subpat_dict:
                # Generate new subpattern
                new_subpat = []
                for i in range(subpat[1]):
                    new_subpat += [random.choice(notes_span)]
                    # Silence
                    if random.choice(self.silence_prob_list) is 1:
                        new_subpat[-1] = -1
                subpat_dict[subpat[0]] = new_subpat
            # Add each subpattern's notes according to pattern
            notes += subpat_dict[subpat[0]]

        # Sustain processing of notes.
        # Add potential sustains instead of silences
        # (0 instead of -1)
        sustain_possible = False
        self.sustain_prob_list = random.choice(SUSTAIN_PROB_LISTS)
        for i in range(len(notes)):
            if notes[i] is not -1:
                # A note can be sustained.
                sustain_possible = True
            if notes[i] is -1 and sustain_possible:
                if random.choice(self.sustain_prob_list) is 1:
                    notes[i] = 0 if i < len(notes)-1 else -1
                else:
                    # A note-off event will happen,
                    # sustain is no longer possible
                    sustain_possible = False

        # Octave-jump-removal processing.
        # Removes jumps of larger than an octave in the middle
        # of a melody. Should help create more
        # melodic structures.
        last_note = -1
        for i in range(len(notes)):
            cur_note = notes[i]
            if last_note - cur_note > 12 and last_note is not -1:
                notes[i] += 12
            elif last_note - cur_note < -12 and last_note is not -1:
                notes[i] -= 12

        print "Notes: " + str(notes)
        return notes

    def start(self):
        self.playing = True
        self.synth.program(*self.cbp)
        now = self.song.cond.get_tick()
        next_tick = now - (now % self.note_grid) + self.note_grid
        self._post_at(next_tick)

    def stop(self):
        self.playing = False
        self.song.sched.remove(self.on_cmd)
        self.song.sched.remove(self.off_cmd)
        if self.off_cmd:
            self.off_cmd.execute()

    def _get_next_pitch(self):
        pitch = self.notes[self.cur_idx]
        notes_len = len(self.notes)

        # keep in bounds:
        if self.cur_idx >= notes_len - 1:
            # one cycle is already done. (do this here
            # because we'd rather cut into the next loop than clip this loop)
            self.numLoops += 1

        # Zeroes in the sequence mark sustains.
        duration = 1
        for i in range(self.cur_idx+1, notes_len):
            if self.notes[i] is 0:
                duration += 1
            else:
                break
        # advance index
        self.cur_idx += self.idx_inc
        # loop (may be prevented by loopOnce)
        self.cur_idx = self.cur_idx % notes_len

        return pitch, duration

    def _post_at(self, tick):
        self.on_cmd = self.song.sched.post_at_tick(tick, self._noteon, None)

    def _noteon(self, tick, ignore):
        pitch, note_duration = self._get_next_pitch()
        if self.loopOnce and self.numLoops > 0:
            # We're already done. Ignore this _noteon call.
            return

        if pitch not in [0, -1]:
            # play note on:
            self.synth.noteon(self.channel, pitch, int(self.note_velocity))

            # post note-off:
            duration = self.note_len_ratio * self.note_grid * note_duration
            off_tick = tick + duration
            self.off_cmd = self.song.sched.post_at_tick(off_tick, self._noteoff, pitch)

        # callback:
        if self.callback:
            self.callback(tick, pitch, self.note_velocity, note_duration)

        # post next note. quantize tick to line up with grid of current note length
        if (self.playing):
            tick -= tick % self.note_grid
            next_beat = tick + self.note_grid
            self._post_at(next_beat)

    def _noteoff(self, tick, pitch):
        self.synth.noteoff(self.channel, pitch)

    def get_focus(self):
        return self.widget.get_focus(self)

    def on_update(self, dt):
        # Find target_velocity based on focus values
        # and distance from the middle of the screen
        focus = self.get_focus()
        if focus is 1:
            self.target_velocity = 100
        else:
            self.target_velocity = (1-(abs(self.widget.width/2\
                - self.pos[0])\
                / float(self.widget.width/2))) * 100
            if focus is -1:
                self.target_velocity /= 10.
            self.target_velocity *= NOTE_VELOCITY_MULT

        # Linearly shift velocity to target_velocity.
        if self.note_velocity < self.target_velocity:
            self.note_velocity = min(self.target_velocity,\
                self.note_velocity + FOCUS_SPEED * dt)
        elif self.note_velocity > self.target_velocity:
            self.note_velocity = max(self.target_velocity,\
                self.note_velocity - FOCUS_SPEED * dt)

        # Update position.
        self.pos = (self.pos[0] + self.vel[0] * dt,\
            self.pos[1] + self.vel[1] * dt)
        move_ellipse(self.circle, self.circle_radius, self.pos)

        # a delayed needs removal
        if self.loopOnce and self.numLoops > 0:
            return True

        # Disappear if it moves off screen.
        w = self.widget.width
        h = self.widget.height
        return self.pos[0] < 0 or self.pos[0] > w\
            or self.pos[1] < 0 or self.pos[1] > h

    def __str__(self):
        seed = self.seed
        key = self.key
        chord = self.chord
        pattern = self.pattern
        silence_prob = self.silence_prob_list
        sustain_prob = self.sustain_prob_list
        txt = "[Hummingloop %s| key:%s, chord:%s, pattern:%s, silence_prob:%s, sustain_prob:%s]\n" % (seed, key, chord, pattern, silence_prob, sustain_prob)
        txt +="             |> NOTES: %s" % self.notes
        return txt

    def __eq__(self, other):
        return (isinstance(other, self.__class__)
            and self.seed == other.seed)

    def __ne__(self, other):
        return not self.__eq__(other)
