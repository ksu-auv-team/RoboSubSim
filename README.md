# RoboSubSim
This is a WIP project aimed at simulating underwater environments for autonomous underwater vehicles (particularly Seawolf 8) targeted at the RoboSub Competitions. There are a lot of features yet to be implemented or even decided, the plan is to get it to work before RoboSub 2024.

### Targets
* Photorealism (HDRP Water, Domain Randomizations, Successful Sim2Real Transfer)
* Robot Simulation (Fake robot that emulate real robot, ie. is controlled by the same code and has the same sort of interaction with the environment)
* Conveniences (Auto labeled bounding boxes, segmentation/depth imaging, data generation, maybe even RL)

### Usage
* Download Unity Hub and get a free license
* Add this project to Unity Hub
* When launching it will ask you to get editor version `2023.1.8f1`, you need to install this version (higher might work)
* It should be good to go now. 
* Tested with Windows 11 and Ubuntu22.04. The editor mode works in Ubuntu, but the build version will crash

### Note
If you need a working data generation that is capable of generating synthetic data that works in the real world, please see the past repo that uses URP at [https://github.com/XingjianL/Robosub_Unity_Sim](https://github.com/XingjianL/Robosub_Unity_Sim)
