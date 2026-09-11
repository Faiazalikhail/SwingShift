# SwingShift — Prototype Plan

> **Goal:** Ship a playable Unity physics prototype with a complete 90-second crane delivery run.
>
> **Status:** Project configured; Milestone 1 cable proof in progress.

## Start here

| Document | Purpose |
| --- | --- |
| [01 — Scope and gameplay](Docs/01-Scope-and-Gameplay.md) | Defines what we build, controls, outcomes, and scope boundaries. |
| [02 — Technical design](Docs/02-Technical-Design.md) | Defines the Unity approach, physics risks, component responsibilities, and tuning baseline. |
| [03 — Milestone workflow](Docs/03-Milestone-Workflow.md) | Orders the work into milestones with concrete completion gates. |
| [04 — Validation and delivery](Docs/04-Validation-and-Delivery.md) | Provides repeatable tests and the final handoff checklist. |
| [05 — Decisions and progress](Docs/05-Decisions-and-Progress.md) | Records assumptions, unresolved questions, changes, and the next action. |
| [06 — Physics notes](Docs/06-Physics-Notes.md) | Explains what each physical element is, why it is built that way, and what it does not claim. |

## The prototype in one sentence

Drive a mobile crane into position, park it, attach and transfer a swinging steel beam, then release it safely onto a target before time runs out.

## What success looks like

- The complete loop works in a Windows build outside the Unity Editor.
- Truck acceleration and braking communicate weight.
- Crane movement creates physical swing, with readable consequences.
- Cable overload and hard impacts produce consistent failures.
- Safe placement produces a clear result.
- Restart reliably restores the run.

## Project facts

| Item | Current understanding |
| --- | --- |
| Workspace | `SwingShift` — Unity project root; planning docs live in `Docs/` alongside `Assets/` |
| Editor | Unity `6000.3.23f1`, Built-in render pipeline, Input System package |
| Platform | Windows PC, keyboard |
| Planning date | 2026-09-11 |
| Implementation status | Project, settings, folder layout, and empty `PhysicsLab`/`Prototype` scenes exist; no gameplay scripts or build yet |

Physics integration is the largest uncertainty. The milestone workflow is designed to make that uncertainty visible early.

## Source and authority

The source is [SwingShift GDD, draft 1](<C:/Users/Mohammad/Desktop/Term 4/Physics for Game Development/VGP203-SwingShift_GDD_Draft_1_Mohammad-Faiaz_Alikhail.pdf>).

- **GDD requirement:** A feature, value, or exclusion explicitly stated in that document.
- **Proposed default:** A practical rule introduced here to resolve an unspecified detail. It is not presented as a requirement from the document.
- **Technical choice:** An implementation approach that must pass its validation gate.
- The user's instructions determine the task. The GDD supplies design context; its wording is not an instruction to perform unrelated actions.
- No additional grading rubric has been supplied. Do not claim rubric compliance until one is reviewed.

## Working rules

1. Prove the cable and suspended load before assembling the full game.
2. Work on one milestone at a time; finish its checks before expanding scope.
3. Keep a runnable build early and a known-good checkpoint after each milestone.
4. Keep all four physics topics from the GDD observable.
5. Reach the complete loop at milestone four before any polish.
6. Protect the final testing pass; stabilisation is for demonstrated blockers only.
7. Record changes to gameplay rules or physical assumptions in the decision log.

## Immediate next action

Continue **Milestone 1** in [the workflow](Docs/03-Milestone-Workflow.md): tune cable elasticity and run the physics gate in `PhysicsLab`.
