#####################################################################
#
# puffer.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan, mnjy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import random
import numpy as np
from synth import Synth

# todo: move into global file
kFramesPerSecond = 44100
kBeatsPerSecond = 8
kTicksPerBeat = 480
kFramesPerTick = kFramesPerSecond / (kBeatsPerSecond * kTicksPerBeat)

# Chord types
I = [0, 4, 7]
ii = [1, 4, 8]
iii = [2, 5, 9]
IV = [3, 7, 10]
V = [4, 8, 11]
vi = [5, 8, 12]
vii0 = [6, 9, 12]
VIII = [7, 11, 14]

# Progressions
PROG_1 = [I, V, I, V]
PROG_2 = [I, ii, V, I]
PROG_3 = [I, iii, vi, V]
PROG_4 = [I, V, vii0, VIII]
PROG_5 = [iii, vi, IV, I]
PROG_6 = [vi, IV, V, I]
PROG_7 = [vi, ii, V, I]
PROG_8 = [VIII, IV, V, I]

# Master progressions list
PROGRESSIONS = [PROG_1, PROG_2, PROG_3, PROG_4, PROG_5, PROG_6, PROG_7, PROG_8]

class Puffer(object):
    def __init__(self, idx):
        idx = (idx-1)%len(PROGRESSIONS)

        # Key and chords
        self.key = 60
        self.chords = PROGRESSIONS[idx]

        self.note_velocity = 60

        self.synth = Synth('../FluidR3_GM.sf2')
        self.channel = 0
        cbp = (self.channel, 0, 58) #tuba
        self.synth.program(*cbp)

        self.cur_idx = 0

    def get_frames(self):
        # Actual script
        # frames
        data_frames = []
         # start generating things
        currentTick = 0
        lastTick = 0
        stopGeneration = False
        pendingOffticks = []
        deltaTicks = 0
        envelope_final_frames = 1000 # leave some frames for the note_off envelope

        while stopGeneration is not True:
            if self.cur_idx < len(self.chords) and currentTick % (8*kTicksPerBeat) == 0:
                # do the next _note_on
                (offTick, chord) = self._chordon(currentTick)
                if offTick > 0:
                    pendingOffticks.append((offTick, chord))
            if len(pendingOffticks) > 0:
                if pendingOffticks[0][0] <= currentTick:
                    (offTick, chord) = pendingOffticks.pop(0)
                    self._chordoff(offTick, chord)
            new_frames, good = self.synth.generate(deltaTicks * kFramesPerTick)
            data_frames = np.append(data_frames, new_frames)

            # get the next beat!
            lastTick = currentTick
            nextBeat = (currentTick + kTicksPerBeat)
            if len(pendingOffticks) > 0:
                currentTick = min(nextBeat, pendingOffticks[0][0])
            else:
                currentTick = nextBeat
            deltaTicks = currentTick - lastTick


            # check if we're done
            if self.cur_idx >= len(self.chords) and len(pendingOffticks) < 1:
                # generate final frames for envelope
                new_frames, good = self.synth.generate(envelope_final_frames)
                data_frames = np.append(data_frames, new_frames)
                stopGeneration = True

        return data_frames

    def _get_next_chord(self):
        chord = self.chords[self.cur_idx]
        duration = 8
        self.cur_idx += 1
        return chord, duration

    # returns off tick
    def _chordon(self, tick):
        chord, note_duration = self._get_next_chord()
        for value in chord:
            pitch = self.key + value
            # play note on:
            self.synth.noteon(self.channel, pitch, int(self.note_velocity))
        # post note-off:
        offTick = tick + note_duration * kTicksPerBeat
        return (offTick, chord)

    def _chordoff(self, tick, chord):
        for value in chord:
            pitch = self.key + value
            self.synth.noteoff(self.channel, pitch)
        
