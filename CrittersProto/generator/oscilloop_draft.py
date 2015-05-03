# oscilloop_draft.py

from pylab import *
import random as r

# Data generation / normalization functions.

def generate_random_oscillation_configuration(base_freq):
    config = []
    for i in range(1, 16, 1):
        freq = base_freq * i * r.choice([-1, 1])
        amp = r.random() * (1./i)
        config.append((freq, amp))
    return config

def osciline(x, config=[(1, 1)]):
    y = 0
    for (freq, amp) in config:
        y += amp * sin(freq*pi*x)
    print "f(%g) = %g" % (x, y)
    return y

def normalize(values):
    max_val = 0
    for value in values:
        if value > max_val:
            max_val = value
    return [x/float(max_val) for x in values]

# Generate the data.

base_freq = 1/16.
config = generate_random_oscillation_configuration(base_freq)

# Plot the data.

xs = arange(0, 32, 0.01)
ys = normalize([osciline(x, config=config) for x in xs])

ylabel("Signal out")
xlabel("Index")
title("Plot")
grid(True)
plot(xs, ys)

show()