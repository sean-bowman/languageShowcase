"""
Physical constants for aerospace calculations.

=============================================================================
AEROSPACE CONCEPT: Physical Constants and Standard Atmosphere
=============================================================================

Aerospace calculations require precise physical constants. This module
provides:

1. EARTH PARAMETERS
   - Radius: Mean equatorial radius (6,371 km)
   - GM (mu): Gravitational parameter = G * M
   - Mass: 5.972 x 10^24 kg

   WHY USE GM INSTEAD OF G AND M SEPARATELY?
   GM can be measured more precisely through satellite observations
   than G or M individually. Using GM directly reduces error.

2. STANDARD GRAVITY (g0)
   The standard gravitational acceleration at sea level.
   Used in specific impulse calculations: F = Isp * g0 * m_dot
   Note: This is a defined constant, not measured local gravity.

3. ISA (INTERNATIONAL STANDARD ATMOSPHERE)
   A reference model of how atmospheric properties vary with altitude.
   Used for aircraft performance calculations, altimeter calibration,
   and engine specifications.

   ISA SEA LEVEL CONDITIONS:
   - Temperature: 288.15 K (15 deg C)
   - Pressure: 101,325 Pa (1 atm)
   - Density: 1.225 kg/m^3

4. ATMOSPHERIC LAYERS
   The atmosphere is divided into layers based on temperature behavior:

   Altitude (km)
        |
    85  + - - - - - - - - - Mesosphere top
        |  Mesosphere (T falls)
    51  + - - - - - - - - - Stratopause
        |  Stratosphere (T rises - ozone)
    20  + - - - - - - - - - Tropopause
        |  Tropopause (T constant)
    11  + - - - - - - - - - Troposphere top
        |  Troposphere (T falls 6.5 K/km)
     0  +------------------ Sea level

=============================================================================
PYTHON CONCEPTS IN THIS FILE
=============================================================================

1. NUMERIC LITERALS WITH UNDERSCORES (Python 3.6+)
   `6_371_000` equals 6371000 but is more readable.
   Underscores are ignored by Python.

2. SCIENTIFIC NOTATION
   `3.986004418e14` means 3.986004418 x 10^14.
   The 'e' notation is standard for very large/small numbers.

3. DOCSTRINGS
   Triple-quoted strings at the start of a module.
   Used by documentation generators and IDE tooltips.

4. LIST OF DICTIONARIES
   `isaLayers = [{"name": ..., "hBase": ...}, ...]`
   Common pattern for structured data in Python.

5. DICTIONARY ACCESS
   `layer["hBase"]` accesses the value for key "hBase".
   Can also use `layer.get("hBase", default)` for safety.

All values in SI units unless otherwise noted.
"""

# =============================================================================
# EARTH PARAMETERS
# =============================================================================

# Mean equatorial radius of Earth [m]
# AEROSPACE: Used as reference for altitude calculations.
# The Earth is actually an oblate spheroid (flattened at poles).
# Equatorial radius: 6,378 km, Polar radius: 6,357 km
earthRadius = 6_371_000  # meters

# Earth's gravitational parameter (mu = G * M) [m^3/s^2]
# AEROSPACE: Fundamental to all orbital calculations.
# This value is known to 8 significant figures from satellite tracking.
# v_circular = sqrt(mu / r)
# T_orbital = 2 * pi * sqrt(r^3 / mu)
earthGm = 3.986004418e14  # m^3/s^2

# Earth's mass [kg]
# Less precisely known than GM due to uncertainty in G.
earthMass = 5.972e24  # kg

# =============================================================================
# STANDARD GRAVITY
# =============================================================================

# Standard gravitational acceleration [m/s^2]
# AEROSPACE: A defined constant (not measured local gravity).
# Used in specific impulse: Isp = F / (m_dot * g0)
# Also used in ISA pressure calculations.
# Defined by international agreement, not actual sea-level gravity.
g0 = 9.80665  # m/s^2

# =============================================================================
# GAS CONSTANTS
# =============================================================================

# Specific gas constant for dry air [J/(kg*K)]
# AEROSPACE: R = R_universal / M_air where M_air = 28.97 g/mol
# Used in ideal gas law: P = rho * R * T
rAir = 287.058  # J/(kg*K)

# Ratio of specific heats for air (cp/cv) [dimensionless]
# AEROSPACE: Appears in speed of sound: a = sqrt(gamma * R * T)
# Also in isentropic flow relations.
# gamma = 1.4 for diatomic gases (N2, O2) at moderate temperatures.
gammaAir = 1.4

# =============================================================================
# ISA SEA LEVEL CONDITIONS
# =============================================================================
# AEROSPACE: These are the reference conditions for the International
# Standard Atmosphere. All aircraft performance is quoted relative to ISA.
# Real atmosphere varies, so we specify deviations (e.g., "ISA +10 deg C").

# Sea level temperature [K]
# 288.15 K = 15 deg C = 59 deg F
isaT0 = 288.15  # K

# Sea level pressure [Pa]
# 101325 Pa = 1013.25 hPa = 1 atm = 29.92 inHg
isaP0 = 101325  # Pa

# Sea level density [kg/m^3]
# Derived from ideal gas law: rho = P / (R * T)
isaRho0 = 1.225  # kg/m^3

# =============================================================================
# ISA ATMOSPHERIC LAYERS
# =============================================================================
# AEROSPACE: Each layer has a base altitude, top altitude, base temperature,
# and lapse rate (temperature change per meter of altitude).
#
# PYTHON: This is a list of dictionaries - a common pattern for structured
# data. Each dictionary represents one atmospheric layer.
#
# Lapse rate sign convention:
#   Negative: Temperature decreases with altitude (troposphere, mesosphere)
#   Zero: Isothermal layer (tropopause, stratopause)
#   Positive: Temperature increases with altitude (stratosphere - ozone heating)

isaLayers = [
    # TROPOSPHERE: Where weather happens. Temperature falls 6.5 K per km.
    {"name": "Troposphere", "hBase": 0, "hTop": 11000, "tBase": 288.15, "lapse": -0.0065},

    # TROPOPAUSE: Isothermal layer. Commercial jets cruise here (11-12 km).
    {"name": "Tropopause", "hBase": 11000, "hTop": 20000, "tBase": 216.65, "lapse": 0.0},

    # STRATOSPHERE 1: Temperature rises due to ozone absorption of UV.
    {"name": "Stratosphere", "hBase": 20000, "hTop": 32000, "tBase": 216.65, "lapse": 0.001},

    # STRATOSPHERE 2: Continued warming.
    {"name": "Stratosphere 2", "hBase": 32000, "hTop": 47000, "tBase": 228.65, "lapse": 0.0028},

    # STRATOPAUSE: Isothermal region at top of stratosphere.
    {"name": "Stratopause", "hBase": 47000, "hTop": 51000, "tBase": 270.65, "lapse": 0.0},

    # MESOSPHERE 1: Temperature falls again (no ozone, radiation cooling).
    {"name": "Mesosphere", "hBase": 51000, "hTop": 71000, "tBase": 270.65, "lapse": -0.0028},

    # MESOSPHERE 2: Coldest part of atmosphere (~-100 deg C).
    {"name": "Mesosphere 2", "hBase": 71000, "hTop": 84852, "tBase": 214.65, "lapse": -0.002},
]

# =============================================================================
# COMMON ORBITAL ALTITUDES
# =============================================================================
# AEROSPACE: Standard orbital altitudes for mission planning.
# Note: These are altitudes (above surface), not radii (from center).

# Low Earth Orbit - ISS altitude [m]
# Period: ~92 minutes, Velocity: ~7.7 km/s
orbitLeo = 400_000  # 400 km

# Medium Earth Orbit - GPS constellation [m]
# Period: ~12 hours, 6 orbital planes
orbitMeo = 20_200_000  # 20,200 km

# Geostationary Orbit [m]
# Period: exactly 24 hours (sidereal)
# Appears stationary from Earth's surface
orbitGeo = 35_786_000  # 35,786 km

# =============================================================================
# VISUALIZATION COLORS
# =============================================================================
# MANIM: Color codes for consistent visualization across animations.
# These are hex color strings that Manim can use directly.

colors = {
    # Atmospheric layers
    "troposphere": "#4a90d9",     # Blue
    "stratosphere": "#4aff4a",    # Green
    "mesosphere": "#ff9040",      # Orange

    # Orbital elements
    "orbitInitial": "#4a90d9",   # Blue - starting orbit
    "orbitFinal": "#4aff4a",     # Green - destination orbit
    "orbitTransfer": "#ff9040",  # Orange - transfer path

    # Bodies
    "earth": "#4080ff",           # Blue planet
    "rocket": "#ff4040",          # Red - easy to track
}
