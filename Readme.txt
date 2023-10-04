


Layer mask (current only work in my scene - Lucas)
	
	Very handy when dealing with collision
	Collision rule is in Edit -> Project Setting -> Physics
	for an example, check Projectile.cs

	Here are the definition:

	Default - 
	Player -
	Enemy -
	PlayerGhost - a 2nd invisible collider on player, improve network experience
	NPC - treated same as player (maybe merge?)

	*projectile use OverlapSphere() for collision, it's cheaper and doesn't need a layer mask


Some note about:

	Unity version (2022.3.10f1)
	Package (must install)
	Github ignore list

	See link:
	https://docs.google.com/spreadsheets/d/1cCBNv72AiMzCmrdhcMFnjmT3eZIiR9R4Ty-s0x_PUt8/edit#gid=0

