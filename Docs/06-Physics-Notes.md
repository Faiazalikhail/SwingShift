# 06 — Physics Notes

[Back to project index](../README.md)

This document records what each physical element of the prototype is, why it is built that way, and what we can and cannot claim about it. It is written so the work can be explained and defended in review. Each section is appended as the corresponding piece is built.

## 1. Units and the simulation step

- Unity's physics engine (PhysX) works in metres, kilograms, seconds, and newtons. The project uses those units directly; nothing is rescaled.
- Gravity is `-9.81 m/s²` on the Y axis, the GDD value.
- Physics advances in fixed steps of `0.02 s` (50 Hz). Rendering runs at whatever frame rate the machine achieves. Keeping physics on a fixed step makes the simulation repeatable regardless of frame rate; forces and joints are only evaluated on those steps.
- Input is read every rendered frame; physics commands are applied in `FixedUpdate`. Presentation (the cable line, HUD) reads state and never drives it.

## 2. Piece 1 — Lab rig (PhysicsLab scene)

Placed by hand in the Unity Editor. The build checklist is at the end of this section; the values in the table are the ones to enter.

### What is in the scene

| Object | Physics role | Key values |
| --- | --- | --- |
| Floor | Static collider | 40 × 1 × 40 m slab, top face at y = 0 |
| Support | Kinematic rigidbody, stands in for the crane trolley | 1 m block, hook point at y = 10 m |
| Beam | The one free dynamic body | 500 kg, 6 × 0.3 × 0.2 m box collider, rests on the floor |
| CableAnchor | Empty transform on the beam | Top-centre of the beam |

### Why these choices

**Why prove the cable in an isolated rig first.** The cable joint is the highest-risk element: if it jitters, stretches, or breaks spuriously, every later system inherits that. A four-object scene isolates the variable. When something misbehaves later in the full crane, the lab is where we reproduce it with the smallest failing rig.

**Why the support is kinematic rather than static.** A Unity joint connects two rigidbodies. A kinematic rigidbody is moved by script or not at all, and is never pushed by forces, so it behaves as an ideal fixed support: infinite effective mass. That is exactly the assumption behind the textbook pendulum period `T = 2π√(L/g)`, which we will measure against in the physics gate. Later, moving the support by script reproduces trolley travel without changing the joint setup.

**Why the beam is a box, and why mass is set by hand.** Unity does not compute mass from volume. Mass is a number we assign; the engine derives the inertia tensor from the collider shape assuming uniform density and then scales it to that mass. A solid steel block of 6 × 0.3 × 0.2 m would weigh about 2,800 kg, so the 500 kg value corresponds to a hollow or I-section member with that outer envelope. The box is that envelope. For swing, tipping, and impact what matters is the mass, the moment of inertia distribution, and the contact shape, and a box captures all three well enough for a greybox prototype. The elongated shape is deliberate: a 6 m beam has a large moment of inertia about its short axes, so it swings and yaws differently from a point mass, which is one of the things the GDD wants to be visible.

**Why the rigidbody and collider sit on an unscaled parent.** Anchors and joint offsets are expressed in the body's local space. If the body itself were a scaled cube, those offsets would be stretched by the scale and easy to get wrong. The visual mesh is a scaled child with no collider; the parent owns the physics shape in real metres.

**Why continuous collision detection on the beam.** Collision detection normally checks for overlap at the end of each step. A beam falling 10 m after a cable break reaches about 14 m/s, which is 0.28 m per step, more than its 0.2 m thickness. It could pass through the floor between checks. Continuous Dynamic detection sweeps the collider along its path within the step, so the fall is caught. This is a correctness setting, not a tuning knob.

**Why linear damping is zero.** Unity's linear damping is an artificial velocity decay, not air resistance modelled from shape or speed. Leaving it at zero means any slowing of the beam comes from contact friction, cable tension, or (later) deliberate control, so the physical causes stay readable. The small default angular damping (0.05) is kept to suppress purely numerical spin; it is far below anything that changes the swing behaviour.

**Why interpolation is on.** Physics updates at 50 Hz and rendering may run faster. Interpolation blends the rendered position between the last two physics states so motion looks smooth. It changes nothing in the simulation.

**Why the hook is at 10 m.** The GDD cable range is 4–10 m. Placing the hook at the top of that range lets the full range be tested from one support position, and the beam on the floor sits 9.7 m below the hook, inside the range, so the first attachment can start from the actual separation without a jump.

### What this piece does not claim

- No cable exists yet; the beam simply rests on the floor under gravity.
- The support has no inertia response. The real crane arm and trolley will be dynamic bodies driven by motors with finite torque (Milestone 2), which is where the "load resists acceleration" behaviour comes from.
- The beam's internal structure is not modelled. Deflection, bending, and material stress are outside the GDD scope.

### Build checklist (Editor)

Hierarchy layout: an empty `LabRoot` holding `Floor`, `Support`, and `Beam`.

1. **Materials.** In `Assets/_SwingShift/Materials` create three Standard materials: `Greybox_Floor` (mid grey), `Greybox_Support` (dark blue-grey), `Greybox_Beam` (orange).
2. **Floor.** Create a Cube named `Floor`. Position `(0, -0.5, 0)`, scale `(40, 1, 40)`. Assign `Greybox_Floor`. Tick **Static** in the top-right of the Inspector. Top face is now at y = 0.
3. **Support.** Create an Empty named `Support` at `(0, 10.5, 0)`. Add a **Rigidbody**: tick **Is Kinematic**, untick **Use Gravity**. Add a child Cube named `SupportVisual` at local `(0, 0, 0)`, scale `(1, 1, 1)`, material `Greybox_Support`. Add a child Empty named `HookPoint` at local `(0, -0.5, 0)`.
4. **Beam.** Create an Empty named `Beam` at `(0, 0.15, 0)`, scale `(1, 1, 1)`. Add a **Box Collider** with size `(6, 0.3, 0.2)`. Add a **Rigidbody**: Mass `500`, Linear Damping `0`, Angular Damping `0.05`, Interpolate `Interpolate`, Collision Detection `Continuous Dynamic`. Add a child Cube named `BeamVisual` at local `(0, 0, 0)`, scale `(6, 0.3, 0.2)`, material `Greybox_Beam`, and **remove its Box Collider**. Add a child Empty named `CableAnchor` at local `(0, 0.15, 0)`.
5. **Camera.** Move `Main Camera` to `(14, 7, -16)` and rotate it to look at about `(0, 5, 0)` (rotation roughly `(5, -41, 0)`). Set Far clip to `200`.
6. Save the scene.

### How to check it

1. Open `PhysicsLab`, press Play.
2. The beam must stay at rest on the floor without sinking, jittering, or sliding. Select it and confirm the Rigidbody reads mass 500 and the collider size 6 × 0.3 × 0.2.
3. In the Inspector during Play, the Rigidbody's velocity should settle to zero and the body should go to sleep within a second or two.

## 3. Piece 2 — Cable connection

Runtime script: `Assets/_SwingShift/Scripts/CableController.cs`, attached to the beam. The joint itself is created by the script at attach time and destroyed at release, because attach and release are gameplay actions, not level layout.

### What the cable is

A single **ConfigurableJoint** on the beam, connected to the support's Rigidbody.

| Setting | Value | Reason |
| --- | --- | --- |
| Anchor | Beam's `CableAnchor` (top-centre) | Where the sling is fixed to the load |
| Connected anchor | Support's `HookPoint`, auto-configure **off** | The distance limit must be centred on the hook, not on the beam's start position |
| X, Y, Z motion | Limited | Together these make one distance limit: a sphere of radius = permitted length around the hook |
| Angular X, Y, Z | Free | A sling does not resist rotation; the beam may swing and yaw freely |
| Linear limit spring | 0 / 0 | A hard limit: the boundary is a rigid constraint, not a spring |
| Limit bounciness | 0 | No artificial rebound when the cable goes taut |
| Enable collision | Off | Beam and support are coupled by the cable, not by contact |
| Break force | Infinity for now | The 10 kN rating is applied in the tension piece so it is tested together with the gauge |

### Why a distance limit is the right model

A cable can pull but cannot push. Inside the radius the joint does nothing, so the beam is in free fall or resting on whatever supports it: that is slack. At the boundary the solver applies only the force needed to keep the anchor inside the sphere, always directed along the cable: that is tension. This gives slack, taut, and swinging behaviour from one constraint with no scripted cases.

A spring joint would allow stretch and store energy; a fixed-length hinge chain would add bodies and instability. The distance limit is the simplest constraint that has the real cable's one-sided behaviour.

### Why attach initialises from the current separation

On attach, the permitted length is set to the actual distance between the two anchors (9.7 m in the lab, inside the 4–10 m range). The constraint is therefore already satisfied at the moment it is created and the solver has nothing to correct, so no impulse is injected. Attaching with a shorter length than the current separation would be an instantaneous violation: PhysX would yank the beam toward the hook in one step, a fake force that could also trip the break threshold. The script refuses to attach outside the allowed range.

### Why release does not touch velocity

`Release()` only destroys the joint. Momentum is a state of the body, not of the constraint; removing the constraint must leave that state alone. This is what lets a released beam keep swinging or flying, which the GDD asks for.

### What is presentation only

The LineRenderer draws a straight line between the two anchors every frame after physics. It reads positions and never writes them. Deleting it changes nothing physical.

### What this piece does not claim

- The cable is massless and perfectly stiff; a real cable has mass, sag, and elasticity.
- No hoisting yet: the permitted length only changes through `SetPermittedLength`, which the next piece drives from input at a bounded rate.
- No tension reading or break yet.

### Build checklist (Editor)

1. In `Assets/_SwingShift/Materials` create a Standard material `Greybox_Cable`, Albedo hex `1A1A1A`.
2. Select `Beam`. Add Component → **Line Renderer**. Set Width to `0.05`. Under Materials, drag `Greybox_Cable` into Element 0. Untick **Cast Shadows** is optional. Leave Positions as they are; the script overwrites them.
3. With `Beam` still selected, Add Component → **Cable Controller**.
4. Fill its fields by dragging from the Hierarchy: Support → `Support`, Hook Point → `HookPoint`, Cable Anchor → `CableAnchor`, Cable Line → the Line Renderer on `Beam` (drag the `Beam` object itself onto the field; Unity picks its Line Renderer). Leave Min 4, Max 10, Attach On Start ticked.
5. Save the scene.

### How to check it

1. Press Play. A dark line should run from the block to the beam, and the beam should stay on the floor: the cable is exactly taut but carries no load because the floor holds the beam.
2. **Slack test.** While playing, select `Support` and set its Position Y to `9` in the Inspector (kinematic bodies may be moved this way). The line shortens and nothing else happens: the cable is slack, the beam does not move. Set Y back to `10.5`.
3. **Hang test.** While playing, select `Floor` and untick the checkbox next to its name to disable it. The beam should drop a few centimetres, catch on the cable, and hang level with a small bounce that dies out. It must not pass through the cable radius, jitter, or spin up.
4. **Swing test.** While hanging, select `Beam`, and in the Rigidbody Info section note the velocity. Stop Play. This is enough for now; a measured swing period comes in the physics gate.
5. Stop Play. Play-mode changes are discarded automatically.

## 4. Piece 3 — Hoist and release from input

Runtime scripts: `PlayerInputRouter.cs` (input intent) and `CraneController.cs` (hoist command). Bindings live in `Assets/_SwingShift/Input/SwingShiftActions.inputactions`, map `Crane`: Hoist is a 1D axis with Q positive (raise) and E negative (lower); AttachRelease is Space.

### How hoisting works

The winch does not move the beam. It changes the **permitted length** of the cable, and the joint's distance limit does the rest: shortening below the current separation pulls the beam up through the constraint, lengthening lets it descend under gravity or go slack if something supports it.

| Parameter | Value | Reason |
| --- | --- | --- |
| Hoist speed | 1.0 m/s | A plausible winch drum rate; adjustable in the Inspector |
| Hoist acceleration | 4 m/s² | The rate ramps to full speed in about 0.25 s rather than stepping |
| Length range | 4–10 m | GDD |

### Why the rate is bounded and ramped

The joint limit is evaluated once per physics step. If the limit jumped by a large amount in one step the solver would correct the whole violation in that step with a single large force. That force is not a real cable load, but it would show on the tension gauge and could trip the break threshold. Bounding the rate keeps each step's change to at most 0.02 m (1 m/s × 0.02 s) and ramping the rate removes the discontinuity when a key is pressed or released. Physically this is the winch's finite drum speed and its motor spin-up.

### Why input is latched

Key events arrive between rendered frames; physics commands are applied in `FixedUpdate`. At high frame rates several rendered frames can pass between two physics steps, so a Space tap read only in `Update` could be missed. The router stores the press until the crane consumes it on the next physics step.

### Why attach and release are the same key

Space toggles: attach when free, release when attached. Release removes the joint and nothing else, so the beam keeps its momentum. Attach re-initialises the permitted length from the current separation, so re-attaching after a release never produces a jump.

### What this piece does not claim

- No hook proximity check yet: in the lab the beam is always under the hook. The game version requires the hook within a small distance of the anchor (Milestone 2).
- No tension reading or break yet (piece 4).

### Build checklist (Editor)

1. Right-click `LabRoot`, Create Empty, name it `Player`. Add Component → **Player Input Router**.
2. Right-click `LabRoot`, Create Empty, name it `Crane`. Add Component → **Crane Controller**. Drag `Player` onto its Input field and `Beam` onto its Cable field. Leave Hoist Speed 1 and Hoist Acceleration 4.
3. Save the scene.

### How to check it

1. Press Play. Hold **Q**. The beam lifts off the floor, hangs level, and rises steadily. Release Q: it stops without a jolt.
2. Hold **E**. The beam descends and settles on the floor; keep holding and the line goes slack. Release E.
3. Press **Space**. The line disappears (released). Press Space again: it reattaches at the current separation with no movement.
4. Hoist with Q to about mid-height, then press Space. The beam drops and lands. It must not pass through the floor.
5. Nudge test: while hanging, select `Support` and set its Position X to `1`, then back to `0`. The beam swings and the swing decays only slowly. That is the pendulum the physics gate will measure.

## 5. Piece 4 — Tension and cable failure

Runtime: tension measurement and break rating in `CableController.cs`; display in `PrototypeHUD.cs`.

### Where the tension number comes from

Each physics step PhysX computes the force the joint had to apply to keep the beam inside the distance limit. Unity exposes it as `Joint.currentForce`. The cable script reads its magnitude every fixed step and stores it as the tension in newtons. When the cable is slack the constraint is inactive and the force is zero, which is correct: a slack cable carries no load.

This is the same quantity Unity compares against `breakForce`. Gauge and failure are therefore two views of one number from one constraint. There is no second formula deciding when the cable breaks.

### Reference values

| Situation | Expected reading | Why |
| --- | --- | --- |
| Hanging at rest | about 4.9 kN | `T = mg = 500 × 9.81 = 4,905 N` |
| Raising at full winch speed after the ramp | about 4.9 kN | Constant velocity: no net force beyond weight |
| During the winch ramp up (4 m/s²) | up to about 6.9 kN | `T = m(g + a) = 500 × 13.81` |
| Swinging through the bottom of an arc | above 4.9 kN | Adds the centripetal term `mv²/L` |
| Resting on the floor, taut cable | about 0 kN | The floor carries the weight |

The GDD formula `T = mg cos θ + mv²/L` is the fixed-support, fixed-length case. The engine result is the general case: it also includes support motion, hoisting, and the beam's rotation. The formula is used as a cross-check at rest and at the bottom of a swing, not as a second authority.

### Break rating

`breakForce` is 10,000 N, the GDD rating. PhysX checks it every step; when exceeded, Unity destroys the joint and calls `OnJointBreak` with the force that broke it. The script records the Broken state, raises an event, and does nothing else: the beam falls under whatever motion it had. No pose is reset.

### Shock loading

If the beam is dropped and the cable catches it, the constraint must remove the beam's downward velocity within one step. That force is `m·Δv / Δt`; for 500 kg stopped from 3 m/s in 0.02 s it is 75 kN, far above the rating. A real cable would also fail under such a snatch load, so this is expected behaviour, not a solver artefact. It is why the winch rate is bounded and why gameplay must keep the cable taut while lowering.

### What is presentation only

The HUD smooths the displayed tension with a short time constant (0.08 s) so the number is readable. The break check and the peak value use the raw per-step force. Display is in kN; physics is in N.

### Build checklist (Editor)

1. Right-click `LabRoot`, UI, Canvas. Unity creates `Canvas` and an `EventSystem`. Leave both.
2. Right-click `Canvas`, UI, Text - TextMeshPro. If a window offers **Import TMP Essentials**, click it, wait, then close the window.
3. Rename the new object `HUDText`. In its Rect Transform, click the anchor square and choose top-left (hold Shift and Alt while clicking to also set pivot and position). Set Pos X `16`, Pos Y `-16`, Width `900`, Height `160`.
4. In the TextMeshPro component set Font Size `22`, tick **Auto Size** off, and set Vertical Alignment to Top. Text can stay as is; the script overwrites it.
5. Right-click `LabRoot`, Create Empty, name it `HUD`. Add Component → **Prototype HUD**. Drag `Beam` onto Cable, `Crane` onto Crane, and `HUDText` onto Text.
6. Save the scene.

### How to check it

1. Play. With the beam on the floor the HUD shows ATTACHED (taut), tension near 0.00 kN.
2. Hold Q. Tension rises toward about 6.9 kN during the ramp, then settles near 4.9 kN while rising steadily. Let go: it stays near 4.9 kN, peak shows the ramp maximum.
3. Hold E until the beam rests on the floor and the line goes slack: state reads slack, tension 0.
4. Nudge the support (Position X to 1 and back). Tension oscillates above and below 4.9 kN with the swing, highest at the bottom of each pass.
5. Deliberate break: stop Play, select `Beam`, set Break Force N on the cable component to `6000`, Play, hold Q from the floor. The cable breaks during the ramp, the HUD reads BROKEN, the Console logs the force, the beam stays down. Stop Play; the value returns to 10,000 automatically.
6. Snatch test at the real rating: Play, hoist to about 3 m off the floor, press Space to release, and press Space again while the beam is still falling. The cable catches it and breaks. Expected: the catch force is a shock load far above 10 kN.
