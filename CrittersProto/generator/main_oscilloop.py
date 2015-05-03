# ==============================================
# Instantaneous wav-file generating audio system
# benchan
# ==============================================


# simple python script that loops through hummingloops
# and generates wave files for them

import hummingloop
import boxworm
import oscilloop
import wave
import numpy as np
import struct

# todo make global
kSamplingRate = 44100
kOutputChannels = 2




for currentSeed in xrange(1, 11):
    # set up wave file writing
    waver = wave.open("../wav/osci_output" + str(currentSeed) + ".wav", 'wb')
    waver.setnchannels(kOutputChannels) #kOutputChannels
    waver.setsampwidth(2) # let's convert things into 16 bit integer format
    waver.setframerate(kSamplingRate)

    # newHL = hummingloop.HummingLoop(currentSeed)
    # newFrames = newHL.get_frames()
    # newBW = boxworm.BoxWorm(currentSeed)
    # newFrames = newBW.get_frames()
    newOL = oscilloop.Oscilloop(currentSeed)
    newFrames = newOL.get_frames()
    data = newFrames * np.iinfo(np.int16).max
    data = data.astype(np.int16)
    fmt = 'h'*len(data)

    packedData = struct.pack(fmt, *data)
    waver.writeframes(packedData)
    waver.close()

    # Output oscillation configuration file.
    # This will be used by Unity to synchronize
    # the critter's motion with the sound of the
    # .wav file.
    config_file = open("../wav/osci_output" + str(currentSeed) + "_config.txt", "wb")
    config_contents = str(newOL.get_norm_const()) + "\n"
    for (freq, amp) in newOL.get_config():
        config_contents += "%s %s\n" % (freq, amp)
    config_file.write(config_contents)
    config_file.close()
    print "Config file written and closed."