# ==============================================
# Instantaneous wav-file generating audio system
# benchan
# ==============================================


# simple python script that loops through hummingloops
# and generates wave files for them

import hummingloop
import wave
import numpy as np
import struct

# todo make global
kSamplingRate = 44100
kOutputChannels = 2


# set up wave file writing
waver = wave.open("output.wav", 'wb')
waver.setnchannels(kOutputChannels) #kOutputChannels
waver.setsampwidth(2) # let's convert things into 16 bit integer format
waver.setframerate(kSamplingRate)

currentSeed = 1
newHL = hummingloop.HummingLoop(currentSeed)
newFrames = newHL.get_frames()
print len(newFrames)
data = newFrames * np.iinfo(np.int16).max
data = data.astype(np.int16)
fmt = 'h'*len(data)
packedData = struct.pack(fmt, *data)
waver.writeframes(packedData)

waver.close()