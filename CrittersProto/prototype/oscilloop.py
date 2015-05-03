# oscilloop.py

# Basically just copied from hummingloop.py

from song import Track
from clock import kTicksPerQuarter

from kivy.graphics.instructions import InstructionGroup
from kivy.graphics import Color, Ellipse, Line, Rectangle
from kivy.graphics import PushMatrix, PopMatrix, Translate, Scale, Rotate

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
class Oscilloop(InstructionGroup, Track):
    def __init__(self, widget, seed, synth, callback=None):
        super(Oscilloop, self).__init__()
        self.playing = False
        self.callback = callback
        self.widget = widget
        self.seed = seed
        random.seed(seed)

        # MIDI parameters
        self.synth = synth
        self.channel = 2
        self.cbp = (self.channel, 0, 89)
        self.synth.cc(self.channel, 91, 124)
        self.synth.cc(self.channel, 93, 100)

        # Performance parameters
        self.note_grid = kTicksPerQuarter / 4
        self.note_len_ratio = 1

        # Key and chord
        self.key = C
        self.chord = random.choice(CHORDS)
        self.notes = self.choose_notes(self.key, self.chord)

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

    def choose_notes(self, key, chord):

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
        # self.pos = (self.pos[0] + self.vel[0] * dt,\
        #     self.pos[1] + self.vel[1] * dt)
        # move_ellipse(self.circle, self.circle_radius, self.pos)

        # a delayed needs removal
        if self.loopOnce and self.numLoops > 0:
            return True

        # Disappear if it moves off screen.
        w = self.widget.width
        h = self.widget.height
        return self.pos[0] < 0 or self.pos[0] > w\
            or self.pos[1] < 0 or self.pos[1] > h

    def __str__(self):
        string = ""
        i = 0
        for note in self.notes:
            string += (str(note) + ", ")
            i += 1
            if i % 32 == 0:
                string += "\n"
        txt = "[Oscilloop > NOTES: %s]" % string
        return txt

    def __eq__(self, other):
        return (isinstance(other, self.__class__)
            and self.seed == other.seed)

    def __ne__(self, other):
        return not self.__eq__(other)
