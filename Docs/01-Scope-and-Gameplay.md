# 01 — Scope and Gameplay

[Back to project index](../README.md)

## 1. Deliverable

A single-player Unity game about loading a container ship with a quayside tower crane. The player slews the jib, runs the trolley, hoists containers of unknown mass on a four-rope rig, carries them over the ship, and stacks them in the hold before the 90-second shift ends. Launch directly into gameplay, show a scored result with a specific failure reason where one applies, and allow immediate restart.

Revised 2026-09-18 after instructor review: the cable physics proof was accepted as sufficient, with the direction to build a game around it and add visual presentation. The truck and drive mode were removed in favour of a fixed tower crane with a slewing jib (see `05-Decisions-and-Progress.md`, change log).

## 2. Scope contract

| Included | Basis |
| --- | --- |
| Tower crane: jib slew, trolley travel along the jib, adjustable hoist | Revised scope |
| Four-rope rig to the container's top corners | Revised scope |
| Follow camera with zoom and orbit; landing footprint marker | Revised scope |
| Physical container swing, release, and stacking | GDD physics, revised theme |
| Containers of three hidden mass classes on one rig rating | Revised scope |
| Cable tension gauge and overload failure | GDD |
| Landing speed readout and impact-dependent grade | GDD |
| 90-second shift timer, score, results, R to restart | GDD, revised format |
| Themed presentation: quay, ship, coloured containers, physics-driven feedback | Instructor direction |

### Exclusions

- Truck, drive mode, and outriggers.
- Story, characters, additional levels, wind, weather, and moving water physics.
- Menus, saving, leaderboards, and progression.
- Cable wrapping, rope-link chains, and a simulated empty hook.
- Third-party asset dependencies unless they solve a demonstrated blocker.

These boundaries simplify implementation; they must not remove the physics behaviours in section 3.

## 3. Physics the game is built on

| Behaviour | Where the player sees it |
| --- | --- |
| Pendulum motion from a moving support | Slewing swings the load tangentially (head speed `v = ω·r`), trolley travel swings it radially; together they make a two-axis pendulum. |
| Four-rope suspension | The container stays level and keeps its heading; per-rope forces show uneven loading. |
| Cable tension `T = mg cos θ + mv²/L`, plus hoist and support acceleration | Gauge rises at the bottom of each swing; heavy containers leave little margin under the rating. The gauge is also the only way to learn a container's mass. |
| Elastic cable and shock loading | Taking up slack gently versus snatching a load. |
| Impact speed and momentum | Landing grade; hard landings damage the container. |
| Rigid-body stacking and stability | Containers must rest supported; a poorly placed one slides or topples off the stack. |

## 4. Player loop

1. Slew and trolley the spreader over a container on the quay and lower it onto the container's top.
2. Press Space to hook the four corners, then hoist. The tension reading reveals how heavy it is.
3. Carry it to the ship while managing the swing; heavier containers need gentler moves.
4. Lower it into the hold or onto another container, using the footprint marker to line it up.
5. Release and let it settle; it scores once it is at rest on the ship.
6. Return for the next container. Repeat until the ship is full or the shift ends.
7. Read the result and press R to retry.

## 5. Controls

| Input | Action |
| --- | --- |
| A / D | Slew jib left / right |
| W / S | Trolley out / in |
| Q / E | Hoist up / down |
| Space | Hook / release |
| Mouse wheel | Zoom |
| Right mouse button + move | Orbit camera |
| R | Restart |

## 6. Rules

| Situation | Rule |
| --- | --- |
| Degrees of freedom | Containers are unconstrained rigid bodies in three dimensions. |
| Rope head | The rope head follows the trolley but keeps a fixed world heading while the jib slews, like a spreader on a rotator, so containers stay square to the hold. |
| Spreader | The empty spreader is a marker straight below the rope head at the permitted rope length. It is not a simulated body. |
| Attach | The spreader must be within a small configurable distance of the centre of a container's four corner anchors. Each rope pairs with its nearest corner. The container is never teleported. |
| Release | The connection is removed and the container keeps its velocity. |
| Container classes | 3,000, 5,500, and 7,500 kg against a 100 kN rig rating, drawn at random per container. Colour is random and unrelated to mass, and the HUD never states mass: the player reads it from the tension after lifting. Heavier containers score more. |
| Start timer | The first valid gameplay input starts the 90-second countdown. |
| Clean landing | Incoming contact speed below 0.5 m/s. Full score for the container. |
| Rough landing | 0.5–1.5 m/s inclusive. Reduced score. |
| Damaging impact | Above 1.5 m/s after the container has first been lifted. The container is damaged and scores nothing. |
| Worst impact counts | The hardest qualifying impact during a carry decides the grade; a later gentle contact does not erase it. |
| Delivered | Released, resting on the ship or on a delivered container, inside the hold footprint, and still (linear and angular) for one continuous second. |
| Lost | A container that leaves the play area or lands in the water scores nothing. |
| Knock-on damage | A delivered container that is later knocked off the ship loses its score. |
| Rig overload | When the net rope force exceeds the rating in a physics step, all ropes fail and the shift ends. The result states the force and the rating. |
| Ship full | Ends the shift early with a bonus for remaining time. |
| Timeout | Ends the shift; the score stands. |
| Result | One terminal outcome per run; gameplay input is disabled and the timer stops. R stays available. |

## 7. Level and camera

### Layout requirements

- A quay with a container yard, a basin (the pit) holding water and the ship, and a tower crane on the quay whose jib reaches both.
- The hold is a whole number of container footprints with clearance, and two layers deep.
- Enough head height and rope travel to lift a container over the ship's side and over an existing stack.
- The basin floor catches anything dropped in the water.

Validate reach and clearance with the real crane dimensions before finalising the layout.

### Camera requirements

- Follows the load, turns with the jib, zooms with the mouse wheel, orbits while the right mouse button is held.
- A footprint marker under the load shows where it would land, because a single view cannot show depth.

## 8. Feedback

| Readout | Information |
| --- | --- |
| Timer and score | Seconds remaining, current score, containers delivered |
| Cable | Tension bar in kN with the rating marked; cable colour shifts with tension |
| Load | Prompt when a container is in reach; per-rope forces. Never the mass. |
| Landing | Impact speed and grade at each landing |
| Outcome | Ship full, timeout, or cable failure, with the score breakdown |

Use text as well as colour. Every visual effect is driven by a measured physics value; none alters the simulation. Detailed diagnostics stay in the `PhysicsLab` scene.
