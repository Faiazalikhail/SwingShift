# 02 — Technical Design

[Back to project index](../README.md)

## 1. Approach

Use Unity `6000.3.23f1`, a basic 3D project on the Built-in render pipeline, and keyboard input through the Input System package (the legacy Input Manager is disabled). Keep that render pipeline for the entire prototype. Avoid package upgrades during the delivery week.

Use metres, kilograms, and seconds. Start with Unity's standard fixed physics step and adjust only in response to measured instability. Read input each rendered frame, apply physics commands on fixed steps, and keep presentation updates separate.

### Proposed project organization

| Location | Contents |
| --- | --- |
| `Assets/_SwingShift/Scenes` | `PhysicsLab` and `Prototype` |
| `Assets/_SwingShift/Scripts` | Small gameplay and presentation components |
| `Assets/_SwingShift/Prefabs` | Crane assembly, beam, target, HUD |
| `Assets/_SwingShift/Materials` | Minimal greybox materials |
| `Assets/_SwingShift/Input` | Keyboard actions and bindings |
| `Builds/Windows` | Local build outputs, excluded from source control |

These folders were created on 2026-09-11 along with the `PhysicsLab` and `Prototype` scenes (both registered in Build Settings, `Prototype` first). Scenes and levels are authored by hand in the Unity Editor; no editor tooling generates content. `Builds/` is git-ignored.

## 2. Physical assembly

| Element | Starting implementation | Validation requirement |
| --- | --- | --- |
| Truck | Dynamic chassis with four WheelColliders | Accelerates and brakes gradually; remains stable on flat ground. |
| Parked base | Lock chassis after checking parking conditions | Does not drift during lifting. |
| Arm | Dynamic body attached by a powered hinge | Finite motor torque allows load-dependent acceleration. |
| Trolley | Dynamic body on a constrained sliding joint | Travels only along the arm with bounded drive force. |
| Beam | One dynamic body with box collider | Swings, rotates, collides, and retains momentum on release. |
| Cable | Single ConfigurableJoint candidate connecting beam attachment to trolley | Acts as a maximum-distance, tension-only constraint; permits slack and angular freedom. |
| Cable drawing | LineRenderer | Follows actual attachment points; does not control beam motion. |
| Unloaded hook | Simple positioning marker | Clearly communicates attachment range without adding another fragile body chain. |

The exact cable joint configuration is subject to the first milestone. Verify its effective distance boundary in several directions; do not assume inspector settings alone prove cable behaviour. It must not push the beam away when slack or lock its swing into a rigid bar.

### Attachment and hoisting

- Attach near the actual beam anchor and initialize from the current separation.
- Validate the connection before applying hoist commands; avoid an initial constraint violation that creates an artificial impulse.
- Hoist by changing permitted length gradually at a bounded rate.
- Keep the beam dynamic while attached.
- Release removes the connection without assigning a new pose or zeroing velocity.
- Use the top-centre beam anchor initially; check rotational behaviour and practical placement.

## 3. Four physics requirements

### A. Mass, force, and rotational inertia

Truck propulsion and braking act through wheel torque. Tune motor force rather than setting velocity every frame.

Arm rotation uses a finite torque budget. Compare the response with the loaded trolley near and far from the pivot. The extended load should resist acceleration and stopping more strongly. If that difference is absent, inspect the joint coupling and motor strength before adding a scripted slowdown.

**Mass interpretation — proposed:** Treat the 8,000 kg truck value as the crane vehicle assembly excluding the 500 kg payload. If chassis, arm, and trolley have separate bodies, distribute the assembly mass rather than giving every part the full truck mass. Confirm against any instructor requirement.

### B. Pendulum motion

Gravity and the cable constraint produce swing. Do not animate the beam along a sine wave or force it back under the hook.

Use a simple fixed-support, small-angle reference test:

| Ideal cable length | Ideal point-mass period |
| --- | --- |
| 4 m | Approximately 4.01 s |
| 10 m | Approximately 6.34 s |

The extended beam, attachment offset, moving support, and hoisting alter the real rig's response. Compare the formula to a simplified test body, then use qualitative and stability checks for the gameplay beam.

### C. Tension and cable failure

Start from the cable joint's solver force and built-in break threshold. Validate that the measured reaction comes from the cable constraint rather than unrelated locked motion. Keep gauge and failure tied to the same physical connection.

- Resting suspended payload reference: `500 × 9.81 = 4,905 N`.
- GDD break rating: `10,000 N`.
- Display in kN; keep physics calculations in N.
- Gauge smoothing is presentation only; it must not silently redefine break behaviour.
- On a break event, record the outcome once and allow the beam to fall.
- Avoid artificial spikes from attachment, abrupt length changes, or impossible joint geometry.

The GDD formula `Ft = mg cos(theta) + mv²/L` is a reference for a simplified taut cable with a fixed support and fixed length. Support acceleration and hoisting require additional consideration. Use it as a controlled cross-check, not a second independent gameplay authority.

At equal tangential speed, a shorter cable increases the centripetal term. Shortening does not guarantee higher tension in every evolving motion. Deliberate overload must be demonstrated at the production settings rather than assumed from the formula.

### D. Landing impact

Define the landing speed as the incoming contact-point closing speed along the surface normal. Record pre-impact linear and angular motion so the measurement is not taken after the solver has already stopped the beam.

- Include rotational velocity: a beam tip can strike quickly while its centre moves slowly.
- For multiple contacts, use the greatest qualifying incoming impact speed.
- Track the worst impact grade throughout a landing attempt.
- Apply damage detection independently of the target's success detector.
- Arm damage evaluation once the beam is actually lifted clear of its original support.

Use the GDD speed thresholds for gameplay. The impulse relationship explains the outcome; do not label an arbitrary speed-derived number as a measured peak force.

## 4. Component responsibilities

Names below are proposed; preserve the boundaries even if names change.

| Component | Owns | Must not own |
| --- | --- | --- |
| `PlayerInputRouter` | Input intent and active bindings | Physical movement or scoring |
| `RunController` | Countdown, terminal outcome, restart | Crane mechanics |
| `CraneModeController` | Mode guards, parking, travel readiness | Landing grade |
| `TruckController` | Steering, propulsion, braking | Cable attachment |
| `CraneController` | Arm motor, trolley, hoist commands | Success detection |
| `CableController` | Attachment, length, tension, break events | Timer and HUD layout |
| `BeamImpactMonitor` | Lift state, impact measurement, damage event | Target success |
| `TargetPlacementDetector` | Support, footprint, release, settling | Input or damage threshold |
| `PrototypeHUD` | Reads and displays current state | Authoritative gameplay decisions |
| `CameraController` | Framing for each mode | Physics movement |

Use direct Inspector references and simple events where needed. Avoid global object searches every frame, framework construction, and dependency-injection infrastructure.

## 5. State ownership

Keep run status and control mode separate:

- **Run:** Ready → Running → Succeeded or Failed.
- **Mode:** Drive or Crane, with guarded transitions.
- **Load:** Unattached, Attached, Released, or Broken.

Only `RunController` commits a terminal result. Resolve competing events consistently. Restart reloads the gameplay scene initially, restoring every system through the same path.

## 6. Tuning baseline

| Parameter | Initial value | Source |
| --- | --- | --- |
| Vehicle mass | 8,000 kg | GDD; distribution is proposed |
| Payload mass | 500 kg | GDD |
| Gravity | 9.81 m/s² | GDD |
| Cable length | 4–10 m | GDD |
| Break rating | 10 kN | GDD |
| Driving speed limit | 6 m/s | GDD |
| Trolley speed limit | 2 m/s | GDD |
| Arm speed limit | 20 degrees/s | GDD |
| Clean / damaging landing | Below 0.5 / above 1.5 m/s | GDD |
| Hoist speed and acceleration ramps | Select during rig testing | Unspecified |
| Motor torque, brake torque, trolley force | Select during rig testing | Unspecified |
| Settling duration | 1 s | Proposed |

Keep limits, rates, and thresholds editable in the Inspector. Change one parameter at a time and record meaningful changes.

## 7. Stability troubleshooting order

1. Check units, collider overlap, anchors, permitted motion, and starting separation.
2. Remove sudden pose changes and abrupt motor or hoist commands.
3. Check mass distribution and motor force limits.
4. Test the smallest failing rig in `PhysicsLab`.
5. Adjust solver settings or timestep only with a repeatable comparison.
6. Recheck force readings and break behaviour after every physics-setting change.

Do not silently use fake mass scaling or strong projection to conceal instability; those choices can change the physical evidence. If the joint approach cannot pass its gate, document the failure and reassess the remaining schedule before substituting another model.

## 8. Unity references

- [WheelCollider implementation workflow](https://docs.unity3d.com/6000.3/Documentation/Manual/WheelColliderTutorial.html)
- [ConfigurableJoint component reference](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ConfigurableJoint.html)
- [Joint.currentForce](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Joint-currentForce.html)
- [Joint.breakForce](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Joint-breakForce.html)
- [Rigidbody.GetPointVelocity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody.GetPointVelocity.html)

These document available engine features. They do not replace validation of this project's settings and behaviour.
