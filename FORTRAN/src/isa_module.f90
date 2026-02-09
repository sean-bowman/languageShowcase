!===============================================================================
! isa_module.f90 - International Standard Atmosphere (ISA) Calculator Module
!
! Implements ISO 2533:1975 Standard Atmosphere Model for calculating
! atmospheric properties at any altitude from sea level to 86 km.
!
! ===========================================================================
! AEROSPACE CONCEPT: The Barometric Formula
! ===========================================================================
!
! The key to calculating pressure at altitude is the BAROMETRIC FORMULA,
! which comes from the hydrostatic equation and ideal gas law.
!
! HYDROSTATIC EQUATION:
!   dP = -rho * g * dh
!   (Pressure decreases as altitude increases)
!
! Combined with ideal gas law (P = rho * R * T):
!   dP/P = -g/(R*T) * dh
!
! For GRADIENT LAYERS (temperature varies linearly with altitude):
!   T = T_base + L * (h - h_base)    where L = lapse rate
!
!   Integrating gives:
!   P = P_base * (T / T_base)^(-g / (L * R))
!
! For ISOTHERMAL LAYERS (constant temperature):
!   P = P_base * exp(-g * (h - h_base) / (R * T))
!
! PHYSICAL INTERPRETATION:
!   - Gradient layers: pressure changes as a power law
!   - Isothermal layers: pressure decreases exponentially
!   - The exponent (-g/LR) is typically around 5.26 for the troposphere
!
! ===========================================================================
! AEROSPACE CONCEPT: Speed of Sound
! ===========================================================================
!
! The speed of sound in air depends ONLY on temperature:
!   a = sqrt(gamma * R * T)
!
! Where:
!   gamma = 1.4 (ratio of specific heats for air)
!   R = 287.05 J/(kg*K) (specific gas constant for air)
!   T = temperature in Kelvin
!
! At sea level (T = 288.15 K):
!   a = sqrt(1.4 * 287.05 * 288.15) = 340.3 m/s
!
! At 11 km (T = 216.65 K):
!   a = sqrt(1.4 * 287.05 * 216.65) = 295.1 m/s
!
! This is why Mach number varies with altitude even at constant airspeed!
! An aircraft flying at 250 m/s:
!   - At sea level: Mach 0.73
!   - At 11 km: Mach 0.85
!
! ===========================================================================
! FORTRAN CONCEPTS IN THIS FILE
! ===========================================================================
!
! 1. MODULE DEPENDENCIES (use statement)
!    - `use constants_module` imports another module
!    - All public names from that module become available
!    - Can use `only:` clause to limit imports
!
! 2. PRIVATE/PUBLIC ACCESS CONTROL
!    - `private` makes everything private by default
!    - `public ::` explicitly exports specific items
!    - Similar to public/private in classes (C++, Java)
!
! 3. DERIVED TYPES (like struct/class)
!    - `type :: AtmosphericProperties` defines a composite type
!    - Access members with `%` operator: `props%temperature`
!    - Can contain any mix of types
!
! 4. PURE FUNCTIONS
!    - `pure function` guarantees no side effects
!    - Cannot modify global state or perform I/O
!    - Enables compiler optimizations
!    - Can be called in parallel safely
!
! 5. INTENT ATTRIBUTE
!    - `intent(in)` = read-only parameter (cannot be modified)
!    - `intent(out)` = output only (initial value undefined)
!    - `intent(inout)` = can be read and modified
!    - Helps compiler catch errors and optimize
!
! 6. RESULT CLAUSE
!    - `function foo(x) result(y)` names the return variable `y`
!    - Alternative to returning the function name
!    - Makes code clearer, especially for recursive functions
!
!===============================================================================
module isa_module
    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: use statement
    !
    ! `use constants_module` makes all PUBLIC symbols from that module
    ! available in this module. This is similar to:
    !   - C++: `using namespace constants;`
    !   - Python: `from constants import *`
    !   - Rust: `use constants::*;`
    !
    ! You can also use selective import:
    !   use constants_module, only: dp, T0, P0, G0, R
    !
    ! Or rename imports:
    !   use constants_module, only: double_precision => dp
    !---------------------------------------------------------------------------
    use constants_module
    implicit none

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: private/public Access Control
    !
    ! `private` sets the DEFAULT visibility for this module to private.
    ! Nothing is accessible from outside unless explicitly made public.
    !
    ! `public :: name1, name2` makes specific items accessible.
    !
    ! This is INFORMATION HIDING - a key principle of good software design:
    !   - Users see only the interface (public items)
    !   - Implementation details (private items) can change freely
    !   - Prevents accidental dependencies on internal details
    !
    ! Without explicit private/public, everything is public by default
    ! (which is rarely what you want in real code).
    !---------------------------------------------------------------------------
    private
    public :: isa_temperature, isa_pressure, isa_density, isa_speed_of_sound
    public :: isa_all_properties, AtmosphericProperties

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Derived Types
    !
    ! A derived type is like a struct (C) or class (C++/Java) without methods.
    ! It groups related data together into a single unit.
    !
    ! DECLARATION:
    !   type :: TypeName
    !       real :: field1
    !       integer :: field2
    !   end type TypeName
    !
    ! USAGE:
    !   type(TypeName) :: myVar
    !   myVar%field1 = 3.14
    !   myVar%field2 = 42
    !
    ! Note: Fortran uses `%` for member access, not `.` like most languages!
    !
    ! CONSTRUCTOR SYNTAX:
    !   myVar = TypeName(3.14, 42)        ! Positional
    !   myVar = TypeName(field1=3.14, field2=42)  ! Named
    !---------------------------------------------------------------------------
    ! Derived type to hold all atmospheric properties at a given altitude.
    ! This allows returning multiple values from isa_all_properties().
    type :: AtmosphericProperties
        real(dp) :: altitude       ! Geometric altitude [m]
        real(dp) :: temperature    ! Temperature [K]
        real(dp) :: pressure       ! Pressure [Pa]
        real(dp) :: density        ! Density [kg/m^3]
        real(dp) :: speed_of_sound ! Speed of sound [m/s]
    end type AtmosphericProperties

contains
    ! FORTRAN CONCEPT: contains
    !
    ! The `contains` statement separates variable declarations from
    ! procedure (function/subroutine) definitions within a module.
    !
    ! Structure of a module:
    !   module name
    !       ! USE statements
    !       ! IMPLICIT NONE
    !       ! Variable and type declarations
    !   contains
    !       ! Function and subroutine definitions
    !   end module name

    !===========================================================================
    ! Calculate temperature at a given altitude.
    !
    ! AEROSPACE: Temperature Profile
    ! ------------------------------
    ! The ISA temperature profile is piecewise linear:
    !   - Each layer has a constant lapse rate (K/m)
    !   - Temperature = T_base + lapse_rate * (h - h_base)
    !
    ! The profile looks like this (not to scale):
    !
    !   86 km ----+
    !             |
    !   71 km ----+----
    !                  \
    !   51 km ----------+
    !                   |
    !   47 km ----------+
    !                  /
    !   32 km --------+
    !                /
    !   20 km ------+
    !               |
    !   11 km ------+
    !                \
    !    0 km --------+
    !         200K   220K   240K   260K   280K   300K
    !
    ! Parameters:
    !   h - Geometric altitude in meters
    !
    ! Returns:
    !   Temperature in Kelvin
    !===========================================================================
    pure function isa_temperature(h) result(T)
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: pure function
        !
        ! A `pure` function has these restrictions:
        !   - Cannot modify any variable outside the function
        !   - Cannot perform I/O (read/write/print)
        !   - Cannot call non-pure procedures
        !   - Cannot have SAVE variables (static locals)
        !
        ! BENEFITS:
        !   - Can be called in FORALL and DO CONCURRENT loops
        !   - Can be called in parallel safely (no data races)
        !   - Compiler can optimize more aggressively
        !   - Easier to reason about (no hidden effects)
        !
        ! Similar to:
        !   - C++: const member functions (weaker guarantee)
        !   - Rust: default for functions (no mut references)
        !   - Haskell: all functions are pure by default
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: intent(in)
        !
        ! The `intent` attribute specifies how a parameter is used:
        !   - intent(in): Input only - cannot be modified
        !   - intent(out): Output only - initial value undefined, must be set
        !   - intent(inout): Both input and output - can be read and modified
        !
        ! ALWAYS use intent for all parameters! It:
        !   - Documents the interface clearly
        !   - Allows compiler to catch errors
        !   - Enables optimization (in params can be passed by value)
        !
        ! For pure functions, all arguments must be intent(in).
        !-----------------------------------------------------------------------
        real(dp), intent(in) :: h      ! Input: altitude in meters
        real(dp) :: T                  ! Output: temperature in Kelvin

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Conditional Expressions (if/else if/else)
        !
        ! Fortran's if statement is similar to other languages:
        !   if (condition) then
        !       statements
        !   else if (condition) then
        !       statements
        !   else
        !       statements
        !   end if
        !
        ! Note: `then` is REQUIRED in Fortran!
        ! Note: No braces - blocks end with `end if`
        !
        ! Comparison operators:
        !   <   or .lt.   (less than)
        !   <=  or .le.   (less than or equal)
        !   ==  or .eq.   (equal)
        !   /=  or .ne.   (not equal)
        !   >   or .gt.   (greater than)
        !   >=  or .ge.   (greater than or equal)
        !-----------------------------------------------------------------------

        if (h < 0.0_dp) then
            ! Below sea level - extrapolate troposphere lapse rate
            ! This is useful for Dead Sea, Death Valley, etc.
            T = T0 + LAPSE_TROPOSPHERE * h

        else if (h <= H_TROPOSPHERE) then
            ! TROPOSPHERE (0 - 11 km): Linear temperature decrease
            ! Lapse rate: -6.5 K/km = -0.0065 K/m
            ! This is where weather occurs and most aircraft fly
            T = T0 + LAPSE_TROPOSPHERE * h

        else if (h <= H_TROPOPAUSE) then
            ! TROPOPAUSE (11 - 20 km): Isothermal layer
            ! Temperature stays constant at 216.65 K (-56.5 C)
            ! Commercial jets cruise here (35,000-40,000 ft)
            T = T11

        else if (h <= H_STRATOSPHERE1) then
            ! LOWER STRATOSPHERE (20 - 32 km): Temperature increases!
            ! Lapse rate: +1.0 K/km = +0.001 K/m
            ! Ozone layer absorbs UV radiation, heating the air
            T = T20 + LAPSE_STRATOSPHERE1 * (h - H_TROPOPAUSE)

        else if (h <= H_STRATOSPHERE2) then
            ! UPPER STRATOSPHERE (32 - 47 km): Steeper temperature increase
            ! Lapse rate: +2.8 K/km = +0.0028 K/m
            T = T32 + LAPSE_STRATOSPHERE2 * (h - H_STRATOSPHERE1)

        else if (h <= H_MESOSPHERE1) then
            ! STRATOPAUSE (47 - 51 km): Isothermal layer
            ! Temperature is at its maximum in this region (~270 K)
            T = T47

        else if (h <= H_MESOSPHERE2) then
            ! MID MESOSPHERE (51 - 71 km): Temperature decreases
            ! Lapse rate: -2.8 K/km = -0.0028 K/m
            ! Meteors burn up here ("shooting stars")
            T = T51 + LAPSE_MESOSPHERE2 * (h - H_MESOSPHERE1)

        else if (h <= H_MAX) then
            ! UPPER MESOSPHERE (71 - 86 km): Temperature decreases
            ! Lapse rate: -2.0 K/km = -0.002 K/m
            ! Noctilucent clouds can form here
            T = T71 + LAPSE_MESOSPHERE3 * (h - H_MESOSPHERE2)

        else
            ! Above model limit (>86 km) - return temperature at 86 km
            ! The ISA model is not valid above 86 km
            ! Use specialized models (NRLMSISE-00) for higher altitudes
            T = T71 + LAPSE_MESOSPHERE3 * (H_MAX - H_MESOSPHERE2)
        end if

    end function isa_temperature

    !===========================================================================
    ! Calculate pressure at a given altitude.
    !
    ! AEROSPACE: Barometric Formula
    ! ------------------------------
    ! Pressure calculation depends on whether the layer is:
    !
    ! GRADIENT LAYER (temperature changes with altitude):
    !   P = P_base * (T / T_base)^(-g / (L * R))
    !
    ! ISOTHERMAL LAYER (constant temperature):
    !   P = P_base * exp(-g * (h - h_base) / (R * T))
    !
    ! The exponent -g/(L*R) for the troposphere:
    !   = -9.80665 / (-0.0065 * 287.05)
    !   = 5.2559
    !
    ! This is why pressure drops roughly as (T/T0)^5.26 in the troposphere.
    !
    ! PRACTICAL EXAMPLE:
    ! At 5,000 m (typical high mountain):
    !   T = 288.15 - 0.0065 * 5000 = 255.65 K
    !   P = 101325 * (255.65/288.15)^5.26 = 54,048 Pa
    !   That's about 53% of sea level pressure!
    !
    ! Parameters:
    !   h - Geometric altitude in meters
    !
    ! Returns:
    !   Pressure in Pascals
    !===========================================================================
    pure function isa_pressure(h) result(P)
        real(dp), intent(in) :: h
        real(dp) :: P
        real(dp) :: T

        !-----------------------------------------------------------------------
        ! Pre-calculated pressures at layer boundaries
        ! These are computed from the barometric formula to avoid
        ! expensive recalculation every time.
        !
        ! FORTRAN CONCEPT: Local Parameters
        ! These are constants that exist only within this function.
        ! The `parameter` attribute makes them compile-time constants.
        !-----------------------------------------------------------------------
        real(dp), parameter :: p11 = 22632.06_dp    ! [Pa] at 11 km (22% of P0)
        real(dp), parameter :: p20 = 5474.889_dp    ! [Pa] at 20 km (5.4% of P0)
        real(dp), parameter :: p32 = 868.0187_dp    ! [Pa] at 32 km (0.86% of P0)
        real(dp), parameter :: p47 = 110.9063_dp    ! [Pa] at 47 km (0.11% of P0)
        real(dp), parameter :: p51 = 66.93887_dp    ! [Pa] at 51 km
        real(dp), parameter :: p71 = 3.956420_dp    ! [Pa] at 71 km (0.004% of P0!)

        ! Get temperature (needed for all calculations)
        T = isa_temperature(h)

        if (h < 0.0_dp) then
            ! Below sea level - use gradient formula
            P = P0 * (T / T0) ** (-G0 / (LAPSE_TROPOSPHERE * R))

        else if (h <= H_TROPOSPHERE) then
            ! TROPOSPHERE: Gradient layer
            ! P = P0 * (T/T0)^(-g/LR)
            P = P0 * (T / T0) ** (-G0 / (LAPSE_TROPOSPHERE * R))

        else if (h <= H_TROPOPAUSE) then
            ! TROPOPAUSE: Isothermal layer
            ! P = p11 * exp(-g*dh / (R*T))
            P = p11 * exp(-G0 * (h - H_TROPOSPHERE) / (R * T11))

        else if (h <= H_STRATOSPHERE1) then
            ! LOWER STRATOSPHERE: Gradient layer
            P = p20 * (T / T20) ** (-G0 / (LAPSE_STRATOSPHERE1 * R))

        else if (h <= H_STRATOSPHERE2) then
            ! UPPER STRATOSPHERE: Gradient layer
            P = p32 * (T / T32) ** (-G0 / (LAPSE_STRATOSPHERE2 * R))

        else if (h <= H_MESOSPHERE1) then
            ! STRATOPAUSE: Isothermal layer
            P = p47 * exp(-G0 * (h - H_STRATOSPHERE2) / (R * T47))

        else if (h <= H_MESOSPHERE2) then
            ! MID MESOSPHERE: Gradient layer
            P = p51 * (T / T51) ** (-G0 / (LAPSE_MESOSPHERE2 * R))

        else if (h <= H_MAX) then
            ! UPPER MESOSPHERE: Gradient layer
            P = p71 * (T / T71) ** (-G0 / (LAPSE_MESOSPHERE3 * R))

        else
            ! Above model limit - extrapolate
            P = p71 * (T / T71) ** (-G0 / (LAPSE_MESOSPHERE3 * R))
        end if

    end function isa_pressure

    !===========================================================================
    ! Calculate density at a given altitude using the ideal gas law.
    !
    ! AEROSPACE: Ideal Gas Law
    ! ------------------------
    ! The ideal gas law relates pressure, density, and temperature:
    !   P = rho * R * T
    !
    ! Solving for density:
    !   rho = P / (R * T)
    !
    ! Where:
    !   P = pressure [Pa]
    !   rho = density [kg/m^3]
    !   R = 287.05 J/(kg*K) (specific gas constant for air)
    !   T = temperature [K]
    !
    ! WHY DENSITY MATTERS:
    !   - Lift = 0.5 * rho * V^2 * S * CL
    !   - Drag = 0.5 * rho * V^2 * S * CD
    !   - Engine thrust depends on air mass flow (proportional to rho)
    !
    ! At high altitude, low density means:
    !   - Less lift (need higher speed or angle of attack)
    !   - Less drag (can fly faster more efficiently)
    !   - Less engine thrust (propeller/jet performance drops)
    !
    ! Parameters:
    !   h - Geometric altitude in meters
    !
    ! Returns:
    !   Density in kg/m^3
    !===========================================================================
    pure function isa_density(h) result(rho)
        real(dp), intent(in) :: h
        real(dp) :: rho
        real(dp) :: T, P

        ! Get temperature and pressure at this altitude
        T = isa_temperature(h)
        P = isa_pressure(h)

        ! Apply ideal gas law: rho = P / (R * T)
        ! At sea level: rho = 101325 / (287.05 * 288.15) = 1.225 kg/m^3
        ! At 11 km: rho = 22632 / (287.05 * 216.65) = 0.364 kg/m^3 (30% of sea level!)
        rho = P / (R * T)

    end function isa_density

    !===========================================================================
    ! Calculate speed of sound at a given altitude.
    !
    ! AEROSPACE: Speed of Sound
    ! -------------------------
    ! For an ideal gas, the speed of sound is:
    !   a = sqrt(gamma * R * T)
    !
    ! IMPORTANT: Speed of sound depends ONLY on temperature!
    ! - Higher temperature = faster sound speed
    ! - Pressure and density don't matter directly
    !
    ! DERIVATION:
    ! Sound is a pressure wave. Its speed depends on:
    !   - How compressible the medium is (gamma, R)
    !   - How fast molecules are moving (related to T)
    !
    ! EXAMPLE VALUES:
    !   Sea level (288 K): a = sqrt(1.4 * 287.05 * 288.15) = 340.3 m/s = 1225 km/h
    !   11 km (217 K):     a = sqrt(1.4 * 287.05 * 216.65) = 295.1 m/s = 1063 km/h
    !
    ! WHY THIS MATTERS:
    ! Mach number = velocity / speed_of_sound
    ! An aircraft at 250 m/s true airspeed:
    !   - At sea level: Mach 0.73 (subsonic)
    !   - At 11 km: Mach 0.85 (high subsonic, near transonic)
    !
    ! This is why aircraft fly at high altitude - same Mach number requires
    ! less fuel (lower drag) even though true airspeed is similar.
    !
    ! Parameters:
    !   h - Geometric altitude in meters
    !
    ! Returns:
    !   Speed of sound in m/s
    !===========================================================================
    pure function isa_speed_of_sound(h) result(a)
        real(dp), intent(in) :: h
        real(dp) :: a
        real(dp) :: T

        T = isa_temperature(h)

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Intrinsic Functions
        !
        ! `sqrt()` is a built-in (intrinsic) function in Fortran.
        ! Common mathematical intrinsics include:
        !   sqrt(x)  - square root
        !   exp(x)   - e^x
        !   log(x)   - natural logarithm
        !   sin(x), cos(x), tan(x) - trigonometric functions
        !   abs(x)   - absolute value
        !   min(a,b), max(a,b) - minimum/maximum
        !
        ! These are automatically available without any `use` statement.
        ! They also work element-wise on arrays!
        !-----------------------------------------------------------------------
        a = sqrt(GAMMA * R * T)

    end function isa_speed_of_sound

    !===========================================================================
    ! Calculate all atmospheric properties at a given altitude.
    !
    ! This is a convenience function that returns all properties at once
    ! in a derived type. This is useful when you need multiple properties
    ! (avoids redundant calculations).
    !
    ! FORTRAN CONCEPT: Returning Derived Types
    ! ----------------------------------------
    ! Functions can return derived types (structs), which is useful for
    ! returning multiple related values. The caller accesses members with %:
    !
    !   type(AtmosphericProperties) :: atm
    !   atm = isa_all_properties(10000.0_dp)
    !   print *, "Temperature:", atm%temperature
    !   print *, "Pressure:", atm%pressure
    !
    ! Note: There's some redundant calculation here (temperature is computed
    ! multiple times). In performance-critical code, you might compute once
    ! and pass to helper functions. Here, clarity is prioritized over speed.
    !
    ! Parameters:
    !   h - Geometric altitude in meters
    !
    ! Returns:
    !   AtmosphericProperties derived type with all values
    !===========================================================================
    pure function isa_all_properties(h) result(props)
        real(dp), intent(in) :: h
        type(AtmosphericProperties) :: props

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Derived Type Member Access
        !
        ! Use the `%` operator to access members of a derived type.
        ! This is different from most languages that use `.` (dot).
        !
        !   props%temperature = value    ! Fortran
        !   props.temperature = value;   // C++, Java, Python, etc.
        !
        ! The `%` was chosen to avoid ambiguity with Fortran's `.` operators
        ! like .and., .or., .eq., .lt., etc.
        !-----------------------------------------------------------------------

        props%altitude = h
        props%temperature = isa_temperature(h)
        props%pressure = isa_pressure(h)
        props%density = isa_density(h)
        props%speed_of_sound = isa_speed_of_sound(h)

    end function isa_all_properties

end module isa_module
