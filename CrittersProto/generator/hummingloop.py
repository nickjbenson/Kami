#####################################################################
#
# hummingloop.py
#
# Copyright (c) 2015, Nick Benson
# Modifications by benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

from hummingloop_algorithm import *
import random
import numpy as np
from synth import Synth

# todo: move into global file
kFramesPerSecond = 44100
kBeatsPerSecond = 8
kTicksPerBeat = 480
kFramesPerTick = kFramesPerSecond / (kBeatsPerSecond * kTicksPerBeat)


class HummingLoop(object):
    def __init__(self, seed):
        random.seed(seed)

        # Key and chord
        key = C
        chord = random.choice(CHORDS)
        self.notes = choose_notes(key, chord)

        self.note_velocity = 60 * 2 * NOTE_VELOCITY_MULT

        self.synth = Synth('../FluidR3_GM.sf2')
        self.channel = 0
        cbp = (self.channel, 0, 69)
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
            if self.cur_idx < len(self.notes) and currentTick % kTicksPerBeat == 0:
                # do the next _note_on
                (offTick, pitch) = self._noteon(currentTick)
                if offTick > 0:
                    pendingOffticks.append((offTick, pitch))
            if len(pendingOffticks) > 0:
                if pendingOffticks[0][0] <= currentTick:
                    (offTick, pitch) = pendingOffticks.pop(0)
                    self._noteoff(offTick, pitch)
            new_frames, good = self.synth.generate(deltaTicks * kFramesPerTick)
            data_frames = np.append(data_frames, new_frames)

            # get the next beat!
            lastTick = currentTick
            nextBeat = ((currentTick + kTicksPerBeat) / kTicksPerBeat) * kTicksPerBeat
            if len(pendingOffticks) > 0:
                currentTick = min(nextBeat, pendingOffticks[0][0])
            else:
                currentTick = nextBeat
            deltaTicks = currentTick - lastTick


            # check if we're done
            if self.cur_idx >= len(self.notes) and len(pendingOffticks) < 1:
                # generate final frames for envelope
                new_frames, good = self.synth.generate(envelope_final_frames)
                data_frames = np.append(data_frames, new_frames)
                stopGeneration = True

        return data_frames

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
        self.cur_idx += 1
        return pitch, duration

    # returns off tick
    def _noteon(self, tick):
        pitch, note_duration = self._get_next_pitch()
        if pitch not in [0, -1]:
            # play note on:
            self.synth.noteon(self.channel, pitch, int(self.note_velocity))
            # post note-off:
            off_tick = tick + note_duration * kTicksPerBeat
            return (off_tick, pitch)
        return (-1, pitch)

    def _noteoff(self, tick, pitch):
        self.synth.noteoff(self.channel, pitch)







