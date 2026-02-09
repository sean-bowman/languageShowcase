"""
ISA Atmosphere Animation

=============================================================================
AEROSPACE CONCEPT: International Standard Atmosphere (ISA)
=============================================================================

The ISA (ISO 2533:1975) is a reference model that defines how atmospheric
properties vary with altitude. It's used worldwide for:

- Aircraft performance calculations
- Altimeter calibration
- Engine specifications
- Flight testing standardization

ISA SEA LEVEL CONDITIONS:
-------------------------
    Temperature:  288.15 K  (15 deg C, 59 deg F)
    Pressure:     101325 Pa (1013.25 hPa, 29.92 inHg, 1 atm)
    Density:      1.225 kg/m^3

ATMOSPHERIC LAYERS:
-------------------

    Altitude (km)    Layer             Temp Behavior       Lapse Rate
    -----------------------------------------------------------------------
       85           Mesosphere top
                    Mesosphere         Decreases           -2.0 K/km
       71
                    Mesosphere         Decreases           -2.8 K/km
       51           Stratopause
                    Stratopause        Constant            0 K/km
       47
                    Stratosphere       Increases           +2.8 K/km
       32
                    Stratosphere       Increases           +1.0 K/km
       20           Tropopause
                    Tropopause         Constant            0 K/km
       11           Troposphere top
                    Troposphere        Decreases           -6.5 K/km
        0           Sea level

WHY TEMPERATURE VARIES:
-----------------------
- TROPOSPHERE: Heated from below (ground absorbs solar radiation).
  Temperature falls with altitude at 6.5 K/km.

- TROPOPAUSE: Isothermal layer. Convection stops here.

- STRATOSPHERE: Ozone layer absorbs UV radiation, heating the air.
  Temperature actually INCREASES with altitude here.

- MESOSPHERE: Above ozone layer, radiation cooling dominates.
  Temperature falls again, reaching ~-100 deg C at mesopause.

KEY EQUATIONS:
--------------
Gradient layers:    T = T_b + L * (h - h_b)
                    P = P_b * (T/T_b)^(-g0/LR)

Isothermal layers:  T = T_b (constant)
                    P = P_b * exp(-g0*(h-h_b)/(R*T))

Density:            rho = P / (R * T)    [Ideal gas law]

Speed of sound:     a = sqrt(gamma * R * T)

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. MULTIPLE SCENES IN ONE FILE
   Manim can render any scene from a file:
   `manim -pql isa_atmosphere.py ISALayersScene`

2. AXES OBJECT
   `Axes(x_range, y_range, x_length, y_length)` creates coordinate axes
   with labeled tick marks.

3. AXES PLOTTING
   `axes.plot(lambda x: func(x))` creates a graph on the axes.
   `axes.c2p(x, y)` converts coordinates to screen position.

4. VMOBJECT FOR CUSTOM CURVES
   `set_points_as_corners(points)` creates straight-line segments.
   `set_points_smoothly(points)` creates smooth Bezier curves.

5. DYNAMIC TEXT WITH VARIABLES
   Text objects can include computed values using f-strings.

Visualizes the International Standard Atmosphere layers and temperature profile.
Demonstrates Manim's ability to create educational aerospace content.
"""

from manim import *
import numpy as np
import sys
import os

# PYTHON: Add parent directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from data.constants import isaLayers, isaT0, isaP0
from components.equations import (
    getIsaTemperatureEquation,
    getIsaPressureGradientEquation,
    getIsaDensityEquation,
    getSpeedOfSoundEquation
)


class ISAIntroScene(Scene):
    """
    Introduction to the International Standard Atmosphere.

    MANIM: A simple scene that introduces the topic with text
    and a list of key properties.
    """

    def construct(self):
        # =====================================================================
        # TITLE AND SUBTITLE
        # =====================================================================
        title = Text("International Standard Atmosphere", font_size=48)
        title.to_edge(UP)
        # ISO 2533:1975 is the official standard designation
        subtitle = Text("ISO 2533:1975", font_size=24, color=GRAY)
        subtitle.next_to(title, DOWN)

        self.play(Write(title), run_time=1.5)
        self.play(FadeIn(subtitle))
        self.wait(1)

        # =====================================================================
        # INTRODUCTION TEXT
        # =====================================================================
        intro_text = VGroup(
            Text("The ISA defines standard atmospheric", font_size=28),
            Text("properties as a function of altitude", font_size=28),
        )
        intro_text.arrange(DOWN, buff=0.3)
        intro_text.next_to(subtitle, DOWN, buff=0.8)

        self.play(Write(intro_text), run_time=2)
        self.wait(1)

        # =====================================================================
        # KEY PROPERTIES LIST
        # =====================================================================
        # AEROSPACE: These are the four primary properties defined by ISA
        properties = VGroup(
            Text("Temperature (T)", font_size=24),
            Text("Pressure (P)", font_size=24),
            Text("Density (p)", font_size=24),
            Text("Speed of Sound (a)", font_size=24),
        )
        properties.arrange(DOWN, buff=0.2, aligned_edge=LEFT)
        properties.next_to(intro_text, DOWN, buff=0.5)

        # MANIM: Animate each property appearing one by one
        for prop in properties:
            self.play(FadeIn(prop, shift=RIGHT), run_time=0.5)
        self.wait(1)

        # Transition out
        self.play(
            FadeOut(title),
            FadeOut(subtitle),
            FadeOut(intro_text),
            FadeOut(properties)
        )


class ISALayersScene(Scene):
    """
    Visualize the atmospheric layers as stacked rectangles.

    AEROSPACE: Shows the vertical structure of the atmosphere
    with different colors for each layer.
    """

    def construct(self):
        # Title
        title = Text("Atmospheric Layers", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # EARTH SURFACE REFERENCE LINE
        # =====================================================================
        # MANIM: Line creates a line between two points
        earth_surface = Line(LEFT * 5, RIGHT * 5, color=GREEN)
        earth_surface.to_edge(DOWN, buff=1)
        earth_label = Text("Earth Surface", font_size=18)
        earth_label.next_to(earth_surface, DOWN, buff=0.2)

        self.play(Create(earth_surface), Write(earth_label))

        # =====================================================================
        # LAYER COLORS AND DATA
        # =====================================================================
        colors = [BLUE, GREEN, YELLOW, ORANGE, RED, PURPLE, PINK]

        # Simplified layers for visualization (first 3 main layers)
        # Format: (name, base altitude m, top altitude m, altitude string, lapse string)
        layers_to_show = [
            ("Troposphere", 0, 11000, "0-11 km", "-6.5 K/km"),
            ("Tropopause", 11000, 20000, "11-20 km", "Isothermal"),
            ("Stratosphere", 20000, 47000, "20-47 km", "+1 to +2.8 K/km"),
        ]

        layer_mobjects = []
        scale = 0.1  # Scale factor: 1 km = 0.1 Manim units

        # =====================================================================
        # CREATE EACH LAYER
        # =====================================================================
        for i, (name, h_base, h_top, altitude_str, lapse_str) in enumerate(layers_to_show):
            # Calculate visual positions using scale factor
            y_base = earth_surface.get_center()[1] + h_base / 1000 * scale
            height = (h_top - h_base) / 1000 * scale

            # MANIM: Rectangle with semi-transparent fill
            layer_rect = Rectangle(
                width=8,
                height=height,
                color=colors[i],
                fill_opacity=0.3,
                stroke_width=2
            )
            # Position rectangle so bottom is at y_base
            layer_rect.move_to([0, y_base + height/2, 0])

            # Layer name label
            label = Text(name, font_size=20, color=colors[i])
            label.next_to(layer_rect, RIGHT, buff=0.3)

            # Altitude range label
            alt_text = Text(altitude_str, font_size=16, color=GRAY)
            alt_text.next_to(label, DOWN, buff=0.1, aligned_edge=LEFT)

            # Lapse rate label
            lapse_text = Text(lapse_str, font_size=14, color=GRAY_B)
            lapse_text.next_to(alt_text, DOWN, buff=0.1, aligned_edge=LEFT)

            # Group all elements for this layer
            layer_group = VGroup(layer_rect, label, alt_text, lapse_text)
            layer_mobjects.append(layer_group)

            # Animate layer appearance
            self.play(
                Create(layer_rect),
                Write(label),
                FadeIn(alt_text),
                FadeIn(lapse_text),
                run_time=1
            )

        self.wait(2)

        # =====================================================================
        # TEMPERATURE VARIATION ARROW
        # =====================================================================
        # AEROSPACE: Arrow indicates temperature varies with altitude
        temp_arrow = Arrow(
            earth_surface.get_center() + RIGHT * 4.5,
            earth_surface.get_center() + RIGHT * 4.5 + UP * 2,
            color=RED
        )
        temp_label = Text("T varies", font_size=16, color=RED)
        temp_label.next_to(temp_arrow, RIGHT, buff=0.1)

        self.play(Create(temp_arrow), Write(temp_label))
        self.wait(2)


class ISAEquationsScene(Scene):
    """
    Show the key ISA equations for temperature, pressure, and density.

    AEROSPACE: These equations are fundamental to aircraft performance
    calculations at any altitude.
    """

    def construct(self):
        # Title
        title = Text("ISA Equations", font_size=36)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # TEMPERATURE EQUATION (GRADIENT LAYERS)
        # =====================================================================
        temp_section = Text("Temperature (Gradient Layers):", font_size=24, color=BLUE)
        temp_section.next_to(title, DOWN, buff=0.5)
        temp_section.to_edge(LEFT, buff=1)

        temp_eq = MathTex(r"T = T_b + L \cdot (h - h_b)")
        temp_eq.next_to(temp_section, DOWN, buff=0.3)
        temp_eq.to_edge(LEFT, buff=1.5)

        # Variable definitions
        temp_vars = VGroup(
            MathTex(r"T_b", r" = \text{Base temperature}"),
            MathTex(r"L", r" = \text{Lapse rate (K/m)}"),
            MathTex(r"h", r" = \text{Altitude}"),
        )
        temp_vars.arrange(DOWN, buff=0.1, aligned_edge=LEFT)
        temp_vars.scale(0.6)
        temp_vars.next_to(temp_eq, DOWN, buff=0.3)
        temp_vars.to_edge(LEFT, buff=2)

        self.play(Write(temp_section))
        self.play(Write(temp_eq))
        self.play(FadeIn(temp_vars))
        self.wait(1)

        # =====================================================================
        # PRESSURE EQUATION (GRADIENT LAYERS)
        # =====================================================================
        # AEROSPACE: This comes from integrating the hydrostatic equation
        # with the linear temperature profile
        press_section = Text("Pressure (Gradient Layers):", font_size=24, color=GREEN)
        press_section.next_to(temp_vars, DOWN, buff=0.5)
        press_section.to_edge(LEFT, buff=1)

        press_eq = MathTex(
            r"P = P_b \left( \frac{T}{T_b} \right)^{-g_0 / (L \cdot R)}"
        )
        press_eq.next_to(press_section, DOWN, buff=0.3)
        press_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(press_section))
        self.play(Write(press_eq))
        self.wait(1)

        # =====================================================================
        # PRESSURE EQUATION (ISOTHERMAL LAYERS)
        # =====================================================================
        # AEROSPACE: When T is constant, pressure decreases exponentially
        iso_section = Text("Pressure (Isothermal Layers):", font_size=24, color=YELLOW)
        iso_section.next_to(press_eq, DOWN, buff=0.5)
        iso_section.to_edge(LEFT, buff=1)

        iso_eq = MathTex(
            r"P = P_b \cdot \exp \left( \frac{-g_0 (h - h_b)}{R \cdot T} \right)"
        )
        iso_eq.next_to(iso_section, DOWN, buff=0.3)
        iso_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(iso_section))
        self.play(Write(iso_eq))
        self.wait(1)

        # =====================================================================
        # DENSITY EQUATION (IDEAL GAS LAW)
        # =====================================================================
        density_section = Text("Density (Ideal Gas Law):", font_size=24, color=ORANGE)
        density_section.next_to(iso_eq, DOWN, buff=0.5)
        density_section.to_edge(LEFT, buff=1)

        density_eq = MathTex(r"\rho = \frac{P}{R \cdot T}")
        density_eq.next_to(density_section, DOWN, buff=0.3)
        density_eq.to_edge(LEFT, buff=1.5)

        self.play(Write(density_section))
        self.play(Write(density_eq))
        self.wait(2)


class ISAProfileScene(Scene):
    """
    Animate the temperature profile through the atmosphere.

    AEROSPACE: Shows the characteristic "zigzag" pattern of temperature
    with altitude - the defining feature of atmospheric structure.

    MANIM: Uses Axes object for coordinate axes and plotting.
    """

    def construct(self):
        # Title
        title = Text("Temperature vs Altitude", font_size=32)
        title.to_edge(UP)
        self.play(Write(title))

        # =====================================================================
        # CREATE COORDINATE AXES
        # =====================================================================
        # MANIM: Axes creates a coordinate system with tick marks and labels
        axes = Axes(
            x_range=[180, 300, 20],   # Temperature: 180K to 300K, tick every 20K
            y_range=[0, 85, 10],       # Altitude: 0 to 85 km, tick every 10 km
            x_length=8,                # Width in Manim units
            y_length=5,                # Height in Manim units
            axis_config={"color": WHITE},
            x_axis_config={"numbers_to_include": [200, 220, 240, 260, 280]},
            y_axis_config={"numbers_to_include": [0, 20, 40, 60, 80]},
        )
        axes.to_edge(DOWN, buff=1)

        # Axis labels
        x_label = Text("Temperature (K)", font_size=20)
        x_label.next_to(axes.x_axis, DOWN, buff=0.3)

        y_label = Text("Altitude (km)", font_size=20)
        y_label.rotate(PI/2)  # Rotate for vertical axis
        y_label.next_to(axes.y_axis, LEFT, buff=0.3)

        self.play(Create(axes), Write(x_label), Write(y_label))

        # =====================================================================
        # ISA LAYER DATA FOR PLOTTING
        # =====================================================================
        # AEROSPACE: These are the key altitude/temperature points from ISA
        # Format: (altitude in km, temperature in K)
        isa_points = [
            (0, 288.15),      # Sea level
            (11, 216.65),     # End of troposphere (-6.5 K/km)
            (20, 216.65),     # End of tropopause (isothermal)
            (32, 228.65),     # End of stratosphere 1 (+1 K/km)
            (47, 270.65),     # End of stratosphere 2 (+2.8 K/km)
            (51, 270.65),     # End of stratopause (isothermal)
            (71, 214.65),     # End of mesosphere 1 (-2.8 K/km)
            (85, 186.65),     # End of mesosphere 2 (-2.0 K/km)
        ]

        # =====================================================================
        # CREATE TEMPERATURE PROFILE LINE
        # =====================================================================
        # MANIM: c2p (coordinates to point) converts axis values to screen coords
        line_points = [axes.c2p(T, h) for h, T in isa_points]

        # MANIM: VMobject with set_points_as_corners creates straight segments
        profile = VMobject()
        profile.set_points_as_corners(line_points)
        profile.set_color(RED)
        profile.set_stroke(width=3)

        # Animate drawing the profile
        self.play(Create(profile), run_time=3)

        # =====================================================================
        # ADD LAYER LABELS
        # =====================================================================
        layer_labels = [
            ("Troposphere", 5, BLUE),      # Average position for label
            ("Tropopause", 15, GREEN),
            ("Stratosphere", 35, YELLOW),
            ("Stratopause", 49, ORANGE),
            ("Mesosphere", 65, PURPLE),
        ]

        for name, h, color in layer_labels:
            T = 260  # X position for label (right side of graph)
            point = axes.c2p(T, h)  # Convert to screen coordinates
            label = Text(name, font_size=14, color=color)
            label.move_to(point)
            self.play(FadeIn(label), run_time=0.3)

        self.wait(2)

        # =====================================================================
        # EXPLANATORY NOTE
        # =====================================================================
        note = Text(
            "Note: Temperature decreases in troposphere,\n"
            "increases in stratosphere (ozone heating)",
            font_size=18,
            color=GRAY
        )
        note.to_edge(DOWN, buff=0.3)
        self.play(Write(note))
        self.wait(2)


# =============================================================================
# MAIN ENTRY POINT
# =============================================================================
if __name__ == "__main__":
    # This file can be rendered with:
    # manim -pql isa_atmosphere.py ISAIntroScene
    # manim -pql isa_atmosphere.py ISALayersScene
    # manim -pql isa_atmosphere.py ISAEquationsScene
    # manim -pql isa_atmosphere.py ISAProfileScene
    #
    # Options:
    #   -p  Preview (auto-open when done)
    #   -q  Quality: l=low, m=medium, h=high, k=4K
    #   -l  Same as -ql (low quality, fast)
    pass
