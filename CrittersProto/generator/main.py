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
newBW = boxworm.BoxWorm(1)
newFrames = newBW.get_frames()
# newHL = hummingloop.HummingLoop(1)
# newFrames = newHL.get_frames()
# newOS = oscilloop.Oscilloop(10)
# newFrames = newOS.get_frames()
# newMN = mine.Mine(currentSeed)
# newFrames = newMN.get_frames()

# Follows code to play things out loud
# =================================================

audioPlayer = audio_player.AudioPlayer()
audioPlayer.queueFramesForPlay(newFrames)

# wait for stream to finish (5)
while audioPlayer.isActive():
    time.sleep(0.1)

audioPlayer.close()



# Uncomment for code to write things into wav files
# =================================================

# for currentSeed in xrange(1, 31):
#     # set up wave file writing
#     waver = wave.open("wav/box_output" + str(currentSeed) + ".wav", 'wb')
#     waver.setnchannels(kOutputChannels) #kOutputChannels
#     waver.setsampwidth(2) # let's convert things into 16 bit integer format
#     waver.setframerate(kSamplingRate)

#     # newHL = hummingloop.HummingLoop(currentSeed)
#     # newFrames = newHL.get_frames()
#     newBW = boxworm.BoxWorm(currentSeed)
#     newFrames = newBW.get_frames()
#     data = newFrames * np.iinfo(np.int16).max
#     data = data.astype(np.int16)
#     fmt = 'h'*len(data)

#     packedData = struct.pack(fmt, *data)
#     waver.writeframes(packedData)
#     waver.close()
