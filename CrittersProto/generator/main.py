# ==============================================
# Instantaneous wav-file generating audio system
# benchan
# ==============================================


# simple python script that loops through hummingloops
# and generates wave files for them

import sys
sys.path.append('../prototype/common')
import hummingloop
import boxworm
import mine
import maracaws
import oscilloop
import wave
import numpy as np
import struct
import audio_player # in progress
import time

# todo make global
kSamplingRate = 44100
kOutputChannels = 2


# get the actual frames

# newCritter = boxworm.BoxWorm(1)
# newCritter = hummingloop.HummingLoop(1)
# newCritter = oscilloop.Oscilloop(10)
# newCritter = mine.Mine(15)
newCritter = maracaws.Maracaws(15)
newFrames = newCritter.get_frames()

# # Follows code to play things out loud
# # =================================================

# audioPlayer = audio_player.AudioPlayer()
# audioPlayer.queueFramesForPlay(newFrames)

# # wait for stream to finish (5)
# while audioPlayer.isActive():
#     time.sleep(0.1)

# audioPlayer.close()


# Uncomment for code to write things into wav files
# =================================================
#
##for currentSeed in xrange(1, 31):
##    # set up wave file writing
##    waver = wave.open("maracaws_output" + str(currentSeed) + ".wav", 'wb')
##    waver.setnchannels(kOutputChannels) #kOutputChannels
##    waver.setsampwidth(2) # let's convert things into 16 bit integer format
##    waver.setframerate(kSamplingRate)
##
##    newCritter = maracaws.Maracaws(currentSeed)
##    newFrames = newCritter.get_frames()
##    data = newFrames * np.iinfo(np.int16).max
##    data = data.astype(np.int16)
##    fmt = 'h'*len(data)
##
##    packedData = struct.pack(fmt, *data)
##    waver.writeframes(packedData)
##    waver.close()
