## Requirements
The project was made using Unity 2021.3.10f1 with ML-Agents Release 20 (Unity package version 2.3.0-exp.2). To train the agents, [the ML-Agents Python package](https://github.com/Unity-Technologies/ml-agents/blob/release_20_docs/docs/Installation.md#install-the-mlagents-python-package) is required.

## Inference
To run inference, use the `Scenes/Test` scene. Checkpoints for hiders and seekers can be loaded in the Unity inspector in `Game Controller` script properties of the platform object. After loading checkpoints, the easiest way to run inference is to run the scene in the editor. Alternatively, you can build the binary (go to `File > Build Settings`, add `Test` scene to build, set the desired target platform, then click Build). The binary can be run using the following command
```
./unity-ml-agents_hide-and-seek.x86_64 [game_params=<path/to/game_params_config.json>] [arena_params=<path/to/arena_params_config.json>]
```
Note that checkpoints for inference cannot be loaded using game controller configs.

## Training
To run training, use the `Scenes/Training` scene. You can adjust the number of training platforms by simply deleting/duplicating platform objects and stacking them vertically.
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
| `episodeSteps` | (default = `500`) The length of a single episode in environment steps. |
| `gracePeriodFraction` | (default = `0.4`) The fraction of episode steps during which hider actions are locked. E.g. for `episodeSteps = 500` and `gracePeriodFraction = 0.4`, the first 200 steps of each episode are considered warmup. |
| `coneAngle` | (default = `67.5`) The fov of agents used for calculating rewards. |
|---------------|-----------------|
| `rewards` | A list of individual rewards to be awarded to agents. Every list entry should be a dictionary with two keys: `type` and `weight`. For the description of all reward types refer to the table below. |
| `winCondition` | What event is considered as a win for either team. For the description of all win condition types refer to the table below. |
| `winConditionRewardMultiplier` | The group reward given to the team which satisfied the set winning condition at the end of every episode. |
| `arenaSize` | (default = `25`) The size of playable area of the platform used for giving out of bounds penalty to agents. |
| `allowCapture` | Whether hider being too close to a seeker should result in him getting caugth and removed from the game. |
| `captureDistance` | (default = `1.2`) The distance between hider and seeker at which hider is considered caugth. Has effect only if `allowCapture` is enabled. |
| `seekersCaptureGoal` | (default = `3`) The number of hiders that seekers need to catch to win. Has effect only if `allowCapture` is enabled and `winCondition` is set to `2`. |
|---------------|-----------------|
| `useCoplay` | (default = `false`) Whether to use coplay during training. |
| `numberOfCoplayAgents` | (default = `1`) The number of coplay agents per episode. |
| `selfPlayRatio` | (default = `0.5`) The probability of using coplay each episode. |

### Arena Parameters
| **Parameter** | **Description** |
|---------------|-----------------|
| `mapSize` | (default = `25.0`) The size of playable area of the platform used during map generation. Should be equal to `arenaSize` set in game parameters. |
| `wallY` | (default = `1.0`) The height at which arena walls spawn. Should be left at default value. |
| `wallThickness` | (default = `0.25`) The thickness of wall prefab objects. Should be left at default value. |
| `wallsPosition` | (default = `0.0`) The distance of walls from the center of the platform. If set to `0`, the distance will match `arenaSize`. |
|---------------|-----------------|
| `numHidersMin` | (default = `3`) The minimum number of hiders in team. |
| `numHidersMax` | (default = `3`) The maximum number of hiders in team. Should be at most `4` and not smaller than `numHidersMin`. |
| `numSeekersMin` | (default = `3`) The minimum number of seekers in team. |
| `numSeekersMax` | (default = `3`) The maximum number of seekers in team. Should be at most `4` and not smaller than `numSeekersMin`. |
| `agentY` | (default = `1.0`) The height at which agents spawn. Should be left at default value. |
| `agentRadius` | (default = `0.75`) The radius around each agent at which no objects will spawn during map generation. |
|---------------|-----------------|
| `instantiateBoxes` | (default = `true`) Whether boxes should be spawned every episode. |
| `numBoxesMin` | (default = `5`) The minimum number of boxes which spawn every episode. |
| `numBoxesMax` | (default = `5`) The maximum number of boxes which spawn every episode. Should be not smaller than `numBoxesMin`. |
| `boxY` | (default = `1.0`) The height at which boxes spawn. Should be left at default value. |
| `objectRadius` | (default = `1.5`) The radius around each box at which no other objects will spawn during map generation. |

### Reward Types
| **ID** | **Editor name** | **Description** |
| `0` | Visibility Individual | `-weight` or `+weight` given every step depending on the individual visibility criteria. A hider gets a positive reward iff no seeker has a line of sight on him. A seeker gets a positive reward iff he has a line of sight on at least one hider. |
| `1` | Visibility Team | `-weight` or `+weight` given every step depending on the team visibility criteria. Hiders get a positive reward iff no seeker has a line of sight on any hider. Seekers get a positive reward iff at least one seeker has a line of sight on at least one hider. |
| `2` | Capture | Given every time a seeker captures a hider. Caught hider is given a `-weight` penalty and seeker who captured him gets a `+weight` reward. Has effect only if `allowCapture`is enabled. |
| `3` | Oob Penalty | `-weight` penalty given every step to every agent which is outside of the box of size `arenaSize` around the center of the arena. |

### Win Condition Types
| **ID** | **Editor name** | **Description** |
| `0` | None | No team is considered winning and no group rewards are given. |
| `1` | Line Of Sight | Seekers win if at any step after the warmup at least one seeker had a line of sight on at least one hider. Hiders win otherwise. |
| `2` | Capture | Seekers win if they capture at least `seekersCaptureGoal` hiders during the episode. Hiders win otherwise. |


