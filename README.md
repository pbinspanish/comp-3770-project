# COMP-3770 Project
> Curtis Chang | Nathan Galasso | Josiah Henson | Bilal Mohamad Ali Ahmad | Yuan Zhang
## Installation
### Prerequisites
1. Install [Unity](https://unity.com/) editor version **2022.3.10f1 LTS**.
2. Install [git](https://git-scm.com/downloads) or [GitHub Desktop](https://desktop.github.com/).

### Instructions
#### git
1. Run `git clone https://github.com/pbinspanish/comp-3770-project` in the folder you wish the project to be stored in.
2. In Unity Hub, under "Projects", select "Add project from disk" and select the folder that was created in the previous step.
#### GitHub Desktop
1. After [installing and authenticating](https://docs.github.com/en/desktop/overview/getting-started-with-github-desktop), select File > Clone repository...
2. Follow the instructions in the dialog to clone this repository.
3. Once you have cloned the repository, in the Unity Hub, under "Projects", select "Add project from disk" and select the folder that was created in the previous step.
#### Troubleshooting
##### I changed one file, but git/GitHub Desktop is showing a bunch of changes
This likely means there is an issue with the `.gitignore` file. Ensure that in the root folder of the Unity project (i.e. where the Assets folder is) there is also a `.gitignore` file. See [this link](https://docs.google.com/spreadsheets/d/1cCBNv72AiMzCmrdhcMFnjmT3eZIiR9R4Ty-s0x_PUt8/edit#gid=0) for more information.
##### The UI/Netcode isn't working in Unity
Ensure the following packages are installed in the project. You can verify this by going to Window > Package Manager in Unity.
- Unity UI
- Netcode for GameObjects

## Development
```
Layer mask (current only work in my scene - Lucas)
	
	Very handy when dealing with collision
	Collision rule is in Edit -> Project Setting -> Physics
	for an example, check Projectile.cs

	Here are the definition:

	Default - 
	Player -
	Enemy -
	PlayerGhost - a 2nd invisible collider on player, improve network experience

	*Projectiles use OverlapCapsule() for collision, it's cheaper
	*Note for OverlapSphere(), if projectile is moving VERY FAST and your collision radius is SMALL, it will miss maybe ~1 out of 10 times.
```
