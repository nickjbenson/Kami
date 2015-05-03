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

import random
import numpy as np
from synth import Synth

# todo: move into global file
kFramesPerSecond = 44100
kBeatsPerSecond = 8
kTicksPerBeat = 480
kFramesPerTick = kFramesPerSecond / (kBeatsPerSecond * kTicksPerBeat)

# To subclass, make sure to call super.constructor
# before your own settings.
class BaseGenerator(object):
    def __init__(self, seed):
        random.seed(seed)

        self.synth = Synth('../FluidR3_GM.sf2')
        self.set_cpb(10, 128, 0)
        self.cur_idx = 0

    # Subclasses should override this
    def get_notes_list(self):
        return []

    # Subclasses should override this
    def get_note_velocity(self):
        return 60

    def set_cpb(self, channel, bank, preset):
        self.channel = channel
        cbp = (self.channel, bank, preset)
        self.synth.program(*cbp)

    def set_cc(self, channel, control, value):
        self.synth.cc(channel, control, value)

    # Internal method
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

        currentNotes = self.get_notes_list()

        while stopGeneration is not True:
            if self.cur_idx < len(currentNotes) and currentTick % kTicksPerBeat == 0:
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
            if self.cur_idx >= len(currentNotes) and len(pendingOffticks) < 1:
                # generate final frames for envelope
                new_frames, good = self.synth.generate(envelope_final_frames)
                data_frames = np.append(data_frames, new_frames)
                stopGeneration = True

        return data_frames

    def _get_next_pitch(self):
        currentNotes = self.get_notes_list()
        pitch = currentNotes[self.cur_idx]

        notes_len = len(currentNotes)

        # Zeroes in the sequence mark sustains.
        duration = 1
        for i in range(self.cur_idx+1, notes_len):
            if currentNotes[i] is 0:
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
            noteVelocity = self.get_note_velocity()
            self.synth.noteon(self.channel, pitch, int(noteVelocity))
            # post note-off:
            off_tick = tick + note_duration * kTicksPerBeat
            return (off_tick, pitch)
        return (-1, pitch)

    def _noteoff(self, tick, pitch):
        self.synth.noteoff(self.channel, pitch)







