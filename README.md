<h4 align="center">
  <br>
  <a href="http://www.unordinal.com"><img src="u.png" alt="Markdownify" width="200"></a>
  <br>FPS-Template<br>
</h1>



<h4 align="center">A Multiplayer FPS Template by <a href="https://www.unordinal.com" target="_blank">Unordinal</a> &#38; <a href="https://github.com/LukeStampfli" target="_blank">LukeStampfli</a>.</h4>

<p align="center">
  <a href="#welcome">Welcome</a> •
  <a href="#tutorial-contents">Tutorial Contents</a> •
  <a href="#getting-started">Getting Started</a> •
  <a href="#setting-up-project">Setting Up Project</a> •
  <a href="#special-thanks">Special Thanks</a> •
</p>

## Welcome
Welcome to this tutorial, which helps you get started creating your own multiplayer game with Unity, Darkrift2, and the Unordinal Multiplayer Factory.

Upon completion, you should have a fully functional Multiplayer FPS shooter with an authoritative cloud server and clients ready to be shared with your friends to play with. This is what it should look like.

https://user-images.githubusercontent.com/34939959/166098072-ce05ad69-34de-4055-9870-1b2d381bfb07.mp4
## Contents

In this template game you will find a fully functional FPS game with an authoritative server, built-in cloud hosting capabilities on the Unordinal platform, automatic server discoverability for clients, one click deployment of your server to cloud, automatic client sharing with your friends.

Additionally to the features provided in the template, you will also find a tutorial and documentation where you can learn more about how to create an authoritative multiplayer first person shooter game with Unity and Darkrift 2. This tutorial covers:

* Multiplayer game project architecture.
* Darkrift 2 basics.
* Embedded Darkrift server.
* Client prediction, reconciliation, interpolation, authoritative movement.
* Lag compensation (shooting in an FPS game).
* Helpful tools for multiplayer game authoring with Unity.

## Getting Started

* Download and install Unity.
* Download the additional assets and clone this template from Github.
* Create an account on Unordinal.com and download the Unity plugin.
* Open this template game in Unity and select "Play with Friends" using the Unordinal plugin.
* Play your own first-person shooter game with your friends.

Here are a few more details to make sure everything works for you:
This template has been tested with the following Unity versions: 2020.3.30f1, 2020.3.33f1.
Before opening the project in Unity. Please download and unzip the external assets archive from [ExternalAssets](https://drive.google.com/file/d/1d-QGKSQvc69VgRJXO5rhPQF6SPK-w9xk/view) and paste it into your Unity Project/ Assets folder. 

The reason for this is to prevent Git clone operation from choking for users with low connection bandwidth.

### Importing Animations

Because of licence restrictions, you must download the animations from Mixamo yourself in order for this project to display them; we are unable to redistribute them as a third party. In the following steps, we'll show you how.
<details><summary><b>How to setup Animation</b></summary>

* Open https://www.mixamo.com/ and sign in or create a Mixamo account with Adobe if necessary.
  
* Navigate to the "Animations" tab and look for "Basic Shooter Pack.
  
* Choose the Basic Shooter Pack (it should have 16 animations, beware of the similarly named Shooter Pack and Slim Shooter Pack)
  
* When you press the Download button to the right, a "Basic Shooter Pack.zip" file should be downloaded.
  
* Unzip the file into the EmbeddedFPSClient/Assets/Character/Animations folder.
  
* Enter Unity and wait for the animations to finish importing.
  
* Select all new animations.
  
* In the Unity project view, select the FBX files and navigate to the Rig tab in the inspector Import settings. This is how it should look:
  Select Humanoid.
  
* Select Copy from Avatar.
  
* Select AvatarArmature as the Source.
  
* To save your changes, click Apply. If everything went well, the files should re-import without any warnings or errors in the console.
  
* Select the asset CharacterPrimary.
  
* Drag and drop the animation assets' mixamo.com sub-assets into the CharacterPrimary's corresponding slots.

That's it; the animations should now be visible in-game.

## Special Thanks
Special thanks to [LukeStampfli](https://github.com/LukeStampfli/EmbeddedFPSExample) for the orignial technical DR2 template project which handles the DR2 networking model.
Special thanks to [DevIos01](https://github.com/DevIos01/FPS-Starter-Assets) for sharing his Graphical Game Assets used in this template.
Special thanks to [KABBOUCHI](https://github.com/KABBOUCHI) for converting this tutorial into a website.