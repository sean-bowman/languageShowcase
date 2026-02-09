!===============================================================================
! main.f90 - ISA Calculator Command Line Interface
!
! International Standard Atmosphere Calculator
! Computes atmospheric properties at specified altitude
!
! The ISA model is used daily in aviation for:
!
! 1. ALTIMETER CALIBRATION
!    - Aircraft altimeters measure pressure and convert to altitude
!    - The conversion uses ISA pressure-altitude relationship
!    - That's why pilots set "altimeter setting" (local pressure correction)
!
! 2. PERFORMANCE CHARTS
!    - Aircraft takeoff/landing distances depend on air density
!    - Performance charts are computed for ISA conditions
!    - Hot days ("ISA + 15C") significantly reduce performance!
!
! 3. ENGINE POWER RATINGS
!    - Jet and piston engines are rated at ISA sea level
!    - Actual power varies with altitude and temperature
!    - "Flat rated" engines maintain power up to certain altitude
!
! 4. FLIGHT LEVELS
!    - Above 18,000 ft, all aircraft use standard altimeter setting (29.92 inHg)
!    - This is the ISA sea level pressure - ensures vertical separation
!    - Called "Flight Level" (FL350 = 35,000 ft on standard altimeter)
!
! ===========================================================================
! FORTRAN CONCEPTS IN THIS FILE
! ===========================================================================
!
! 1. PROGRAM UNIT
!    - `program name ... end program name` is the main entry point
!    - Only ONE program can exist per executable
!    - Similar to `int main()` in C/C++
!
! 2. SUBROUTINES vs FUNCTIONS
!    - Subroutines: called with `call name(args)`, no return value
!    - Functions: called as `result = name(args)`, returns a value
!    - Subroutines can have multiple output arguments (intent out/inout)
!
! 3. INTERNAL SUBPROGRAMS (contains)
!    - Subroutines/functions defined inside a program after `contains`
!    - Have access to all host's variables (like closures)
!    - Only visible within the host program
!
! 4. FORMATTED OUTPUT (print/write)
!    - Format strings in parentheses: '(A)', '(F10.2)', etc.
!    - A = character string
!    - F = fixed-point real (F10.2 = 10 chars wide, 2 decimal places)
!    - ES = scientific notation (ES12.4 = 1.2345E+02)
!    - I = integer
!
! 5. COMMAND-LINE ARGUMENTS
!    - command_argument_count() returns number of arguments
!    - get_command_argument(n, arg) gets nth argument as string
!    - Must parse strings to numbers manually (unlike Python)
!
! 6. ARRAY LITERALS AND SIZE
!    - [1.0, 2.0, 3.0] creates an array literal
!    - size(array) returns number of elements
!    - do i = 1, size(arr) iterates over array
!
! Usage:
!   isa_calculator              - Display table of ISA values
!   isa_calculator 10000        - Calculate properties at 10 km altitude
!
!===============================================================================
program isa_calculator
    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: use statement
    !
    ! Import modules at the very beginning of the program.
    ! This makes all PUBLIC items from those modules available.
    !
    ! Order matters: modules must be compiled before programs that use them.
    ! Build systems (fpm, CMake) handle this automatically.
    !---------------------------------------------------------------------------
    use constants_module    ! dp, T0, P0, RHO0
    use isa_module         ! isa_all_properties, AtmosphericProperties
    implicit none

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Variable Declarations
    !
    ! All variables MUST be declared before use (implicit none enforces this).
    ! Declaration syntax: type[, attributes] :: name [= initial_value]
    !
    ! character(len=100) :: arg
    !   - Character string of fixed length 100
    !   - Fortran strings have fixed length (unlike C++ std::string)
    !   - Extra space is filled with blanks
    !
    ! real(dp) :: altitude
    !   - Real number with kind `dp` (double precision)
    !   - dp was defined in constants_module
    !
    ! integer :: num_args, ios
    !   - Default integer (usually 32-bit)
    !   - ios = I/O status code for error checking
    !---------------------------------------------------------------------------
    character(len=100) :: arg          ! Buffer for command-line argument
    real(dp) :: altitude               ! Altitude in meters
    type(AtmosphericProperties) :: props  ! Result from ISA calculation
    integer :: num_args                ! Number of command-line arguments
    integer :: ios                     ! I/O status code

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Intrinsic Functions for Command Line
    !
    ! command_argument_count() - Returns number of arguments (not including program name)
    ! get_command_argument(n, arg) - Gets nth argument as a string
    !
    ! Unlike C/C++ argc/argv:
    !   - Argument 0 is the program name
    !   - Arguments 1, 2, 3, ... are user arguments
    !   - Count does NOT include program name (unlike argc)
    !
    ! Example: ./isa_calculator 10000
    !   command_argument_count() = 1
    !   get_command_argument(0, arg) -> "isa_calculator"
    !   get_command_argument(1, arg) -> "10000"
    !---------------------------------------------------------------------------
    num_args = command_argument_count()

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: if/else if/else Structure
    !
    ! if (condition) then
    !     statements
    ! else if (condition) then
    !     statements
    ! else
    !     statements
    ! end if
    !
    ! Note: `then` is REQUIRED (unlike C/C++/Java)
    ! Note: Blocks end with `end if`, not braces
    !---------------------------------------------------------------------------
    if (num_args == 0) then
        ! No arguments provided - show the full atmosphere table
        ! This is the "demo mode" - useful for seeing ISA at a glance
        call print_header()
        call print_atmosphere_table()

    else if (num_args == 1) then
        ! One argument provided - interpret as altitude in meters
        ! Get the argument as a string
        call get_command_argument(1, arg)

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Internal Read (String to Number)
        !
        ! read(string, format, iostat=ios) variable
        !
        ! This reads FROM a string INTO a variable (internal read).
        ! It's like sscanf() in C or parseInt() in JavaScript.
        !
        ! format = *  means "list-directed" (Fortran figures it out)
        ! iostat = ios stores status code (0 = success, nonzero = error)
        !
        ! ALWAYS use iostat for user input - never trust external data!
        !-----------------------------------------------------------------------
        read(arg, *, iostat=ios) altitude

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Error Handling
        !
        ! iostat returns:
        !   0 = success
        !   > 0 = error (invalid input, etc.)
        !   < 0 = end-of-file or end-of-record
        !
        ! /= is "not equal" in Fortran (like != in C/C++)
        !-----------------------------------------------------------------------
        if (ios /= 0) then
            print '(A)', 'Error: Invalid altitude value. Please enter a number in meters.'
            stop 1  ! Exit with error code 1
        end if

        ! Valid altitude - calculate and display properties
        call print_header()
        props = isa_all_properties(altitude)
        call print_single_altitude(props)

    else
        ! Too many arguments - show usage
        print '(A)', 'Usage: isa_calculator [altitude_in_meters]'
        print '(A)', '       Without arguments: displays atmosphere table'
        print '(A)', '       With altitude: calculates properties at that altitude'
        stop 1  ! Exit with error code 1
    end if

!===============================================================================
! FORTRAN CONCEPT: Internal Subprograms (contains section)
!
! The `contains` statement introduces internal subprograms.
! These are subroutines/functions defined INSIDE the program.
!
! KEY PROPERTIES:
!   1. They can access all variables from the host program (like closures)
!   2. They are ONLY visible within the host program
!   3. They can use the host's modules (constants_module, isa_module)
!   4. They appear AFTER contains and BEFORE end program
!
! WHEN TO USE:
!   - Helper functions specific to this program
!   - Functions that need access to program variables
!   - Keeping implementation details private
!
! For reusable code, put procedures in a MODULE instead.
!===============================================================================
contains

    !---------------------------------------------------------------------------
    ! Print program header and sea level reference values.
    !
    ! FORTRAN CONCEPT: Subroutines
    ! ----------------------------
    ! A subroutine is called with `call name(args)` and has no return value.
    ! It can have zero or more arguments.
    !
    ! subroutine name(arg1, arg2)
    !     ! declarations
    !     ! statements
    ! end subroutine name
    !
    ! Compare to functions:
    !   - Subroutine: `call print_header()` - no return value
    !   - Function: `result = isa_temperature(h)` - returns a value
    !
    ! Use subroutines when:
    !   - No value needs to be returned (I/O, side effects)
    !   - Multiple values need to be returned (use intent(out) args)
    !---------------------------------------------------------------------------
    subroutine print_header()
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Formatted Output
        !
        ! print '(format)', variables
        !
        ! Format specifiers (in parentheses):
        !   A     = Character string (automatic length)
        !   An    = Character string, n characters wide
        !   Fw.d  = Fixed-point real, w wide, d decimal places
        !   ESw.d = Scientific notation (1.23E+04)
        !   ENw.d = Engineering notation (exponent multiple of 3)
        !   Iw    = Integer, w characters wide
        !   X     = Skip one space
        !   /     = Newline
        !
        ! Multiple specifiers are comma-separated: '(A, F10.2, A)'
        !
        ! AEROSPACE NOTE:
        ! These sea level values are the foundation of the ISA model.
        ! Every aircraft altimeter is calibrated against these values!
        !-----------------------------------------------------------------------
        print '(A)', ''
        print '(A)', '=============================================================='
        print '(A)', '     International Standard Atmosphere (ISA) Calculator'
        print '(A)', '              Based on ISO 2533:1975'
        print '(A)', '=============================================================='
        print '(A)', ''
        print '(A)', 'Sea Level Reference Values:'
        ! F10.2 = 10 characters total, 2 after decimal point
        print '(A,F10.2,A)', '  Temperature:     ', T0, ' K  (15.00 C)'
        print '(A,F10.1,A)', '  Pressure:        ', P0, ' Pa (101.325 kPa)'
        print '(A,F10.4,A)', '  Density:         ', RHO0, ' kg/m^3'
        print '(A)', ''
    end subroutine print_header

    !---------------------------------------------------------------------------
    ! Print atmospheric properties for a single altitude.
    !
    ! This shows the user-friendly output with multiple unit conversions.
    !
    ! AEROSPACE CONTEXT - UNIT CONVERSIONS:
    !   - Altitude: m, km, ft (pilots use feet in most of the world)
    !   - Temperature: K (scientific), C (everyday), F (US aviation)
    !   - Pressure: Pa (SI), kPa, atm (relative to sea level)
    !   - Speed: m/s (SI), km/h (metric countries), knots (aviation)
    !
    ! 1 meter = 3.28084 feet
    ! 1 m/s = 3.6 km/h = 1.94384 knots
    ! Kelvin = Celsius + 273.15
    !
    ! Parameters:
    !   props - AtmosphericProperties derived type with all values
    !---------------------------------------------------------------------------
    subroutine print_single_altitude(props)
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Subroutine Arguments with intent
        !
        ! type, intent(in) :: arg
        !
        ! intent(in) = input only, cannot be modified
        ! intent(out) = output only, must be set before returning
        ! intent(inout) = can be read and modified
        !
        ! For derived types, the whole structure is passed by reference.
        ! intent(in) prevents modifying any member.
        !-----------------------------------------------------------------------
        type(AtmosphericProperties), intent(in) :: props
        real(dp) :: temp_celsius  ! Local variable for unit conversion

        ! Convert Kelvin to Celsius
        temp_celsius = props%temperature - 273.15_dp

        print '(A)', 'Atmospheric Properties:'
        print '(A)', '------------------------'

        ! Altitude in multiple units
        print '(A,F12.1,A)', '  Altitude:         ', props%altitude, ' m'
        print '(A,F12.1,A)', '                    ', props%altitude / 1000.0_dp, ' km'
        print '(A,F12.1,A)', '                    ', props%altitude * 3.28084_dp, ' ft'
        print '(A)', ''

        ! Temperature in Kelvin and Celsius
        print '(A,F12.2,A)', '  Temperature:      ', props%temperature, ' K'
        print '(A,F12.2,A)', '                    ', temp_celsius, ' C'
        print '(A)', ''

        ! Pressure in multiple units
        ! ES12.4 = scientific notation, 12 wide, 4 significant figures after decimal
        print '(A,ES12.4,A)', '  Pressure:         ', props%pressure, ' Pa'
        print '(A,F12.4,A)', '                    ', props%pressure / 1000.0_dp, ' kPa'
        print '(A,F12.4,A)', '                    ', props%pressure / P0, ' atm'
        print '(A)', ''

        ! Density (absolute and relative to sea level)
        print '(A,ES12.4,A)', '  Density:          ', props%density, ' kg/m^3'
        print '(A,F12.4,A)', '                    ', props%density / RHO0, ' (ratio to sea level)'
        print '(A)', ''

        ! Speed of sound in multiple units
        ! Note: 1 knot = 1 nautical mile per hour = 1.852 km/h = 0.514444 m/s
        ! So 1 m/s = 1/0.514444 = 1.94384 knots
        print '(A,F12.2,A)', '  Speed of Sound:   ', props%speed_of_sound, ' m/s'
        print '(A,F12.2,A)', '                    ', props%speed_of_sound * 3.6_dp, ' km/h'
        print '(A,F12.2,A)', '                    ', props%speed_of_sound * 1.94384_dp, ' knots'
        print '(A)', ''
    end subroutine print_single_altitude

    !---------------------------------------------------------------------------
    ! Print a table of atmospheric properties at various altitudes.
    !
    ! This provides a quick reference for common altitudes, from sea level
    ! to 80 km (near the edge of space).
    !
    ! AEROSPACE CONTEXT - WHY THESE ALTITUDES?
    !   0 km:  Sea level reference
    !   1-5 km: Light aircraft, mountain airports
    !   6-10 km: Regional jets, turboprops
    !   11 km: Commercial jet cruise begins (36,000 ft)
    !   15-20 km: High-altitude reconnaissance (U-2, SR-71)
    !   25-35 km: Weather balloons, stratospheric aircraft
    !   50+ km: Edge of space, mesosphere
    !   80 km: Near Karman line (100 km = "space")
    !---------------------------------------------------------------------------
    subroutine print_atmosphere_table()
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Array Literals
        !
        ! Arrays can be initialized with literal values:
        !   array = [value1, value2, value3, ...]
        !
        ! For long arrays, use & for line continuation:
        !   array = [value1, value2, &
        !            value3, value4]
        !
        ! The & MUST be at the end of the line (not the beginning of next).
        ! Comments are NOT allowed after &.
        !
        ! Array indexing in Fortran starts at 1 by default (not 0!)
        !-----------------------------------------------------------------------
        real(dp) :: altitudes(21)  ! Array of 21 altitudes
        type(AtmosphericProperties) :: props
        integer :: i

        ! Define altitudes to display (in meters)
        ! These span from sea level to near the edge of space
        altitudes = [0.0_dp, 1000.0_dp, 2000.0_dp, 3000.0_dp, 4000.0_dp, 5000.0_dp, &
                     6000.0_dp, 7000.0_dp, 8000.0_dp, 9000.0_dp, 10000.0_dp, &
                     11000.0_dp, 15000.0_dp, 20000.0_dp, 25000.0_dp, 30000.0_dp, &
                     35000.0_dp, 40000.0_dp, 50000.0_dp, 60000.0_dp, 80000.0_dp]

        print '(A)', 'Standard Atmosphere Table'
        print '(A)', '========================='
        print '(A)', ''
        print '(A)', '  Altitude      Temperature      Pressure        Density       Sound Speed'
        print '(A)', '    [km]           [K]            [Pa]          [kg/m^3]         [m/s]'
        print '(A)', '  --------      -----------      --------       ---------      -----------'

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: do Loop
        !
        ! do variable = start, end [, step]
        !     statements
        ! end do
        !
        ! - start, end, step are expressions (evaluated once at loop start)
        ! - step defaults to 1 if not specified
        ! - Loop runs while variable <= end (or >= end if step < 0)
        !
        ! The size() intrinsic returns the number of elements in an array.
        ! So this loops from i=1 to i=21 (21 elements in altitudes array).
        !
        ! IMPORTANT: Fortran arrays are 1-indexed by default!
        !   altitudes(1) is the first element (not altitudes(0))
        !-----------------------------------------------------------------------
        do i = 1, size(altitudes)
            props = isa_all_properties(altitudes(i))

            !-------------------------------------------------------------------
            ! FORTRAN CONCEPT: Complex Format Strings
            !
            ! '(F10.1, F14.2, ES16.4, ES14.4, F14.2)'
            !
            ! This format string prints 5 values on one line:
            !   F10.1  - Fixed-point, 10 chars, 1 decimal (altitude in km)
            !   F14.2  - Fixed-point, 14 chars, 2 decimals (temperature)
            !   ES16.4 - Scientific, 16 chars, 4 decimals (pressure)
            !   ES14.4 - Scientific, 14 chars, 4 decimals (density)
            !   F14.2  - Fixed-point, 14 chars, 2 decimals (speed of sound)
            !
            ! The commas in the format string add no extra space; the widths
            ! (10, 14, 16, etc.) control the spacing.
            !-------------------------------------------------------------------
            print '(F10.1, F14.2, ES16.4, ES14.4, F14.2)', &
                props%altitude / 1000.0_dp, &   ! Convert m to km
                props%temperature, &            ! Already in K
                props%pressure, &               ! Already in Pa
                props%density, &                ! Already in kg/m^3
                props%speed_of_sound            ! Already in m/s
        end do

        ! Print explanatory notes
        print '(A)', ''
        print '(A)', 'Note: Temperature lapse rate varies by atmospheric layer:'
        print '(A)', '  Troposphere (0-11 km):    -6.5 K/km'
        print '(A)', '  Tropopause (11-20 km):     0.0 K/km (isothermal)'
        print '(A)', '  Stratosphere (20-32 km): +1.0 K/km'
        print '(A)', ''
    end subroutine print_atmosphere_table

end program isa_calculator
