"""
Spacecraft graphics components for Manim animations.

=============================================================================
AEROSPACE CONCEPT: Spacecraft Visualization for Education
=============================================================================

Visualizing spacecraft helps communicate key aerospace concepts:

1. ROCKET STRUCTURE:
   - Nose cone: Aerodynamic fairing to reduce drag
   - Body: Contains propellant tanks and payload
   - Fins: Provide stability during atmospheric flight
   - Nozzle: Where exhaust gases exit (thrust generation)

2. PROPELLANT REPRESENTATION:
   - Shows fuel consumption during burns
   - Illustrates mass ratio (m0/mf) from rocket equation
   - Green fill = propellant remaining

3. EXHAUST VISUALIZATION:
   - Flame indicates active burn (thrust)
   - Flame color indicates temperature:
     - Yellow (inner): Hottest (~3000K)
     - Orange (outer): Cooler exhaust

4. DELTA-V ARROWS:
   - Show direction and magnitude of velocity changes
   - Prograde: Same direction as motion (speed up)
   - Retrograde: Opposite to motion (slow down)

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. VGROUP COMPOSITION
   `class Rocket(VGroup)` - VGroup is a container for multiple Mobjects.
   All children move/scale/rotate together as a unit.
   Use `self.add(child1, child2, ...)` to add components.

2. POLYGON FOR CUSTOM SHAPES
   `Polygon([x1, y1, 0], [x2, y2, 0], ...)` creates a shape from vertices.
   Vertices are specified as 3D points (z=0 for 2D).

3. VALUETRACKER AND UPDATERS
   `ValueTracker(initial_value)` stores a numeric value that can be animated.
   `mobject.add_updater(func)` calls func(mobject) every frame.
   Used for dynamic visualizations (e.g., propellant draining).

4. THE `become()` METHOD
   `mobject.become(new_mobject)` replaces mobject's appearance.
   Used in updaters to completely redraw an object each frame.

5. INSTANCE ATTRIBUTES FOR ANIMATION ACCESS
   `self.body = body` stores references to child objects.
   Allows external code to animate specific parts.

Provides visual elements for rockets and spacecraft.
"""

from manim import *
import numpy as np


class Rocket(VGroup):
    """
    A simple rocket graphic composed of body, nose cone, and fins.

    AEROSPACE: This is a stylized representation of a typical rocket.
    Real rockets have much more complex structures including multiple
    stages, propellant tanks, and engine assemblies.

    MANIM: Extends VGroup (Vector Group), which is a container for
    multiple vector objects that can be manipulated as a single unit.

    Parameters
    ----------
    body_height : float
        Height of the main rocket body [Manim units]
    body_width : float
        Width of the main rocket body [Manim units]
    color : Color
        Color of the rocket body
    nose_color : Color
        Color of the nose cone (typically red for visibility)
    fin_color : Color
        Color of the stabilizing fins
    """

    def __init__(
        self,
        body_height=1.5,
        body_width=0.4,
        color=WHITE,
        nose_color=RED,
        fin_color=GRAY,
        **kwargs
    ):
        # PYTHON: super().__init__(**kwargs) calls parent class constructor
        # **kwargs passes any extra Manim parameters (opacity, z_index, etc.)
        super().__init__(**kwargs)

        # =====================================================================
        # ROCKET BODY
        # =====================================================================
        # MANIM: Rectangle creates a rectangular shape centered at origin
        body = Rectangle(
            height=body_height,
            width=body_width,
            color=color,
            fill_opacity=1
        )

        # =====================================================================
        # NOSE CONE (Aerodynamic Fairing)
        # =====================================================================
        # AEROSPACE: The nose cone reduces aerodynamic drag during
        # atmospheric ascent. Shape affects wave drag at supersonic speeds.
        #
        # MANIM: Polygon creates a shape from a list of vertices.
        # Each vertex is [x, y, z] - Manim uses 3D coordinates.
        nose = Polygon(
            [-body_width/2, body_height/2, 0],      # Left base
            [body_width/2, body_height/2, 0],       # Right base
            [0, body_height/2 + body_width, 0],     # Apex (pointed top)
            color=nose_color,
            fill_opacity=1
        )

        # =====================================================================
        # STABILIZING FINS
        # =====================================================================
        # AEROSPACE: Fins provide stability during atmospheric flight.
        # They keep the rocket pointing in the direction of travel
        # by creating restoring forces if the rocket starts to rotate.
        fin_height = body_height * 0.3
        fin_width = body_width * 0.6

        # Left fin - triangular shape
        left_fin = Polygon(
            [-body_width/2, -body_height/2, 0],              # Inner top
            [-body_width/2, -body_height/2 + fin_height, 0], # Inner bottom
            [-body_width/2 - fin_width, -body_height/2, 0],  # Outer point
            color=fin_color,
            fill_opacity=1
        )

        # Right fin - mirror of left fin
        right_fin = Polygon(
            [body_width/2, -body_height/2, 0],
            [body_width/2, -body_height/2 + fin_height, 0],
            [body_width/2 + fin_width, -body_height/2, 0],
            color=fin_color,
            fill_opacity=1
        )

        # MANIM: self.add() adds child objects to the VGroup
        # All children will move, scale, and rotate together
        self.add(body, nose, left_fin, right_fin)

        # Store references for external animation access
        # PYTHON: Instance attributes allow other code to modify specific parts
        self.body = body
        self.nose = nose


class RocketWithExhaust(VGroup):
    """
    A rocket with animated exhaust flames for thrust visualization.

    AEROSPACE: The exhaust plume indicates active thrust. In reality,
    exhaust characteristics depend on:
    - Chamber pressure and temperature
    - Propellant type (LOX/RP-1, LOX/LH2, solid, etc.)
    - Atmospheric pressure (expansion in vacuum)

    MANIM: Composition pattern - this class contains a Rocket
    rather than inheriting from it. Allows independent control.
    """

    def __init__(self, rocket_scale=0.5, **kwargs):
        super().__init__(**kwargs)

        # Create rocket using composition (has-a relationship)
        # PYTHON: Composition is often preferred over inheritance
        self.rocket = Rocket()
        self.rocket.scale(rocket_scale)

        # Create exhaust flames
        self.exhaust = self._createExhaust(rocket_scale)

        self.add(self.rocket, self.exhaust)

    def _createExhaust(self, scale):
        """
        Create exhaust flame graphic with hot inner core.

        AEROSPACE: Real rocket exhaust has temperature gradients:
        - Core: 2500-3500 K (brightest, yellow-white)
        - Outer: 1500-2000 K (orange-red)
        - Edge: Cooler mixing with atmosphere

        PYTHON: Methods starting with _ are conventionally private.
        They're meant for internal use only.
        """
        body_width = 0.4 * scale
        exhaust_height = 0.8 * scale

        # Outer flame (cooler, orange)
        flame = Polygon(
            [-body_width/2 * 0.8, -0.75 * scale, 0],
            [body_width/2 * 0.8, -0.75 * scale, 0],
            [0, -0.75 * scale - exhaust_height, 0],
            color=ORANGE,
            fill_opacity=0.9
        )

        # Inner flame (hotter, yellow) - smaller and brighter
        inner_flame = Polygon(
            [-body_width/4, -0.75 * scale, 0],
            [body_width/4, -0.75 * scale, 0],
            [0, -0.75 * scale - exhaust_height * 0.7, 0],
            color=YELLOW,
            fill_opacity=1
        )

        # MANIM: VGroup combines multiple objects into one
        exhaust = VGroup(flame, inner_flame)
        return exhaust

    def setExhaustVisible(self, visible):
        """
        Show or hide exhaust to indicate engine on/off.

        AEROSPACE: Engines don't run continuously - they fire for
        specific burns and coast between maneuvers.

        Parameters
        ----------
        visible : bool
            True to show exhaust (engine on), False to hide (coasting)
        """
        # MANIM: set_opacity(1) = fully visible, set_opacity(0) = invisible
        self.exhaust.set_opacity(1 if visible else 0)


class PropellantTank(VGroup):
    """
    Visual representation of a propellant tank with animated fill level.

    AEROSPACE: Propellant tanks typically use:
    - Cryogenic tanks for LOX, LH2 (insulated)
    - Room-temperature tanks for RP-1, hypergolics
    - Pressurized tanks for attitude control propellants

    The fill level visualizes the mass ratio:
    - Full tank: m0 (initial mass)
    - Empty tank: mf (final/dry mass)
    - Mass ratio: MR = m0/mf

    MANIM: Uses ValueTracker and updater pattern for animation.
    """

    def __init__(
        self,
        width=1,
        height=2,
        fill_level=1.0,
        tank_color=GRAY,
        propellant_color=GREEN,
        **kwargs
    ):
        super().__init__(**kwargs)

        # Store dimensions for updater calculations
        self.width = width
        self.height = height
        self.fill_level = fill_level

        # =====================================================================
        # TANK OUTLINE
        # =====================================================================
        # MANIM: RoundedRectangle has rounded corners (corner_radius)
        # Makes the tank look more realistic
        self.tank = RoundedRectangle(
            width=width,
            height=height,
            corner_radius=0.1,
            color=tank_color,
            fill_opacity=0.3,  # Semi-transparent to see propellant
            stroke_width=2
        )

        # =====================================================================
        # PROPELLANT FILL
        # =====================================================================
        fill_height = height * fill_level * 0.9  # Leave margin at top
        self.propellant = Rectangle(
            width=width * 0.9,  # Slightly narrower than tank
            height=fill_height,
            color=propellant_color,
            fill_opacity=0.7,
            stroke_width=0  # No outline - just solid fill
        )
        # MANIM: align_to positions one object relative to another
        self.propellant.align_to(self.tank, DOWN)  # Align bottoms
        self.propellant.shift(UP * height * 0.05)  # Small offset from bottom

        self.add(self.tank, self.propellant)

    def getFillUpdater(self, value_tracker):
        """
        Return an updater function that animates fill level.

        MANIM: Updaters are functions called every frame to update an object.
        Pattern: mobject.add_updater(lambda m: update_function(m))

        The updater reads from a ValueTracker and redraws the propellant
        rectangle with the new fill level.

        Parameters
        ----------
        value_tracker : ValueTracker
            Manim ValueTracker containing current fill level (0 to 1)

        Returns
        -------
        callable
            Updater function to attach to the propellant Mobject
        """
        # PYTHON: Closure - inner function captures outer variables
        # (self, value_tracker) are available inside updater
        def updater(mob):
            fill_level = value_tracker.get_value()
            fill_height = self.height * fill_level * 0.9

            # MANIM: become() replaces the mobject with a new one
            # This completely redraws the propellant each frame
            mob.become(
                Rectangle(
                    width=self.width * 0.9,
                    height=max(fill_height, 0.01),  # Minimum height to avoid zero
                    color=mob.get_color(),          # Preserve original color
                    fill_opacity=0.7,
                    stroke_width=0
                ).align_to(self.tank, DOWN).shift(UP * self.height * 0.05)
            )
        return updater


class MassIndicator(VGroup):
    """
    Visual indicator showing current mass value.

    AEROSPACE: Tracking mass is crucial for:
    - Delta-v calculations (rocket equation)
    - Center of mass changes during burn
    - Staging decisions

    MANIM: Uses DecimalNumber for animated numeric display.
    """

    def __init__(self, label="m", value=1000, unit="kg", **kwargs):
        super().__init__(**kwargs)

        # MANIM: MathTex for mathematical label (renders LaTeX)
        self.label_text = MathTex(f"{label} = ")

        # MANIM: DecimalNumber automatically formats and can be animated
        # num_decimal_places controls precision of display
        self.value_text = DecimalNumber(value, num_decimal_places=0)

        # MANIM: Tex for regular text (unit label)
        self.unit_text = Tex(f" {unit}")

        self.add(self.label_text, self.value_text, self.unit_text)
        # MANIM: arrange(RIGHT, buff=0.1) positions children in a row
        self.arrange(RIGHT, buff=0.1)

    def getValueUpdater(self, value_tracker):
        """
        Return an updater for animated value display.

        MANIM: DecimalNumber has a set_value() method that smoothly
        updates the displayed number.
        """
        def updater(mob):
            mob.set_value(value_tracker.get_value())
        return updater


class VelocityVector(VGroup):
    """
    Velocity vector visualization with optional label.

    AEROSPACE: Velocity vectors show:
    - Direction of motion (tangent to orbit for circular)
    - Magnitude (arrow length proportional to speed)

    Important velocity directions:
    - Prograde: Direction of motion
    - Retrograde: Opposite to motion
    - Normal: Perpendicular to orbital plane
    - Radial: Toward/away from central body
    """

    def __init__(
        self,
        direction=UP,
        magnitude=1,
        color=YELLOW,
        label=None,
        **kwargs
    ):
        super().__init__(**kwargs)

        # MANIM: Arrow creates an arrow with head at the end
        # buff=0 means arrow starts exactly at start point
        self.arrow = Arrow(
            ORIGIN,
            direction * magnitude,
            color=color,
            buff=0
        )
        self.add(self.arrow)

        # Optional label (usually Greek letter or dv symbol)
        # PYTHON: `if label:` is True if label is not None and not empty
        if label:
            self.label = MathTex(label, color=color)
            self.label.next_to(self.arrow, direction, buff=0.1)
            self.add(self.label)


def createDeltaVArrow(start, end, color=RED, label=r"\Delta v"):
    """
    Create a delta-v arrow with label for burn visualization.

    AEROSPACE: Delta-v arrows show the velocity change from a burn.
    - Magnitude indicates fuel required (via rocket equation)
    - Direction indicates burn direction (prograde, retrograde, etc.)

    MANIM: Returns a VGroup containing arrow and label, so they
    can be animated together.

    Parameters
    ----------
    start : array-like
        Starting position [x, y, z]
    end : array-like
        Ending position [x, y, z]
    color : Color
        Arrow and label color
    label : str
        LaTeX label (default is Delta-v symbol)

    Returns
    -------
    VGroup
        Arrow with attached label
    """
    # MANIM: Arrow from start to end point
    arrow = Arrow(start, end, color=color, buff=0)

    # MANIM: MathTex renders LaTeX mathematics
    # r"\Delta v" renders as the Greek capital delta followed by v
    label_mob = MathTex(label, color=color)

    # Position label above or below based on arrow direction
    # PYTHON: end[1] > start[1] checks if arrow points up
    label_mob.next_to(arrow, UP if end[1] > start[1] else DOWN, buff=0.1)

    return VGroup(arrow, label_mob)
