"""
Reusable orbit Mobjects for Manim animations.

=============================================================================
AEROSPACE CONCEPT: Orbital Paths and Conic Sections
=============================================================================

All orbits are conic sections - curves formed by intersecting a cone with
a plane. The type of orbit depends on the eccentricity (e):

    e = 0        Circle      (bound orbit)
    0 < e < 1    Ellipse     (bound orbit)
    e = 1        Parabola    (escape trajectory)
    e > 1        Hyperbola   (escape trajectory)

ORBITAL GEOMETRY:
----------------

                 Apoapsis (farthest point)
                        *
                      / | \
                     /  |  \       a = semi-major axis
                    /   |   \      b = semi-minor axis
         Periapsis *----O----*     c = focal distance = a * e
                    \   |   /      e = eccentricity = c / a
                     \  |  /
                      \ | /
                        *
              (Central body at focus O)

ELLIPSE EQUATION (polar form from focus):
    r = a(1 - e^2) / (1 + e * cos(theta))

Where:
- r = distance from focus (central body)
- a = semi-major axis
- e = eccentricity
- theta = true anomaly (angle from periapsis)

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. CLASS INHERITANCE (VMobject, VGroup)
   `class OrbitPath(VMobject):`
   VMobject = Vector Mobject (mathematical object with vector graphics)
   VGroup = Group of VMobjects that can be manipulated as one

2. SUPER().__INIT__(**KWARGS)
   Calls parent class constructor, passing keyword arguments.
   Essential for proper Manim object initialization.

3. **KWARGS (KEYWORD ARGUMENTS)
   Collects extra keyword arguments into a dictionary.
   Allows passing Manim parameters (color, opacity, etc.) through.

4. NUMPY ARRAYS FOR POSITIONS
   `np.array([x, y, z])` creates a 3D point/vector.
   Manim uses NumPy for all geometry calculations.

5. LIST COMPREHENSIONS
   `[np.array(p) + shift for p in points]`
   Concise way to transform each element in a list.

Provides classes for creating and animating orbital paths.
"""

from manim import *
import numpy as np


class OrbitPath(VMobject):
    """
    A circular or elliptical orbit path.

    AEROSPACE: Orbits can be circular (e=0) or elliptical (0<e<1).
    The central body (planet/star) sits at one focus of the ellipse.

    MANIM: VMobject is the base class for all vector graphics objects.
    It stores points that define Bezier curves for smooth rendering.

    Parameters
    ----------
    radius : float
        Semi-major axis (or radius for circular orbit)
    eccentricity : float
        Orbital eccentricity (0 = circle, 0-1 = ellipse)
    color : Color
        Stroke color for the orbit path
    stroke_width : float
        Line thickness
    """

    def __init__(
        self,
        radius=2,
        eccentricity=0,
        color=BLUE,
        stroke_width=2,
        **kwargs
    ):
        # PYTHON: super().__init__() calls parent class constructor
        # **kwargs passes any extra arguments (opacity, z_index, etc.)
        super().__init__(**kwargs)

        # Store parameters as instance attributes
        self.radius = radius
        self.eccentricity = eccentricity

        if eccentricity == 0:
            # =================================================================
            # CIRCULAR ORBIT (e = 0)
            # =================================================================
            # AEROSPACE: A circle is a special case where both foci coincide
            # at the center. All points are equidistant from the center.
            # v = sqrt(mu/r) everywhere on the orbit.
            circle = Circle(radius=radius, color=color, stroke_width=stroke_width)
            self.add(circle)
        else:
            # =================================================================
            # ELLIPTICAL ORBIT (0 < e < 1)
            # =================================================================
            # AEROSPACE: The central body sits at one focus of the ellipse.
            # We need to shift the ellipse so the focus is at the origin.

            a = radius  # Semi-major axis (longest "radius")

            # Semi-minor axis from eccentricity
            # MATH: b = a * sqrt(1 - e^2) comes from ellipse geometry
            b = a * np.sqrt(1 - eccentricity**2)

            # MANIM: Ellipse takes width and height (2*a and 2*b)
            ellipse = Ellipse(width=2*a, height=2*b, color=color, stroke_width=stroke_width)

            # Focal distance: c = a * e
            # AEROSPACE: The central body is at a focus, not the center
            c = a * eccentricity

            # Shift ellipse left so the right focus is at origin
            ellipse.shift(LEFT * c)

            self.add(ellipse)


class TransferOrbit(VMobject):
    """
    A Hohmann transfer orbit (half ellipse).

    AEROSPACE: The Hohmann transfer is the most fuel-efficient two-burn
    maneuver to transfer between two circular coplanar orbits.

    The transfer orbit is an ellipse with:
    - Periapsis (closest point) tangent to the inner orbit
    - Apoapsis (farthest point) tangent to the outer orbit

    Only half the ellipse is used (180 degrees of travel).

    Parameters
    ----------
    r1 : float
        Initial orbit radius
    r2 : float
        Final orbit radius
    color : Color
        Stroke color for the transfer path
    """

    def __init__(
        self,
        r1,
        r2,
        color=ORANGE,
        stroke_width=2,
        **kwargs
    ):
        super().__init__(**kwargs)
        self.r1 = r1
        self.r2 = r2

        # =================================================================
        # TRANSFER ELLIPSE GEOMETRY
        # =================================================================

        # Semi-major axis = average of inner and outer radii
        # AEROSPACE: a = (r_periapsis + r_apoapsis) / 2
        a = (r1 + r2) / 2

        # Eccentricity of the transfer ellipse
        # AEROSPACE: e = (r_apo - r_peri) / (r_apo + r_peri)
        e = abs(r2 - r1) / (r2 + r1)

        # Semi-minor axis from eccentricity
        b = a * np.sqrt(1 - e**2)

        # =================================================================
        # CREATE HALF ELLIPSE (TRANSFER PATH)
        # =================================================================
        # MANIM: We generate points manually and use set_points_smoothly()
        # to create a smooth curve through them.
        points = []
        numPoints = 100

        # PYTHON: range(numPoints + 1) generates 0, 1, 2, ..., numPoints
        for i in range(numPoints + 1):
            # Angle from 0 to PI (half orbit)
            theta = i * PI / numPoints

            # Parametric ellipse centered at origin
            # MATH: x = a*cos(theta), y = b*sin(theta)
            x = a * np.cos(theta)
            y = b * np.sin(theta)
            points.append([x, y, 0])

        # =================================================================
        # SHIFT TO PLACE FOCUS AT ORIGIN
        # =================================================================
        # Focal distance
        c = a * e

        # If r1 < r2 (transferring outward), periapsis is on the right
        # so we shift the ellipse left to put the focus at origin
        if r1 < r2:
            shift = LEFT * c
        else:
            shift = RIGHT * c

        # PYTHON: List comprehension transforms each point
        # MANIM: set_points_smoothly creates Bezier curves through points
        self.set_points_smoothly([np.array(p) + np.array([shift[0], shift[1], 0]) for p in points])
        self.set_color(color)
        self.set_stroke(width=stroke_width)


class Spacecraft(VGroup):
    """
    A simple spacecraft icon.

    MANIM: VGroup is a container for multiple VMobjects.
    The spacecraft is composed of a body (triangle) and exhaust (smaller triangle).

    Using VGroup means:
    - All parts move together
    - All parts can be styled together
    - The group can be animated as a single unit
    """

    def __init__(self, color=RED, scale=0.3, **kwargs):
        super().__init__(**kwargs)

        # Create a simple triangle/arrow shape for the body
        body = Triangle(color=color, fill_opacity=1)
        body.scale(scale)
        body.rotate(-PI/2)  # Point right (prograde direction)

        # Add a small exhaust plume (suggests thrust)
        exhaust = Triangle(color=YELLOW, fill_opacity=0.8)
        exhaust.scale(scale * 0.5)
        exhaust.rotate(PI/2)  # Point opposite to body
        # MANIM: next_to positions relative to another object
        exhaust.next_to(body, LEFT, buff=0)

        # MANIM: self.add() adds child objects to the group
        self.add(body, exhaust)


class CentralBody(VGroup):
    """
    A central body (planet/star) for orbit visualization.

    AEROSPACE: In orbital mechanics, everything orbits around a central
    body (the "primary"). The central body sits at the focus of all orbits.

    Parameters
    ----------
    radius : float
        Visual radius of the body
    color : Color
        Body color
    name : str, optional
        Label to display below the body
    """

    def __init__(self, radius=0.5, color=BLUE, name=None, **kwargs):
        super().__init__(**kwargs)

        # Main body as a filled circle
        body = Circle(radius=radius, color=color, fill_opacity=1)
        self.add(body)

        # Optional name label
        # PYTHON: `if name:` is True if name is not None and not empty
        if name:
            label = Text(name, font_size=20)
            label.next_to(body, DOWN, buff=0.2)
            self.add(label)


def animateAlongOrbit(mobject, orbit, run_time=3, rate_func=linear):
    """
    Create an animation of a mobject moving along an orbit.

    MANIM: MoveAlongPath is a built-in animation that moves an object
    along any path (VMobject) over a specified time.

    Parameters
    ----------
    mobject : Mobject
        The object to animate (e.g., Spacecraft)
    orbit : VMobject
        The path to follow (e.g., OrbitPath, TransferOrbit)
    run_time : float
        Animation duration in seconds
    rate_func : callable
        Timing function (linear = constant speed)

    Returns
    -------
    Animation
        A MoveAlongPath animation ready to be played
    """
    return MoveAlongPath(mobject, orbit, run_time=run_time, rate_func=rate_func)


def getPositionOnCircle(radius, angle):
    """
    Get position on a circular orbit at given angle.

    MATH: Parametric circle equation
        x = r * cos(theta)
        y = r * sin(theta)
        z = 0 (2D visualization)

    Parameters
    ----------
    radius : float
        Orbital radius
    angle : float
        Angle in radians (0 = right, PI/2 = top)

    Returns
    -------
    np.ndarray
        3D position vector [x, y, z]
    """
    return np.array([
        radius * np.cos(angle),
        radius * np.sin(angle),
        0
    ])


def getPositionOnEllipse(a, e, angle):
    """
    Get position on an ellipse at given true anomaly.

    AEROSPACE: Uses the orbit equation in polar form:
        r = a(1 - e^2) / (1 + e*cos(theta))

    Where theta is the TRUE ANOMALY - the angle from periapsis
    to the current position, measured from the focus.

    Parameters
    ----------
    a : float
        Semi-major axis
    e : float
        Eccentricity (0 to 1)
    angle : float
        True anomaly in radians

    Returns
    -------
    np.ndarray
        3D position vector [x, y, z]
    """
    # AEROSPACE: The orbit equation gives distance from focus
    r = a * (1 - e**2) / (1 + e * np.cos(angle))

    # Convert polar to Cartesian
    return np.array([
        r * np.cos(angle),
        r * np.sin(angle),
        0
    ])
