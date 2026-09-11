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
- The support has no inertia response. The real crane arm and trolley will be dynamic bodies driven by motors with finite torque (Session 2), which is where the "load resists acceleration" behaviour comes from.
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
