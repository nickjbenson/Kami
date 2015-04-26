#####################################################################
#
# kinect.py
#
# Copyright (c) 2015, Eran Egozy
#
# Released under the MIT License (http://opensource.org/licenses/MIT)
#
#####################################################################


from OSC import OSCServer, ThreadingOSCServer, OSCClient, OSCMessage
import time
import threading
import numpy as np
import math
import core
import socket

kJointRightHand = "/righthand"
kJointLeftHand = "/lefthand"
kJointRightElbow = "/rightelbow"
kJointLeftElbow = "/leftelbow"
kJointRightFoot = "/rightfoot"
kJointLeftFoot = "/leftfoot"
kJointRightKnee = "/rightknee"
kJointLeftKnee = "/leftknee"
kJointHead = "/head"
kJointTorso = "/torso"
kJointLeftShoulder = "/leftshoulder"
kJointRightShoulder = "/rightshoulder"
kJointLeftHip = "/lefthip"
kJointRightHip = "/righthip"
kJointClosestHand = "/closesthand"


# This class assumes that Synapse is running.
# It handles communications with Synapse and presents joint data to
# the app.
class Kinect(threading.Thread):
   def __init__(self, remote_ip = None):
      super(Kinect, self).__init__()

      core.register_terminate_func(self.close)

      # Synapse is running on a remote machine:
      if remote_ip:
         listen_ip = socket.gethostbyname(socket.gethostname())
         listen_port = 12345

         send_ip = remote_ip
         send_port = 12321

      # Synapse is running locally on this machine, using localhost
      else:
         listen_ip = 'localhost'
         listen_port = 12345

         send_ip = 'localhost'
         send_port = 12346

      self.server = OSCServer( (listen_ip, listen_port) )
      self.server.addMsgHandler( '/tracking_skeleton', self.callback_tracking_skeleton )
      self.server.addMsgHandler( 'default', self.callback_ignore )

      # create the client, which sends control messages to Synapse
      self.client = OSCClient()
      self.client.connect( (send_ip, send_port) )

      # member vars
      self.active_joints = {}
      self.last_heartbeat_time = 0
      self.done_running = False

      # start the server listening for messages
      self.start()

   # close must be called before app termination or the app might hang
   def close(self):
      # this is a workaround of a bug in the OSC server
      # we have to stop the thread first, make sure it is done,
      # and only then class server.close()
      self.server.running = False
      while not self.done_running:
         time.sleep(.01)
      self.server.close()

   def run(self):
      print "Worker thread entry point"
      self.server.serve_forever()
      self.done_running = True

   def add_joint(self, joint):
      self.server.addMsgHandler(joint + '_pos_body', self.callback)
      self.active_joints[joint] = np.array([0.0, 0.0, 0.0])

   def remove_joint(self, joint):
      self.server.delMsgHandler(joint + '_pos_body')
      del self.active_joints[joint]

   def on_update(self):
      now = time.time()
      send_heartbeat = now - self.last_heartbeat_time > 3.0
      if send_heartbeat:
         self.last_heartbeat_time = now

      try:
         for j in self.active_joints:
            if send_heartbeat:
               #print 'heartbeat:', j
               self.client.send( OSCMessage(j + "_trackjointpos", 1) )
      except Exception as x:
         print x, 'sending to', self.client.client_address

   def get_joint(self, joint) :
      return np.copy(self.active_joints[joint])

   def callback_ignore(self, path, tags, args, source):
      pass

   def callback(self, path, tags, args, source):
      #print path, args
      joint_name = path.replace("_pos_body", "")
      self.active_joints[joint_name] = np.array(args)

   def callback_tracking_skeleton(self, path, tags, args, source):
      pass
      
