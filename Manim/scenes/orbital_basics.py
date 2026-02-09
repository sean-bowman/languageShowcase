"""
Orbital Basics Animation

=============================================================================
AEROSPACE CONCEPT: Fundamentals of Orbital Mechanics
=============================================================================

This module covers the foundational concepts of orbital mechanics:

1. CIRCULAR ORBITAL VELOCITY
   -------------------------
   For an object to maintain a circular orbit, centripetal force must
   equal gravitational force:

       F_gravity = F_centripetal
       GMm/r^2 = mv^2/r

   Solving for v:

       v_circ = sqrt(mu/r)

   Where:
       mu = GM = gravitational parameter [km^3/s^2]
       r = orbital radius from center of body [km]

   COUNTERINTUITIVE: Higher orbits have LOWER velocities!
       LEO (400 km): 7.67 km/s
       GEO (35,786 km): 3.07 km/s

2. ESCAPE VELOCITY
   ----------------
   Minimum velocity to escape a body's gravitational influence:

       v_esc = sqrt(2*mu/r) = sqrt(2) * v_circ

   Escape velocity is only 41% higher than circular velocity!

   From Earth's surface: 11.2 km/s
   From Moon's surface: 2.4 km/s

3. ORBITAL ELEMENTS (KEPLERIAN ELEMENTS)
   -------------------------------------
   Six parameters uniquely define any orbit:

       a - Semi-major axis [km]
           Half the longest diameter of the ellipse

       e - Eccentricity [dimensionless]
           Shape: 0 = circle, 0-1 = ellipse, 1 = parabola, >1 = hyperbola

       i - Inclination [degrees]
           Tilt of orbital plane from reference (equator)

       Omega - Right Ascension of Ascending Node (RAAN) [degrees]
           Where orbit crosses equator going north

       omega - Argument of Periapsis [degrees]
           Angle from ascending node to periapsis

       nu - True Anomaly [degrees]
           Current position on orbit (angle from periapsis)

4. ORBIT TYPES BY ECCENTRICITY
   ---------------------------
       e = 0:     Circle (bound, constant radius)
       0 < e < 1: Ellipse (bound, varying radius)
       e = 1:     Parabola (escape, zero energy)
       e > 1:     Hyperbola (escape, positive energy)

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. UPDATER WITH DT (TIME PARAMETER)
   `mob.add_updater(lambda m, dt: m.rotate(dt * speed))`
   dt is the time since last frame, enabling smooth animation.

2. POINT_FROM_PROPORTION
   `circle.point_from_proportion(0.25)` returns the point 25% around.
   Useful for placing objects on paths.

3. PUT_START_AND_END_ON
   `arrow.put_start_and_end_on(start, end)` repositions an arrow.
   Commonly used in updaters to track moving objects.

4. NUMPY FOR VECTOR MATH
   `np.linalg.norm(vector)` computes vector magnitude.
   `vector / np.linalg.norm(vector)` normalizes to unit vector.

5. GRAPH OBJECTS
   `Axes.plot(lambda x: f(x))` creates a function graph.
   `VMobject.set_points_smoothly()` creates parametric curves.

Introduction to orbital mechanics fundamentals:
- Circular orbital velocity
- Escape velocity
- Orbital elements
"""

from manim import *
import numpy as np


class OrbitalVelocityScene(Scene):
    """
    Derive and visualize circular orbital velocity.

    AEROSPACE: Shows the force balance that determines orbital velocity
    and visualizes a satellite moving with constant speed on a circular orbit.
    """

    def construct(self):
        # Title
        title = Text("Circular Orbital Velocity", font_size=40)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # FORCE BALANCE DERIVATION
        # =====================================================================
        # AEROSPACE: Gravitational force provides the centripetal acceleration
        force_label = Text("Force Balance:", font_size=24, color=BLUE)
        force_label.next_to(title, DOWN, buff=0.5)
        force_label.to_edge(LEFT, buff=1)

        self.play(Write(force_label))

        # F_gravity = F_centripetal
        force_eq = MathTex(
            r"\frac{GMm}{r^2} = \frac{mv^2}{r}"
        )
        force_eq.next_to(force_label, DOWN, buff=0.3)
        force_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(force_eq))
        self.wait(1)

        # Simplify (cancel m and r)
        simplify_label = Text("Simplify:", font_size=24, color=GREEN)
        simplify_label.next_to(force_eq, DOWN, buff=0.4)
        simplify_label.to_edge(LEFT, buff=1)

        simplify_eq = MathTex(r"\frac{GM}{r} = v^2")
        simplify_eq.next_to(simplify_label, DOWN, buff=0.3)
        simplify_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(simplify_label))
        self.play(Write(simplify_eq))
        self.wait(1)

        # =====================================================================
        # FINAL RESULT
        # =====================================================================
        result_label = Text("Circular Velocity:", font_size=24, color=YELLOW)
        result_label.next_to(simplify_eq, DOWN, buff=0.5)
        result_label.to_edge(LEFT, buff=1)

        result_eq = MathTex(
            r"v_{circ} = \sqrt{\frac{\mu}{r}}",
            font_size=48
        )
        result_eq.next_to(result_label, DOWN, buff=0.3)
        result_eq.to_edge(LEFT, buff=1.5)

        # Note: mu = GM
        note = MathTex(r"\mu = GM", font_size=24, color=GRAY)
        note.next_to(result_eq, RIGHT, buff=0.5)

        self.play(Write(result_label))
        self.play(Write(result_eq))
        self.play(FadeIn(note))
        self.wait(1)

        # =====================================================================
        # VISUAL DEMONSTRATION
        # =====================================================================
        # Move equations to make room for visualization
        self.play(
            force_label.animate.shift(UP * 5),
            force_eq.animate.shift(UP * 5),
            simplify_label.animate.shift(UP * 5),
            simplify_eq.animate.shift(UP * 5),
            result_label.animate.to_edge(LEFT, buff=0.5).shift(UP * 0.5),
            result_eq.animate.scale(0.7).to_edge(LEFT, buff=0.5),
            note.animate.scale(0.7).to_edge(LEFT, buff=3),
        )

        # Create orbit visualization
        earth = Circle(radius=0.5, color=BLUE, fill_opacity=1)
        earth.shift(RIGHT * 2)
        earth_label = Text("Earth", font_size=14)
        earth_label.next_to(earth, DOWN, buff=0.2)

        orbit = Circle(radius=2, color=WHITE, stroke_width=1)
        orbit.shift(RIGHT * 2)

        # Satellite as a dot
        satellite = Dot(color=RED, radius=0.1)
        # MANIM: point_from_proportion(0) gives the rightmost point of a circle
        satellite.move_to(orbit.point_from_proportion(0))

        # Velocity arrow (tangent to orbit)
        velocity_arrow = Arrow(
            satellite.get_center(),
            satellite.get_center() + UP * 0.8,
            color=YELLOW,
            buff=0
        )

        self.play(Create(earth), Write(earth_label))
        self.play(Create(orbit))
        self.play(Create(satellite), Create(velocity_arrow))

        # =====================================================================
        # ANIMATE SATELLITE ORBITING
        # =====================================================================
        # MANIM: Updater with dt parameter for time-based animation
        def satelliteUpdater(mob, dt):
            # Rotate around Earth's center
            mob.rotate(dt * 0.5, about_point=earth.get_center())

        # MANIM: Arrow updater to keep velocity tangent to orbit
        def arrowUpdater(mob):
            pos = satellite.get_center()
            # MATH: Velocity is perpendicular to radius (tangent to circle)
            radius_vec = pos - earth.get_center()
            # Rotate 90 degrees: [x, y] -> [-y, x]
            tangent = np.array([-radius_vec[1], radius_vec[0], 0])
            # NUMPY: Normalize to unit vector, then scale
            tangent = tangent / np.linalg.norm(tangent) * 0.8
            # MANIM: Reposition arrow from satellite in tangent direction
            mob.put_start_and_end_on(pos, pos + tangent)

        satellite.add_updater(satelliteUpdater)
        velocity_arrow.add_updater(arrowUpdater)

        # Let it animate for a while
        self.wait(5)

        # Clean up updaters
        satellite.remove_updater(satelliteUpdater)
        velocity_arrow.remove_updater(arrowUpdater)


class EscapeVelocityScene(Scene):
    """
    Derive escape velocity using energy conservation.

    AEROSPACE: Shows that escape velocity is only sqrt(2) times
    circular velocity - surprisingly close!
    """

    def construct(self):
        # Title
        title = Text("Escape Velocity", font_size=40)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # ENERGY APPROACH
        # =====================================================================
        # AEROSPACE: Total energy = KE + PE = 0 at escape
        energy_label = Text("Energy Conservation:", font_size=24, color=BLUE)
        energy_label.next_to(title, DOWN, buff=0.5)
        energy_label.to_edge(LEFT, buff=1)

        self.play(Write(energy_label))

        # Initial energy = final energy (at infinity, both are zero)
        energy_eq = MathTex(
            r"\frac{1}{2}mv_{esc}^2 - \frac{GMm}{r} = 0"
        )
        energy_eq.next_to(energy_label, DOWN, buff=0.3)
        energy_eq.to_edge(LEFT, buff=1.5)

        note = Text("At infinity: KE = 0, PE = 0", font_size=18, color=GRAY)
        note.next_to(energy_eq, DOWN, buff=0.2)
        note.to_edge(LEFT, buff=1.5)

        self.play(Write(energy_eq))
        self.play(FadeIn(note))
        self.wait(1)

        # =====================================================================
        # SOLVE FOR ESCAPE VELOCITY
        # =====================================================================
        solve_label = Text("Solve for escape velocity:", font_size=24, color=GREEN)
        solve_label.next_to(note, DOWN, buff=0.4)
        solve_label.to_edge(LEFT, buff=1)

        solve_eq = MathTex(r"v_{esc}^2 = \frac{2GM}{r}")
        solve_eq.next_to(solve_label, DOWN, buff=0.3)
        solve_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(solve_label))
        self.play(Write(solve_eq))
        self.wait(1)

        # =====================================================================
        # FINAL RESULT
        # =====================================================================
        result_eq = MathTex(
            r"v_{esc} = \sqrt{\frac{2\mu}{r}} = \sqrt{2} \cdot v_{circ}",
            font_size=44
        )
        result_eq.next_to(solve_eq, DOWN, buff=0.5)
        result_eq.to_edge(LEFT, buff=1.5)

        # MANIM: SurroundingRectangle highlights an important result
        box = SurroundingRectangle(result_eq, color=YELLOW, buff=0.2)

        self.play(Write(result_eq))
        self.play(Create(box))
        self.wait(1)

        # =====================================================================
        # KEY INSIGHT
        # =====================================================================
        insight = VGroup(
            Text("Key Insight:", font_size=22, color=YELLOW),
            MathTex(r"v_{esc} = \sqrt{2} \times v_{circ}", font_size=28),
            Text("Escape velocity is only 41% more", font_size=18),
            Text("than circular velocity!", font_size=18),
        )
        insight.arrange(DOWN, buff=0.2)
        insight.to_edge(RIGHT, buff=1)

        self.play(Write(insight), run_time=2)
        self.wait(2)


class OrbitalElementsScene(Scene):
    """
    Visualize the classical Keplerian orbital elements.

    AEROSPACE: Shows how six parameters uniquely define any orbit
    at any instant in time.
    """

    def construct(self):
        # Title
        title = Text("Keplerian Orbital Elements", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # REFERENCE PLANE (EQUATORIAL PLANE)
        # =====================================================================
        # MANIM: Polygon creates a parallelogram to suggest 3D perspective
        ref_plane = Polygon(
            [-3, -1.5, 0], [3, -1.5, 0], [4, 0.5, 0], [-2, 0.5, 0],
            color=BLUE,
            fill_opacity=0.2,
            stroke_width=1
        )
        ref_plane.shift(DOWN * 0.5)
        ref_label = Text("Reference Plane", font_size=14, color=BLUE)
        ref_label.next_to(ref_plane, DOWN, buff=0.1)

        self.play(Create(ref_plane), Write(ref_label))

        # Central body (focus of orbit)
        central = Dot(ORIGIN, color=YELLOW, radius=0.15)
        self.play(Create(central))

        # =====================================================================
        # ORBIT ELLIPSE (TILTED FOR 3D EFFECT)
        # =====================================================================
        # MANIM: Ellipse with rotation to show inclination
        orbit = Ellipse(width=4, height=2.5, color=WHITE, stroke_width=2)
        orbit.rotate(PI/6)  # Inclination effect
        orbit.shift(RIGHT * 0.5)  # Offset for eccentricity (focus not at center)

        self.play(Create(orbit))

        # =====================================================================
        # LIST OF ORBITAL ELEMENTS
        # =====================================================================
        elements = VGroup(
            Text("6 Classical Elements:", font_size=22, color=YELLOW),
            VGroup(
                MathTex(r"a", r" - \text{Semi-major axis}"),
                MathTex(r"e", r" - \text{Eccentricity}"),
                MathTex(r"i", r" - \text{Inclination}"),
                MathTex(r"\Omega", r" - \text{RAAN}"),
                MathTex(r"\omega", r" - \text{Argument of periapsis}"),
                MathTex(r"\nu", r" - \text{True anomaly}"),
            ).arrange(DOWN, buff=0.15, aligned_edge=LEFT).scale(0.7)
        )
        elements[1].next_to(elements[0], DOWN, buff=0.3, aligned_edge=LEFT)
        elements.to_edge(RIGHT, buff=0.5)
        elements.shift(UP)

        # Animate each element appearing
        for elem in [elements[0]] + list(elements[1]):
            self.play(FadeIn(elem, shift=LEFT), run_time=0.4)

        self.wait(1)

        # =====================================================================
        # HIGHLIGHT SEMI-MAJOR AXIS
        # =====================================================================
        a_line = Line(central.get_center(), orbit.get_right(), color=RED, stroke_width=3)
        a_label = MathTex("a", color=RED)
        a_label.next_to(a_line, UP, buff=0.1)

        self.play(Create(a_line), Write(a_label))
        self.wait(1)

        # =====================================================================
        # PERIAPSIS AND APOAPSIS
        # =====================================================================
        # AEROSPACE: Periapsis = closest point, Apoapsis = farthest point
        periapsis = Dot(orbit.get_left(), color=GREEN, radius=0.1)
        apoapsis = Dot(orbit.get_right(), color=GREEN, radius=0.1)

        peri_label = Text("Periapsis", font_size=12, color=GREEN)
        peri_label.next_to(periapsis, LEFT, buff=0.1)

        apo_label = Text("Apoapsis", font_size=12, color=GREEN)
        apo_label.next_to(apoapsis, RIGHT, buff=0.1)

        self.play(
            Create(periapsis), Create(apoapsis),
            Write(peri_label), Write(apo_label)
        )
        self.wait(2)

        # =====================================================================
        # SUMMARY
        # =====================================================================
        summary = Text(
            "These 6 elements uniquely define\nany orbit at any instant",
            font_size=18,
            color=GRAY
        )
        summary.to_edge(DOWN, buff=0.5)
        self.play(Write(summary))
        self.wait(2)


class OrbitTypesScene(Scene):
    """
    Show different types of orbits based on eccentricity.

    AEROSPACE: Visualizes how eccentricity determines orbit shape
    and whether the trajectory is bound or escape.
    """

    def construct(self):
        # Title
        title = Text("Orbit Types by Eccentricity", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # Central body
        center = ORIGIN
        central = Dot(center, color=YELLOW, radius=0.2)
        self.play(Create(central))

        # =====================================================================
        # CREATE DIFFERENT ORBIT TYPES
        # =====================================================================
        orbits_data = [
            ("Circle", 0, BLUE, "e = 0"),           # Bound, special case
            ("Ellipse", 0.5, GREEN, "0 < e < 1"),   # Bound, general case
            ("Parabola", 1.0, ORANGE, "e = 1"),     # Escape, zero energy
            ("Hyperbola", 1.5, RED, "e > 1"),       # Escape, positive energy
        ]

        orbit_group = VGroup()

        for i, (name, e, color, e_text) in enumerate(orbits_data):
            if e == 0:
                # =====================================================================
                # CIRCLE (e = 0)
                # =====================================================================
                # AEROSPACE: Perfect circle - all points equidistant from center
                orbit = Circle(radius=1.5, color=color, stroke_width=2)

            elif e < 1:
                # =====================================================================
                # ELLIPSE (0 < e < 1)
                # =====================================================================
                # AEROSPACE: Bound orbit with varying distance from focus
                a = 2
                b = a * np.sqrt(1 - e**2)  # Semi-minor axis
                orbit = Ellipse(width=2*a, height=2*b, color=color, stroke_width=2)
                # Shift so focus is at center (focus offset = a * e)
                c = a * e
                orbit.shift(RIGHT * c)

            elif e == 1:
                # =====================================================================
                # PARABOLA (e = 1)
                # =====================================================================
                # AEROSPACE: Escape trajectory with exactly zero total energy
                parabola_points = []
                for t in np.linspace(-2, 2, 100):
                    x = t**2 / 2  # x = y^2 / 2p (parabola equation)
                    y = t
                    parabola_points.append([x, y, 0])
                orbit = VMobject()
                orbit.set_points_smoothly([np.array(p) for p in parabola_points])
                orbit.set_color(color)
                orbit.set_stroke(width=2)
                orbit.shift(LEFT * 0.5)

            else:
                # =====================================================================
                # HYPERBOLA (e > 1)
                # =====================================================================
                # AEROSPACE: Unbound trajectory with positive total energy
                hyperbola_points = []
                for t in np.linspace(-1.5, 1.5, 100):
                    # MATH: Parametric hyperbola x = cosh(t), y = sinh(t)
                    x = np.cosh(t)
                    y = np.sinh(t) * 0.8
                    hyperbola_points.append([x, y, 0])
                orbit = VMobject()
                orbit.set_points_smoothly([np.array(p) for p in hyperbola_points])
                orbit.set_color(color)
                orbit.set_stroke(width=2)
                orbit.shift(LEFT * 0.5)

            # Scale and position for side-by-side display
            orbit.scale(0.6)
            orbit.shift(LEFT * 4 + RIGHT * i * 2.5 + DOWN * 0.5)

            # Labels
            label = Text(name, font_size=16, color=color)
            label.next_to(orbit, DOWN, buff=0.3)

            e_label = MathTex(e_text, font_size=18, color=GRAY)
            e_label.next_to(label, DOWN, buff=0.1)

            orbit_group.add(VGroup(orbit, label, e_label))

        # Animate each orbit type appearing
        for orb in orbit_group:
            self.play(Create(orb[0]), Write(orb[1]), Write(orb[2]), run_time=1)

        self.wait(2)

        # =====================================================================
        # ENERGY INTERPRETATION
        # =====================================================================
        energy_note = VGroup(
            Text("Energy Interpretation:", font_size=20, color=YELLOW),
            Text("e < 1: Bound orbit (negative total energy)", font_size=16),
            Text("e = 1: Escape trajectory (zero energy)", font_size=16),
            Text("e > 1: Unbound (positive energy)", font_size=16),
        )
        energy_note.arrange(DOWN, buff=0.15, aligned_edge=LEFT)
        energy_note.to_edge(DOWN, buff=0.5)

        self.play(Write(energy_note), run_time=2)
        self.wait(2)


# =============================================================================
# MAIN ENTRY POINT
# =============================================================================
if __name__ == "__main__":
    # Render with:
    # manim -pql orbital_basics.py OrbitalVelocityScene
    # manim -pql orbital_basics.py EscapeVelocityScene
    # manim -pql orbital_basics.py OrbitalElementsScene
    # manim -pql orbital_basics.py OrbitTypesScene
    pass
