#fluidsynth API:

class Synth:
   """Synth represents a FluidSynth synthesizer"""
   def __init__(self, gain=0.2, samplerate=44100):
      """Create new synthesizer object to control sound generation

      Optional keyword arguments:
        gain : scale factor for audio output, default is 0.2
             lower values are quieter, allow more simultaneous notes
        samplerate : output samplerate in Hz, default is 44100 Hz

      """
      pass

   def sfload(self, filename, update_midi_preset=0):
      """Load SoundFont and return its ID"""
      pass

   def program_change(self, chan, prg):
      """Change the program"""
      pass

   def bank_select(self, chan, bank):
      """Choose a bank"""
      pass
            
   def sfont_select(self, chan, sfid):
      """Choose a SoundFont"""
      pass

   def program_select(self, chan, sfid, bank, preset):
      """Select a program"""
      pass

   def noteon(self, chan, key, vel):
      """Play a note"""
      pass

   def noteoff(self, chan, key):
      """Stop a note"""
      pass

   def pitch_bend(self, chan, val):
      """Adjust pitch of a playing channel by small amounts

      A pitch bend value of 0 is no pitch change from default.
      A value of -2048 is 1 semitone down.
      A value of 2048 is 1 semitone up.
      Maximum values are -8192 to +8192 (transposing by 4 semitones).
      
      """
      pass

   def cc(self, chan, ctrl, val):
      """Send control change value

      The controls that are recognized are dependent on the
      SoundFont.  Values are always 0 to 127.  Typical controls
      include:
        1 : vibrato
        7 : volume
        10 : pan (left to right)
        11 : expression (soft to loud)
        64 : sustain
        91 : reverb
        93 : chorus
      """ 
      pass

            
    def get_reverb_params(self) :
      """Return the 4 reverb parameters: (roomsize, damping, width, level)"""
      pass

    def set_reverb_params(self, roomsize, damping, width, level) :
      """Set the 4 reverb parameters: (roomsize, damping, width, level)"""
      pass

    def set_reverb_on(self, on) :
      """Turns reverb on (True) or off (False)"""
      pass
      
   def program_reset(self):
      """Reset the programs on all channels"""
      pass
   
   def system_reset(self):
      """Stop all notes and reset all programs"""
      pass
   
   def get_samples(self, len=1024):
      pass
      