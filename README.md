# Procedural Path Generation in Unity by Ángel Formoso Caamaño.

This project demonstrates a procedural path generation system in Unity, designed to create dynamic and varied paths within a grid-based chunk system. The system is implemented using C# scripts and Unity's editor tools to allow for easy customization and testing.

## How It Works

1. **ChunksManager**:
   - **Role**: Acts as the central controller for procedural chunk generation, managing the creation and initialization of each chunk.
   - **Path Processing**: Utilizes a queue to systematically process paths, ensuring new paths are added to the end for orderly generation.
   - **Chunk Initialization**: Configures each chunk with parameters like size and position, ensuring a cohesive scene layout.

2. **Chunk**:
   - **Path Generation**: Responsible for generating paths within its boundaries using weighted direction selection and backtracking.
     - **Weighted Direction Selection**: Prioritizes path directions based on criteria like alignment and proximity.
     - **Backtracking**: Corrects paths that reach dead ends, ensuring complete and valid path networks.
   - **Constraints**: Ensures paths remain within chunk limits and avoid unnecessary overlaps.

3. **RandomSeedController**:
   - **Seed Management**: Handles the random seed for reproducible path generation, allowing consistent results across runs.
   - **Seed Operations**: Provides tools to generate, set, and copy seeds, facilitating debugging and configuration sharing.
   - **Deterministic Behavior**: Converts non-numeric seeds into deterministic hash codes for consistent behavior.

4. **MeshesCombiner**:
   - **Performance Optimization**: Combines meshes within a chunk to reduce draw calls, enhancing performance in large scenes.
   - **Material Handling**: Groups meshes by material to optimize rendering.
   - **Vertex Limit Management**: Ensures combined meshes do not exceed Unity's vertex limits, splitting them if necessary.

### Usage

1. **Included Scene**: The project comes with a pre-configured scene that includes all necessary components to test the procedural path generation system. Simply open the scene in Unity to get started.

2. **Set Up the Scene**: Ensure your scene contains a GameObject with the scrips `ChunksManager`, `RandomSeedController` and `MeshesCombiner`.

3. **Configure ChunksManager**: In the Unity Inspector, configure the `ChunksManager` with the desired chunk size, number of chunks, and other parameters to tailor the path generation to your needs.

4. **Run the Scene**: Enter Play mode in Unity. Use the custom editor buttons to initialize chunks and manage the random seed. This allows you to see the procedural path generation in action and experiment with different configurations.

5. **Customize**: Modify the scripts and parameters to experiment with different path generation behaviors and optimizations. The system is designed to be flexible, allowing for a wide range of path generation scenarios.
 
