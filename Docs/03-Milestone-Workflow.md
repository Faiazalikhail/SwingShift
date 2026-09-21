# 03 — Milestone Workflow

[Back to project index](../README.md)

## 1. Approach

- Work is organised into milestones, each with a concrete deliverable and a pass/fail gate.
- A milestone is complete only when its gate has been checked in the Editor and recorded in the progress log.
- The physics gate for the cable came first; the game is built on top of that accepted rig.
- A failed physics gate is resolved in the isolated lab rig before dependent work continues. Delivery testing is never skipped to compensate.

## 2. Milestones

| Milestone | Deliverable | Gate |
| --- | --- | --- |
| 1 — Foundation and cable proof | Configured project and working suspended-load rig | Cable hangs, swings, hoists, releases, and reports plausible tension. Accepted at instructor review. |
| 2 — Tower crane | Slew, trolley, lowering spreader, four-rope pickup of any container, elastic ropes, follow camera | Pick up from the ground, carry, lower, release; deliberate overload is testable. |
| 3 — Port scene | Quay, basin, ship with hold, tower and jib, containers with hidden mass | Every hold slot is reachable; containers stack and stay stacked. |
| 4 — Complete run | Landing grade, delivery check, score, timer, results, restart | A full shift can be completed and each ending can be reproduced. |
| 5 — Visual presentation | Materials, lighting, tension-driven cable colour, gauge bar, landing and break effects | A viewer can read load, tension, and landing quality without the debug text. |
| 6 — Tuning and usability | Readable, repeatable gameplay | Several full shifts without unexplained behaviour. |
| 7 — Build and delivery checks | Tested Windows release candidate and project handoff | Packaged game passes the delivery checklist. |

**Feature-complete point: end of milestone five.** Milestones six and seven are required work, not optional polish.

## 3. Milestone checklists

### Milestone 1 — Foundation and cable proof

- [x] Create the project, source control, and `PhysicsLab` and `Prototype` scenes.
- [x] Create a fixed support, suspended beam, and floor.
- [x] Add cable length control, release, and tension diagnostics.
- [x] Check slack behaviour, attachment initialisation, and rest tension.
- [x] Instructor review: physics proof accepted.

### Milestone 2 — Tower crane

- [x] Move the cable onto the crane so it can hook any `CableLoad`.
- [x] Add rope elasticity (soft limit) and slow slack take-up.
- [x] Lab check of the single-rope version (user confirmed).
- [x] Add jib slew and trolley travel in polar form with end stops.
- [x] Add the four-rope rig with whole-rig overload.
- [x] Add the follow camera (zoom, orbit) and the landing footprint marker.
- [ ] Play-test in the port scene; tune stiffness, damping, and rates so a careful lift of the heaviest class survives and a careless one fails.
- [ ] Save a checkpoint.

### Milestone 3 — Port scene

- [ ] Build quay, basin, water, and ship with hold by hand in `Prototype` (Physics Notes §7).
- [ ] Build the tower, jib, trolley, rope head, and spreader.
- [ ] Build the container prefab with four corner anchors and place the yard.
- [ ] Wire crane, camera, marker, and HUD.
- [ ] Verify every hold slot and the second layer are reachable.
- [ ] Save a checkpoint.

### Milestone 4 — Complete run

- [ ] Measure incoming contact speed with rotation accounted for.
- [ ] Add damage handling after the first lift.
- [x] Add hold bounds, released, and at-rest checks for delivery (`RunManager`).
- [ ] Add lost-container and knocked-off handling.
- [x] Add the 90-second timer with first-input start.
- [x] Add score, one authoritative outcome, result text, and scene-reload restart.
- [ ] Reproduce ship full, timeout, cable failure, damaged container, and lost container.
- [ ] Save a checkpoint.

### Milestone 5 — Visual presentation

- [ ] Materials and colours for quay, ship, water, crane, and container classes.
- [ ] Lighting and shadows that show load height.
- [ ] Cable colour and tension bar driven by measured tension, rating marked.
- [ ] Hook-in-reach highlight on the candidate container.
- [ ] Landing dust, impact and cable-break camera shake, floating score text.
- [ ] Replace the debug text with the player HUD.
- [ ] Save a checkpoint.

### Milestone 6 — Tuning and usability

- [ ] Tune trolley and hoist rates for a shift that leaves room for one recovery.
- [ ] Confirm cautious input completes the heaviest class at the normal rating.
- [ ] Confirm excessive movement produces a demonstrable overload.
- [ ] Balance class scores, landing multipliers, and the time bonus.
- [ ] Check readability of height, alignment, and cable state.
- [ ] Freeze features and save a checkpoint.

### Milestone 7 — Delivery

- [ ] Run the full validation matrix.
- [ ] Create the Windows release candidate.
- [ ] Test launch, a complete shift, each ending, and restart outside the Editor.
- [ ] Remove or disable development-only shortcuts in the delivered build.
- [ ] Prepare controls, editor version, physics notes, and known limitations.
- [ ] Record the final tested revision and build location.
- [ ] After this point, address only reproducible delivery blockers and re-test after each fix.

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

1. Decorative quay and ship geometry.
2. Camera shake and particle effects.
3. Floating score text.
4. Knock-on damage scoring.
5. Extra developer visualisation.

### Preserve

- Pendulum swing from trolley motion, measured tension, and cable failure.
- Elastic cable and physical release.
- Impact-graded landings and physical stacking.
- One complete scored shift with timer, result, and restart.
- A tested standalone build.

If the protected scope no longer fits, record the remaining work and request a concrete scope decision. Do not silently replace required physics with animation or deliver an untested build as complete.
