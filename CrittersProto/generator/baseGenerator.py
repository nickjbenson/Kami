#####################################################################
#
# BaseGenerator.py
#
# benchan
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################

import random
import numpy as np
from synth import Synth

# todo: move into global file
kFramesPerSecond = 44100
# 120 beats per minute = 2 beats per second
kBeatsPerSecond = 2
kTicksPerBeat = 480
kFramesPerTick = int(kFramesPerSecond / (kBeatsPerSecond * kTicksPerBeat))
# can be fractional
kDefaultNumNotesPerBeat = 4

# To subclass, make sure to call super.constructor
# before your own settings.
# Extend this class as necessary here, e.g. to allow for
# variable Beats per second, or just tell me (benchan)
# to do that.
class BaseGenerator(object):

    def __init__(self, seed, synthName='../FluidR3_GM.sf2'):
        random.seed(seed)

        self.synth = Synth(synthName)
        self.set_cpb(10, 0, 0)
        self.cur_idx = 0
        self.set_num_notes_per_beat(kDefaultNumNotesPerBeat)
        #self.synth.set_reverb_on(True)

    # Subclasses should override this
    # can return an array of scalars, or an
    # array of chords, or a mixture
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

    def set_num_notes_per_beat(self, notesPerBeat):
        self.notesPerBeat = notesPerBeat
        self.ticksPerNote = int(kTicksPerBeat / notesPerBeat)

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
            # First, generate ticks up to this point
            new_frames, good = self.synth.generate(deltaTicks * kFramesPerTick)

            # check if we're done, before committing new frames
            if self.cur_idx >= len(currentNotes) and len(pendingOffticks) < 1:
                if np.absolute(new_frames).max() <= 0.001:
                    stopGeneration = True
                    continue

            data_frames = np.append(data_frames, new_frames)

            # Apply config changes for this new tick
            if self.cur_idx < len(currentNotes) and currentTick % self.ticksPerNote == 0:
                # do the next _note_on
                (offTick, pitch) = self._noteon(currentTick)
                if offTick > 0:
                    pendingOffticks.append((offTick, pitch))
            if len(pendingOffticks) > 0:
                if pendingOffticks[0][0] <= currentTick:
                    (offTick, pitch) = pendingOffticks.pop(0)
                    self._noteoff(offTick, pitch)
            

            # get the next tick
            lastTick = currentTick
            tickOfNextNote = int((currentTick + self.ticksPerNote) / self.ticksPerNote) * self.ticksPerNote
            nextTick = tickOfNextNote
            if len(pendingOffticks) > 0:
                currentTick = min(nextTick, pendingOffticks[0][0])
            else:
                currentTick = nextTick
            deltaTicks = currentTick - lastTick

        return data_frames

    # noteset is either a single midi pitch or
    # a list of midi pitches, based on the implementation
    # of get_notes_list
    def _get_next_noteset(self):
        currentNotes = self.get_notes_list()
        noteset = currentNotes[self.cur_idx]

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

        return noteset, duration

    # returns off tick
    def _noteon(self, tick):
        noteset, note_duration = self._get_next_noteset()
        if isinstance(noteset, list):
            for value in noteset:
                pitch = value
                # play note on:
                noteVelocity = self.get_note_velocity()
                self.synth.noteon(self.channel, pitch, int(noteVelocity))
            # post note-off:
            off_tick = tick + note_duration * self.ticksPerNote
            return (off_tick, noteset)
        else:
            pitch = noteset
            if pitch not in [0, -1]:
                # play note on:
                noteVelocity = self.get_note_velocity()
                self.synth.noteon(self.channel, pitch, int(noteVelocity))
                # post note-off:
                off_tick = tick + note_duration * self.ticksPerNote
                return (off_tick, pitch)
            return (-1, pitch)

    def _noteoff(self, tick, noteset):
        if isinstance(noteset, list):
            for value in noteset:
                pitch = value
                self.synth.noteoff(self.channel, pitch)
        else:
            self.synth.noteoff(self.channel, noteset)


    # Used by Main
    def get_config(self):
        notesToConfig = []
        # generate sixteenth notes. Our notes list may be in quarter notes, etc.
        # depending on our notesPerBeat
        extraNoteRatio = float(kDefaultNumNotesPerBeat) / float(self.notesPerBeat)
        if extraNoteRatio < 1:
            print "We don't support so few notes ;("
        notesList = self.get_notes_list()
        for i in xrange(0, int(len(notesList) * extraNoteRatio)):
            relevantIndex = int(i / extraNoteRatio)
            note = notesList[relevantIndex]
            if extraNoteRatio > 1:
                if i % extraNoteRatio != 0:
                    if note != -1:
                        notesToConfig.append("0")
                        continue
            if isinstance(note, list):
                notesToConfig.append(str(sum(note)))
            else:
                notesToConfig.append(str(note))

        return " ".join(notesToConfig)







