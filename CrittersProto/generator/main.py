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

# # Follows code to play things out loud
# # =================================================

# audioPlayer = audio_player.AudioPlayer()

# #newCritter = boxworm.BoxWorm(1)
# # newCritter = hummingloop.HummingLoop(1)
# # newCritter = oscilloop.Oscilloop(10)
# #newCritter = swarp.Swarp(1)
# newCritter = maracaws.Maracaws(15)
# #newFrames = newCritter.get_frames()

# newFrames = newCritter.get_frames()
# audioPlayer.queueFramesForPlay(newFrames)

# # wait for stream to finish (5)
# while audioPlayer.isActive():
#    time.sleep(0.1)

# audioPlayer.close()



#Uncomment for code to write things into wav files or config files
#=============================================================

for currentSeed in xrange(1, 31):
    # UNCOMMENT FOR WAV FILE WRITING
    # set up wave file writing
    waver = wave.open("wav/maracaws_output" + str(currentSeed) + ".wav", 'wb')
    waver.setnchannels(kOutputChannels) #kOutputChannels
    waver.setsampwidth(2) # let's convert things into 16 bit integer format
    waver.setframerate(kSamplingRate)

    #newCritter = hummingloop.HummingLoop(currentSeed)
    #newCritter = boxworm.BoxWorm(currentSeed)
    newCritter = maracaws.Maracaws(currentSeed)
    #newCritter = mine.Mine(currentSeed)
    newFrames = newCritter.get_frames()    
    data = newFrames * np.iinfo(np.int16).max
    data = data.astype(np.int16)
    fmt = 'h'*len(data)

    packedData = struct.pack(fmt, *data)
    waver.writeframes(packedData)
    waver.close()

    ## UNCOMMENT FOR CONFIG FILE WRITING
    # #newCritter = boxworm.BoxWorm(currentSeed)
    # #newCritter = hummingloop.HummingLoop(currentSeed)
    # newCritter = maracaws.Maracaws(currentSeed)

    # # Output oscillation configuration file.
    # # This will be used by Unity to synchronize
    # # the critter's motion with the sound of the
    # # .wav file.
    # config_file = open("wav/maracaws_output" + str(currentSeed) + "_config.txt", "wb")
    # config_contents = str(newCritter.get_config()) + "\n"
    # config_file.write(config_contents)
    # config_file.close()
    # print "Config file written and closed."
    
