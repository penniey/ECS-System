# ECS-System
A complex Unity ECS system for navigation and attack behaviors.

## Performance
- Performance is over 144 fps at 1000 entities.
- The test was done with the entity using the ECS pathfinding system as well as attack system
- I made the TakeDamage for the player just return, but this shouldn't impact performance.

## Use case
- This is used when you need to spawn a lot of enemies or entities that are following the player.
- If you have 100 or less entities, it would be better to use NavMesh as you don't need this performance optimization.

## How to use
- This code is straight ripped from the code I have done from my game project.
- Due to it not being a standalone you will miss a lot of reference.
- Remove the unused ones.

### Enemy
- EnemyAttack-, EnemyMovement-,EnemyHealthAuthoring, EnemySetup, EnemyNavMeshBridge on the enemy prefab.
- Physics body, Physics Shape, Nav Mesh Agent (turned off) on the enemy prefab.

### In scene Managers
- EnemySpawnerBridge
- EnemyDamageTextDisplay

### Baking and Subscene
- Arena with colliders
- Subscene copy of the arena with physics shape and body (static)
- You can turn off mesh renderer for the subscene copy
- Turn the subscene off to bake it.

## Video showcase 
### 1000 to 2000 entities 
A complex ECS system for navigation and attack behaviors.
[![Watch the video](https://img.youtube.com/vi/sXphc-nu2yM/maxresdefault.jpg)](https://youtu.be/sXphc-nu2yM))
