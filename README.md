<p align="center" width="100%">
    <img width="20%" src="https://github.com/kuentzel/ROVER/assets/12643357/49d4f75e-1496-4c4f-be84-a1e751915ef0">
</p>

# ROVER: Rating Overlays for Virtual Environments in Research

<p align="center" width="100%">
    <img width="100%" src="https://github.com/kuentzel/ROVER/assets/12643357/971e7830-ca48-46b7-856b-0e35a9faf7d9">
</p>

# New Feature!
You can use this webapp as a graphical user interface to configure your questionnaires for ROVER: https://jrmykch.github.io/fbk_webapp/
We will make the multi-platform Flutter project available open-source for native use after the project's conclusion.

# !!!Disclaimer!!!
We still aim to maintain this project. Due to various updates (SteamVR, OpenVR, Meta XR Core SDK/Oculus Integration) and how these software components interface, ROVER no longer works with some OpenXR applications that have updated their Oculus SDKs (e.g. BeatSaber, OhShape and many more Quest ports and PC originals). 
The issue lies with the OpenVR Initialization of the SteamVR Unity Plugin. ROVER works with applications targeting OpenVR (but not OpenXR) when used with Steam or SteamLink. However, when connecting to Quest Headsets through AirLink, ROVER works with SteamVR and OpenXR applications. We have raised an issue with the developers of the SteamVR Unity Plugin, but are working on transitioning away from depending on the SteamVR plugin for interactions and tracking by Fall '25, which will solve the issue on the ROVER side.
However, if you want to use ROVER I recommend switching your own applications to use OpenVR Loader / SteamVR Plugin in the Unity XR Management. For closed-source/commercial applications see if you can downgrade the version (works for BeatSaber by selecting a beta branch in Steam). For this issue it does not matter if you enable Meta Plugin Compatibility in SteamVR or try to force SteamVR through OVR Toolkit. 

# Description

ROVER is a Virtual Reality / VR questionnaire toolkit to enable researchers with a limited IT background to easily integrate immersive rating scales and questionnaires into their VR user experience studies.

ROVER uses the SteamVR Overlay Interface together with the OpenVR Compositor Interface to project 2D content over the 3D Virtual Environment.

ROVER has been evaluated as part of a user experience pilot study in collaboration with health psychologists.

The research paper introducing the tool is open access and can be found here: https://dl.acm.org/doi/10.1145/3660515.3661328.
Other research using ROVER is listed below.

Find a video of ROVER's features here: https://github.com/kuentzel/ROVER/ROVER_Overview/ROVER_FeatureVideo

## Download

ROVER is meant to be used as a standalone executable. It does not need to be and is not meant to be integrated into your own project.
ROVER can be downloaded here: https://github.com/kuentzel/ROVER/releases/tag/v2408r1

You can download more sample questionnaires (Usability Metric for User Experience, System Usability Scale, NASA Task Load Index, igroup Presence Questionnaire, etc.) here: https://github.com/kuentzel/ROVER/releases/tag/qp1

## Getting Started
1. Download ROVER: https://github.com/kuentzel/ROVER/releases/tag/v2406r2
2. Read README_ROVER.txt
3. Configure ROVER
4. Start ROVER
5. Load sample survey
6. Explore

You are welcome to contribute and we will review any pull-requests (commit to the development branch).
Feel free to open an issue at https://github.com/kuentzel/ROVER/issues> or contact us at kuentzel@hochschule-trier.de

## Planned Updates

- Graphical User Interface / Web App for easy Survey/Questionnaire Configuration (feel free to reach out if you require assistance) -> Q1 2025
- Improvement and expansion of the graphical desktop UI to cover the configuration option currently set via the text files -> Q4 2024
- Improved guide on setting up and using ROVER (feel free to reach out if you require assistance) -> Q4 2024
- Cleaned-up and improved item layouts, more item types (images, sliders, etc.) -> Q1 2025

## Dependencies and Attribution

The code is provided to enable individual adjustments and builds. ROVER has been developed using Unity 2021.3.x.

ROVER has several dependencies: SteamVR Unity Plugin, OpenVR, OpenVR Unity XR Plugin, Ready Player Me, ManagedRenderEvent. These dependencies have been released open source. Find their respective licenses in the License file. ROVER comes with all dependencies.

ROVER is provided under a MIT License.

Please cite ROVER and the research paper in your academic works.

```
@inproceedings{10.1145/3660515.3661328,
author = {K\"{u}ntzer, Lucas and Schwab, Sandra U. and Spaderna, Heike and Rock, Georg},
title = {ROVER: A Standalone Overlay Tool for Questionnaires in Virtual Reality},
year = {2024},
isbn = {9798400706516},
publisher = {Association for Computing Machinery},
address = {New York, NY, USA},
url = {https://doi.org/10.1145/3660515.3661328},
doi = {10.1145/3660515.3661328},
booktitle = {Companion Proceedings of the 16th ACM SIGCHI Symposium on Engineering Interactive Computing Systems},
pages = {31–39},
numpages = {9},
keywords = {accessibility, immersive virtual reality, inVR-questionnaires, questionnaires, tool, usability, user experience, user study, virtual reality, vr},
location = {Cagliari, Italy},
series = {EICS '24 Companion}
}
```

## Works using ROVER

Lucas Küntzer, Moritz Scherer, Tilo Mentler, and Georg Rock. 2024. Dynamic Difficulty Adjustment in Virtual Reality Exergaming to Regulate Exertion Levels via Heart Rate Monitoring. In Proceedings of the 30th ACM Symposium on Virtual Reality Software and Technology (VRST '24). Association for Computing Machinery, New York, NY, USA, Article 82, 1–2. https://doi.org/10.1145/3641825.3689504

L. Küntzer, S. Schwab, H. Spaderna and G. Rock, "Measuring User Experience of Older Adults during Virtual Reality Exergaming," 2024 16th International Conference on Quality of Multimedia Experience (QoMEX), Karlshamn, Sweden, 2024, pp. 153-159, doi: 10.1109/QoMEX61742.2024.10598263.


<p align="center" width="100%">
    <img width="33%" src="https://github.com/kuentzel/ROVER/assets/12643357/e6d27166-a3b6-4aa4-97a4-a39ec3ad2f8d">
</p>

