# Mini 3D Explorer — Pro (C# + OpenTK)

This repository is a compact 3D demo built with **C# (.NET 8)** and **OpenTK 4.x**. It demonstrates:
- Multiple meshes & VAO/VBO/EBO
- **Phong lighting** (point light + camera **spotlight**)
- **Texturing** (checkerboard PNG)
- **FPS camera** with mouse look + WASD + jump
- **Interactions:** toggle lights (**F/T**), collect a gem, **unlock & open a door** with **E**
- Simple **physics** (gravity + ground) and basic collision with a door


Steps:
```bash
cd Game
dotnet restore
dotnet run
# or use F5 in VS Code (launch.json included)
```

## Controls
- **W/A/S/D** move, **Mouse** look, **Shift** sprint, **Space** jump
- **F** toggle **flashlight** (camera spotlight)
- **T** toggle **torch** (point light near the rotating cube)
- **E** interact (open/close the door once the gem is collected)
- **Esc** toggle mouse capture

## Feature Checklist → Rubric
- Window + loop 
- 3+ objects (floor, cube, pyramid, door) 
- Texturing (repeating checker) 
- **Phong lighting with ambient/diffuse/specular** (point + spotlight) 
- Camera (mouse + keyboard) 
- Interactions: light toggles + **door unlock** after gem collect 
- Code structure (`GL/Shader.cs`, `GL/Texture.cs`, `GL/Mesh.cs`, `GL/Camera.cs`) + comments 
- **Bonus:** multiple lights, basic physics 

## Technical Notes
- Shaders are **GLSL 330**.
- Light uniforms:
  - `uPoint`: position/ambient/diffuse/specular (torch near cube)
  - `uSpot`: camera-mounted flashlight with inner/outer cutoff and attenuation
- Door animation: rotates around a hinge using a model matrix chain.
- Physics is intentionally simple (arcade-style). Collisions are limited to a door AABB check.

## Credits
- Texture: programmatically generated checkerboard (no third-party assets).
- References: OpenTK docs, OpenGL/GLSL specs, LearnOpenGL (concepts translated to C#).

---

### For the Instructor
This project intentionally keeps the code **small but idiomatic** to showcase first-half course outcomes:
- Separates **GL abstractions** (Shader/Mesh/Texture/Camera)
- Demonstrates **Phong lighting** with two light types and toggles
- Provides an **interaction loop** (collect → unlock → open door)
- Includes **VS Code** configs and a **.gitignore**
