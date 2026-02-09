"""
Rocket Equation Animation

=============================================================================
AEROSPACE CONCEPT: The Tsiolkovsky Rocket Equation
=============================================================================

The rocket equation is THE fundamental equation of astronautics. It relates
the change in velocity (delta-v) to the exhaust velocity and mass ratio.

THE EQUATION:
-------------
    dv = v_e * ln(m0 / mf)

    or equivalently:

    dv = I_sp * g0 * ln(m0 / mf)

Where:
    dv    = Change in velocity [m/s]
    v_e   = Effective exhaust velocity [m/s]
    I_sp  = Specific impulse [seconds]
    g0    = Standard gravity = 9.80665 [m/s^2]
    m0    = Initial mass (wet mass, with propellant) [kg]
    mf    = Final mass (dry mass, empty) [kg]
    ln    = Natural logarithm

MASS RATIO:
-----------
    MR = m0 / mf = e^(dv / v_e)

The exponential relationship is often called "the tyranny of the rocket
equation" because:

    dv = 1 * v_e  -->  MR = e^1 = 2.72  (63% propellant)
    dv = 2 * v_e  -->  MR = e^2 = 7.39  (86% propellant)
    dv = 3 * v_e  -->  MR = e^3 = 20.1  (95% propellant)

To DOUBLE the delta-v, you must SQUARE the mass ratio!

EXAMPLE - CHEMICAL ROCKET TO LEO:
---------------------------------
    Required dv: ~9.3 km/s (including gravity and drag losses)
    Exhaust velocity: ~4.4 km/s (LOX/RP-1 at sea level average)
    Mass ratio: e^(9.3/4.4) = e^2.11 = 8.2

    This means 88% of the launch mass must be propellant!
    For a 100-ton rocket, only 12 tons reaches orbit.

STAGING:
--------
Staging helps overcome the rocket equation by discarding empty mass:
- Each stage has its own mass ratio
- Total dv = sum of each stage's dv
- Smaller upper stages need less structure mass

SPECIFIC IMPULSE (I_sp):
------------------------
Specific impulse measures propulsion efficiency:
    I_sp = v_e / g0 [seconds]

    Chemical rockets: 250-470 s
    Ion engines: 1000-10000 s
    Nuclear thermal: 800-1000 s

Higher I_sp means less propellant needed for the same delta-v.

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. VALUETRACKER FOR ANIMATION
   `ValueTracker(initial_value)` stores a value that can be animated.
   `self.play(tracker.animate.set_value(new_value))` animates the change.

2. UPDATER FUNCTIONS
   `mobject.add_updater(func)` calls func(mobject) every frame.
   Used to link visual elements to ValueTracker values.

3. DECIMALNUMBER
   `DecimalNumber(value, num_decimal_places=n)` displays a number.
   Can be animated with updaters for dynamic displays.

4. THE BECOME() PATTERN
   `mobject.become(new_mobject)` replaces appearance.
   Used in updaters to completely redraw objects each frame.

5. RATE_FUNC FOR TIMING
   `rate_func=linear` means constant speed.
   Other options: smooth, there_and_back, ease_in_out, etc.

Visualizes the Tsiolkovsky rocket equation and mass ratio concepts.
Shows the fundamental relationship between delta-v, exhaust velocity, and mass ratio.
"""

from manim import *
import numpy as np
import sys
import os

# Add parent directory to path
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from components.spacecraft import Rocket, RocketWithExhaust, PropellantTank, MassIndicator


class RocketEquationIntroScene(Scene):
    """
    Introduction to the rocket equation with variable explanations.

    AEROSPACE: Presents the fundamental equation that governs all
    rocket propulsion and space mission design.
    """

    def construct(self):
        # =====================================================================
        # TITLE
        # =====================================================================
        title = Text("The Tsiolkovsky Rocket Equation", font_size=44)
        title.to_edge(UP)

        self.play(Write(title))
        self.wait(0.5)

        # =====================================================================
        # THE EQUATION
        # =====================================================================
        # MANIM: MathTex renders LaTeX; font_size controls scale
        equation = MathTex(
            r"\Delta v = v_e \ln \left( \frac{m_0}{m_f} \right)",
            font_size=56
        )
        equation.next_to(title, DOWN, buff=1)

        self.play(Write(equation), run_time=2)
        self.wait(1)

        # =====================================================================
        # VARIABLE EXPLANATIONS
        # =====================================================================
        variables = VGroup(
            MathTex(r"\Delta v", r" = \text{Change in velocity}"),
            MathTex(r"v_e", r" = \text{Exhaust velocity}"),
            MathTex(r"m_0", r" = \text{Initial mass (with propellant)}"),
            MathTex(r"m_f", r" = \text{Final mass (dry mass)}"),
        )
        variables.arrange(DOWN, buff=0.3, aligned_edge=LEFT)
        variables.scale(0.8)
        variables.next_to(equation, DOWN, buff=0.8)

        # Animate each variable explanation
        for var in variables:
            self.play(FadeIn(var, shift=RIGHT), run_time=0.6)

        self.wait(2)

        # =====================================================================
        # ALTERNATIVE FORM WITH SPECIFIC IMPULSE
        # =====================================================================
        # AEROSPACE: Specific impulse is commonly used in industry
        alt_label = Text("Alternative form with specific impulse:", font_size=24, color=YELLOW)
        alt_label.next_to(variables, DOWN, buff=0.6)

        alt_equation = MathTex(
            r"\Delta v = I_{sp} \cdot g_0 \cdot \ln \left( \frac{m_0}{m_f} \right)"
        )
        alt_equation.next_to(alt_label, DOWN, buff=0.3)

        self.play(Write(alt_label))
        self.play(Write(alt_equation))
        self.wait(2)


class RocketVisualScene(Scene):
    """
    Visual demonstration of propellant consumption during a burn.

    MANIM: Demonstrates the ValueTracker and updater pattern for
    dynamic animations.
    """

    def construct(self):
        # Title
        title = Text("Propellant Mass vs Delta-v", font_size=32)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # ROCKET VISUALIZATION
        # =====================================================================
        # Rocket body (simplified as rectangle)
        rocket_body = Rectangle(
            width=1.2,
            height=3,
            color=WHITE,
            fill_opacity=0.3,
            stroke_width=2
        )
        rocket_body.shift(LEFT * 3)

        # Nose cone
        nose = Polygon(
            [-0.6, 1.5, 0],
            [0.6, 1.5, 0],
            [0, 2.3, 0],
            color=RED,
            fill_opacity=1
        )
        nose.shift(LEFT * 3)

        # Propellant (animated fill) - starts full
        propellant = Rectangle(
            width=1.0,
            height=2.7,
            color=GREEN,
            fill_opacity=0.7,
            stroke_width=0
        )
        propellant.shift(LEFT * 3)
        propellant.align_to(rocket_body, DOWN)
        propellant.shift(UP * 0.15)

        rocket_label = Text("Rocket", font_size=18)
        rocket_label.next_to(rocket_body, DOWN, buff=0.3)

        self.play(
            Create(rocket_body),
            Create(nose),
            FadeIn(propellant),
            Write(rocket_label)
        )

        # =====================================================================
        # VALUETRACKER FOR ANIMATION
        # =====================================================================
        # MANIM: ValueTracker stores a value that can be animated
        # Start at 1.0 (100% full), will animate to 0.0 (empty)
        mass_ratio = ValueTracker(1.0)

        # =====================================================================
        # MASS DISPLAYS
        # =====================================================================
        m0_text = MathTex(r"m_0 = 10000 \text{ kg}", font_size=24)
        m0_text.to_edge(RIGHT, buff=1)
        m0_text.shift(UP * 2)

        mf_text = MathTex(r"m_f = 2000 \text{ kg}", font_size=24)
        mf_text.next_to(m0_text, DOWN, buff=0.3)

        self.play(Write(m0_text), Write(mf_text))

        # MANIM: DecimalNumber for animated numeric display
        current_mass = DecimalNumber(10000, num_decimal_places=0, font_size=32)
        current_mass_label = MathTex(r"m = ", font_size=32)
        current_mass_unit = Text(" kg", font_size=24)

        mass_group = VGroup(current_mass_label, current_mass, current_mass_unit)
        mass_group.arrange(RIGHT, buff=0.1)
        mass_group.next_to(mf_text, DOWN, buff=0.5)

        # Delta-v display
        dv_value = DecimalNumber(0, num_decimal_places=0, font_size=32)
        dv_label = MathTex(r"\Delta v = ", font_size=32)
        dv_unit = Text(" m/s", font_size=24)

        dv_group = VGroup(dv_label, dv_value, dv_unit)
        dv_group.arrange(RIGHT, buff=0.1)
        dv_group.next_to(mass_group, DOWN, buff=0.3)

        self.play(FadeIn(mass_group), FadeIn(dv_group))

        # Exhaust velocity note
        ve_note = MathTex(r"v_e = 3000 \text{ m/s}", font_size=20, color=GRAY)
        ve_note.next_to(dv_group, DOWN, buff=0.5)
        self.play(Write(ve_note))

        # =====================================================================
        # PROPELLANT UPDATER
        # =====================================================================
        # MANIM: Updater function called every frame to redraw propellant
        def propellantUpdater(mob):
            ratio = mass_ratio.get_value()
            height = 2.7 * max(ratio, 0.01)  # Minimum height to avoid zero
            new_prop = Rectangle(
                width=1.0,
                height=height,
                color=GREEN,
                fill_opacity=0.7,
                stroke_width=0
            )
            new_prop.shift(LEFT * 3)
            new_prop.align_to(rocket_body, DOWN)
            new_prop.shift(UP * 0.15)
            # MANIM: become() replaces the mobject's appearance
            mob.become(new_prop)

        propellant.add_updater(propellantUpdater)

        # =====================================================================
        # MASS UPDATER
        # =====================================================================
        def massUpdater(mob):
            ratio = mass_ratio.get_value()
            # Linear interpolation: full (10000) to empty (2000)
            mass = 2000 + 8000 * ratio
            mob.set_value(mass)

        current_mass.add_updater(massUpdater)

        # =====================================================================
        # DELTA-V UPDATER
        # =====================================================================
        # AEROSPACE: Calculate delta-v from rocket equation in real-time
        def dvUpdater(mob):
            ratio = mass_ratio.get_value()
            mass = 2000 + 8000 * ratio
            if mass < 10000:
                ve = 3000  # m/s
                # dv = v_e * ln(m0 / m_current)
                dv = ve * np.log(10000 / mass)
            else:
                dv = 0
            mob.set_value(dv)

        dv_value.add_updater(dvUpdater)

        # =====================================================================
        # ANIMATE PROPELLANT BURN
        # =====================================================================
        # MANIM: animate.set_value() creates an animation of the value change
        # rate_func=linear means constant rate (not easing)
        self.play(mass_ratio.animate.set_value(0.0), run_time=5, rate_func=linear)
        self.wait(1)

        # Remove updaters (important to prevent issues)
        propellant.remove_updater(propellantUpdater)
        current_mass.remove_updater(massUpdater)
        dv_value.remove_updater(dvUpdater)

        # =====================================================================
        # FINAL RESULT
        # =====================================================================
        # AEROSPACE: Show the final calculated delta-v
        # dv = 3000 * ln(10000/2000) = 3000 * ln(5) = 4828 m/s
        final_dv = MathTex(
            r"\Delta v = 3000 \cdot \ln(5) \approx 4828 \text{ m/s}",
            font_size=28,
            color=YELLOW
        )
        final_dv.to_edge(DOWN, buff=1)
        self.play(Write(final_dv))
        self.wait(2)


class MassRatioExponentialScene(Scene):
    """
    Show the exponential nature of the rocket equation.

    AEROSPACE: Visualizes "the tyranny of the rocket equation" -
    the exponential growth of required mass ratio with delta-v.
    """

    def construct(self):
        # Title
        title = Text("The Tyranny of the Rocket Equation", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # CREATE AXES FOR MASS RATIO VS DELTA-V
        # =====================================================================
        axes = Axes(
            x_range=[0, 5, 1],
            y_range=[0, 10, 2],
            x_length=8,
            y_length=5,
            axis_config={"color": WHITE},
            x_axis_config={"numbers_to_include": [1, 2, 3, 4, 5]},
            y_axis_config={"numbers_to_include": [2, 4, 6, 8, 10]},
        )
        axes.to_edge(DOWN, buff=1)
        axes.shift(LEFT * 0.5)

        x_label = MathTex(r"\Delta v / v_e", font_size=24)
        x_label.next_to(axes.x_axis, DOWN, buff=0.3)

        y_label = Text("Mass Ratio", font_size=20)
        y_label.rotate(PI/2)
        y_label.next_to(axes.y_axis, LEFT, buff=0.3)

        self.play(Create(axes), Write(x_label), Write(y_label))

        # =====================================================================
        # PLOT EXPONENTIAL CURVE
        # =====================================================================
        # MANIM: axes.plot() creates a graph of a function
        # MR = e^(dv/ve)
        graph = axes.plot(
            lambda x: np.exp(x),
            x_range=[0, 2.3],  # Up to about mass ratio of 10
            color=RED
        )

        graph_label = MathTex(r"MR = e^{\Delta v / v_e}", color=RED)
        graph_label.next_to(graph, UP + RIGHT, buff=0.2)

        self.play(Create(graph), Write(graph_label), run_time=2)
        self.wait(1)

        # =====================================================================
        # HIGHLIGHT KEY POINTS
        # =====================================================================
        # Point 1: dv/ve = 1 -> MR = e = 2.72
        point1 = Dot(axes.c2p(1, np.e), color=YELLOW)
        label1 = MathTex(r"\frac{\Delta v}{v_e} = 1 \rightarrow MR = 2.72", font_size=20)
        label1.next_to(point1, UP + RIGHT, buff=0.1)

        # Point 2: dv/ve = 2 -> MR = e^2 = 7.39
        point2 = Dot(axes.c2p(2, np.exp(2)), color=YELLOW)
        label2 = MathTex(r"\frac{\Delta v}{v_e} = 2 \rightarrow MR = 7.39", font_size=20)
        label2.next_to(point2, RIGHT, buff=0.1)

        self.play(Create(point1), Write(label1))
        self.play(Create(point2), Write(label2))
        self.wait(1)

        # =====================================================================
        # KEY INSIGHT
        # =====================================================================
        insight = VGroup(
            Text("To double the delta-v, you must", font_size=22),
            Text("SQUARE the mass ratio!", font_size=22, color=YELLOW),
        )
        insight.arrange(DOWN, buff=0.2)
        insight.to_edge(RIGHT, buff=0.5)
        insight.shift(UP)

        self.play(Write(insight), run_time=2)
        self.wait(2)

        # =====================================================================
        # EXAMPLE CALCULATION
        # =====================================================================
        example = VGroup(
            Text("Example: Chemical rocket to Moon", font_size=20),
            MathTex(r"\Delta v \approx 6 \text{ km/s}", font_size=20),
            MathTex(r"v_e \approx 4.4 \text{ km/s}", font_size=20),
            MathTex(r"MR = e^{6/4.4} \approx 3.9", font_size=20, color=GREEN),
        )
        example.arrange(DOWN, buff=0.15, aligned_edge=LEFT)
        example.to_edge(LEFT, buff=0.5)
        example.shift(UP * 2)

        self.play(Write(example), run_time=2)
        self.wait(2)


class StagingConceptScene(Scene):
    """
    Explain why staging helps overcome the rocket equation.

    AEROSPACE: Staging allows rockets to discard empty mass,
    improving the effective mass ratio for each stage.
    """

    def construct(self):
        # Title
        title = Text("Staging: Beating the Rocket Equation", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # PROBLEM STATEMENT
        # =====================================================================
        problem = Text(
            "Problem: Carrying empty tanks wastes propellant",
            font_size=24,
            color=YELLOW
        )
        problem.next_to(title, DOWN, buff=0.5)
        self.play(Write(problem))
        self.wait(1)

        # =====================================================================
        # SINGLE STAGE ROCKET
        # =====================================================================
        single_label = Text("Single Stage", font_size=20)
        single_label.shift(LEFT * 4 + UP)

        # Create a simple stacked rocket representation
        single_rocket = VGroup()
        for i in range(3):
            stage = Rectangle(
                width=0.8,
                height=1,
                color=[GREEN, BLUE, WHITE][i],
                fill_opacity=0.5
            )
            stage.shift(UP * i)
            single_rocket.add(stage)

        single_rocket.shift(LEFT * 4 + DOWN)
        single_label.next_to(single_rocket, UP, buff=0.3)

        self.play(Create(single_rocket), Write(single_label))

        # =====================================================================
        # MULTI-STAGE ROCKET
        # =====================================================================
        multi_label = Text("Multi-Stage", font_size=20)
        multi_label.shift(RIGHT * 2 + UP)

        # Stage 1 (largest, first to burn)
        stage1 = VGroup(
            Rectangle(width=1.2, height=1.5, color=GREEN, fill_opacity=0.5),
            Text("Stage 1", font_size=12)
        )
        stage1[1].move_to(stage1[0])

        # Stage 2 (medium, second to burn)
        stage2 = VGroup(
            Rectangle(width=0.9, height=1.2, color=BLUE, fill_opacity=0.5),
            Text("Stage 2", font_size=12)
        )
        stage2[1].move_to(stage2[0])

        # Payload (what actually reaches orbit)
        payload = VGroup(
            Rectangle(width=0.5, height=0.6, color=RED, fill_opacity=0.5),
            Text("Payload", font_size=10)
        )
        payload[1].move_to(payload[0])

        # Stack the stages
        multi_rocket = VGroup(stage1, stage2, payload)
        multi_rocket.arrange(UP, buff=0.05)
        multi_rocket.shift(RIGHT * 2 + DOWN * 0.5)
        multi_label.next_to(multi_rocket, UP, buff=0.3)

        self.play(Create(multi_rocket), Write(multi_label))
        self.wait(1)

        # =====================================================================
        # STAGING ADVANTAGES
        # =====================================================================
        advantage = VGroup(
            Text("Advantages of staging:", font_size=22, color=GREEN),
            Text("Discard empty mass", font_size=18),
            Text("Each stage optimized", font_size=18),
            Text("Higher total delta-v", font_size=18),
        )
        advantage.arrange(DOWN, buff=0.2, aligned_edge=LEFT)
        advantage.to_edge(RIGHT, buff=0.5)

        self.play(Write(advantage), run_time=2)
        self.wait(1)

        # =====================================================================
        # STAGED ROCKET EQUATION
        # =====================================================================
        # AEROSPACE: Total delta-v is the sum of each stage's contribution
        staged_eq = MathTex(
            r"\Delta v_{total} = \sum_{i} v_{e,i} \ln\left(\frac{m_{0,i}}{m_{f,i}}\right)"
        )
        staged_eq.to_edge(DOWN, buff=1)
        staged_eq.shift(LEFT)

        self.play(Write(staged_eq))
        self.wait(2)


# =============================================================================
# MAIN ENTRY POINT
# =============================================================================
if __name__ == "__main__":
    # Render with:
    # manim -pql rocket_equation.py RocketEquationIntroScene
    # manim -pql rocket_equation.py RocketVisualScene
    # manim -pql rocket_equation.py MassRatioExponentialScene
    # manim -pql rocket_equation.py StagingConceptScene
    pass
