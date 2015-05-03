# oscilloop_draft2.py

import random as r

def generate_random_oscillation_configuration(base_freq):
    config = []
    for i in range(1, 9, 1):
        config.append((base_freq*i*r.choice([-1, 1]), r.random()))
    print config

print generate_random_oscillation_configuration(1/16.)