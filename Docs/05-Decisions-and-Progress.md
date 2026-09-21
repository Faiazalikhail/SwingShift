# 05 — Decisions and Progress

[Back to project index](../README.md)

## 1. Current status

| Item | Status |
| --- | --- |
| GDD reviewed | Complete |
| Workspace inspected | Complete; empty before planning documents |
| Planning documents | Complete |
| Unity project | Created 2026-09-11 with Unity 6000.3.23f1 (Built-in RP); settings, packages, folders, scenes, and git configured |
| Physics proof | Accepted at instructor review 2026-09-18 |
| Direction | Revised 2026-09-18: tower crane loading a container ship, score-attack shift (see change log) |
| Crane scripts | Single-rope version checked in `PhysicsLab`; tower crane, four-rope rig, camera, and marker written, awaiting the port scene |
| Full gameplay loop | Not started |
| Standalone build validation | Not started |

**Next action:** Milestone 4: add the `HoldZone` trigger and `RunManager` by hand, play one full shift to each outcome. Remaining Milestone 4 work after that: landing impact grade and damaged or lost containers. Windows builds are deferred until the user requests one.

## 2. Confirmed direction

| ID | Decision | Basis |
| --- | --- | --- |
| C01 | Build the prototype in Unity | User request |
| C02 | Keep development within the GDD prototype scope | User request |
| C04 | Store the plan as structured Markdown files | User request |
| C05 | One crane, beam, target, and 90-second runs | GDD |
| C06 | Preserve the visible physics systems listed in `01-Scope-and-Gameplay.md` §3 | GDD, revised 2026-09-18 |
| C07 | The cable physics proof is sufficient; build a game around it and add visual presentation | Instructor review 2026-09-18 |
| C08 | Fixed tower crane (slewing jib, trolley, hoist) replaces the truck, drive mode, and outriggers | User decision 2026-09-18 |
| C10 | Four ropes to the container's corners; container mass hidden from the HUD and found by lifting; follow camera with zoom and orbit; built structures for crane, basin, and ship | User decision 2026-09-18 |
| C09 | Port theme: load a ship with containers of different masses; they must fit, stay stacked, and land undamaged; score-attack format | User decision 2026-09-18 |

## 3. Proposed defaults

These are the current planning defaults. They are not additional claims about the GDD or instructor requirements.

| ID | Default | Reason | Review point |
| --- | --- | --- | --- |
| D01 | Use installed Unity 6000.3.23f1 | Avoid setup and upgrade overhead | Project creation; instructor version requirements |
| D02 | Start the timer on first valid gameplay input | Gives time to read controls | First full run |
| D03 | 0.5–1.5 m/s inclusive is rough but potentially successful | Fills the gap between GDD clean/fail thresholds | Landing implementation |
| D04 | Release, support, footprint, and one second of settling required | Prevents trigger-only false wins | Target testing |
| D05 | Damage applies to post-lift impacts beyond the target as well | Avoids inconsistent impact rules | Impact testing |
| D06 | R reloads the scene | Simple, consistent restart | Run integration |
| D07 | Superseded 2026-09-18 (truck removed). Outriggers lock the base | Stable support within prototype scope | Mode integration |
| D08 | Superseded 2026-09-18 (truck removed). Require a stowed crane before driving | Keeps transitions coherent | Mode integration |
| D09 | Superseded 2026-09-18 (truck removed). 8,000 kg is distributed over the vehicle assembly | Avoids double-counting component masses | Physics assembly |
| D10 | Use a single cable constraint and unloaded hook marker | Reduces joint complexity | Physics proof |
| D11 | Superseded 2026-09-18 (truck removed). Brake first, then allow reverse | Makes the small driving area recoverable | Truck playtest |
| D12 | Grade incoming normal contact speed, including rotation | Gives a concrete impact definition | Impact implementation |
| D13 | Defer Windows builds until the user requests one; an empty scene proves nothing about build-only failures | User decision 2026-09-11 | First build request |
| D14 | Beam is a 6 × 0.3 × 0.2 m box collider with mass set to 500 kg; inertia computed by Unity from the box | Envelope of a structural beam; see Physics Notes §2 | Swing and impact testing |
| D15 | Lab support is a kinematic rigidbody (ideal fixed support); the real trolley will be dynamic later | Matches the fixed-support pendulum reference used for the gate | Milestone 2 |
| D16 | Beam uses Continuous Dynamic collision detection and zero linear damping | Prevents floor tunnelling on a cable break; keeps slowing causes physical | Stability checks |
| D18 | Cable joint is owned by the trolley and connects to any `CableLoad`; empty hook is a non-simulated marker | One cable must lift many containers; keeps a single constraint (extends D10) | Piece 5 checks |
| D19 | Soft joint limit: k = 100 kN/m, c = 6 kN·s/m; slack take-up at 0.15 m/s until tension reaches 80 % of load weight; winch ramp 1 m/s² | A rigid limit turns every pickup into a shock load above the rating; see Physics Notes §6 | Piece 5 tuning |
| D20 | Rope head is the kinematic support moved with `MovePosition` from slew angle and trolley radius; end stops halt it dead | Keeps the ideal-support pendulum reference; end-stop swing is a real hazard | Trolley playtest |
| D21 | Superseded 2026-09-18 by the tower crane: containers are unconstrained 3D bodies | Two travel axes can correct any placement error | Port scene |
| D22 | Container classes 3,000 / 5,500 / 7,500 kg on a 100 kN rig rating; k = 1,000 kN/m, c = 60 kN·s/m for the whole rig | Real container masses; the GDD's 10 kN suited a 500 kg beam. Swing margins are none / about 55° / about 35° | Tuning |
| D23 | Four rope joints from a world-aligned rope head to the container's corners; overload judged on the magnitude of the summed joint forces in `FixedUpdate` | Resists tilt and yaw by geometry; a per-joint break would cascade unpredictably | Port scene checks |
| D24 | Rope head keeps a fixed world heading while the jib slews | Keeps containers square to yard and hold; equivalent to a spreader rotator | Port scene checks |
| D25 | Mass and colour are randomised independently at run start by `Container`; the HUD shows tension only | The tension gauge becomes the weighing instrument | Playtest |
| D27 | Anti-sway damper: hand-computed horizontal force `F = −2ζmω·v_rel` on the load, ζ = 0.35, scaled by the carried share of weight; crane rates lowered to slew 10°/s at 5°/s², trolley 2.5 m/s at 1.2 m/s² | Playtest 2026-09-21: undamped swing was too hard to control | Tuning |
| D26 | `PhysicsLab` is retired as of the four-rope rig; `Prototype` is the working scene | The lab proved the single cable; the rig now needs the crane around it | — |
| D17 | Scenes and levels are built by hand in the Editor; no editor scripts generate content. Runtime gameplay scripts only. | User decision 2026-09-11: keeps the level work reviewable as authored work | Ongoing |

## 4. Open questions

| Question | Why it matters | Current handling |
| --- | --- | --- |
| Additional grading rubric? | May constrain physics implementation or evidence | Do not claim rubric compliance; review when supplied. |
| Required Unity version or submission format? | May affect project creation and handoff | Use the installed editor unless requirements differ. |
| Does the course require custom physics calculations rather than engine joints? | Could change the architecture materially | Instructor accepted the joint-based proof on 2026-09-18. Keep the hand calculations in the physics notes current as the defence. |

These questions do not prevent organizing the project or preparing the first validation rig. New requirements that affect architecture must be incorporated before dependent implementation.

## 5. Risk register

| Risk | Early signal | Response |
| --- | --- | --- |
| Unstable cable | Jitter, stretch, unexplained break on attachment/hoist | Isolate rig; inspect anchors, constraints, rates, and masses before integration. |
| Stack instability | Resting containers jitter, creep, or topple without cause | Check friction material, solver iterations, sleep threshold, and plane constraints before changing masses. |
| Overload is unreachable or too frequent | Normal play always survives or always breaks | Measure forces; tune acceleration and level demands while recording parameter changes. |
| False impact grade | Hard strike registers gentle, especially at beam tips | Verify pre-impact motion and angular contribution. |
| Impossible delivery | Pickup or target exceeds reach/clearance | Validate the full path before finalizing layout. |
| Physics gate does not pass | Cable instability persists in the lab rig | Resolve in the isolated rig before dependent work; never skip build testing. |
| Build-only failure | Editor works but player fails | Build early and test again at feature completion. |
| Scope expansion | Work shifts to assets, extra systems, or polish | Compare against the scope contract; defer unrelated additions. |

## 6. Progress log

Append a short entry after each work session.

| Date / session | Completed | Evidence / checkpoint | Blocker | Next action |
| --- | --- | --- | --- | --- |
| 2026-09-11 — Planning | Reviewed GDD and organized implementation plan | Markdown documents in this repository | Rubric unconfirmed | Start Milestone 1 |
| 2026-09-11 — Milestone 1 (part 1) | Created the Unity project, applied project settings, added Input System and UGUI packages, created folder layout, `PhysicsLab` and `Prototype` scenes, Build Settings scene list, and git repository with Unity ignore rules | Initial git commit; headless editor run compiled with zero errors | None | Build and launch an initial Windows executable, then start the suspended-beam rig |
| 2026-09-11 — Milestone 1 (piece 1) | Lab rig built by hand in the Editor per `06-Physics-Notes.md` §2; physics notes started | Play test: beam rests on the floor, no jitter (user confirmed). Beam mass was found at 1 and corrected to 500. | None | Piece 2: cable joint via `CableController` |
| 2026-09-11 — Milestone 1 (piece 2) | `CableController` (ConfigurableJoint distance limit, attach from current separation, release preserves velocity, LineRenderer presentation) added to the beam by hand; physics notes §3 | Play test: slack test, hang test pass; beam hangs level, no jitter (user confirmed) | None | Piece 3: hoist and release from input |
| 2026-09-11 — Milestone 1 (piece 3) | Input Actions asset (Crane map), `PlayerInputRouter` (latched intent), `CraneController` (bounded, ramped hoist; Space attach/release); physics notes §4 | Play test: lift, lower to slack, release/reattach, drop lands on floor, nudge swing all pass (user confirmed) | None | Piece 4: tension gauge and break |
| 2026-09-11 — Milestone 1 (piece 4) | Tension from `Joint.currentForce`, 10 kN `breakForce`, `CableState`, `PrototypeHUD` (TMP); physics notes §5 | HUD reads 4.90 kN at rest (expected 4.905 kN); deliberate break at 6 kN rating and snatch test both break (user confirmed) | Hard limit makes any release-and-reattach a snatch load that breaks the cable; needs elasticity tuning | Piece 5: soft limit tuning and physics gate |
| 2026-09-18 — Direction change and Milestone 2 scripts | Instructor accepted the physics proof. Scope revised to a gantry crane loading a container ship. `CableLoad` added; `CableController` moved to the trolley with soft limit and proximity attach; `CraneController` gained trolley travel, slack take-up, and drum-end braking; input gained A/D and R; HUD shows load and trolley | Input wrapper regenerated by the Editor without errors; scripts not yet play-tested | `PhysicsLab` must be rewired by hand before play testing | Piece 5 checklist and checks, then tune |
| 2026-09-18 — Tower crane and four-rope rig | User confirmed the single-rope lab checks. Crane changed to a slewing tower crane; `CableController` now drives up to four rope joints with whole-rig overload; `CableLoad` takes corner anchors; added `Container` (hidden random mass, random colour), `CameraRig` (follow, zoom, orbit), `DropMarker` (landing footprint); HUD shows per-rope forces and no mass; masses and rating moved to real container scale | Scripts written; not yet compiled or play-tested in the Editor | Port scene must be built by hand | Physics Notes §7 checklist and checks |
| 2026-09-21 — Port scene up, anti-sway | Port scene built by hand in `Prototype`; marker transforms moved off scaled parents; pickup, lift, and carry work on the four-rope rig (user confirmed). Added the anti-sway damper and lowered crane rates; scaled-parent warning added | Play test: pickup and carry work; swing was too large before the damper | Damper and new rates not yet play-tested | Set new rates in the Inspector, test swing, then Milestone 4 |
| 2026-09-21 — Run loop | Anti-sway and new rates confirmed in play (user). Added `RunManager`: first-input start, 90 s timer, delivered count (released, centre of mass inside the hold bounds, linear and angular speed under 0.1), score with time bonus, outcomes ShipLoaded / TimeUp / RigFailed, R reloads the scene; HUD shows run status | Script written; hold zone and wiring to be added by hand | None | Wire `RunManager`, play one full shift; then landing impact grade |

## 7. Change log

| Date | Change | Reason | Affected documents |
| --- | --- | --- | --- |
| 2026-09-11 | Converted the conversational plan into linked Markdown documents and clarified defaults, acceptance gates, and delivery evidence | User requested reusable, well-structured MD files | All planning documents |
| 2026-09-11 | Recorded project creation settings: Built-in render pipeline, Input System package as the only active input handler, windowed 1600×900 resizable player, gravity −9.81 m/s², fixed step 0.02 s, Force Text serialization | Project created; these are the settings implementation now depends on | 02, 04, 05, README |
| 2026-09-18 | Old: truck-mounted crane with drive and crane modes, one beam, one target, pass/fail. New: fixed gantry crane, containers of three mass classes stacked in a ship's hold, scored 90-second shift. Retests: all cable checks repeat under the soft limit (Physics Notes §6) | Instructor accepted the physics and asked for gameplay and visual presentation; the truck added cost without adding to either | 01, 03, 05, 06 |
| 2026-09-18 | Old: gantry with one travel axis, single cable, planar containers, fixed camera, 10 kN rating. New: tower crane with slew and trolley, four-rope rig, free 3D containers with hidden mass, follow camera, 100 kN rating. Retests: all rope checks repeat on the four-rope rig | User direction after the lab check: reach in all directions, a usable camera, stable containers, and built structures | 01, 03, 05, 06 |

For later changes, record the old rule, new rule, reason, and required retests. Update the authoritative document rather than leaving contradictory versions in the log.
