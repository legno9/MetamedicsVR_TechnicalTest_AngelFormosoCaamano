# MetamedicsVR_TechnicalTest_AngelFormosoCaamano

# Procedural Path Generation in Unity

This project demonstrates a procedural path generation system in Unity, designed to create dynamic and varied paths within a grid-based chunk system. The system is implemented using C# scripts and Unity's editor tools to allow for easy customization and testing.

## How It Works

1. **ChunksManager**: Manages the creation and initialization of chunks. It uses a queue to process paths in a specific order, ensuring that new paths are added to the end of the queue for future processing.

2. **Chunk**: Each chunk is responsible for generating paths within its boundaries. It uses a combination of weighted direction selection and backtracking to create paths that adhere to specified constraints.

3. **RandomSeedController**: Manages the random seed used for path generation. It can generate a new seed, set a specific seed, and copy the current seed to the clipboard.

4. **MeshesCombiner**: Combines meshes within a chunk to optimize performance by reducing the number of draw calls.

### Usage

1. **Set Up the Scene**: Ensure your scene contains a `ChunksManager` and `RandomSeedController` GameObject. Attach the respective scripts to these GameObjects.

2. **Configure ChunksManager**: In the Unity Inspector, configure the `ChunksManager` with the desired chunk size, number of chunks, and other parameters.

3. **Run the Scene**: Enter Play mode in Unity. Use the custom editor buttons to initialize chunks and manage the random seed.

4. **Customize**: Modify the scripts and parameters to experiment with different path generation behaviors and optimizations.
 
