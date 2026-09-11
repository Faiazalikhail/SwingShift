# 03 — Milestone Workflow

[Back to project index](../README.md)

## 1. Approach

- Work is organised into milestones, each with a concrete deliverable and a pass/fail gate.
- A milestone is complete only when its gate has been checked in the Editor and recorded in the progress log.
- The physics gate for the cable comes first; nothing is built on top of it until it passes.
- A failed physics gate is resolved in the isolated lab rig before dependent work continues. Delivery testing is never skipped to compensate.

## 2. Milestones

| Milestone | Deliverable | Gate |
| --- | --- | --- |
| 1 — Foundation and cable proof | Configured project and working suspended-beam rig | Cable hangs, swings, hoists, releases, and reports plausible tension. |
| 2 — Complete lifting assembly | Fixed-base crane with arm, trolley, attachment, and break handling | Pick up, transfer, lower, release; deliberate overload is testable. |
| 3 — Truck and mode integration | Drive, park, operate, return to travel configuration | No simultaneous driving and lifting; camera supports both modes. |
| 4 — Complete run | Target, impact grade, timer, results, restart | Full run can succeed and each failure can be reproduced. |
| 5 — Tuning and usability | Readable, repeatable gameplay | Several full runs without unexplained behaviour. |
| 6 — Build and delivery checks | Tested Windows release candidate and project handoff | Packaged game passes the delivery checklist. |
| 7 — Stabilisation | Repairs for demonstrated blockers only | Final build remains tested after fixes. |

**Feature-complete point: end of milestone four.** Milestones five and six are required work, not optional polish.

## 3. Milestone checklists

### Milestone 1 — Foundation and cable proof

- [x] Create a basic 3D project using the selected installed editor.
- [x] Confirm the Windows build module is available; resolve setup blockers immediately.
- [x] Set up source control, Unity ignore rules, and readable asset serialization.
- [x] Create `PhysicsLab` and `Prototype` scenes.
- [x] Create a fixed support, simple suspended body, beam, and floor.
- [x] Add cable length control, release, and tension diagnostics.
- [x] Check slack behaviour, attachment initialization, and rest tension.
- [ ] Tune cable elasticity so brief catches do not break the cable at the rating.
- [ ] Run the physics gate in the validation document, including the pendulum period check.
- [ ] Save a known-good checkpoint.

**Stop condition:** If ordinary hoisting or attachment produces unresolved instability, keep working in the isolated rig. Record the exact reproduction steps before starting truck work.

### Milestone 2 — Lifting assembly

- [ ] Add powered arm rotation and constrained trolley travel.
- [ ] Add hook proximity feedback and valid attachment handling.
- [ ] Add bounded hoist speed and smooth motor commands.
- [ ] Connect tension display and cable-break outcome.
- [ ] Confirm near/far load inertia is observable.
- [ ] Verify pickup and target positions are reachable from one base position.
- [ ] Perform a complete transfer with the base fixed.
- [ ] Save a checkpoint.

### Milestone 3 — Truck and modes

- [ ] Add chassis, wheels, steering, throttle, and braking.
- [ ] Check the assembly mass and centre of mass.
- [ ] Add grounded/stopped parking checks and simple outriggers.
- [ ] Route input exclusively to the active mode.
- [ ] Define and display travel-readiness requirements.
- [ ] Add driving and crane camera framing.
- [ ] Drive to the pickup, park, attach, and lift.
- [ ] Save a checkpoint.

### Milestone 4 — Complete run

- [ ] Measure incoming contact speed with rotation accounted for.
- [ ] Add damage handling after the initial lift.
- [ ] Add target support, footprint, release, and settling checks.
- [ ] Add the 90-second timer and first-input start.
- [ ] Add one authoritative success/failure outcome.
- [ ] Add clear result text and scene-reload restart.
- [ ] Reproduce success, rough placement, cable failure, damage, and timeout.
- [ ] Create a feature-complete build and save a checkpoint.

### Milestone 5 — Tuning and usability

- [ ] Test braking distance and low-speed steering.
- [ ] Tune arm/trolley acceleration and hoist speed.
- [ ] Confirm cautious input permits safe completion at the normal break rating.
- [ ] Confirm excessive movement can produce a demonstrable overload.
- [ ] Tune the yard and target for a careful run with recovery time.
- [ ] Check visibility of beam height, target alignment, and cable state.
- [ ] Make blocked actions understandable through short prompts.
- [ ] Freeze features and save a checkpoint.

### Milestone 6 — Delivery

- [ ] Run the full validation matrix.
- [ ] Create the Windows release candidate.
- [ ] Test launch, complete run, failures, and restart outside the Editor.
- [ ] Remove or disable development-only shortcuts in the delivered build.
- [ ] Prepare controls, editor version, physics notes, and known limitations.
- [ ] Verify the project and build packages are complete.
- [ ] Record the final tested revision and build location.

### Milestone 7 — Stabilisation

- [ ] Address only reproducible delivery blockers.
- [ ] Re-run affected tests after each fix.
- [ ] Rebuild and smoke-test the actual final package.
- [ ] Avoid new features, dependencies, and engine upgrades.

## 4. How each work session runs

1. Open the last known-good state and confirm it still runs.
2. Select one milestone outcome and its acceptance checks.
3. Implement in small batches, checking compilation and scene references as work proceeds.
4. Playtest the changed behaviour in the smallest useful scene.
5. Run the relevant integrated check.
6. Save a working checkpoint and update the progress log.

End each session with a concrete next action, any blocker, and the current build status. Avoid ending with unrecorded tuning experiments.

## 5. Division of work

| Assistant | Author |
| --- | --- |
| Draft code and supporting notes | Build scenes and levels in the Editor; supply the rubric if available |
| Inspect diagnostics and repair reproducible bugs | Give short feedback on control feel and clarity |
| Maintain checks and planning records | Test the build on the intended submission machine |
| Prepare build and handoff instructions | Make any required course submission |

Do not mark a build or playtest as passed without evidence from that environment. Where an Editor interaction is needed, provide one short, exact setup checklist and continue independent work.

## 6. Scope priority policy

### Simplify first

1. Decorative yard geometry.
2. Camera transitions and presentation effects.
3. Outrigger movement animation.
4. Numerical score beyond clean/rough result and remaining time.
5. Extra developer visualization.

### Preserve

- The four physics topics.
- Adjustable cable and physical release.
- Separate drive/crane modes.
- One complete pickup-to-placement run.
- Tension failure, impact result, timer, and restart.
- A tested standalone build.

If the protected scope no longer fits, record the remaining work and request a concrete scope decision. Do not silently replace required physics with animation or deliver an untested build as complete.
