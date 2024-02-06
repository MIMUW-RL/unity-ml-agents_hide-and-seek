## Requirements
The project was made using Unity 2021.3.10f1 with ML-Agents Release 20 (Unity package version 2.3.0-exp.2). To train the agents, [the ML-Agents Python package](https://github.com/Unity-Technologies/ml-agents/blob/release_20_docs/docs/Installation.md#install-the-mlagents-python-package) is required.

## Inference
To run inference, use the `Scenes/Test` scene. Checkpoints for hiders and seekers can be loaded in the Unity inspector in `Game Controller` script properties of the platform object. After loading checkpoints, the easiest way to run inference is to run the scene in the editor. Alternatively, you can build the binary (go to `File > Build Settings`, add `Test` scene to build, set the desired target platform, then click Build). The binary can be run using the following command
```
./unity-ml-agents_hide-and-seek.x86_64 [game_params=<path/to/game_params_config.json>] [arena_params=<path/to/arena_params_config.json>]
```
Note that checkpoints for inference cannot be loaded using game controller configs.

## Training
Training can be run using the following command
```
mlagents-learn --env <path/to/hide-and-seek_binary> --num-envs 3 --no-graphics <path/to/ml-agents_config.yaml> --run-id <run_id> [--env-args <game_params=<path/to/game_params_config.json> arena_params=<path/to/arena_params_config.json>]
```
See the [ML-Agents getting started guide](https://github.com/Unity-Technologies/ml-agents/blob/release_20_docs/docs/Getting-Started.md) for more details on using ML-Agents trainer script.

## Environment Settings
The following tables describe the parameters of `Game Controller` and `Map Generator` scripts attached to platform objects.
The parameters can be either changed in the Unity inspector, or overriden by passing a json config to the environment binary. Example configs are located in the `config` directory.

### Game Parameters
| **Parameter** | **Description** |
|---------------|-----------------|
|               |                 |
