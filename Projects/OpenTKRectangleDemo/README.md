# OpenTK Rectangle Demo

## Library Used
- **OpenTK 4.9.4**: Used for creating the window, rendering the rectangle, and handling graphics operations.
- **Custom Math Library (MathLib)**: Implemented vector (`Vector3D`) and matrix (`Matrix4x4D`) operations.

## Implemented Operations

### Vector Operations
- **Addition**: `v1 + v2`
- **Subtraction**: `v1 - v2`
- **Dot Product**: `Vector3D.Dot(v1, v2)`
- **Cross Product**: `Vector3D.Cross(v1, v2)`

### Matrix Operations
- **Identity Matrix**
- **Scaling Matrix**: `Matrix4x4D.CreateScale(sx, sy, sz)`
- **Rotation Matrix**: `Matrix4x4D.CreateRotationZ(angle)` (rotation around Z-axis)
- **Matrix Multiplication**: `Matrix4x4D * Matrix4x4D`
- **Vector Transformation**: Apply a matrix to a vector (`Matrix * Vector3D`)

## Example Output

=== Vector Operations ===
v1 = (1, 2, 3)
v2 = (4, 5, 6)
v1 + v2 = (5, 7, 9)
v1 - v2 = (-3, -3, -3)
Dot(v1, v2) = 32
Cross(v1, v2) = (-3, 6, -3)

=== Matrix Operations ===
Original Vector: (1, 2, 3)
After Scaling + Rotation: (-1.4142135623730951, 4.242640687119286, 6)
