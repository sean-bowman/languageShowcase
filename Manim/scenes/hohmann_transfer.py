"""
Hohmann Transfer Animation

=============================================================================
AEROSPACE CONCEPT: Hohmann Transfer Orbit
=============================================================================

The Hohmann transfer is the most fuel-efficient two-impulse maneuver to
transfer between two circular coplanar orbits. Named after Walter Hohmann
who first described it in 1925.

HOW IT WORKS:
-------------

     Apoapsis (at r2)
           *
         / | \            STEP 1: At periapsis (r1), burn prograde
        /  |  \                   to raise apoapsis to r2
       /   |   \
      /    |    \         STEP 2: Coast along transfer ellipse
     /     |     \                (half an orbit, 180 degrees)
    *------O------* r1
     \           /        STEP 3: At apoapsis (r2), burn prograde
      \         /                 to circularize at new altitude
       \       /
        \     /
         \   /            Transfer ellipse is tangent to both orbits
          \ /             at periapsis and apoapsis
           *

KEY EQUATIONS:
--------------

Semi-major axis of transfer:  a_t = (r1 + r2) / 2

First burn (delta-v at periapsis):
    dv1 = sqrt(mu/r1) * (sqrt(2*r2/(r1+r2)) - 1)

Second burn (delta-v at apoapsis):
    dv2 = sqrt(mu/r2) * (1 - sqrt(2*r1/(r1+r2)))

Total delta-v:  dv_total = dv1 + dv2

Transfer time:  t = pi * sqrt(a^3/mu)

EXAMPLE (LEO to GEO):
---------------------
    r1 = 6771 km (400 km altitude)
    r2 = 42157 km (GEO)
    mu = 398600 km^3/s^2

    dv1 = 2.46 km/s
    dv2 = 1.48 km/s
    Total = 3.94 km/s
    Time = 5.24 hours

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. SCENE CLASS
   `class HohmannIntroScene(Scene)` - Scene is the base class for all
   Manim animations. Override `construct(self)` to define what happens.

2. THE CONSTRUCT METHOD
   `def construct(self):` is automatically called when the scene renders.
   All animations are defined inside this method.

3. ANIMATION METHODS
   `self.play(animation)` - plays animation
   `self.wait(seconds)` - pauses for given time
   `self.play(anim1, anim2)` - plays animations simultaneously

4. COMMON ANIMATIONS
   - Write(text): Handwriting effect for text
   - Create(mobject): Draws a shape
   - FadeIn/FadeOut: Opacity transitions
   - MoveAlongPath(mob, path): Moves object along a curve

5. POSITIONING
   - `mob.to_edge(UP)`: Move to top edge
   - `mob.next_to(other, DOWN)`: Position below another object
   - `mob.shift(RIGHT * 2)`: Move by a vector

6. VGROUP AND ARRANGE
   VGroup combines objects; arrange(DOWN) stacks them vertically.

Visualizes the Hohmann transfer orbit and derives the delta-v equations.
Demonstrates orbital mechanics concepts through animation.
"""

from manim import *
import numpy as np
import sys
import os

# PYTHON: sys.path manipulation for sibling directory imports
# This allows importing from ../components/ and ../data/
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from components.orbit import OrbitPath, TransferOrbit, Spacecraft, CentralBody
from components.equations import (
    getCircularVelocityEquation,
    getVisVivaEquation,
    getHohmannDv1Equation,
    getHohmannDv2Equation,
    getHohmannTransferTimeEquation
)


class HohmannIntroScene(Scene):
    """
    Introduction to Hohmann transfer orbits.

    MANIM: Scene is the base class for all animations.
    The `construct` method defines what happens in the scene.
    """

    def construct(self):
        """
        MANIM: construct() is the main method where all animation logic goes.
        It's automatically called when the scene is rendered.
        """
        # =====================================================================
        # TITLE
        # =====================================================================
        # MANIM: Text creates a text object with specified font size
        title = Text("Hohmann Transfer Orbit", font_size=48)
        # MANIM: to_edge(UP) positions the object at the top of the screen
        title.to_edge(UP)

        # MANIM: self.play() executes animations
        # Write() creates a handwriting-like effect
        self.play(Write(title))
        # MANIM: self.wait() pauses for specified seconds
        self.wait(0.5)

        # =====================================================================
        # DESCRIPTION
        # =====================================================================
        # MANIM: VGroup combines multiple Mobjects into one
        desc = VGroup(
            Text("The most fuel-efficient two-impulse maneuver", font_size=28),
            Text("to transfer between circular coplanar orbits", font_size=28),
        )
        # MANIM: arrange(DOWN) stacks the group vertically
        desc.arrange(DOWN, buff=0.3)
        # MANIM: next_to() positions relative to another object
        desc.next_to(title, DOWN, buff=0.8)

        # MANIM: run_time controls animation duration
        self.play(Write(desc), run_time=2)
        self.wait(1)

        # =====================================================================
        # KEY POINTS
        # =====================================================================
        points = VGroup(
            Text("1. Two impulsive burns required", font_size=24),
            Text("2. Transfer follows elliptical path", font_size=24),
            Text("3. Minimizes total delta-v", font_size=24),
        )
        # MANIM: aligned_edge=LEFT aligns all elements to the left edge
        points.arrange(DOWN, buff=0.3, aligned_edge=LEFT)
        points.next_to(desc, DOWN, buff=0.6)

        # PYTHON: Iterate through points with animation
        for point in points:
            # MANIM: FadeIn with shift creates a sliding entrance effect
            self.play(FadeIn(point, shift=RIGHT), run_time=0.7)
        self.wait(2)

        # MANIM: Multiple FadeOut animations in parallel
        self.play(FadeOut(title), FadeOut(desc), FadeOut(points))


class HohmannVisualizationScene(Scene):
    """
    Visualize the Hohmann transfer maneuver with animated spacecraft.

    AEROSPACE: Shows the complete transfer from inner to outer orbit,
    including both burns and the coast phase.
    """

    def construct(self):
        # Title
        title = Text("Hohmann Transfer Visualization", font_size=32)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # CENTRAL BODY (EARTH)
        # =====================================================================
        # MANIM: Circle creates a circle; fill_opacity=1 makes it solid
        earth = Circle(radius=0.5, color=BLUE, fill_opacity=1)
        earth_label = Text("Earth", font_size=16)
        earth_label.next_to(earth, DOWN, buff=0.2)
        self.play(Create(earth), Write(earth_label))

        # =====================================================================
        # ORBIT RADII (VISUAL UNITS, NOT REAL SCALE)
        # =====================================================================
        # VISUALIZATION: We use arbitrary visual units, not real km
        # Real LEO is 400 km altitude, GEO is 35,786 km
        r1 = 1.5  # Initial orbit (represents LEO)
        r2 = 3.5  # Final orbit (represents GEO)

        # =====================================================================
        # INITIAL ORBIT (BLUE)
        # =====================================================================
        initial_orbit = Circle(radius=r1, color=BLUE, stroke_width=2)
        initial_label = Text("Initial Orbit", font_size=16, color=BLUE)
        # MANIM: get_top() returns the topmost point of an object
        initial_label.move_to(initial_orbit.get_top() + UP * 0.3)

        self.play(Create(initial_orbit), Write(initial_label))

        # =====================================================================
        # FINAL ORBIT (GREEN)
        # =====================================================================
        final_orbit = Circle(radius=r2, color=GREEN, stroke_width=2)
        final_label = Text("Final Orbit", font_size=16, color=GREEN)
        final_label.move_to(final_orbit.get_top() + UP * 0.3)

        self.play(Create(final_orbit), Write(final_label))

        # =====================================================================
        # TRANSFER ORBIT (HALF ELLIPSE, ORANGE)
        # =====================================================================
        # AEROSPACE: The transfer ellipse has:
        # - Semi-major axis a = (r1 + r2) / 2
        # - Periapsis at r1, apoapsis at r2
        a = (r1 + r2) / 2  # Semi-major axis
        b = np.sqrt(r1 * r2)  # Semi-minor axis = geometric mean

        # MANIM: Generate points for smooth curve
        transfer_points = []
        for i in range(101):
            theta = i * PI / 100  # 0 to PI (half orbit)
            # Parametric ellipse equations
            x = a * np.cos(theta)
            y = b * np.sin(theta)
            transfer_points.append([x, y, 0])

        # MANIM: VMobject is the base class for vector graphics
        transfer_orbit = VMobject()
        # MANIM: set_points_smoothly() creates a smooth Bezier curve
        transfer_orbit.set_points_smoothly([np.array(p) for p in transfer_points])
        transfer_orbit.set_color(ORANGE)
        transfer_orbit.set_stroke(width=2)
        # Shift so periapsis is at r1 from center (focus at origin)
        transfer_orbit.shift(LEFT * (a - r1))

        transfer_label = Text("Transfer Orbit", font_size=16, color=ORANGE)
        transfer_label.move_to([0, b * 0.7, 0])

        self.play(Create(transfer_orbit), Write(transfer_label), run_time=2)

        # =====================================================================
        # SPACECRAFT
        # =====================================================================
        # MANIM: Triangle creates a triangle shape
        spacecraft = Triangle(color=RED, fill_opacity=1)
        spacecraft.scale(0.15)
        spacecraft.rotate(-PI/2)  # Point in direction of motion
        spacecraft.move_to([r1, 0, 0])  # Start at periapsis

        self.play(Create(spacecraft))

        # =====================================================================
        # FIRST BURN (DELTA-V1)
        # =====================================================================
        # AEROSPACE: Burn prograde (in direction of motion) at periapsis
        # to raise apoapsis to the target orbit
        dv1_arrow = Arrow(
            [r1, 0, 0],
            [r1, 0.5, 0],
            color=RED,
            buff=0
        )
        dv1_label = MathTex(r"\Delta v_1", color=RED)
        dv1_label.next_to(dv1_arrow, RIGHT, buff=0.1)

        burn_text = Text("First Burn: Prograde at periapsis", font_size=20)
        burn_text.to_edge(DOWN, buff=0.5)

        self.play(
            Create(dv1_arrow),
            Write(dv1_label),
            Write(burn_text)
        )
        self.wait(1)

        # =====================================================================
        # COAST ALONG TRANSFER ORBIT
        # =====================================================================
        self.play(FadeOut(dv1_arrow), FadeOut(dv1_label), FadeOut(burn_text))

        # MANIM: Create path for MoveAlongPath animation
        transfer_path = VMobject()
        transfer_path.set_points_smoothly(
            [np.array(p) + np.array([-(a - r1), 0, 0]) for p in transfer_points]
        )

        # MANIM: MoveAlongPath animates object along a curve
        self.play(MoveAlongPath(spacecraft, transfer_path), run_time=3)

        # =====================================================================
        # SECOND BURN (DELTA-V2)
        # =====================================================================
        # Position spacecraft at apoapsis (left side at r2)
        spacecraft.move_to([-r2, 0, 0])

        # AEROSPACE: Burn prograde at apoapsis to circularize
        dv2_arrow = Arrow(
            [-r2, 0, 0],
            [-r2, 0.5, 0],
            color=RED,
            buff=0
        )
        dv2_label = MathTex(r"\Delta v_2", color=RED)
        dv2_label.next_to(dv2_arrow, RIGHT, buff=0.1)

        burn_text2 = Text("Second Burn: Prograde at apoapsis", font_size=20)
        burn_text2.to_edge(DOWN, buff=0.5)

        self.play(
            Create(dv2_arrow),
            Write(dv2_label),
            Write(burn_text2)
        )
        self.wait(1)

        # =====================================================================
        # CONTINUE ON FINAL ORBIT
        # =====================================================================
        self.play(FadeOut(dv2_arrow), FadeOut(dv2_label), FadeOut(burn_text2))

        # MANIM: Arc creates a circular arc
        # start_angle=PI means start on left, angle=PI means half circle
        arc = Arc(radius=r2, start_angle=PI, angle=PI, color=GREEN)
        self.play(MoveAlongPath(spacecraft, arc), run_time=2)

        # =====================================================================
        # SUMMARY
        # =====================================================================
        summary = VGroup(
            MathTex(r"\Delta v_{total} = \Delta v_1 + \Delta v_2"),
        )
        summary.to_edge(DOWN, buff=0.5)
        self.play(Write(summary))
        self.wait(2)


class HohmannDerivationScene(Scene):
    """
    Derive the Hohmann transfer equations step by step.

    AEROSPACE: Shows how the delta-v equations come from the
    vis-viva equation and orbital geometry.
    """

    def construct(self):
        # Title
        title = Text("Hohmann Transfer Equations", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # STEP 1: VIS-VIVA EQUATION
        # =====================================================================
        # AEROSPACE: The vis-viva equation is fundamental to orbital mechanics
        # It relates velocity to position and orbital shape
        step1_label = Text("Step 1: Vis-Viva Equation", font_size=24, color=BLUE)
        step1_label.next_to(title, DOWN, buff=0.5)
        step1_label.to_edge(LEFT, buff=1)

        # MANIM: MathTex renders LaTeX mathematics
        vis_viva = MathTex(r"v^2 = \mu \left( \frac{2}{r} - \frac{1}{a} \right)")
        vis_viva.next_to(step1_label, DOWN, buff=0.3)
        vis_viva.to_edge(LEFT, buff=1.5)

        self.play(Write(step1_label))
        self.play(Write(vis_viva))
        self.wait(1)

        # =====================================================================
        # STEP 2: CIRCULAR ORBIT VELOCITY
        # =====================================================================
        step2_label = Text("Step 2: Circular Orbit Velocity", font_size=24, color=GREEN)
        step2_label.next_to(vis_viva, DOWN, buff=0.5)
        step2_label.to_edge(LEFT, buff=1)

        circ_vel = MathTex(r"v_{circ} = \sqrt{\frac{\mu}{r}}")
        circ_vel_note = Text("(when a = r)", font_size=16, color=GRAY)
        circ_vel.next_to(step2_label, DOWN, buff=0.3)
        circ_vel.to_edge(LEFT, buff=1.5)
        circ_vel_note.next_to(circ_vel, RIGHT, buff=0.3)

        self.play(Write(step2_label))
        self.play(Write(circ_vel), FadeIn(circ_vel_note))
        self.wait(1)

        # =====================================================================
        # STEP 3: TRANSFER SEMI-MAJOR AXIS
        # =====================================================================
        step3_label = Text("Step 3: Transfer Semi-Major Axis", font_size=24, color=YELLOW)
        step3_label.next_to(circ_vel, DOWN, buff=0.5)
        step3_label.to_edge(LEFT, buff=1)

        transfer_a = MathTex(r"a_{transfer} = \frac{r_1 + r_2}{2}")
        transfer_a.next_to(step3_label, DOWN, buff=0.3)
        transfer_a.to_edge(LEFT, buff=1.5)

        self.play(Write(step3_label))
        self.play(Write(transfer_a))
        self.wait(1)

        # =====================================================================
        # CLEAR AND SHOW FINAL EQUATIONS
        # =====================================================================
        self.play(
            FadeOut(step1_label), FadeOut(vis_viva),
            FadeOut(step2_label), FadeOut(circ_vel), FadeOut(circ_vel_note),
            FadeOut(step3_label), FadeOut(transfer_a)
        )

        # =====================================================================
        # FINAL DELTA-V EQUATIONS
        # =====================================================================
        final_label = Text("Delta-v Equations:", font_size=28, color=ORANGE)
        final_label.next_to(title, DOWN, buff=0.6)

        # First burn equation
        dv1_eq = MathTex(
            r"\Delta v_1 = \sqrt{\frac{\mu}{r_1}} "
            r"\left( \sqrt{\frac{2 r_2}{r_1 + r_2}} - 1 \right)"
        )
        dv1_eq.next_to(final_label, DOWN, buff=0.5)

        dv1_note = Text("First burn at periapsis", font_size=18, color=GRAY)
        dv1_note.next_to(dv1_eq, DOWN, buff=0.2)

        self.play(Write(final_label))
        self.play(Write(dv1_eq))
        self.play(FadeIn(dv1_note))
        self.wait(1)

        # Second burn equation
        dv2_eq = MathTex(
            r"\Delta v_2 = \sqrt{\frac{\mu}{r_2}} "
            r"\left( 1 - \sqrt{\frac{2 r_1}{r_1 + r_2}} \right)"
        )
        dv2_eq.next_to(dv1_note, DOWN, buff=0.4)

        dv2_note = Text("Second burn at apoapsis", font_size=18, color=GRAY)
        dv2_note.next_to(dv2_eq, DOWN, buff=0.2)

        self.play(Write(dv2_eq))
        self.play(FadeIn(dv2_note))
        self.wait(1)

        # Transfer time equation
        time_eq = MathTex(r"t_{transfer} = \pi \sqrt{\frac{a^3}{\mu}}")
        time_eq.next_to(dv2_note, DOWN, buff=0.5)

        time_note = Text("Half the orbital period of transfer ellipse", font_size=18, color=GRAY)
        time_note.next_to(time_eq, DOWN, buff=0.2)

        self.play(Write(time_eq))
        self.play(FadeIn(time_note))
        self.wait(2)


class HohmannExampleScene(Scene):
    """
    Numerical example of LEO to GEO transfer.

    AEROSPACE: This is one of the most common transfer scenarios -
    placing communication satellites in geostationary orbit.
    """

    def construct(self):
        # Title
        title = Text("Example: LEO to GEO Transfer", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # GIVEN VALUES
        # =====================================================================
        given_label = Text("Given:", font_size=24, color=BLUE)
        given_label.next_to(title, DOWN, buff=0.5)
        given_label.to_edge(LEFT, buff=1)

        # AEROSPACE: Real orbital parameters for LEO to GEO
        # r1 = Earth radius (6371 km) + altitude (400 km)
        # r2 = GEO radius (fixed for 24-hour period)
        given_values = VGroup(
            MathTex(r"r_1 = 6771 \text{ km (LEO at 400 km altitude)}"),
            MathTex(r"r_2 = 42157 \text{ km (GEO)}"),
            MathTex(r"\mu = 398600 \text{ km}^3/\text{s}^2"),
        )
        given_values.arrange(DOWN, buff=0.2, aligned_edge=LEFT)
        given_values.scale(0.8)
        given_values.next_to(given_label, DOWN, buff=0.3)
        given_values.to_edge(LEFT, buff=1.5)

        self.play(Write(given_label))
        for val in given_values:
            self.play(FadeIn(val), run_time=0.5)
        self.wait(1)

        # =====================================================================
        # RESULTS
        # =====================================================================
        results_label = Text("Results:", font_size=24, color=GREEN)
        results_label.next_to(given_values, DOWN, buff=0.5)
        results_label.to_edge(LEFT, buff=1)

        # AEROSPACE: These are the calculated values from the equations
        results = VGroup(
            MathTex(r"\Delta v_1 = 2.46 \text{ km/s}"),
            MathTex(r"\Delta v_2 = 1.48 \text{ km/s}"),
            MathTex(r"\Delta v_{total} = 3.94 \text{ km/s}", color=YELLOW),
            MathTex(r"t_{transfer} = 5.24 \text{ hours}"),
        )
        results.arrange(DOWN, buff=0.2, aligned_edge=LEFT)
        results.scale(0.8)
        results.next_to(results_label, DOWN, buff=0.3)
        results.next_to(results_label, DOWN, buff=0.3)
        results.to_edge(LEFT, buff=1.5)

        self.play(Write(results_label))
        for res in results:
            self.play(Write(res), run_time=0.7)
        self.wait(1)

        # =====================================================================
        # HIGHLIGHT TOTAL DELTA-V
        # =====================================================================
        # MANIM: SurroundingRectangle creates a box around an object
        box = SurroundingRectangle(results[2], color=YELLOW, buff=0.1)
        self.play(Create(box))
        self.wait(2)


# =============================================================================
# MAIN ENTRY POINT
# =============================================================================
# PYTHON: This block only runs if the file is executed directly,
# not when imported as a module.
if __name__ == "__main__":
    # MANIM: Render individual scenes from command line:
    # manim -pql hohmann_transfer.py HohmannIntroScene
    #   -p: Preview (open video when done)
    #   -q: Quality (l=low, m=medium, h=high)
    #   -l: Low quality (faster rendering for testing)
    #
    # manim -pql hohmann_transfer.py HohmannVisualizationScene
    # manim -pql hohmann_transfer.py HohmannDerivationScene
    # manim -pql hohmann_transfer.py HohmannExampleScene
    pass
