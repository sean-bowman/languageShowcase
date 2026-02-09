!===============================================================================
! constants_module.f90 - Physical constants for ISA calculations
!
! ===========================================================================
! AEROSPACE CONCEPT: International Standard Atmosphere (ISA)
! ===========================================================================
!
! The International Standard Atmosphere (ISA) is a model of how atmospheric
! properties (temperature, pressure, density) vary with altitude. It's used
! worldwide as a reference for:
!   - Aircraft performance calculations
!   - Altimeter calibration
!   - Engine certification
!   - Comparing flight test data
!
! The ISA is defined by ISO 2533:1975 and represents idealized "standard"
! conditions. Real atmospheric conditions vary daily and by location!
!
! KEY ISA ASSUMPTIONS:
!   - Dry air (no humidity)
!   - Hydrostatic equilibrium
!   - Ideal gas behavior
!   - Defined temperature profile vs altitude
!
! ISA STRUCTURE (Temperature Profile):
!
!   Altitude (km)  |  Name            | Lapse Rate (K/km)
!   ----------------------------------------------------------------
!      0 - 11      |  Troposphere     |  -6.5  (temperature decreases)
!     11 - 20      |  Tropopause      |   0.0  (isothermal layer)
!     20 - 32      |  Stratosphere 1  |  +1.0  (temperature increases!)
!     32 - 47      |  Stratosphere 2  |  +2.8
!     47 - 51      |  Stratopause     |   0.0  (isothermal)
!     51 - 71      |  Mesosphere 1    |  -2.8  (temperature decreases)
!     71 - 86      |  Mesosphere 2    |  -2.0
!
! WHY DOES TEMPERATURE INCREASE IN THE STRATOSPHERE?
!   The ozone layer (15-35 km) absorbs UV radiation from the Sun,
!   heating the air. This creates the temperature inversion that
!   defines the tropopause-stratosphere boundary.
!
! ===========================================================================
! FORTRAN CONCEPTS IN THIS FILE
! ===========================================================================
!
! 1. MODULES
!    - Fortran's way of organizing code into reusable units
!    - Similar to namespaces (C++) or modules (Python/Rust)
!    - `use constants_module` makes these constants available
!
! 2. IMPLICIT NONE
!    - Forces all variables to be explicitly declared
!    - Without this, Fortran would assume types from first letter!
!    - (i-n = integer, everything else = real) - very error-prone
!    - ALWAYS use `implicit none` in modern Fortran
!
! 3. KIND PARAMETERS
!    - Fortran's way of specifying numeric precision
!    - `selected_real_kind(15, 307)` requests:
!      - At least 15 significant decimal digits
!      - Exponent range at least 10^(-307) to 10^(307)
!    - This typically gives IEEE 754 double precision (64-bit)
!
! 4. PARAMETER ATTRIBUTE
!    - Makes a variable a compile-time constant (cannot be changed)
!    - Similar to `const` (C++), `final` (Java), or `let` (Rust)
!    - Constants are often uppercase by convention
!
! 5. LITERAL SUFFIXES (_dp)
!    - `288.15_dp` is a real literal with kind `dp`
!    - Without the suffix, `288.15` would be single precision!
!    - This is a common Fortran gotcha - always use suffixes for precision
!
!===============================================================================
module constants_module
    ! FORTRAN CONCEPT: implicit none
    ! ---------------------------------
    ! This statement MUST be first after module/program/subroutine statements.
    ! It disables Fortran's implicit typing (where I-N = integer, A-H,O-Z = real).
    !
    ! Without implicit none, this would compile silently:
    !   total = temperatura + presure   ! "temperatura" creates a new variable!
    !
    ! With implicit none, undeclared variables cause compile errors.
    ! This catches typos immediately.
    implicit none

    !---------------------------------------------------------------------------
    ! KIND PARAMETER FOR DOUBLE PRECISION
    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: selected_real_kind
    ! ------------------------------------
    ! `selected_real_kind(p, r)` returns a kind number that provides:
    !   - At least `p` decimal digits of precision
    !   - At least `r` powers of 10 in the exponent range
    !
    ! selected_real_kind(15, 307) typically returns the kind for
    ! IEEE 754 double precision (64-bit floating point):
    !   - 15-16 significant digits
    !   - Range: ~10^(-308) to ~10^(308)
    !
    ! WHY USE THIS INSTEAD OF JUST `double precision`?
    !   - More portable across compilers
    !   - Self-documenting: states the precision you actually need
    !   - `double precision` meaning varies by compiler/platform
    !
    ! ALTERNATIVE: Modern Fortran also provides iso_fortran_env module with
    ! predefined kinds like `real64`, `real32`, etc.
    !---------------------------------------------------------------------------
    integer, parameter :: dp = selected_real_kind(15, 307)

    !===========================================================================
    ! SEA LEVEL REFERENCE VALUES
    !
    ! These define the "zero altitude" conditions of the ISA model.
    ! All calculations start from these reference values.
    !
    ! AEROSPACE NOTE: These are idealized values, not measurements!
    ! Real sea level conditions vary significantly:
    !   - Temperature: -40C to +50C depending on location/season
    !   - Pressure: 870 to 1084 hPa (weather-dependent)
    !   - Density: varies with temperature and humidity
    !
    ! Aircraft instruments use these ISA values for calibration.
    ! When your altimeter reads "10,000 ft", it's calculating altitude
    ! based on ISA assumptions, not actual atmospheric conditions.
    !===========================================================================

    ! Sea level temperature: 288.15 K = 15 degrees C = 59 degrees F
    ! This is a comfortable "average" day at sea level
    real(dp), parameter :: T0 = 288.15_dp      ! [K]

    ! Sea level pressure: 101,325 Pa = 1013.25 hPa = 1 atm = 29.92 inHg
    ! This is the definition of "one atmosphere" of pressure
    real(dp), parameter :: P0 = 101325.0_dp    ! [Pa]

    ! Sea level density: 1.225 kg/m^3
    ! This comes from the ideal gas law: rho = P / (R * T)
    !   rho = 101325 / (287.05 * 288.15) = 1.225 kg/m^3
    real(dp), parameter :: RHO0 = 1.225_dp     ! [kg/m^3]

    !===========================================================================
    ! PHYSICAL CONSTANTS
    !
    ! These are fundamental physical constants used in atmospheric calculations.
    !===========================================================================

    ! Specific gas constant for dry air: R = R_universal / M_air
    ! R = 8314.47 / 28.9644 = 287.05 J/(kg*K)
    !
    ! This relates pressure, density, and temperature: P = rho * R * T
    ! Different gases have different R values (depends on molecular weight)
    real(dp), parameter :: R = 287.05287_dp    ! [J/(kg*K)]

    ! Ratio of specific heats (gamma = cp/cv) for air
    ! cp = specific heat at constant pressure
    ! cv = specific heat at constant volume
    !
    ! For diatomic gases (N2, O2 - which make up 99% of air): gamma = 1.4
    ! This is crucial for calculating speed of sound: a = sqrt(gamma * R * T)
    !
    ! AEROSPACE USE: Mach number = velocity / speed_of_sound
    real(dp), parameter :: GAMMA = 1.4_dp      ! [dimensionless]

    ! Standard gravity at sea level
    ! This is a defined constant, not a measurement.
    ! Actual gravity varies from ~9.78 to ~9.83 m/s^2 depending on latitude
    ! and local geological features.
    real(dp), parameter :: G0 = 9.80665_dp     ! [m/s^2]

    ! Molar mass of dry air (average molecular weight)
    ! Air is ~78% N2 (28 g/mol) + ~21% O2 (32 g/mol) + ~1% Ar (40 g/mol)
    ! Weighted average: ~28.96 g/mol = 0.02896 kg/mol
    real(dp), parameter :: M = 0.0289644_dp    ! [kg/mol]

    ! Universal gas constant (same for all ideal gases)
    ! R_specific = R_universal / M for each gas
    real(dp), parameter :: R_UNIVERSAL = 8.31447_dp  ! [J/(mol*K)]

    !===========================================================================
    ! ATMOSPHERIC LAYER BOUNDARIES
    !
    ! The atmosphere is divided into layers based on temperature behavior.
    ! These are GEOPOTENTIAL altitudes, not geometric altitudes!
    !
    ! GEOPOTENTIAL vs GEOMETRIC ALTITUDE:
    !   - Geometric altitude: actual distance above sea level
    !   - Geopotential altitude: accounts for gravity variation with height
    !   - Difference is small at low altitudes but grows at high altitudes
    !   - At 86 km geopotential ~ 84.8 km geometric
    !
    ! The ISA model uses geopotential altitude because it simplifies
    ! the hydrostatic equation (pressure change with height).
    !===========================================================================

    ! Troposphere: 0 to 11 km
    ! "Tropos" = turning/changing. Weather happens here.
    ! Temperature decreases with altitude (lapse rate -6.5 K/km)
    ! Contains ~80% of atmosphere's mass
    real(dp), parameter :: H_TROPOSPHERE = 11000.0_dp    ! [m]

    ! Tropopause: 11 to 20 km
    ! Transition layer - temperature is constant (isothermal)
    ! Commercial aircraft cruise here (35,000-40,000 ft)
    real(dp), parameter :: H_TROPOPAUSE = 20000.0_dp     ! [m]

    ! Lower Stratosphere: 20 to 32 km
    ! Temperature INCREASES with altitude!
    ! Ozone layer absorbs UV radiation, heating the air
    real(dp), parameter :: H_STRATOSPHERE1 = 32000.0_dp  ! [m]

    ! Upper Stratosphere: 32 to 47 km
    ! Temperature continues to increase
    ! Weather balloons reach this altitude
    real(dp), parameter :: H_STRATOSPHERE2 = 47000.0_dp  ! [m]

    ! Stratopause/Lower Mesosphere: 47 to 51 km
    ! Isothermal layer at the top of the stratosphere
    real(dp), parameter :: H_MESOSPHERE1 = 51000.0_dp    ! [m]

    ! Upper Mesosphere: 51 to 71 km
    ! Temperature decreases again
    ! Meteors burn up in this region ("shooting stars")
    real(dp), parameter :: H_MESOSPHERE2 = 71000.0_dp    ! [m]

    ! Model limit: 86 km
    ! The ISA model ends here. Above this altitude:
    !   - Air becomes too thin to behave as a continuous fluid
    !   - Molecular effects dominate
    !   - Different models needed (e.g., NRLMSISE-00)
    real(dp), parameter :: H_MAX = 86000.0_dp            ! [m]

    !===========================================================================
    ! TEMPERATURE LAPSE RATES
    !
    ! The lapse rate is how temperature changes with altitude: dT/dh
    ! Negative = temperature decreases with altitude
    ! Positive = temperature increases with altitude
    ! Zero = isothermal (constant temperature)
    !
    ! UNITS: K/m (Kelvin per meter) - note these are small numbers!
    ! -0.0065 K/m = -6.5 K/km = -6.5 degrees C per kilometer
    !
    ! WHY DOES TEMPERATURE CHANGE?
    !   Troposphere: Rising air expands and cools (adiabatic cooling)
    !   Stratosphere: Ozone absorbs UV, heating the air from above
    !   Mesosphere: Radiative cooling dominates
    !===========================================================================

    ! Troposphere lapse rate: -6.5 K/km = -0.0065 K/m
    ! This is close to the "environmental lapse rate" observed in reality
    real(dp), parameter :: LAPSE_TROPOSPHERE = -0.0065_dp      ! [K/m]

    ! Tropopause: isothermal (no temperature change)
    real(dp), parameter :: LAPSE_TROPOPAUSE = 0.0_dp           ! [K/m]

    ! Lower stratosphere: +1.0 K/km (temperature increases!)
    real(dp), parameter :: LAPSE_STRATOSPHERE1 = 0.001_dp      ! [K/m]

    ! Upper stratosphere: +2.8 K/km
    real(dp), parameter :: LAPSE_STRATOSPHERE2 = 0.0028_dp     ! [K/m]

    ! Lower mesosphere: isothermal (stratopause)
    real(dp), parameter :: LAPSE_MESOSPHERE1 = 0.0_dp          ! [K/m]

    ! Mid mesosphere: -2.8 K/km
    real(dp), parameter :: LAPSE_MESOSPHERE2 = -0.0028_dp      ! [K/m]

    ! Upper mesosphere: -2.0 K/km
    real(dp), parameter :: LAPSE_MESOSPHERE3 = -0.002_dp       ! [K/m]

    !===========================================================================
    ! BASE TEMPERATURES AT LAYER BOUNDARIES
    !
    ! These are pre-calculated temperatures at each layer boundary.
    ! Using these avoids recalculating from sea level every time.
    !
    ! Each temperature is calculated from the layer below:
    !   T(h) = T_base + lapse_rate * (h - h_base)
    !
    ! Example for T11 (at 11 km):
    !   T11 = T0 + LAPSE_TROPOSPHERE * (11000 - 0)
    !       = 288.15 + (-0.0065) * 11000
    !       = 288.15 - 71.5
    !       = 216.65 K = -56.5 C
    !
    ! This is COLD! Commercial aircraft at cruise altitude (11 km)
    ! experience outside temperatures around -60 C (-76 F).
    !===========================================================================

    real(dp), parameter :: T11 = 216.65_dp     ! [K] at 11 km (-56.5 C)
    real(dp), parameter :: T20 = 216.65_dp     ! [K] at 20 km (isothermal layer)
    real(dp), parameter :: T32 = 228.65_dp     ! [K] at 32 km
    real(dp), parameter :: T47 = 270.65_dp     ! [K] at 47 km (-2.5 C, warming!)
    real(dp), parameter :: T51 = 270.65_dp     ! [K] at 51 km (stratopause)
    real(dp), parameter :: T71 = 214.65_dp     ! [K] at 71 km (-58.5 C)

    !===========================================================================
    ! MATHEMATICAL CONSTANTS
    !===========================================================================

    ! Pi to full double precision
    ! FORTRAN NOTE: The _dp suffix ensures this is stored as double precision.
    ! Without it, the trailing digits would be lost!
    real(dp), parameter :: PI = 3.14159265358979323846_dp

end module constants_module
