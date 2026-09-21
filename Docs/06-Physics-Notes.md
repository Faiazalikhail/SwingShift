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

## 6. Piece 5 — Gantry cable: any load, elastic cable, trolley

Runtime: `CableLoad.cs` (new), `CableController.cs` (moved from the beam to the trolley), `CraneController.cs` (trolley and take-up), `PlayerInputRouter.cs` (A/D, R).

### What changed and why

**The cable now lives on the trolley.** Before, the joint was a component of the beam, so the cable could only ever hold that beam. The game needs one cable that picks up many containers. The joint is now added to the trolley with the load as its connected body. The constraint is symmetric, so the physics is unchanged; only ownership moved. `OnJointBreak` is delivered to the object that owns the joint, which is why the break handling moved with it.

**`CableLoad` marks what can be lifted.** It holds one reference, the cable anchor transform, and exposes the rigidbody and its weight `W = m·g`. Enabled loads register in a static list; the cable searches that list for an anchor within `attachRange` of the hook. No physics lives in this component.

**The empty hook is a marker.** Unattached, the hook is drawn straight below the cable origin at the permitted length. It has no rigidbody and does not swing. This keeps one constraint in the scene instead of two chained ones (decision D10). Once attached, the cable end is the load's anchor.

### Elastic cable

The hard limit from piece 4 treats the cable as perfectly rigid. Any velocity mismatch when the cable goes taut must then be removed in one 0.02 s step, and the force `m·Δv/Δt` is enormous: even lifting a resting 500 kg load at 1 m/s demands 25 kN. A real wire rope stretches, which spreads that change over time.

The joint limit is now soft. Beyond the permitted length `L` the cable force is

`F = k·x + c·ẋ`  where `x = separation − L` (the stretch), `k = 100,000 N/m`, `c = 6,000 N·s/m`.

Inside the limit the force is zero, so the cable can pull but never push.

| Quantity | Formula | 300 kg | 550 kg | 800 kg |
| --- | --- | --- | --- | --- |
| Weight | `m·g` | 2.94 kN | 5.40 kN | 7.85 kN |
| Static stretch | `m·g / k` | 2.9 cm | 5.4 cm | 7.8 cm |
| Stretch natural frequency | `(1/2π)·√(k/m)` | 2.9 Hz | 2.1 Hz | 1.8 Hz |
| Damping ratio | `c / (2·√(k·m))` | 0.55 | 0.40 | 0.34 |
| Swing angle that reaches 10 kN at the bottom | from `T = m·g·(3 − 2cos θ₀)` | never | about 55° | about 30° |

The last row is energy conservation for a pendulum released from rest at `θ₀`: `v² = 2gL(1 − cos θ₀)` at the bottom, and `T = mg + mv²/L`. It is why mass class matters to the player without any scripted rule: the same rating leaves the heavy container much less swing margin.

The stretch frequency (about 2 Hz) is well below the 50 Hz physics rate, so the spring is resolved with about 25 steps per cycle and stays stable.

### Dynamic load when lifting from rest

If the winch is already moving at speed `v` when the load leaves the ground, the load must catch up. For an undamped spring the extra force peaks at `v·√(k·m)` above the weight. At 1 m/s with 800 kg that is 8.9 kN on top of 7.85 kN: a break. So the winch takes up slack at 0.15 m/s until the tension reaches 80 % of the load's weight, which keeps the overshoot near 1.3 kN, and only then ramps to full speed at 1 m/s² (`m·a` = 0.8 kN for 800 kg).

The winch also brakes before either end of the drum using `v² = 2·a·s`: it never travels faster than it can stop in the remaining length `s`. Stopping the winch dead while raising would let the load coast up, go slack, and fall back onto the cable.

### Trolley

The trolley is the kinematic support from piece 1, now moved each fixed step with `Rigidbody.MovePosition` along world X. `MovePosition` tells PhysX the body's velocity for that step, so the joint sees a moving support rather than a teleport. The commanded velocity ramps at 2 m/s² to 3 m/s. The load is not driven sideways by any script: it lags the accelerating support, the cable tilts, and the horizontal component of tension accelerates it. That lag is the swing.

At the rail ends the trolley stops at once. The load keeps its momentum and swings out, as it would against a real buffer.

A kinematic trolley has infinite effective mass: the load cannot pull it back. For a gantry trolley that outweighs the load and is driven by a geared motor this is a reasonable idealisation, and it keeps the pendulum reference case exact.

### What this piece does not claim

- The empty hook has no dynamics.
- The cable has no mass, no sag, and no bending; it is a straight massless spring-damper in tension only.
- `k` and `c` are chosen for stable, readable behaviour at the game's scale, not measured from a specific wire rope.

### Build checklist (Editor, `PhysicsLab`)

1. Select `Beam`. On the old **Cable Controller** component click the three dots → Remove Component. Do the same for its **Line Renderer**.
2. With `Beam` selected, Add Component → **Cable Load**. Drag `CableAnchor` onto Cable Anchor.
3. Select `Support`. Add Component → **Line Renderer**: Width `0.05`, material `Greybox_Cable`, Cast Shadows off.
4. Right-click `Support` → 3D Object → Cube, name it `HookVisual`. Scale `0.3, 0.3, 0.3`. Remove its **Box Collider**. Give it the `Greybox_Cable` material.
5. With `Support` selected, Add Component → **Cable Controller**. Cable Origin → `HookPoint`, Hook Visual → `HookVisual`, Cable Line → drag `Support` itself. Set Min Length `2`, Max Length `12`, Start Length `6`. Leave Break Force `10000`, Attach Range `0.6`, Stiffness `100000`, Damping `6000`.
6. Select `Crane`. On **Crane Controller**: Cable → `Support`, Trolley → `Support`. Set Hoist Acceleration `1`, Take Up Speed `0.15`, Rail Min X `-12`, Rail Max X `12`. With `Crane` selected a yellow line in the Scene view shows the rail.
7. Select `PrototypeHUD`. On **Prototype HUD** drag `Support` onto Cable. In `HUDText` set Height to `220`.
8. Duplicate `Beam` twice for mass tests: name them `Load300` and `Load800`, move them to X `-8` and X `8`, set Rigidbody Mass `300` and `800`.
9. Save the scene.

### How to check it

1. Play. The hook cube hangs 6 m below the support. Hold E: it lowers. The HUD shows **none in reach**.
2. Lower until the hook is at the beam's anchor. The HUD shows **in reach: 500 kg**. Press Space: ATTACHED, tension near 0.
3. Hold Q. The HUD shows **taking up slack**, tension climbs smoothly to about 4.9 kN, the beam lifts, then the winch speeds up. Peak should stay under about 6.5 kN. Rest reading: 4.90 kN. This confirms the soft limit reports the same force as before.
4. Hold D for two seconds and let go. The beam lags, then swings. Tension is highest at the bottom of each pass.
5. Run into a rail end at full speed. The trolley stops dead; the beam swings out.
6. Release with Space over open floor, move the hook to `Load800`, hook it, and lift it with Q held. It must survive a careful straight lift. Peak should be under 10 kN.
7. With `Load800` hanging, drive D at full speed into the rail end. Expect a swing large enough to break the cable or come close. Repeat with `Load300`: it should be safe.
8. Hoist fully up with Q held. The winch slows before the top; tension shows no spike.

## 7. Piece 6 — Tower crane, four-rope rig, port scene

Runtime: `CraneController.cs` (slew, trolley, hoist), `CableController.cs` (up to four ropes), `CableLoad.cs` (corner anchors), `Container.cs`, `CameraRig.cs`, `DropMarker.cs`. Scene: `Prototype`. `PhysicsLab` is retired from this piece on; section 6 remains the record of the single-rope checks.

### Slew and trolley

The rope head's position is held in polar form about the tower axis: slew angle `ψ` (yaw, 0 along world +Z) and trolley radius `r`. Each fixed step both rates are ramped toward the commanded values, integrated, clamped at their end stops, and converted:

`x = x₀ + r·sin ψ`  `z = z₀ + r·cos ψ`  `y = head height`

The result goes to `Rigidbody.MovePosition`, so PhysX knows the head's velocity during the step. The head's speed over the ground from slewing alone is `v = ω·r`: at 15°/s (0.26 rad/s) and 20 m radius that is 5.2 m/s, faster than the 3 m/s trolley. Slewing at long radius is therefore the most violent move available to the player, and running the trolley in before slewing is the safe technique.

While slewing at a steady rate the load needs a centripetal force `m·ω²·r` toward the tower to stay on the circle. Only the ropes can supply it, so the load hangs outward at an angle `tan φ = ω²·r / g`. At 0.26 rad/s and 20 m that is about 8°. When the slew stops, that offset becomes a radial swing on top of the tangential one.

Only the rope head is a physics body. The jib and trolley models are posed each frame from the head's interpolated position, so they cannot disagree with the physics.

### Four ropes

One rope to the top centre leaves the container free to tilt and spin. Four ropes to the four top corners remove that freedom by geometry, not by an angular constraint: tilting the container would lengthen the ropes on the rising side beyond their limit, so they pull it back level; yawing it would skew all four ropes and lengthen them, so they pull it back square.

The rope head keeps a fixed world heading while the jib slews (a spreader on a rotator). The four rope origins are laid out in the same 2.2 × 5.6 m rectangle as the container's anchors, so the ropes hang parallel. Parallel ropes of equal length form a parallelogram linkage: the container translates on a circular arc without rotating, and its swing period is that of a simple pendulum of the rope length, `T = 2π√(L/g)` (7.8 s at 15 m).

Each rope is one soft distance-limit joint with a quarter of the rig's stiffness and damping. Springs in parallel add, so the rig as a whole keeps `k = 1,000 kN/m`, `c = 60 kN·s/m`.

On attach, each origin pairs with its nearest free anchor in plan view. The common permitted length starts at the longest of the four, so no joint starts stretched and none applies an impulse.

### Tension and overload

Each joint reports its constraint force as a vector (`Joint.currentForce`). The ropes are not exactly parallel once the load swings, so the load on the rig is the magnitude of the vector sum, not the sum of magnitudes. That is what a load cell at the rope head would read, and it is the HUD value. The four individual magnitudes are shown as well: equal at rest (`m·g/4`), unequal when the load is tilted or was hooked off-centre.

Overload is judged on that net value in `FixedUpdate`: above the 100 kN rating, all four joints are destroyed and the state becomes Broken. The joints' own `breakForce` is infinite. A per-joint rating would let one rope fail, shift its load to the other three, and cascade within a few steps, which is realistic but unreadable for a player.

### Masses and the weighing mechanic

| Quantity | Formula | 3,000 kg | 5,500 kg | 7,500 kg |
| --- | --- | --- | --- | --- |
| Weight = tension at rest | `m·g` | 29.4 kN | 54.0 kN | 73.6 kN |
| Per rope at rest | `m·g / 4` | 7.4 kN | 13.5 kN | 18.4 kN |
| Static stretch | `m·g / k` | 2.9 cm | 5.4 cm | 7.4 cm |
| Damping ratio | `c / (2·√(k·m))` | 0.55 | 0.40 | 0.35 |
| Take-up overshoot at 0.15 m/s | `v·√(k·m)` | 8.2 kN | 11.1 kN | 13.0 kN |
| Winch ramp at 1 m/s² | `m·a` | 3.0 kN | 5.5 kN | 7.5 kN |
| Swing release angle that reaches 100 kN at the bottom | `T = m·g·(3 − 2cos θ₀)` | never | about 55° | about 35° |

`Container` draws the mass and the paint colour independently at run start, so colour carries no information. The HUD never prints mass. Once the container hangs still the tension equals its weight, so the player reads 29, 54, or 74 kN and knows what they are carrying and how much margin is left.

The winch controller does use the load's true weight, to decide when slack take-up is over (tension above 80 % of weight). That is a property of the crane's control system, not information given to the player.

### Anti-sway damper

With nothing but air around it, a pendulum on 15 m ropes keeps swinging for minutes, and a player cannot place a container that way. Harbour cranes solve this with an anti-sway system. Here it is modelled as a viscous damper between the rope head and the load, computed by hand in `CableController.ApplyAntiSway` every fixed step:

1. Head velocity from its own displacement, because the head is kinematic: `v_head = Δx / Δt`.
2. Relative horizontal velocity of the load: `v_rel = v_load − v_head`, with the vertical component set to zero.
3. Pendulum natural frequency for the current rope length: `ω = √(g / L)`.
4. Damping coefficient for a chosen damping ratio `ζ`, from `ζ = c / (2·m·ω)`: `c = 2·ζ·m·ω`.
5. Force on the load: `F = −c · v_rel · (T / m·g)`, clamped so the last factor stays between 0 and 1.

Because `c` scales with mass, every container class settles at the same rate. With `ζ = 0.35` a swing loses about 90 % of its amplitude in one cycle (`e^(−2πζ/√(1−ζ²)) ≈ 0.10`). At 15 m, `ω = 0.81 rad/s`, so for 7,500 kg `c ≈ 4,250 N·s/m`, and a 2 m/s relative velocity draws about 8.5 kN.

The factor `T / m·g` is the share of the weight the ropes carry. The damper acts through the ropes, so it fades out as a container is set down and does nothing to one resting on the ground.

The force is horizontal, so it does not enter the rope tension directly; it lowers tension indirectly by reducing swing speed and therefore the `m·v²/L` term. The HUD shows its magnitude so its effect is never hidden.

What it does not claim: a real system damps sway by accelerating the trolley, and the reaction goes into the crane. Here the head is an ideal support, so the reaction is not modelled.

Crane rates were lowered with it: slew 10°/s at 5°/s², trolley 2.5 m/s at 1.2 m/s². Swing amplitude from a velocity change `Δv` of the head is about `Δv/√(g·L)` radians when the change is fast compared with the swing period, so halving the head's acceleration and top speed directly reduces the swing the player has to manage.

### Camera and footprint marker

Both are presentation only and run in `LateUpdate`. The camera follows a smoothed point between the rope head and the load, turns with the jib so W is always away from the tower on screen, zooms by a fixed fraction of the current distance per wheel notch, and orbits while the right mouse button is held.

The marker raycasts straight down from the load's centre of mass, ignores the load itself, and lays a container-sized quad on the first surface hit. It reads the scene; it writes nothing to physics.

### What this piece does not claim

- The jib, mast, and trolley have no mass or flexibility; the rope head is an ideal support.
- The rotator that keeps the head's heading is assumed, not simulated.
- The ship does not float or heel; it is a static collider. Water is a visual plane.
- Ropes are massless straight spring-dampers in tension only.

### Scene values (`Prototype`, built by hand)

Project Settings → Physics: Default Solver Iterations `12`, Default Solver Velocity Iterations `4`.

| Object (parent) | Type | Position | Scale | Notes |
| --- | --- | --- | --- | --- |
| Quay (Environment) | Cube | −15, −5, 0 | 50, 10, 80 | top at y = 0, edge at x = 10 |
| BasinFloor (Environment) | Cube | 30, −10.5, 0 | 40, 1, 80 | catches dropped containers |
| BasinWallFar / N / S (Environment) | Cube | 50.5, −5, 0 / 30, −5, 40.5 / 30, −5, −40.5 | 1, 10, 80 / 40, 10, 1 / 40, 10, 1 | |
| Water (Environment) | Plane | 30, −1.5, 0 | 4, 1, 8 | Mesh Collider removed |
| Ship | Empty | 20, 0, 0 | 1, 1, 1 | |
| Hull_Floor (Ship) | Cube | 0, −4.5, 0 | 9, 1, 40 | hold floor top at y = −4 |
| Hull_SideQuay / Hull_SideSea (Ship) | Cube | ∓4, −1.5, 0 | 1, 5, 40 | rim at y = 1 |
| Hull_Bow / Hull_Stern (Ship) | Cube | 0, −1.5, ±13.5 | 7, 5, 13 | hold is 7 × 14 m, 5 m deep |
| Bridge (Ship) | Cube | 0, 4, −16 | 7, 6, 5 | |
| Crane | Empty | 0, 0, 0 | 1, 1, 1 | `CraneController`, `DropMarker` |
| Mast (Crane) | Cube | 0, 10, 0 | 2, 20, 2 | |
| JibPivot (Crane) | Empty | 0, 19, 0 | 1, 1, 1 | rotated by script |
| Jib (JibPivot) | Cube | 0, 0.5, 12 | 1.2, 1, 36 | collider removed |
| Counterweight (JibPivot) | Cube | 0, −0.5, −5 | 2.5, 2, 3 | collider removed |
| TrolleyVisual (JibPivot) | Cube | 0, −0.3, 16 | 1.6, 0.6, 2 | collider removed |
| RopeHead (Crane, not JibPivot) | Empty | 0, 18, 16 | 1, 1, 1 | Rigidbody: kinematic, Interpolate; `CableController` |
| HeadFrame (RopeHead) | Cube | 0, 0, 0 | 2.4, 0.3, 5.8 | collider removed |
| RopeOrigin_0–3 (RopeHead) | Empty | ±1.1, −0.2, ±2.8 | | |
| Rope_0–3 (RopeHead) | Empty + Line Renderer | 0, 0, 0 | | width 0.06, `Greybox_Cable` |
| Spreader (Crane) | Cube | any | 2.5, 0.25, 6 | collider removed; posed by script |
| LandingMarker (Crane) | Quad | any | 2.5, 6, 1 | Mesh Collider removed; transparent material |
| Container prefab root | Empty | y = 1.25 | 1, 1, 1 | Rigidbody (Interpolate, Continuous Dynamic, damping 0), Box Collider 2.5 × 2.5 × 6, `CableLoad`, `Container` |
| Body (Container) | Cube | 0, 0, 0 | 2.5, 2.5, 6 | collider removed |
| Anchor_0–3 (Container) | Empty | ±1.1, 1.25, ±2.8 | | |
| Yard | 8 containers | x ∈ {−6, −2, 2, 6}, z ∈ {15, 22}, y = 1.25 | | all inside 6–28 m radius and −30° to 135° slew |

`CableController`: Min `3`, Max `22`, Start `10`, Break Force `100000`, Attach Range `0.8`, Stiffness `1000000`, Damping `60000`. `CraneController`: defaults.

Reach check: the hold centre is 20 m from the tower at 90° slew, its far corner 24.5 m. The head is at y = 18; a container top on the quay is 15.3 m below the origins, on the hold floor 19.3 m, both inside the 3–22 m rope range.

### How to check it

1. Play. The spreader hangs 10 m under the head on four vertical lines. A/D slews the jib, W/S runs the trolley, and the head frame keeps its heading. The camera follows and turns with the jib; wheel zooms; right mouse orbits.
2. Put the yellow footprint on a yard container, lower with E until the HUD shows **container in reach**, press Space.
3. Hold Q. Take-up, then lift. At rest the tension reads about 29, 54, or 74 kN and the four per-rope values are nearly equal. Record the peak for a 74 kN container; it must stay under 100 kN on a straight lift.
4. Slew at full rate at long radius and stop. The container swings but stays level and square. Tension peaks at the bottom of each pass.
5. Run the trolley in to about 8 m and repeat the slew: the swing is much smaller (`v = ω·r`).
6. Place a container on the hold floor using the marker, release, and stack a second on top. The stack must rest without creeping or jitter.
7. With a 74 kN container at long radius, slew hard into the end stop. Expect the rig to fail or come close.
8. Release a container from height over the basin: it falls through the water plane and rests on the basin floor.
