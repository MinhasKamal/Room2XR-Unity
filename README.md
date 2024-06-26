# CIDER-Unity

## Introduction
(Add a structure figure here)  
(Add introduction text from the system paper)  

## Dependencies
To customize your own experience for using the CIDER system on HoloLens 2, you need to install the following dependent software.  
### Install Unity Editor
To use Unity Editor on your own Windows computer, you need to follow this [instruction](https://learn.unity.com/tutorial/install-the-unity-hub-and-editor-4?uv=2021.3#) to install Unity Hub and Unity Editor version "2021.3.10f1", and please include modules: 
- Microsoft Visual Studio Community
- Android Build Support
- Universal Windows Platform Build Support
- WebGL Build Support
- Windows Build Support (IL2CPP))

Note: Other Unity Editor versions might result in unexpected errors. If you cannot find the specified version, try [here](https://unity.com/cn/releases/editor/archive)  
### Create Your Own Photon Unity Networking (PUN) Server
To hold CIDER's Leader side in your own Photon server for ease of management, follow this [instruction](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/tutorials/mr-learning-sharing-02#creating-the-pun-application) to create your own Photon account, create your own application, and copy the App ID to the CIDER-Unity framework. 
### CIDER-Python
To use the full functionality of our CIDER framework, including capturing large furniture in the Leader's physical environment into CAD objects, please follow the instructions [here](someLink) to set up the CIDER-Python framework. 

## Installation Instruction
### Project Installation
1. Download this CIDER-Unity package and extract it in your local computer
2. Open Unity Hub, then press "Add" to add the downloaded Unity project from your disk
3. Wait until the loading is complete
4. The installation is completed!  
### Build and Deploy Package from Unity Editor to HoloLens 2
1. Go to Unity project, navigate to "File" --> "Build Settings"
2. Follow the settings in this [instruction](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/build-and-deploy-to-hololens#build-the-unity-project), press "Build," and select a folder you want to store the built package
3. Once complete, open the ".sln" file with Microsoft Visual Studio, and follow this [instruction](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2#deploying-a-hololens-app-over-wi-fi-or-usb) to deploy your app over Wi-Fi or USB
4. Once complete, the app will automatically start in HoloLens 2; please press "yes" two times to allow the app to access your sensors
5. Now, the app is successfully deployed to your HoloLens 2!
### Deployable Package Installation (See [Deployable Package](#deployable-package) section for package downloading)
#### HoloLens 2 Leader/Follower Package Installation
1. Set up Windows Device Portal for HoloLens 2 in your Windows computer following this [instruction](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-the-windows-device-portal)
2. Connect your HoloLens 2 to your computer and connect to Windows Device Portal
3. Follower this [instruction](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-the-windows-device-portal#installing-an-app) to install the downloaded Leader/Follower HoloLens 2 package
#### Meta (Oculus) Quest Follower Package Installation 
(Pending...)

## User Instruction
### Leader's Physical Environment Scanning Instruction
1. Launch "PhotonSceneUnderstanding2021.3.10" app in your HoloLens 2 (Please be sure to follow [HoloLens 2 Leader Package Installation](#hololens-2-leaderfollower-package-installation))
    - (If you are a Follower launching the app, the CIDER system will automatically connect to the existing Leader)
3. Once connected to the CIDER-Python server, press "Detail-Medium" in "WORLD MESH DETAIL LEVEL" on the control panel (a button-pressed sound will be generated if you successfully press it)
4. Press "World mesh" on the control panel to initialize the first update (first column, fourth row)
5. A virtual environment will be generated in your HoloLens 2 view, aligning with your physical environment, and automatically sent to other connected Followers.
### Leader's Environment Update Options
1. Manually press the "Update scene" button on the control panel (third column, second row) on demand
2. Toggle the "Auto update" button on the control panel (first column, first row) to enable auto-update every 5 seconds

### Leader/Follower Switching in Unity Editor (HoloLens 2 Version Only)
1. Go to the "Hierarchy" tab, find "TestConnection" GameObject
2. In "Inspector," check/uncheck the "TCP Test Server" script to switch the CIDER to Leader/Follower

## Deployable Package
CIDER HoloLens 2 Leader Side [Package](https://utdallas.box.com/s/z39vssfdu8bqfe1j0t08njfvwlzqvljg)  
CIDER HoloLens 2 Follower Side [Package](https://utdallas.box.com/s/tf8xgft15eavd7n4llpnt5sijqjrb02s)  
CIDER Meta (Oculus) Quest 2/3 Follower Side [Package](https://utdallas.box.com/s/qtbn0hcn2ufrvy2mqs2pru4vol45ersz)

## Contact
The code of this repository was implemented by [Hung-Jui Guo](mailto:hxg190003@utdallas.edu) and [Hiranya Garbha Kumar](mailto:hiranya@utdallas.edu)  
Please contact them directly or contact [Minhas Kamal](mailto:minhas.kamal@utdallas.edu)

## Citation
(Will include once paper published)

## License
CIDER-Unity is LICENSE under GNU General Public License v3.0
