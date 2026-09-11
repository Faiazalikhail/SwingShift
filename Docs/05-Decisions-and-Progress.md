# 05 — Decisions and Progress

[Back to project index](../README.md)

## 1. Current status

| Item | Status |
| --- | --- |
| GDD reviewed | Complete |
| Workspace inspected | Complete; empty before planning documents |
| Planning documents | Complete |
| Unity project | Created 2026-09-11 with Unity 6000.3.23f1 (Built-in RP); settings, packages, folders, scenes, and git configured |
| Physics proof | Not started |
| Full gameplay loop | Not started |
| Standalone build validation | Not started |

**Next action:** Session 1, piece 2: cable connection (ConfigurableJoint from beam anchor to support). Windows builds are deferred until the user requests one.

## 2. Confirmed direction

| ID | Decision | Basis |
| --- | --- | --- |
| C01 | Build the prototype in Unity | User request |
| C02 | Keep development within the GDD prototype scope | User request |
| C03 | Plan for one week around work, study, and other projects | User clarification |
| C04 | Store the plan as structured Markdown files | User request |
| C05 | One crane, beam, target, and 90-second runs | GDD |
| C06 | Preserve the four visible physics systems | GDD |

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
| D07 | Outriggers lock the base | Stable support within prototype scope | Mode integration |
| D08 | Require a stowed crane before driving | Keeps transitions coherent | Mode integration |
| D09 | 8,000 kg is distributed over the vehicle assembly | Avoids double-counting component masses | Physics assembly |
| D10 | Use a single cable constraint and unloaded hook marker | Reduces joint complexity | Physics proof |
| D11 | Brake first, then allow reverse | Makes the small driving area recoverable | Truck playtest |
| D12 | Grade incoming normal contact speed, including rotation | Gives a concrete impact definition | Impact implementation |
| D13 | Defer Windows builds until the user requests one; an empty scene proves nothing about build-only failures | User decision 2026-09-11 | First build request |
| D14 | Beam is a 6 × 0.3 × 0.2 m box collider with mass set to 500 kg; inertia computed by Unity from the box | Envelope of a structural beam; see Physics Notes §2 | Swing and impact testing |
| D15 | Lab support is a kinematic rigidbody (ideal fixed support); the real trolley will be dynamic later | Matches the fixed-support pendulum reference used for the gate | Session 2 |
| D16 | Beam uses Continuous Dynamic collision detection and zero linear damping | Prevents floor tunnelling on a cable break; keeps slowing causes physical | Stability checks |
| D17 | Scenes and levels are built by hand in the Editor; no editor scripts generate content. Runtime gameplay scripts only. | User decision 2026-09-11: keeps the level work reviewable as authored work | Ongoing |

## 4. Open questions

| Question | Why it matters | Current handling |
| --- | --- | --- |
| Exact deadline and submission time? | Determines final freeze and packaging time | Use the relative seven-day schedule. |
| Actual available focused hours? | Determines feasibility and session sizes | Use provisional 11.5 h baseline plus 2 h reserve. |
| Additional grading rubric? | May constrain physics implementation or evidence | Do not claim rubric compliance; review when supplied. |
| Required Unity version or submission format? | May affect project creation and handoff | Use the installed editor unless requirements differ. |
| Does the course require custom physics calculations rather than engine joints? | Could change the architecture materially | Surface this early if a rubric specifies it. |

These questions do not prevent organizing the project or preparing the first validation rig. New requirements that affect architecture must be incorporated before dependent implementation.

## 5. Risk register

| Risk | Early signal | Response |
| --- | --- | --- |
| Unstable cable | Jitter, stretch, unexplained break on attachment/hoist | Isolate rig; inspect anchors, constraints, rates, and masses before integration. |
| Arm feels weightless | Near and far loads accelerate identically | Check finite motor torque and physical coupling. |
| Overload is unreachable or too frequent | Normal play always survives or always breaks | Measure forces; tune acceleration and level demands while recording parameter changes. |
| False impact grade | Hard strike registers gentle, especially at beam tips | Verify pre-impact motion and angular contribution. |
| Impossible delivery | Pickup or target exceeds reach/clearance | Validate the full path before finalizing layout. |
| Session estimate exceeded | Physics gate not passed within initial budget | Re-estimate immediately; preserve build-testing time. |
| Build-only failure | Editor works but player fails | Build on day one and test again at feature completion. |
| Scope expansion | Work shifts to assets, extra systems, or polish | Compare against the scope contract; defer unrelated additions. |

## 6. Progress log

Append a short entry after each work session.

| Date / session | Completed | Evidence / checkpoint | Blocker | Next action |
| --- | --- | --- | --- | --- |
| 2026-09-11 — Planning | Reviewed GDD and organized implementation plan | Markdown documents in this repository | Actual available hours and rubric unconfirmed | Start Session 1 when requested |
| 2026-09-11 — Session 1 (part 1) | Created the Unity project, applied project settings, added Input System and UGUI packages, created folder layout, `PhysicsLab` and `Prototype` scenes, Build Settings scene list, and git repository with Unity ignore rules | Initial git commit; headless editor run compiled with zero errors | None | Build and launch an initial Windows executable, then start the suspended-beam rig |
| 2026-09-11 — Session 1 (piece 1) | Lab rig specified (checklist in `06-Physics-Notes.md` §2) and physics notes started | Rig placed by hand in the Editor; scene saved | None | Piece 2: cable joint and CableController |

## 7. Change log

| Date | Change | Reason | Affected documents |
| --- | --- | --- | --- |
| 2026-09-11 | Converted the conversational plan into linked Markdown documents and clarified defaults, acceptance gates, and delivery evidence | User requested reusable, well-structured MD files | All planning documents |
| 2026-09-11 | Recorded project creation settings: Built-in render pipeline, Input System package as the only active input handler, windowed 1600×900 resizable player, gravity −9.81 m/s², fixed step 0.02 s, Force Text serialization | Project created; these are the settings implementation now depends on | 02, 04, 05, README |

For later changes, record the old rule, new rule, reason, and required retests. Update the authoritative document rather than leaving contradictory versions in the log.
