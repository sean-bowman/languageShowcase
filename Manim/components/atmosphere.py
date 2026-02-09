"""
Atmosphere visualization components for Manim.

=============================================================================
AEROSPACE CONCEPT: International Standard Atmosphere (ISA)
=============================================================================

The ISA is a reference model of how atmospheric properties vary with altitude.
It's used worldwide for aircraft performance, altimeter calibration, and engine
specifications.

ISA TEMPERATURE PROFILE:
-----------------------

Temperature does not decrease uniformly with altitude. The atmosphere has
distinct layers with different temperature behaviors:

    Altitude (km)  |  Layer         |  Temperature Behavior
    ---------------|----------------|------------------------
    0 - 11         |  Troposphere   |  Decreases: -6.5 deg C/km
    11 - 20        |  Tropopause    |  Constant: -56.5 deg C
    20 - 32        |  Stratosphere  |  Increases: +1 deg C/km
    32 - 47        |  Stratosphere  |  Increases: +2.8 deg C/km
    47 - 51        |  Stratopause   |  Constant: -2.5 deg C
    51 - 71        |  Mesosphere    |  Decreases: -2.8 deg C/km
    71 - 85        |  Mesosphere    |  Decreases: -2.0 deg C/km

ISA PRESSURE CALCULATION:
------------------------

Pressure calculation differs for gradient and isothermal layers:

GRADIENT LAYERS (lapse rate != 0):
    P = P_b * (T / T_b)^(-g0 / (L * R))

ISOTHERMAL LAYERS (lapse rate = 0):
    P = P_b * exp(-g0 * h / (R * T))

Where:
    P_b = pressure at base of layer
    T_b = temperature at base of layer
    L = lapse rate [K/m]
    g0 = standard gravity [9.80665 m/s^2]
    R = gas constant for air [287.058 J/(kg*K)]

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. SYS.PATH MANIPULATION
   `sys.path.insert(0, dirname)` adds a directory to the import search path.
   Allows importing from sibling directories.

2. DICTIONARY ACCESS IN LOOPS
   `for layer in isaLayers:` iterates through list of dictionaries.
   `layer["hBase"]` accesses dictionary values by key.

3. VMobject CURVE CREATION
   `set_points_smoothly(points)` creates a smooth curve through points.
   More visually pleasing than straight line segments.

4. SCALE FACTORS
   Real-world values (meters, Pascals) are converted to visual units
   for reasonable display in Manim's coordinate system.

Provides visual elements for atmospheric layers and profiles.
"""

from manim import *
import numpy as np
import sys
import os

# PYTHON: Modify sys.path to allow importing from parent directory
# os.path.dirname() gets the parent directory
# This is necessary because Python doesn't automatically search parent directories
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from data.constants import isaLayers, isaT0, isaP0, g0, rAir


class AtmosphereLayer(VGroup):
    """
    Visual representation of an atmospheric layer.

    AEROSPACE: Each atmospheric layer is shown as a colored rectangle
    at the correct altitude range. The visualization helps understand
    the vertical structure of the atmosphere.

    Parameters
    ----------
    name : str
        Layer name (e.g., "Troposphere")
    hBase : float
        Base altitude in meters
    hTop : float
        Top altitude in meters
    color : Color
        Layer color
    width : float
        Rectangle width in Manim units
    opacity : float
        Fill opacity (0-1)
    scale_factor : float
        Conversion from meters to Manim units
    """

    def __init__(
        self,
        name,
        hBase,
        hTop,
        color,
        width=5,
        opacity=0.3,
        scale_factor=0.0001,  # Converts meters to visual units
        **kwargs
    ):
        super().__init__(**kwargs)
        self.name = name
        self.hBase = hBase
        self.hTop = hTop

        # Calculate visual dimensions
        # VISUALIZATION: scale_factor converts real meters to Manim coordinates
        # Without scaling, 11000 m would be huge on screen
        height = (hTop - hBase) * scale_factor
        y_base = hBase * scale_factor

        # Create layer rectangle
        # MANIM: Rectangle centered at origin by default
        self.layer_rect = Rectangle(
            width=width,
            height=height,
            color=color,
            fill_opacity=opacity,
            stroke_width=1
        )
        # Position: move center to correct Y position
        self.layer_rect.move_to(ORIGIN)
        self.layer_rect.shift(UP * (y_base + height/2))

        # Create label with layer name
        self.label = Text(name, font_size=24, color=color)
        self.label.next_to(self.layer_rect, RIGHT, buff=0.3)

        # Create altitude markers at base and top
        self.hBase_text = Text(f"{hBase/1000:.0f} km", font_size=16)
        self.hBase_text.next_to(self.layer_rect, LEFT, buff=0.2)
        self.hBase_text.align_to(self.layer_rect, DOWN)

        self.hTop_text = Text(f"{hTop/1000:.0f} km", font_size=16)
        self.hTop_text.next_to(self.layer_rect, LEFT, buff=0.2)
        self.hTop_text.align_to(self.layer_rect, UP)

        # Add all elements to the group
        self.add(self.layer_rect, self.label, self.hBase_text, self.hTop_text)


def calculateIsaTemperature(altitude):
    """
    Calculate ISA temperature at given altitude.

    AEROSPACE: Temperature varies with altitude according to defined
    lapse rates in each atmospheric layer.

    For gradient layers: T = tBase + lapse_rate * (h - hBase)
    For isothermal layers (lapse_rate = 0): T = tBase (constant)

    Parameters
    ----------
    altitude : float
        Altitude in meters

    Returns
    -------
    float
        Temperature in Kelvin
    """
    # PYTHON: Iterate through layers to find the one containing this altitude
    for layer in isaLayers:
        if layer["hBase"] <= altitude < layer["hTop"]:
            # AEROSPACE: Linear temperature variation within layer
            T = layer["tBase"] + layer["lapse"] * (altitude - layer["hBase"])
            return T

    # Above defined layers - return temperature of highest layer
    return isaLayers[-1]["tBase"]


def calculateIsaPressure(altitude):
    """
    Calculate ISA pressure at given altitude.

    AEROSPACE: Pressure must be calculated layer by layer from the ground up,
    because each layer's base pressure depends on the layer below.

    Two different formulas are used:
    - Gradient layers: P = P_b * (T/T_b)^(-g0/(L*R))
    - Isothermal layers: P = P_b * exp(-g0*h/(R*T))

    Parameters
    ----------
    altitude : float
        Altitude in meters

    Returns
    -------
    float
        Pressure in Pascals
    """
    # Start at sea level
    P = isaP0  # 101325 Pa
    T_prev = isaT0  # 288.15 K

    # ALGORITHM: Process each layer from bottom to top
    for layer in isaLayers:
        if altitude < layer["hBase"]:
            # Haven't reached this layer yet
            break

        # Height within this layer (may be partial if altitude is in this layer)
        h_in_layer = min(altitude, layer["hTop"]) - layer["hBase"]

        if layer["lapse"] != 0:
            # GRADIENT LAYER: Temperature varies with altitude
            # P = P_b * (T/T_b)^(-g0/(L*R))
            T = layer["tBase"] + layer["lapse"] * h_in_layer
            P = P * (T / T_prev) ** (-g0 / (layer["lapse"] * rAir))
        else:
            # ISOTHERMAL LAYER: Temperature is constant
            # P = P_b * exp(-g0*h/(R*T))
            T = layer["tBase"]
            P = P * np.exp(-g0 * h_in_layer / (rAir * T))

        T_prev = T

        if altitude <= layer["hTop"]:
            # We're in this layer, done calculating
            break

    return P


def createTemperatureProfile(
    max_altitude=85000,
    num_points=200,
    x_scale=0.03,
    y_scale=0.00005,
    color=RED
):
    """
    Create a temperature vs altitude curve for visualization.

    AEROSPACE: The resulting curve shows the characteristic "zigzag"
    pattern of atmospheric temperature - falling in troposphere,
    constant in tropopause, rising in stratosphere, etc.

    Parameters
    ----------
    max_altitude : float
        Maximum altitude in meters
    num_points : int
        Number of points to sample
    x_scale : float
        Scale factor for temperature axis
    y_scale : float
        Scale factor for altitude axis
    color : Color
        Curve color

    Returns
    -------
    VMobject
        Smooth curve showing temperature profile
    """
    points = []

    # Sample temperature at many altitudes
    for i in range(num_points):
        h = i * max_altitude / num_points
        T = calculateIsaTemperature(h)

        # Convert to visual coordinates
        x = T * x_scale
        y = h * y_scale

        points.append([x, y, 0])

    # MANIM: Create smooth curve through all points
    curve = VMobject()
    curve.set_points_smoothly([np.array(p) for p in points])
    curve.set_color(color)
    curve.set_stroke(width=3)

    return curve


def createPressureProfile(
    max_altitude=85000,
    num_points=200,
    x_scale=0.00003,
    y_scale=0.00005,
    color=BLUE
):
    """
    Create a pressure vs altitude curve (logarithmic x-axis effect).

    AEROSPACE: Pressure decreases roughly exponentially with altitude.
    Using log scale for the x-axis makes the variation visible across
    the full altitude range.

    At sea level: P = 101,325 Pa
    At 5.5 km: P ~ 50,000 Pa (half)
    At 11 km: P ~ 22,600 Pa (22%)
    At 30 km: P ~ 1,200 Pa (1.2%)

    Parameters
    ----------
    max_altitude : float
        Maximum altitude in meters
    num_points : int
        Number of points to sample
    x_scale : float
        Scale factor for (log) pressure axis
    y_scale : float
        Scale factor for altitude axis
    color : Color
        Curve color

    Returns
    -------
    VMobject
        Smooth curve showing pressure profile
    """
    points = []

    for i in range(num_points):
        h = i * max_altitude / num_points
        P = calculateIsaPressure(h)

        # MATH: Use log10 scale for pressure to make it visible
        # Without log, high-altitude pressures would be invisible
        x = np.log10(max(P, 1)) * x_scale * 20
        y = h * y_scale

        points.append([x, y, 0])

    curve = VMobject()
    curve.set_points_smoothly([np.array(p) for p in points])
    curve.set_color(color)
    curve.set_stroke(width=3)

    return curve


class TemperatureAxis(VGroup):
    """
    Temperature axis for atmospheric profile chart.

    Creates a horizontal axis with tick marks and labels for temperature
    values in Kelvin.
    """

    def __init__(self, length=6, T_min=180, T_max=300, **kwargs):
        super().__init__(**kwargs)

        # Main axis line
        axis = Line(ORIGIN, RIGHT * length, color=WHITE)
        self.add(axis)

        # Create tick marks and labels
        num_ticks = 5
        for i in range(num_ticks + 1):
            # Position along axis
            x = i * length / num_ticks
            # Corresponding temperature value
            T = T_min + i * (T_max - T_min) / num_ticks

            # Tick mark
            tick = Line(UP * 0.1, DOWN * 0.1, color=WHITE)
            tick.move_to(RIGHT * x)
            self.add(tick)

            # Value label
            label = Text(f"{T:.0f}", font_size=14)
            label.next_to(tick, DOWN, buff=0.15)
            self.add(label)

        # Axis title
        axis_label = Text("Temperature (K)", font_size=18)
        axis_label.next_to(axis, DOWN, buff=0.5)
        self.add(axis_label)


class AltitudeAxis(VGroup):
    """
    Altitude axis for atmospheric profile chart.

    Creates a vertical axis with tick marks and labels for altitude
    values in kilometers.
    """

    def __init__(self, length=4, h_max=85, **kwargs):
        super().__init__(**kwargs)

        # Main axis line (vertical)
        axis = Line(ORIGIN, UP * length, color=WHITE)
        self.add(axis)

        # Create tick marks and labels
        num_ticks = 5
        for i in range(num_ticks + 1):
            # Position along axis
            y = i * length / num_ticks
            # Corresponding altitude value
            h = i * h_max / num_ticks

            # Tick mark
            tick = Line(LEFT * 0.1, RIGHT * 0.1, color=WHITE)
            tick.move_to(UP * y)
            self.add(tick)

            # Value label
            label = Text(f"{h:.0f}", font_size=14)
            label.next_to(tick, LEFT, buff=0.15)
            self.add(label)

        # Axis title (rotated for vertical axis)
        axis_label = Text("Altitude (km)", font_size=18)
        axis_label.rotate(PI/2)
        axis_label.next_to(axis, LEFT, buff=0.7)
        self.add(axis_label)
