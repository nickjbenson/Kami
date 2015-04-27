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




for currentSeed in xrange(1, 31):
    # set up wave file writing
    waver = wave.open("wav/hum_output" + str(currentSeed) + ".wav", 'wb')
    waver.setnchannels(kOutputChannels) #kOutputChannels
    waver.setsampwidth(2) # let's convert things into 16 bit integer format
    waver.setframerate(kSamplingRate)

    newHL = hummingloop.HummingLoop(currentSeed)
    newFrames = newHL.get_frames()
    data = newFrames * np.iinfo(np.int16).max
    data = data.astype(np.int16)
    fmt = 'h'*len(data)

    packedData = struct.pack(fmt, *data)
    waver.writeframes(packedData)
    waver.close()