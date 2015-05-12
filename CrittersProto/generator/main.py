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
#import oscilloop
import wave
import numpy as np
import struct
import audio_player # in progress
import swarp
import time
import uplift
import clang
import angel

# todo make global
kSamplingRate = 44100
kOutputChannels = 2


## Interesting midi instrumnts:
## - 1xx: Goblin
## - 86: sawtooth
## - 84: charang
## - 106: Shamisen high
## - 128 0 81: triangle
## - Really fast church bells

# Follows code to play things out loud
# =================================================

# audioPlayer = audio_player.AudioPlayer()

# def queueCritterForPlay(aCritter):
#     newFrames = aCritter.get_frames()
#     audioPlayer.queueFramesForPlay(newFrames)

# def queueCrittersForPlay(listOfCritters):
#     newFrames = []
#     for critter in listOfCritters:
#         nextFrames = critter.get_frames()
#         if len(nextFrames) > len(newFrames):
#             newFrames = np.pad(newFrames, (0, len(nextFrames) - len(newFrames)), mode="constant")
#         else:
#             nextFrames = np.pad(nextFrames, (0, len(newFrames) - len(nextFrames)), mode="constant")
#         newFrames = newFrames + nextFrames
#     audioPlayer.queueFramesForPlay(newFrames)

# # #newCritter = boxworm.BoxWorm(1)
# # # newCritter = hummingloop.HummingLoop(1)
# # # newCritter = oscilloop.Oscilloop(10)
# # newCritter = swarp.Swarp(1)
# # #newCritter = maracaws.Maracaws(15)
# # #newFrames = newCritter.get_frames()

# # newFrames = newCritter.get_frames()
# # newCritter = maracaws.Maracaws(15)

# queueCrittersForPlay([angel.Angel(3), angel.Angel(12), angel.Angel(24), uplift.Uplift(5)])
# #queueCrittersForPlay([swarp.Swarp(1), maracaws.Maracaws(15)])
# #queueCrittersForPlay([maracaws.Maracaws(15), hummingloop.HummingLoop(2)])

# # wait for stream to finish (5)
# while audioPlayer.isActive():
#    time.sleep(0.1)

# audioPlayer.close()



#Uncomment for code to write things into wav files or config files
#=============================================================

for currentSeed in xrange(1, 11):
    # UNCOMMENT FOR WAV FILE WRITING
    # set up wave file writing
    waver = wave.open("wav/angel_output" + str(currentSeed) + ".wav", 'wb')
    waver.setnchannels(kOutputChannels) #kOutputChannels
    waver.setsampwidth(2) # let's convert things into 16 bit integer format
    waver.setframerate(kSamplingRate)

    #newCritter = hummingloop.HummingLoop(currentSeed)
    #newCritter = boxworm.BoxWorm(currentSeed)
    #newCritter = maracaws.Maracaws(currentSeed)
    #newCritter = mine.Mine(currentSeed)
    newCritter = clang.Clang(currentSeed)
    newFrames = newCritter.get_frames()    
    data = newFrames * np.iinfo(np.int16).max
    data = data.astype(np.int16)
    fmt = 'h'*len(data)

    packedData = struct.pack(fmt, *data)
    waver.writeframes(packedData)
    waver.close()

    # UNCOMMENT FOR CONFIG FILE WRITING
    #newCritter = boxworm.BoxWorm(currentSeed)
    #newCritter = hummingloop.HummingLoop(currentSeed)
    #newCritter = maracaws.Maracaws(currentSeed)
    #newCritter = swarp.Swarp(1)

    # Output oscillation configuration file.
    # This will be used by Unity to synchronize
    # the critter's motion with the sound of the
    # .wav file.
    config_file = open("wav/angel_output" + str(currentSeed) + "_config.txt", "wb")
    config_contents = str(newCritter.get_config()) + "\n"
    config_file.write(config_contents)
    config_file.close()
    print "Config file written and closed."
    
