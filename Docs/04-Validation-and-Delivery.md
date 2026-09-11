# 04 — Validation and Delivery

[Back to project index](../README.md)

## 1. Test policy

Use short, repeatable playtests and targeted diagnostics. Add automated tests only where they meaningfully verify rules such as threshold boundaries, outcome precedence, or restart state. Do not create a large testing framework for this prototype.

For every failure, record the scene, starting state, input sequence, actual result, and expected result. After a repair, rerun the failing check and the directly affected integration checks.

All checks below are **not run** until implementation supplies evidence.

## 2. Physics gate — before full integration

| ID | Check | Pass condition |
| --- | --- | --- |
| P01 | Static suspension | Freely suspended 500 kg payload settles near 4.905 kN; investigate sustained deviation beyond an initial 10% diagnostic tolerance. |
| P02 | Small-angle reference | Simplified point-mass rig gives a period near 4.01 s at 4 m and 6.34 s at 10 m; measure several cycles. |
| P03 | Support movement | Moving the support creates swing without scripted beam motion. |
| P04 | Cable length | Continuous hoisting across the allowed range causes no teleportation, severe jitter, or unexplained ordinary-use break. |
| P05 | Slack and boundary | Connection permits reduced separation and constrains extension consistently in multiple directions. |
| P06 | Attach and release | Nearby attachment does not jerk the beam into place; release preserves momentum. |
| P07 | Overload | A controlled excessive load breaks the cable and the beam continues falling. |
| P08 | Arm inertia | With equal motor settings, a farther suspended load has observably slower start/stop response. |

The 10% diagnostic tolerance is proposed, not a grading requirement. Formula comparisons use the simplified rig, not the extended gameplay beam.

## 3. Gameplay validation matrix

| ID | Scenario | Expected result |
| --- | --- | --- |
| G01 | Accelerate and brake | Truck gains/loses speed gradually and can stop within the available approach. |
| G02 | Tab while moving | Crane mode is blocked with an explanatory prompt. |
| G03 | Operate crane while parked | Base remains stable; drive commands do not move it. |
| G04 | Request driving with load attached | Mode change is rejected. |
| G05 | Request driving before stowing | Unmet travel condition is shown. |
| G06 | Attach from beyond hook range | No connection or beam movement. |
| G07 | Gentle pickup | Beam clears the ground without false damage from its original resting contact. |
| G08 | Release with sideways motion | Beam retains its motion and collides naturally. |
| G09 | Enter target trigger while suspended | No success. |
| G10 | Released beam partly off pad | No success. |
| G11 | Beam briefly bounces on pad | Success waits for continuous settling. |
| G12 | Clean, supported placement | Clean success below 0.5 m/s incoming contact speed. |
| G13 | Boundary speeds | Exactly 0.5 and 1.5 m/s follow the documented rough-landing rule, allowing measurement tolerance in physical tests. |
| G14 | Rough placement | 0.5–1.5 m/s can succeed with rough result after settling. |
| G15 | Hard impact | Above 1.5 m/s fails; target contact cannot override damage. |
| G16 | Rotating beam tip strikes | Angular contribution is included in the impact evaluation. |
| G17 | Hard impact followed by soft contact | Hard impact is retained; no false clean result. |
| G18 | Cable overload | One cable-failure outcome; displayed load and break behaviour remain physically consistent. |
| G19 | Timer expires | One timeout outcome; controls stop affecting the run. |
| G20 | Completion at deadline | Recorded event timing and documented precedence produce one consistent outcome. |
| G21 | Restart from every result | Original scene, timer, mode, attachment state, and outcome restored. |
| G22 | Ten consecutive restarts | No stale references, duplicate events, or worsening behaviour. |
| G23 | Change rendering frame rate | Control rates, timer, and outcome rules remain consistent; exact physics trajectories need not be identical. |
| G24 | Full careful run | Reachable pickup-to-target route completes within 90 seconds with some correction time. |

Use controlled initial conditions for impact boundary checks; do not rely only on a human attempting to land at an exact speed.

## 4. Readability checks

- [ ] Player can identify the active mode and relevant controls.
- [ ] Hook attachment range is understandable.
- [ ] Camera shows enough ground context to judge height.
- [ ] Target remains visible during the critical landing phase.
- [ ] Tension display shows units and rated limit.
- [ ] Failure message identifies the actual reason.
- [ ] Restart prompt is visible on every result.
- [ ] Text remains readable at the intended build resolution.
- [ ] Meaning is not conveyed by colour alone.

## 5. Final Windows build checks

- [ ] Required gameplay scene is included and opens first.
- [ ] Build launches outside Unity.
- [ ] Keyboard controls work in the packaged player.
- [ ] Complete a successful run in the build.
- [ ] Verify damage, cable failure, and timeout in the build.
- [ ] Verify restart in the build.
- [ ] Inspect the player log for recurring errors.
- [ ] No missing scripts, materials, or required references.
- [ ] No development-only invulnerability, disabled timer, or altered break threshold.
- [ ] Final package is retested after the last gameplay change.

## 6. Handoff contents

| Deliverable | Required contents |
| --- | --- |
| Unity project | Assets with `.meta` files, Packages, ProjectSettings, and planning/usage notes |
| Windows build | Executable plus every generated companion file and folder required to run it |
| Usage notes | Editor version, launch instructions, controls, objective, restart |
| Physics notes | Four demonstrated systems, key values, simplifications, and known limitations |
| Test record | Tested revision/build, pass/fail results, unresolved issues |
| Optional demonstration | Short recording if requested or required by the rubric |

Exclude regenerable project folders such as Library, Temp, and Logs from the project handoff unless the course explicitly requires them. Do not omit `.meta` files. Keep the packaged Windows build separate from the editable project.

## 7. Completion record

| Field | Value |
| --- | --- |
| Tested revision | Pending |
| Editor version actually used | Unity 6000.3.23f1, Built-in render pipeline (project created 2026-09-11) |
| Final build path | Pending |
| Standalone validation date | Pending |
| Required checks passed | Pending |
| Known limitations | Pending |
| Rubric reviewed | No rubric supplied |

**Definition of done:** Required scope is implemented, relevant checks pass in the standalone build, and the handoff is complete. Code compilation alone is not completion.
