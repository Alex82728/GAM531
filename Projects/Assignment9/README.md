# 3D Object Collision Detection in OpenTK (C# + OpenGL 3.3)

This project is my implementation of Assignment 9 for 3D collision detection using OpenTK and OpenGL 3.3. The scene is a small enclosed area made from walls, a solid door, a couple of box/pillar structures, and two simple NPC placeholders. Each object has a `Transform` (position, scale, yaw) and an `AABBCollider` to represent its collision volume. The player is represented by a collider as well and is controlled with a first-person style controller (WASD for movement on the XZ plane, left/right arrows for yaw rotation). A simple perspective projection and basic directional lighting are used to render the objects as colored cubes.

For collision detection, I use **axis-aligned bounding boxes (AABB)** for both the player and all static objects. Every frame, the `PlayerController` builds an intended displacement based on input and passes it into the `CollisionSystem`. The system then resolves movement axis-by-axis (X and Z separately) to avoid jittering at corners. For each axis, it constructs a temporary AABB at the proposed new position and checks for overlaps against all non-trigger colliders in the scene. If a collision is detected, that axis movement is canceled, effectively blocking the player from going through walls, the door, or solid boxes / pillars. This cleanly separates collision logic (`CollisionSystem`) from rendering and input.

On top of blocking colliders, there are also **trigger colliders** for the door area and each NPC. These are marked as `IsTrigger = true` and do not block movement. Instead, when the player's collider overlaps a trigger, it calls an `OnTriggerEnter` callback that logs messages like “You are near the door” or “You are near NPC 1.” This demonstrates simple “touch” detection which could be extended to real interactions when the player presses `E`. One of the main challenges was making movement feel smooth and preventing clipping; resolving collisions axis-by-axis and always testing the *predicted* position before applying it solved most of these issues. Another challenge was keeping the structure organized, so I split responsibilities between `GameObject`, `Transform`, `AABBCollider`, `PlayerController`, and `CollisionSystem`, which made the project easier to understand and extend.
Controls:

W / A / S / D – move

Left / Right Arrow – rotate view (yaw)

E – “interact” (prints to console if you’re in a trigger)

Esc – quit