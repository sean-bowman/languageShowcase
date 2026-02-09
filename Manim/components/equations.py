"""
Common aerospace equations as Manim MathTex objects.

=============================================================================
AEROSPACE CONCEPT: Key Equations of Orbital and Atmospheric Flight
=============================================================================

This module provides LaTeX-formatted equations for Manim animations.
Each equation is fundamental to aerospace engineering:

1. ISA (INTERNATIONAL STANDARD ATMOSPHERE) EQUATIONS:
   - Temperature: T = T_b + L * (h - h_b)
   - Pressure (gradient): P = P_b * (T/T_b)^(-g0/LR)
   - Pressure (isothermal): P = P_b * exp(-g0*h / RT)
   - Density: rho = P / (R*T)  [Ideal gas law]
   - Speed of sound: a = sqrt(gamma * R * T)

2. ORBITAL MECHANICS EQUATIONS:
   - Circular velocity: v = sqrt(mu/r)
   - Vis-viva equation: v^2 = mu * (2/r - 1/a)
   - Orbital period: T = 2*pi*sqrt(a^3/mu)  [Kepler's 3rd Law]
   - Escape velocity: v_esc = sqrt(2*mu/r) = sqrt(2) * v_circ

3. ROCKET EQUATION:
   - Tsiolkovsky: dv = Isp * g0 * ln(m0/mf)
   - Mass ratio: MR = exp(dv/ve)

=============================================================================
PYTHON/MANIM CONCEPTS IN THIS FILE
=============================================================================

1. FUNCTION FACTORIES
   Each function returns a new Manim MathTex object.
   This pattern allows creating multiple instances with the same equation.

2. RAW STRINGS (r"...")
   The 'r' prefix prevents backslash escaping.
   Essential for LaTeX which uses \\ extensively.

3. LATEX IN MANIM
   MathTex renders LaTeX math mode.
   Tex renders LaTeX text mode.
   VGroup arranges multiple objects.

4. F-STRINGS IN LATEX
   `f"\\textbf{{{layer_name}}}"` - triple braces needed because
   f-strings use {} and LaTeX uses {} for grouping.

Provides pre-formatted LaTeX equations for aerospace animations.
"""

from manim import MathTex, Tex, VGroup


# =============================================================================
# ISA ATMOSPHERE EQUATIONS
# =============================================================================

def getIsaTemperatureEquation():
    """
    ISA temperature equation for gradient layers.

    AEROSPACE: In gradient (non-isothermal) layers, temperature varies
    linearly with altitude:

        T = T_b + L * (h - h_b)

    Where:
        T = temperature at altitude h [K]
        T_b = temperature at base of layer [K]
        L = lapse rate [K/m] (negative = T decreases with altitude)
        h = altitude [m]
        h_b = altitude at base of layer [m]

    Example (Troposphere): L = -0.0065 K/m = -6.5 deg C/km
    """
    # PYTHON: r"..." is a raw string - backslashes are literal
    # LATEX: \cdot produces a centered dot (multiplication)
    return MathTex(r"T = T_b + L \cdot (h - h_b)")


def getIsaPressureGradientEquation():
    """
    ISA pressure equation for gradient layers.

    AEROSPACE: When temperature varies with altitude, the hydrostatic
    equation integrates to give:

        P = P_b * (T / T_b)^(-g0 / (L * R))

    This is the barometric formula for layers with temperature gradient.
    The exponent is typically around 5.26 for the troposphere.
    """
    # LATEX: \left( and \right) auto-size parentheses
    # LATEX: \frac{a}{b} creates a fraction
    return MathTex(
        r"P = P_b \left( \frac{T}{T_b} \right)^{-g_0 / (L \cdot R)}"
    )


def getIsaPressureIsothermalEquation():
    """
    ISA pressure equation for isothermal layers.

    AEROSPACE: When temperature is constant (L = 0), pressure decreases
    exponentially with altitude:

        P = P_b * exp(-g0 * (h - h_b) / (R * T))

    This applies to the tropopause and stratopause.
    """
    # LATEX: \exp produces the exponential function
    return MathTex(
        r"P = P_b \cdot \exp \left( \frac{-g_0 (h - h_b)}{R \cdot T} \right)"
    )


def getIsaDensityEquation():
    """
    ISA density from ideal gas law.

    AEROSPACE: Density is derived from pressure and temperature using
    the ideal gas law:

        rho = P / (R * T)

    Where R = 287.058 J/(kg*K) for dry air.

    This is fundamental - density determines:
    - Lift: L = 0.5 * rho * V^2 * S * Cl
    - Drag: D = 0.5 * rho * V^2 * S * Cd
    - Engine thrust
    """
    # LATEX: \rho is the Greek letter rho (density symbol)
    return MathTex(r"\rho = \frac{P}{R \cdot T}")


def getSpeedOfSoundEquation():
    """
    Speed of sound equation.

    AEROSPACE: The speed of sound in an ideal gas depends only on
    temperature, not pressure or density:

        a = sqrt(gamma * R * T)

    Where gamma = 1.4 for air (ratio of specific heats).

    At sea level (T = 288.15 K): a = 340.3 m/s
    At cruise altitude (T = 216.65 K): a = 295.1 m/s

    This is why Mach number (V/a) is so important - same airspeed
    gives different Mach at different altitudes.
    """
    # LATEX: \sqrt{} creates a square root symbol
    # LATEX: \gamma is the Greek letter gamma
    return MathTex(r"a = \sqrt{\gamma \cdot R \cdot T}")


# =============================================================================
# ORBITAL MECHANICS EQUATIONS
# =============================================================================

def getCircularVelocityEquation():
    """
    Circular orbital velocity.

    AEROSPACE: The velocity required to maintain a circular orbit:

        v_circ = sqrt(mu / r)

    Where:
        mu = G*M = gravitational parameter [km^3/s^2]
        r = orbital radius from center of body [km]

    Higher orbits have LOWER velocities (counterintuitive!):
        LEO (400 km): 7.67 km/s
        GEO (35,786 km): 3.07 km/s
    """
    return MathTex(r"v_{circ} = \sqrt{\frac{\mu}{r}}")


def getVisVivaEquation():
    """
    Vis-viva equation for orbital velocity.

    AEROSPACE: The most important equation in orbital mechanics!
    Gives velocity at any point on any orbit:

        v^2 = mu * (2/r - 1/a)

    Where:
        v = velocity at current position [km/s]
        mu = gravitational parameter [km^3/s^2]
        r = current distance from center of body [km]
        a = semi-major axis of orbit [km]

    Special cases:
        Circular orbit (a = r): v^2 = mu/r
        Escape (a = infinity): v^2 = 2*mu/r
    """
    return MathTex(r"v^2 = \mu \left( \frac{2}{r} - \frac{1}{a} \right)")


def getHohmannDv1Equation():
    """
    Hohmann transfer first burn delta-v.

    AEROSPACE: At periapsis (inner orbit), burn prograde to enter
    the transfer ellipse:

        dv1 = sqrt(mu/r1) * (sqrt(2*r2/(r1+r2)) - 1)

    This is the difference between:
    - Transfer velocity at periapsis (higher)
    - Circular velocity at r1 (lower)
    """
    # LATEX: \Delta produces uppercase delta
    return MathTex(
        r"\Delta v_1 = \sqrt{\frac{\mu}{r_1}} "
        r"\left( \sqrt{\frac{2 r_2}{r_1 + r_2}} - 1 \right)"
    )


def getHohmannDv2Equation():
    """
    Hohmann transfer second burn delta-v.

    AEROSPACE: At apoapsis (outer orbit), burn prograde to circularize:

        dv2 = sqrt(mu/r2) * (1 - sqrt(2*r1/(r1+r2)))

    This is the difference between:
    - Circular velocity at r2 (higher)
    - Transfer velocity at apoapsis (lower)
    """
    return MathTex(
        r"\Delta v_2 = \sqrt{\frac{\mu}{r_2}} "
        r"\left( 1 - \sqrt{\frac{2 r_1}{r_1 + r_2}} \right)"
    )


def getHohmannTransferTimeEquation():
    """
    Hohmann transfer time equation.

    AEROSPACE: Transfer time is half the period of the transfer ellipse:

        t_transfer = pi * sqrt(a^3 / mu)

    This comes from Kepler's Third Law: T = 2*pi*sqrt(a^3/mu)
    Divided by 2 because we only travel half the ellipse.

    LEO to GEO: ~5.2 hours
    Earth to Mars: ~259 days
    """
    return MathTex(r"t_{transfer} = \pi \sqrt{\frac{a^3}{\mu}}")


# =============================================================================
# ROCKET EQUATIONS
# =============================================================================

def getTsiolkovskyEquation():
    """
    Tsiolkovsky rocket equation (with specific impulse).

    AEROSPACE: The fundamental equation of rocketry:

        dv = Isp * g0 * ln(m0 / mf)

    Where:
        dv = change in velocity [m/s]
        Isp = specific impulse [s]
        g0 = standard gravity [9.80665 m/s^2]
        m0 = initial mass (with propellant) [kg]
        mf = final mass (dry mass) [kg]

    Named after Konstantin Tsiolkovsky (1857-1935), Russian rocket pioneer.
    """
    # LATEX: \ln produces natural logarithm
    return MathTex(r"\Delta v = I_{sp} \cdot g_0 \cdot \ln \left( \frac{m_0}{m_f} \right)")


def getTsiolkovskyExpanded():
    """
    Tsiolkovsky equation expanded form (with exhaust velocity).

    AEROSPACE: Using exhaust velocity instead of specific impulse:

        dv = ve * ln(m0 / mf)

    Where ve = Isp * g0 is the effective exhaust velocity.

    This form shows the logarithmic relationship more clearly -
    each doubling of dv requires SQUARING the mass ratio.
    """
    return MathTex(r"\Delta v = v_e \cdot \ln \left( \frac{m_0}{m_f} \right)")


def getMassRatioEquation():
    """
    Mass ratio equation.

    AEROSPACE: The mass ratio relates delta-v to propellant mass:

        MR = m0 / mf = exp(dv / ve)

    The exponential relationship is "the tyranny of the rocket equation":
    - dv = 1 * ve: MR = 2.72 (63% propellant)
    - dv = 2 * ve: MR = 7.39 (86% propellant)
    - dv = 3 * ve: MR = 20.1 (95% propellant)
    """
    return MathTex(r"MR = \frac{m_0}{m_f} = e^{\Delta v / v_e}")


def getOrbitalPeriodEquation():
    """
    Orbital period equation (Kepler's Third Law).

    AEROSPACE: The time to complete one orbit:

        T = 2*pi * sqrt(a^3 / mu)

    Where a is the semi-major axis.

    Examples:
        LEO (a = 6771 km): T = 92 min
        GEO (a = 42164 km): T = 24 hr (by design!)
        Moon (a = 384400 km): T = 27.3 days
    """
    return MathTex(r"T = 2\pi \sqrt{\frac{a^3}{\mu}}")


def getEscapeVelocityEquation():
    """
    Escape velocity equation.

    AEROSPACE: Minimum velocity to escape a body's gravity:

        v_esc = sqrt(2 * mu / r) = sqrt(2) * v_circ

    Escape velocity is only 41% higher than circular velocity!
    This is why we can reach other planets.

    From Earth's surface: v_esc = 11.2 km/s
    From Moon's surface: v_esc = 2.4 km/s
    """
    return MathTex(r"v_{esc} = \sqrt{\frac{2\mu}{r}}")


# =============================================================================
# HELPER FUNCTIONS
# =============================================================================

def createEquationGroup(equations, spacing=0.5):
    """
    Create a vertically arranged group of equations.

    MANIM: VGroup is a container that treats multiple objects as one.
    arrange(DOWN, buff=spacing) stacks them vertically with given spacing.

    Parameters
    ----------
    equations : list
        List of MathTex or Tex objects
    spacing : float
        Vertical spacing between equations

    Returns
    -------
    VGroup
        Vertically arranged equation group
    """
    # PYTHON: *equations unpacks the list as separate arguments
    group = VGroup(*equations)
    # MANIM: DOWN is a constant vector pointing downward
    group.arrange(DOWN, buff=spacing)
    return group


def getIsaLayerInfo(layerName, hBase, hTop, tBase, lapse):
    """
    Create a text group describing an ISA layer.

    Parameters
    ----------
    layerName : str
        Name of the atmospheric layer
    hBase : float
        Base altitude in meters
    hTop : float
        Top altitude in meters
    tBase : float
        Base temperature in Kelvin
    lapse : float
        Lapse rate in K/m (0 for isothermal)

    Returns
    -------
    VGroup
        Formatted text describing the layer
    """
    # PYTHON: f-strings with triple braces for LaTeX escaping
    # {{{ }}} -> { } in LaTeX after f-string processing
    layerInfo = VGroup(
        Tex(f"\\textbf{{{layerName}}}"),
        Tex(f"Altitude: {hBase/1000:.0f} - {hTop/1000:.0f} km"),
        Tex(f"Base Temperature: {tBase:.2f} K"),
        Tex(f"Lapse Rate: {lapse*1000:.1f} K/km" if lapse != 0 else "Isothermal"),
    )
    # MANIM: aligned_edge=LEFT aligns all elements to left edge
    layerInfo.arrange(DOWN, buff=0.2, aligned_edge=LEFT)
    layerInfo.scale(0.6)
    return layerInfo
