#####################################################################
#
# boxworm.py
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

import random as r

# Rhythm patterns
PATTERN_1 = ("ABABABAC", [("A", 8), ("B", 8), ("C", 8)])
PATTERNS = [PATTERN_1]

# 16th slot hit probabilities
HIT_PROB_1 = [0.6, 0.4, 0.5, 0.4, 0.6, 0.4, 0.5, 0.4]*2
HIT_PROB_2 = []
HIT_PROBS = [HIT_PROB_1]

# Possible hits
HITS = [48, 45, 42, 35]
ALT_HITS = {-1:-1, 48:50, 45:47, 42:-1, 35:36}

# Interaction configuration
FOCUS_SPEED = 100 # velocity ints / second
MAX_VEL = 50
MIN_VEL = 50
NOTE_VELOCITY_MULT = 0.5

# Synth track
class Boxworm(InstructionGroup, Track):
    def __init__(self, widget, seed, synth, callback=None):
        super(Boxworm, self).__init__()
        self.playing = False
        self.callback = callback
        self.widget = widget
        self.seed = seed
        r.seed(seed)

        # MIDI parameters
        self.synth = synth
        channel = 10
        bank = 128
        preset = 0
        self.cbp = (channel, bank, preset)
        #self.synth.cc(self.channel, 1, 40)
        self.synth.program(*self.cbp)

        # Performance parameters
        self.note_grid = kTicksPerQuarter / 4
        self.note_len_ratio = 1

        # Notes
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
            r.randint(100, 500))
        self.vel = ((-1 if self.pos > self.widget.width/2. else 1) * \
            r.randint(MIN_VEL, MAX_VEL), 0)

        # InstructionGroup init (visuals)
        self.visual_color = Color(r.random(),\
            r.random(), r.random())
        self.box_radius = r.randint(20, 50)
        self.visual_box = Rectangle(pos=(self.pos[0]-self.box_radius, self.pos[1]-self.box_radius), size=(self.box_radius*2,self.box_radius*2))
        self.add(self.visual_color)
        self.add(self.visual_box)

    def choose_notes(self):
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

    def start(self):
        self._start_looper()

    def stop(self):
        self._stop_looper()

    def _start_looper(self):
        self.playing = True
        now = self.song.cond.get_tick()
        next_tick = now - (now % self.note_grid) + self.note_grid
        self._post_at(next_tick)

    def _stop_looper(self):
        self.playing = False
        self.song.sched.remove(self.on_cmd)
        self.song.sched.remove(self.off_cmd)
        if self.off_cmd:
            self.off_cmd.execute()

    def _post_at(self, tick):
        self.on_cmd = self.song.sched.post_at_tick(tick, self._noteon, None)

    def _get_next_pitch(self):
        pitch = self.notes[self.cur_idx]

        notes_len = len(self.notes)

        # Zeroes in the sequence mark sustains.
        duration = 1
        for i in range(self.cur_idx+1, notes_len):
            if self.notes[i] is 0:
                duration += 1
            else:
                break

        # advance index
        self.cur_idx += self.idx_inc

        # keep in bounds:
        self.cur_idx = self.cur_idx % notes_len

        return pitch, duration

    def _noteon(self, tick, ignore):
        pitch, note_duration = self._get_next_pitch()

        if pitch not in [0, -1]:
            # play note on:
            self.synth.noteon(self.cbp[0], pitch, int(self.note_velocity))

            # post note-off:
            duration = self.note_len_ratio * self.note_grid * note_duration
            off_tick = tick + duration
            self.off_cmd = self.song.sched.post_at_tick(off_tick, self._noteoff, pitch)

        # callback:
        if self.callback:
            self.callback(tick, pitch, velocity, duration)

        # post next note. quantize tick to line up with grid of current note length
        if (self.playing):
            tick -= tick % self.note_grid
            next_beat = tick + self.note_grid
            self._post_at(next_beat)

    def _noteoff(self, tick, pitch):
        self.synth.noteoff(self.cbp[0], pitch)

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
        self.visual_box.pos = (self.pos[0]-self.box_radius, self.pos[1]-self.box_radius)

        # Disappear if it moves off screen.
        w = self.widget.width
        h = self.widget.height
        return self.pos[0] < 0 or self.pos[0] > w\
            or self.pos[1] < 0 or self.pos[1] > h

    def __str__(self):
        seed = self.seed
        txt = "[Boxworm %s| ]\n" % (seed)
        txt +="             |> NOTES: %s\n%s\n%s\n%s" % (self.notes[0:16], self.notes[16:32], self.notes[32:48], self.notes[48:64])
        return txt

    def __eq__(self, other):
        return (isinstance(other, self.__class__)
            and self.seed == other.seed)

    def __ne__(self, other):
        return not self.__eq__(other)
