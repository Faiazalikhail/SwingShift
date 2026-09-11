# 01 — Scope and Gameplay

[Back to project index](../README.md)

## 1. Deliverable

A single-player, greybox Unity game with one crane truck, one steel beam, one target pad, and a 90-second run. Launch directly into gameplay, show success or a specific failure reason, and allow immediate restart.

## 2. Scope contract

| Included | Basis |
| --- | --- |
| Drive and crane modes, switched with Tab | GDD |
| Parked lifting with outriggers | GDD |
| Arm rotation, trolley travel, adjustable hoist | GDD |
| Physical beam swing and release | GDD |
| Cable tension gauge and overload failure | GDD |
| Landing readout and impact-dependent outcome | GDD |
| 90-second timer | GDD |
| One crane, beam, and target; grey geometry | GDD |
| On-screen controls, result text, R to restart | Proposed usability defaults supporting the loop |

### Explicit GDD exclusions

- Story and characters.
- Additional levels, wind, and weather.
- Driving while carrying a load.
- Sound, menus, and saving.
- Art production.

### Additional implementation boundaries

- No cable wrapping, chain of rope links, or tangled-rope simulation.
- No crane tipping, terrain deformation, or hydraulic simulation.
- No vehicle selection, upgrades, leaderboards, or progression.
- No extra boom articulation beyond the specified rotation and trolley movement.
- No third-party asset dependency unless it solves a demonstrated blocker.

These boundaries simplify implementation; they must not remove the four required physics behaviours.

## 3. Player loop

1. Drive to a clearly marked operating area.
2. Stop and press Tab to deploy outriggers and enter crane mode.
3. Position the hook near the beam's attachment point.
4. Press Space to attach, then hoist the beam clear of the ground.
5. Rotate the arm and move the trolley to transfer the load.
6. Control swing and lower onto the target.
7. Release and allow the beam to settle.
8. Read the result and press R to retry.

## 4. Controls

| Mode | Input | Action |
| --- | --- | --- |
| Drive | W | Throttle |
| Drive | S | Brake; reverse after stopping is a proposed default |
| Drive | A / D | Steer left / right |
| Both | Tab | Request mode change |
| Crane | A / D | Rotate arm left / right |
| Crane | W / S | Trolley out / in |
| Crane | Q / E | Hoist up / down |
| Crane | Space | Attach / release |
| Any run state | R | Restart — proposed default |

Only the active mode receives movement commands. Clear held-input state when changing modes so an input from the previous mode does not trigger unintended movement.

## 5. Proposed rules for unspecified details

| Situation | Default for implementation |
| --- | --- |
| Enter crane mode | Truck must be grounded and nearly stationary; initial speed threshold 0.1 m/s, also checking rotation. |
| Park | Deploy simple outrigger geometry and lock the base at its current valid pose. |
| Return to drive mode | No beam attached; trolley, hoist, and arm must be in the documented travel configuration. Display the unmet condition. |
| Attach | Hook is within a small configurable distance of the beam's marked attachment point. Do not teleport the beam into place. |
| Release | Remove the connection and preserve physical velocity. |
| Start timer | First valid gameplay input starts the 90-second countdown. |
| Clean landing | Incoming contact speed is below 0.5 m/s. |
| Rough landing | Incoming contact speed is 0.5–1.5 m/s inclusive; may still succeed with a rough result. |
| Damaging impact | Incoming contact speed exceeds 1.5 m/s after the beam has first been lifted; fails the run. |
| Where damage applies | Beam impacts with the ground, target, or crane after the lift. Initial resting contact is excluded. |
| Placement success | Released beam is supported by the target, fully inside its usable footprint, and sufficiently still for one continuous second. |
| Suspended target overlap | Never sufficient for success. |
| Invalid placement | Run continues while time remains, provided there has been no damaging impact. |
| Multiple events together | Damage or cable failure takes priority over placement success. Evaluate timeout consistently against the recorded completion time. |
| Result | One terminal outcome per run; disable gameplay input and stop the timer. Keep R available. |

Target settling uses both linear and angular motion. The worst qualifying impact during a landing attempt determines its grade; a later gentle contact must not erase an earlier hard hit.

## 6. Level and camera

### Layout requirements

- Flat ground and a short driving approach.
- Pickup and target reachable from the same operating position.
- Enough boom height and cable travel to pick up, clear, transfer, and place the beam.
- An unobstructed transfer route and a target larger than the beam footprint.
- Ground boundaries or reset handling that prevent an irrecoverable off-map run.

Validate reach and clearance with actual crane dimensions before finalizing the layout. Do not place the target using visual guesswork alone.

### Camera requirements

- Elevated driving view showing the truck and approach.
- Crane view showing the hook, beam, target, and surrounding ground.
- Simple shadows and target markings to communicate height and depth.
- No mandatory cinematic transitions or free-camera controls.

## 7. Minimum feedback

| Readout | Required information |
| --- | --- |
| Mode | Drive or Crane |
| Timer | Seconds remaining |
| Cable | Measured load in kN and rated limit |
| Landing | Incoming impact speed and grade when applicable |
| Context | Current controls or reason an action is blocked |
| Outcome | Success, rough placement, cable failure, damaged beam, or timeout |

Use text as well as colour. Keep physics diagnostics in the test scene or developer view rather than cluttering the player HUD.

## 8. Run pacing target

| Activity | Initial target |
| --- | --- |
| Approach and park | 10–15 s |
| Attach and lift | 15–20 s |
| Transfer and manage swing | 20–25 s |
| Lower, release, settle | 15–20 s |
| Recovery allowance within 90 s | Approximately 10–30 s |

These are playtest targets. Tune distances and controls so careful play leaves room for a correction.
