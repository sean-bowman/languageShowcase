!===============================================================================
! test_isa.f90 - Unit tests for ISA Calculator
!
! Validates calculations against known reference values from ICAO/ISO 2533.
!
! ===========================================================================
! SOFTWARE ENGINEERING CONCEPT: Unit Testing
! ===========================================================================
!
! Unit tests verify that individual pieces of code work correctly.
! For scientific/engineering code, this typically means:
!   1. Testing against known reference values (from standards, tables)
!   2. Testing edge cases (zero, negative, extreme values)
!   3. Testing boundary conditions (layer transitions in ISA)
!
! WHY UNIT TEST SCIENTIFIC CODE?
!   - Physics equations are easy to get wrong (signs, units, factors)
!   - Typos in constants can silently produce wrong results
!   - Refactoring can accidentally break working code
!   - Tests serve as documentation of expected behavior
!
! TESTING ISA CALCULATIONS:
! We compare against values from ISO 2533:1975 tables. These are the
! "official" values that our implementation should match.
!
! KEY REFERENCE VALUES (from ISO 2533):
!   Altitude    Temperature    Pressure       Density
!   -------     -----------    --------       -------
!   0 m         288.15 K       101325 Pa      1.225 kg/m^3
!   5000 m      255.65 K       54020 Pa       0.736 kg/m^3
!   11000 m     216.65 K       22632 Pa       0.364 kg/m^3
!   20000 m     216.65 K       5475 Pa        0.088 kg/m^3
!
! ===========================================================================
! FORTRAN CONCEPTS IN THIS FILE
! ===========================================================================
!
! 1. LOGICAL TYPE
!    - .true. and .false. (note the dots!)
!    - Used for boolean conditions and test results
!
! 2. CHARACTER(*) ASSUMED LENGTH
!    - `character(len=*)` takes any length string
!    - Only allowed for dummy arguments, not declarations
!
! 3. HOST ASSOCIATION
!    - Internal subprograms can access host's variables
!    - tests_passed, tests_failed modified by subroutines
!    - This is like closure variables in other languages
!
! 4. FLOATING-POINT COMPARISON
!    - Never use == for floats (rounding errors!)
!    - Use tolerance-based comparison: |actual - expected| <= tol * |expected|
!
! Usage:
!   fpm test          - Run all tests
!   ./test_isa        - Run directly
!
!===============================================================================
program test_isa
    use constants_module    ! dp = double precision kind
    use isa_module         ! All ISA calculation functions
    implicit none

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Module-Level Variables for Test State
    !
    ! These variables track test results across all test subroutines.
    ! Because internal subprograms (after `contains`) can access these
    ! via HOST ASSOCIATION, we don't need to pass them as arguments.
    !
    ! HOST ASSOCIATION means internal procedures can:
    !   - READ variables from the enclosing scope
    !   - MODIFY variables from the enclosing scope
    !
    ! This is similar to closures in JavaScript/Python, but more powerful
    ! because you can modify the captured variables.
    !---------------------------------------------------------------------------
    integer :: tests_passed    ! Counter for successful tests
    integer :: tests_failed    ! Counter for failed tests
    logical :: all_passed      ! Overall result flag (not currently used)

    ! Initialize test counters
    tests_passed = 0
    tests_failed = 0

    ! Print test header
    print '(A)', ''
    print '(A)', '============================================'
    print '(A)', '     ISA Calculator - Unit Tests'
    print '(A)', '============================================'
    print '(A)', ''

    !---------------------------------------------------------------------------
    ! Run test suites for each property
    ! Each subroutine tests one ISA calculation function
    !---------------------------------------------------------------------------
    call test_temperature()      ! Test isa_temperature()
    call test_pressure()         ! Test isa_pressure()
    call test_density()          ! Test isa_density()
    call test_speed_of_sound()     ! Test isa_speed_of_sound()

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Formatted Integer Output
    !
    ! '(A,I3,A)' format:
    !   A  = character string
    !   I3 = integer, 3 characters wide (right-aligned)
    !   A  = another string (empty in this case)
    !
    ! For numbers that might be larger, use more width or I0 (auto-width).
    !---------------------------------------------------------------------------
    print '(A)', ''
    print '(A)', '============================================'
    print '(A,I3,A)', 'Tests passed: ', tests_passed, ''
    print '(A,I3,A)', 'Tests failed: ', tests_failed, ''
    print '(A)', '============================================'

    !---------------------------------------------------------------------------
    ! FORTRAN CONCEPT: Exit Status
    !
    ! `stop n` exits the program with status code n.
    !   - stop    (no number) = success (code 0)
    !   - stop 1  = error (code 1)
    !
    ! Test frameworks and CI/CD systems check exit codes:
    !   0 = all tests passed
    !   non-zero = some tests failed
    !
    ! The shell can check this: `./test_isa && echo "All tests passed"`
    !---------------------------------------------------------------------------
    if (tests_failed > 0) then
        print '(A)', 'SOME TESTS FAILED!'
        stop 1  ! Exit with error code 1 (indicates failure)
    else
        print '(A)', 'ALL TESTS PASSED!'
        ! Implicit stop 0 at end of program
    end if

!===============================================================================
! INTERNAL TEST HELPER FUNCTIONS AND SUBROUTINES
!===============================================================================
contains

    !---------------------------------------------------------------------------
    ! Check if two floating-point values are approximately equal.
    !
    ! FORTRAN CONCEPT: Floating-Point Comparison
    ! -------------------------------------------
    ! NEVER use == to compare floating-point numbers!
    !
    ! Due to rounding errors, calculations that should be equal often differ
    ! by tiny amounts:
    !   0.1 + 0.2 == 0.3  ! Often FALSE due to binary representation!
    !
    ! Instead, use a tolerance-based comparison:
    !   |actual - expected| <= tolerance * |expected|
    !
    ! This is RELATIVE tolerance - the acceptable error scales with the value.
    ! For example, tolerance = 0.001 means:
    !   - For expected = 100, allow error up to 0.1
    !   - For expected = 1000000, allow error up to 1000
    !
    ! ALTERNATIVE: Absolute tolerance (fixed error regardless of magnitude)
    !   |actual - expected| <= tolerance
    !
    ! Parameters:
    !   actual    - The computed value
    !   expected  - The reference value
    !   tolerance - Relative tolerance (e.g., 0.001 = 0.1%)
    !
    ! Returns:
    !   .true. if values are approximately equal
    !---------------------------------------------------------------------------
    function approx_equal(actual, expected, tolerance) result(isEqual)
        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Function with result Clause
        !
        ! `function name(args) result(varname)`
        !
        ! The result clause names the return variable. This is clearer than
        ! using the function name as the return variable (old Fortran style).
        !
        ! You can use the result variable just like any local variable,
        ! then it automatically becomes the return value.
        !-----------------------------------------------------------------------
        real(dp), intent(in) :: actual     ! Computed value from ISA functions
        real(dp), intent(in) :: expected   ! Reference value from ISO 2533
        real(dp), intent(in) :: tolerance  ! Relative tolerance (fraction)
        logical :: isEqual                 ! Result: true if close enough

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: abs() Intrinsic
        !
        ! abs(x) returns the absolute value of x.
        ! Works for real, integer, and complex numbers.
        !
        ! The comparison |actual - expected| <= tolerance * |expected|
        ! is a relative tolerance test. For small expected values near zero,
        ! you might want an absolute tolerance instead.
        !-----------------------------------------------------------------------
        isEqual = abs(actual - expected) <= tolerance * abs(expected)
    end function approx_equal

    !---------------------------------------------------------------------------
    ! Report the result of a single test.
    !
    ! Updates the global test counters and prints pass/fail status.
    !
    ! FORTRAN CONCEPT: Assumed-Length Character Arguments
    ! ----------------------------------------------------
    ! `character(len=*)` declares a character argument that can accept
    ! any length string. The actual length is determined by the caller.
    !
    ! This is ONLY allowed for dummy arguments (parameters), not for
    ! regular variable declarations.
    !
    ! Without len=*, you'd have to declare a fixed maximum length:
    !   character(len=100) :: name  ! Can only hold up to 100 characters
    !
    ! With len=*, the subroutine accepts any length:
    !   call report_test('Short', ...)
    !   call report_test('This is a much longer test name', ...)
    !
    ! Parameters:
    !   name     - Description of the test
    !   passed   - True if test passed, false otherwise
    !   actual   - Computed value (for error reporting)
    !   expected - Expected value (for error reporting)
    !---------------------------------------------------------------------------
    subroutine report_test(name, passed, actual, expected)
        character(len=*), intent(in) :: name   ! Test description (any length)
        logical, intent(in) :: passed          ! Did the test pass?
        real(dp), intent(in) :: actual         ! What we calculated
        real(dp), intent(in) :: expected       ! What we expected

        !-----------------------------------------------------------------------
        ! FORTRAN CONCEPT: Host Association
        !
        ! This subroutine directly modifies `tests_passed` and `tests_failed`
        ! which are declared in the enclosing program.
        !
        ! No need to pass them as arguments - internal subprograms
        ! automatically have access to the host's variables.
        !
        ! This is powerful but can make code harder to understand if overused.
        ! For larger programs, explicit arguments are usually clearer.
        !-----------------------------------------------------------------------
        if (passed) then
            tests_passed = tests_passed + 1    ! Modify host's variable
            print '(A,A,A)', '  [PASS] ', name, ''
        else
            tests_failed = tests_failed + 1    ! Modify host's variable
            print '(A,A,A)', '  [FAIL] ', name, ''
            !-------------------------------------------------------------------
            ! ES14.6 format: Scientific notation, 14 chars wide, 6 decimals
            ! Example: "  1.234567E+02"
            !
            ! This is useful for showing the exact values when tests fail.
            !-------------------------------------------------------------------
            print '(A,ES14.6)', '         Expected: ', expected
            print '(A,ES14.6)', '         Actual:   ', actual
        end if
    end subroutine report_test

    !---------------------------------------------------------------------------
    ! Test temperature calculations at various altitudes.
    !
    ! AEROSPACE: Temperature Test Points
    ! -----------------------------------
    ! We test at strategically chosen altitudes:
    !   0 m:     Sea level reference (T0 = 288.15 K)
    !   5000 m:  Mid-troposphere (verifies lapse rate)
    !   11000 m: Tropopause (boundary condition)
    !   15000 m: Inside isothermal layer (should be same as 11 km)
    !   20000 m: Top of tropopause (boundary condition)
    !   25000 m: Stratosphere (verifies positive lapse rate)
    !
    ! Formula verification at 5000 m:
    !   T = T0 + L * h
    !   T = 288.15 + (-0.0065) * 5000
    !   T = 288.15 - 32.5
    !   T = 255.65 K
    !---------------------------------------------------------------------------
    subroutine test_temperature()
        real(dp) :: T        ! Calculated temperature
        logical :: passed    ! Test result

        print '(A)', 'Temperature Tests:'
        print '(A)', '------------------'

        ! Test 1: Sea level (the reference point)
        ! t0 = 288.15 K = 15 degrees C = 59 degrees F
        T = isa_temperature(0.0_dp)
        passed = approx_equal(T, 288.15_dp, 0.0001_dp)  ! 0.01% tolerance
        call report_test('Sea level (0 m): 288.15 K', passed, T, 288.15_dp)

        ! Test 2: 5000 m (mid-troposphere)
        ! Verifies the -6.5 K/km lapse rate
        ! T = 288.15 - 0.0065*5000 = 255.65 K
        T = isa_temperature(5000.0_dp)
        passed = approx_equal(T, 255.65_dp, 0.001_dp)   ! 0.1% tolerance
        call report_test('5000 m: 255.65 K', passed, T, 255.65_dp)

        ! Test 3: 11000 m (tropopause base)
        ! This is where temperature stops decreasing
        ! T = 288.15 - 0.0065*11000 = 216.65 K = -56.5 C
        T = isa_temperature(11000.0_dp)
        passed = approx_equal(T, 216.65_dp, 0.0001_dp)
        call report_test('11000 m (tropopause): 216.65 K', passed, T, 216.65_dp)

        ! Test 4: 15000 m (inside isothermal layer)
        ! Temperature should be the same as at 11 km
        T = isa_temperature(15000.0_dp)
        passed = approx_equal(T, 216.65_dp, 0.0001_dp)
        call report_test('15000 m (isothermal): 216.65 K', passed, T, 216.65_dp)

        ! Test 5: 20000 m (top of tropopause)
        ! Still isothermal, same temperature
        T = isa_temperature(20000.0_dp)
        passed = approx_equal(T, 216.65_dp, 0.0001_dp)
        call report_test('20000 m: 216.65 K', passed, T, 216.65_dp)

        ! Test 6: 25000 m (lower stratosphere)
        ! Temperature now INCREASES at +1.0 K/km
        ! T = 216.65 + 0.001*(25000-20000) = 216.65 + 5 = 221.65 K
        T = isa_temperature(25000.0_dp)
        passed = approx_equal(T, 221.65_dp, 0.001_dp)
        call report_test('25000 m: 221.65 K', passed, T, 221.65_dp)

        print '(A)', ''
    end subroutine test_temperature

    !---------------------------------------------------------------------------
    ! Test pressure calculations at various altitudes.
    !
    ! AEROSPACE: Pressure Test Points
    ! --------------------------------
    ! Pressure decreases roughly exponentially with altitude.
    ! At 5.5 km, pressure is about half of sea level.
    ! At 11 km, pressure is about 22% of sea level.
    ! At 20 km, pressure is about 5% of sea level.
    !
    ! These tests verify both:
    !   - Gradient layer formula (troposphere, stratosphere)
    !   - Isothermal layer formula (tropopause)
    !---------------------------------------------------------------------------
    subroutine test_pressure()
        real(dp) :: P        ! Calculated pressure
        logical :: passed    ! Test result

        print '(A)', 'Pressure Tests:'
        print '(A)', '---------------'

        ! Test 1: Sea level (the reference point)
        ! p0 = 101325 Pa = 1013.25 hPa = 1 atm
        P = isa_pressure(0.0_dp)
        passed = approx_equal(P, 101325.0_dp, 0.0001_dp)
        call report_test('Sea level (0 m): 101325 Pa', passed, P, 101325.0_dp)

        ! Test 2: 5000 m
        ! About 53% of sea level pressure
        ! Calculated using gradient layer barometric formula
        P = isa_pressure(5000.0_dp)
        passed = approx_equal(P, 54019.9_dp, 0.01_dp)   ! 1% tolerance
        call report_test('5000 m: ~54020 Pa', passed, P, 54019.9_dp)

        ! Test 3: 11000 m (tropopause base)
        ! About 22% of sea level pressure
        ! This is an important reference point
        P = isa_pressure(11000.0_dp)
        passed = approx_equal(P, 22632.06_dp, 0.001_dp)
        call report_test('11000 m: 22632 Pa', passed, P, 22632.06_dp)

        ! Test 4: 20000 m (top of tropopause)
        ! About 5.4% of sea level pressure
        ! Tests the isothermal layer formula
        P = isa_pressure(20000.0_dp)
        passed = approx_equal(P, 5474.889_dp, 0.001_dp)
        call report_test('20000 m: ~5475 Pa', passed, P, 5474.889_dp)

        print '(A)', ''
    end subroutine test_pressure

    !---------------------------------------------------------------------------
    ! Test density calculations at various altitudes.
    !
    ! AEROSPACE: Why Density Matters
    ! ------------------------------
    ! Density affects:
    !   - Lift: L = 0.5 * rho * V^2 * S * CL
    !   - Drag: D = 0.5 * rho * V^2 * S * CD
    !   - Engine performance (mass flow rate)
    !
    ! At high altitude, low density means:
    !   - Need higher airspeed to generate same lift
    !   - Less drag (good for fuel efficiency)
    !   - Reduced engine thrust
    !
    ! Density is calculated from P and T using ideal gas law:
    !   rho = P / (R * T)
    !---------------------------------------------------------------------------
    subroutine test_density()
        real(dp) :: rho      ! Calculated density
        logical :: passed    ! Test result

        print '(A)', 'Density Tests:'
        print '(A)', '--------------'

        ! Test 1: Sea level
        ! rho0 = 1.225 kg/m^3 (this is the ISA reference value)
        rho = isa_density(0.0_dp)
        passed = approx_equal(rho, 1.225_dp, 0.001_dp)
        call report_test('Sea level (0 m): 1.225 kg/m^3', passed, rho, 1.225_dp)

        ! Test 2: 5000 m
        ! About 60% of sea level density
        ! rho = 54020 / (287.05 * 255.65) = 0.736 kg/m^3
        rho = isa_density(5000.0_dp)
        passed = approx_equal(rho, 0.7364_dp, 0.01_dp)
        call report_test('5000 m: ~0.736 kg/m^3', passed, rho, 0.7364_dp)

        ! Test 3: 10000 m (near cruise altitude)
        ! About 34% of sea level density
        ! This low density is why jets cruise at high altitude (less drag)
        rho = isa_density(10000.0_dp)
        passed = approx_equal(rho, 0.4135_dp, 0.01_dp)
        call report_test('10000 m: ~0.414 kg/m^3', passed, rho, 0.4135_dp)

        print '(A)', ''
    end subroutine test_density

    !---------------------------------------------------------------------------
    ! Test speed of sound calculations.
    !
    ! AEROSPACE: Speed of Sound and Mach Number
    ! ------------------------------------------
    ! Speed of sound depends ONLY on temperature:
    !   a = sqrt(gamma * R * T)
    !
    ! Mach number = V / a (velocity relative to sound speed)
    !
    ! At sea level (T = 288.15 K):
    !   a = sqrt(1.4 * 287.05 * 288.15) = 340.3 m/s = 1225 km/h
    !
    ! At 11 km (T = 216.65 K):
    !   a = sqrt(1.4 * 287.05 * 216.65) = 295.1 m/s = 1063 km/h
    !
    ! This is why an aircraft at Mach 0.8 flies faster (in m/s) at sea level
    ! than at 11 km - but uses more fuel due to higher drag!
    !---------------------------------------------------------------------------
    subroutine test_speed_of_sound()
        real(dp) :: a        ! Calculated speed of sound
        logical :: passed    ! Test result

        print '(A)', 'Speed of Sound Tests:'
        print '(A)', '---------------------'

        ! Test 1: Sea level
        ! a = sqrt(1.4 * 287.05 * 288.15) = 340.29 m/s
        a = isa_speed_of_sound(0.0_dp)
        passed = approx_equal(a, 340.29_dp, 0.001_dp)
        call report_test('Sea level (0 m): 340.29 m/s', passed, a, 340.29_dp)

        ! Test 2: 11000 m (tropopause)
        ! a = sqrt(1.4 * 287.05 * 216.65) = 295.07 m/s
        ! Note: Sound speed is constant throughout the isothermal layer
        ! (11-20 km) because temperature is constant
        a = isa_speed_of_sound(11000.0_dp)
        passed = approx_equal(a, 295.07_dp, 0.001_dp)
        call report_test('11000 m: 295.07 m/s', passed, a, 295.07_dp)

        print '(A)', ''
    end subroutine test_speed_of_sound

end program test_isa
