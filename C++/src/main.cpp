/*
 * main.cpp - Entry point for the Hohmann Transfer Calculator command-line tool
 *
 * ==============================================================================
 * C++ CONCEPT: The main() Function - Program Entry Point
 * ==============================================================================
 *
 * Every C++ program begins execution at the main() function. The operating
 * system calls main() when you run your program. This is different from some
 * other languages:
 *
 *   C++:        int main() { ... }         <-- Function, not inside a class
 *   Java:       public static void main()  <-- Must be inside a class
 *   C#:         static void Main()         <-- Must be inside a class
 *   Python:     if __name__ == "__main__": <-- Convention, not required
 *   Rust:       fn main() { ... }          <-- Similar to C++
 *
 * RETURN VALUE:
 *   main() returns an int to the operating system:
 *     - 0 = success (program completed normally)
 *     - non-zero = error (convention: different codes for different errors)
 *
 *   You can check this in the shell:
 *     $ ./hohmann; echo $?    # Linux/Mac - prints return value
 *     > hohmann.exe & echo %ERRORLEVEL%  # Windows
 *
 * COMMAND-LINE ARGUMENTS:
 *   int main(int argc, char* argv[])
 *
 *   argc = "argument count" - number of arguments (including program name)
 *   argv = "argument vector" - array of C-style strings
 *
 *   Example: ./hohmann 400 35786
 *     argc = 3
 *     argv[0] = "./hohmann"    (program name)
 *     argv[1] = "400"          (first argument - string, not number!)
 *     argv[2] = "35786"        (second argument)
 *
 * ==============================================================================
 * C++ CONCEPT: #include and Header Files
 * ==============================================================================
 *
 * #include copies the contents of another file into this file before compiling.
 * This is how we access functions/classes defined elsewhere.
 *
 * TWO STYLES OF INCLUDE:
 *   #include <iostream>      Angle brackets: search system/library paths
 *   #include "hohmann/..."   Quotes: search project directories first
 *
 * WHAT'S IN THESE HEADERS:
 *   <iostream>   - std::cout, std::cin, std::cerr (console I/O)
 *   <iomanip>    - std::setprecision, std::fixed (formatting)
 *   <string>     - std::string class
 *   <cstdlib>    - std::stod (string to double conversion)
 *
 * ==============================================================================
 * AEROSPACE APPLICATION: Mission Planning Tool
 * ==============================================================================
 *
 * This program calculates Hohmann transfers between Earth orbits. Real mission
 * planning is more complex (see notes below), but Hohmann transfers provide
 * excellent first approximations for:
 *
 *   - Initial fuel budget estimates
 *   - Mission timeline planning
 *   - Comparing different orbital destinations
 *
 * REAL-WORLD CONSIDERATIONS NOT MODELED HERE:
 *   - Launch window timing (planets/orbits must be aligned)
 *   - Orbital plane changes (inclination changes are expensive!)
 *   - Atmospheric drag in low orbits
 *   - Solar radiation pressure
 *   - Third-body effects (Moon, Sun gravity)
 *   - Spacecraft mass changes as fuel is burned
 *
 * See also:
 *   orbit.hpp for orbit definitions
 *   hohmann_transfer.hpp for transfer calculations
 */

#include "hohmann/celestial_body.hpp"
#include "hohmann/orbit.hpp"
#include "hohmann/hohmann_transfer.hpp"
#include "hohmann/constants.hpp"

#include <iostream>    // std::cout, std::cerr for console output
#include <iomanip>     // std::setprecision, std::fixed for formatting
#include <string>      // std::string for string handling
#include <cstdlib>     // std::stod for string-to-double conversion

/**
 * C++ CONCEPT: "using namespace"
 * ------------------------------
 * The `using namespace hohmann;` directive lets us write:
 *   CelestialBody::Earth()
 * instead of:
 *   hohmann::CelestialBody::Earth()
 *
 * PROS:
 *   - Less typing, cleaner code
 *   - Good for small programs or when one namespace dominates
 *
 * CONS:
 *   - Can cause name collisions if multiple namespaces have same names
 *   - Harder to see where functions come from
 *
 * BEST PRACTICE:
 *   - In header files: NEVER use "using namespace" (affects all includers)
 *   - In .cpp files: OK for your own namespaces
 *   - For std:: : Many style guides say always write std:: explicitly
 *
 * We use it here because this is a .cpp file and hohmann:: would be repetitive.
 */
using namespace hohmann;

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

/**
 * Print usage instructions for the command-line interface.
 *
 * C++ CONCEPT: std::cout and Output Streams
 * -----------------------------------------
 * std::cout is the "standard character output" stream - it writes to the
 * console/terminal. The << operator "inserts" data into the stream.
 *
 * You can chain multiple << operators:
 *   std::cout << "Value: " << 42 << "\n";
 *
 * The stream automatically converts types to text:
 *   int, double, string, etc. all work with <<
 *
 * NEWLINES:
 *   "\n"         - Newline character (fast)
 *   std::endl    - Newline + flush buffer (slower, use when you need immediate output)
 *
 * For normal output, prefer "\n" over std::endl for better performance.
 */
void printUsage() {
    std::cout << "Hohmann Transfer Calculator\n";
    std::cout << "===========================\n\n";
    std::cout << "Usage: hohmann [initial_alt_km] [final_alt_km]\n\n";
    std::cout << "Arguments:\n";
    std::cout << "  initial_alt_km  Initial orbit altitude in km (default: 400 = LEO)\n";
    std::cout << "  final_alt_km    Final orbit altitude in km (default: 35786 = GEO)\n\n";
    std::cout << "Examples:\n";
    std::cout << "  hohmann              # LEO to GEO transfer\n";
    std::cout << "  hohmann 400 20200    # LEO to GPS orbit\n";
    std::cout << "  hohmann 420 35786    # ISS altitude to GEO\n";
}

/**
 * Print a table of common Earth orbit transfers.
 *
 * This demonstrates several useful patterns for formatted console output.
 *
 * C++ CONCEPT: Output Formatting with <iomanip>
 * ---------------------------------------------
 * The <iomanip> header provides "manipulators" that control how numbers
 * are formatted when sent to cout:
 *
 *   std::fixed          - Use fixed-point notation (not scientific)
 *   std::setprecision(n) - Show n digits after decimal point
 *   std::setw(n)        - Set minimum field width to n characters
 *   std::left/right     - Alignment within field
 *
 * IMPORTANT: These manipulators are "sticky" - they remain in effect
 * until changed. So std::setprecision(2) affects ALL subsequent output
 * until you change it again.
 *
 * Example:
 *   std::cout << std::fixed << std::setprecision(2);
 *   std::cout << 3.14159;  // Outputs: 3.14
 *   std::cout << 2.71828;  // Still outputs: 2.72 (setting persists)
 *
 * C++ CONCEPT: Block Scope with Braces { }
 * ----------------------------------------
 * The braces around each transfer calculation create a new SCOPE.
 * Variables declared inside { } are destroyed when the } is reached.
 *
 * Why use this pattern?
 *   1. Reuse variable names (leo, transfer, result) in each block
 *   2. Release memory early (objects destroyed at })
 *   3. Group related code visually
 *
 * This is similar to Python's lack of block scope but explicitly controlled:
 *   Python: variables leak out of if/for blocks
 *   C++:    variables are confined to their { } block
 */
void printCommonTransfers() {
    // Get Earth's parameters for all calculations
    auto earth = CelestialBody::Earth();

    std::cout << "\n========================================\n";
    std::cout << "       Common Earth Orbit Transfers\n";
    std::cout << "========================================\n\n";

    // Set formatting: fixed-point notation, no decimal places initially
    std::cout << std::fixed << std::setprecision(0);

    // -------------------------------------------------------------------------
    // LEO to GEO: The "classic" Hohmann transfer
    // -------------------------------------------------------------------------
    // This is the most common example in textbooks because:
    //   - LEO is where most launches end up initially
    //   - GEO is highly valuable (communications, weather satellites)
    //   - It's a realistic mission profile
    //
    // Real cost: ~4 km/s delta-v, ~5 hours transfer time
    // -------------------------------------------------------------------------
    {
        auto leo = Orbit::LEO(earth);    // 400 km altitude
        auto geo = Orbit::GEO(earth);    // 35,786 km altitude
        HohmannTransfer transfer(leo, geo);
        auto result = transfer.result();

        std::cout << "LEO (400 km) -> GEO (35,786 km):\n";
        std::cout << std::setprecision(2);  // 2 decimal places for velocity
        std::cout << "  Total dv: " << result.totalDeltaV << " m/s\n";
        std::cout << "  Time:     " << result.transferTimeHours() << " hours\n\n";
    }

    // -------------------------------------------------------------------------
    // LEO to GPS: Medium Earth Orbit transfer
    // -------------------------------------------------------------------------
    // GPS satellites orbit at ~20,200 km - halfway to GEO
    // This transfer is smaller than LEO-GEO but still substantial
    // -------------------------------------------------------------------------
    {
        auto leo = Orbit::LEO(earth);
        auto gps = Orbit::GPS(earth);    // 20,200 km altitude
        HohmannTransfer transfer(leo, gps);
        auto result = transfer.result();

        std::cout << "LEO (400 km) -> GPS (20,200 km):\n";
        std::cout << "  Total dv: " << result.totalDeltaV << " m/s\n";
        std::cout << "  Time:     " << result.transferTimeHours() << " hours\n\n";
    }

    // -------------------------------------------------------------------------
    // ISS to GEO: Starting from ISS altitude
    // -------------------------------------------------------------------------
    // The ISS orbits at 420 km - slightly higher than our "standard" LEO
    // This shows the delta-v is very similar to LEO-GEO (20 km doesn't matter much)
    // -------------------------------------------------------------------------
    {
        auto iss = Orbit::ISS(earth);    // 420 km altitude
        auto geo = Orbit::GEO(earth);
        HohmannTransfer transfer(iss, geo);
        auto result = transfer.result();

        std::cout << "ISS (420 km) -> GEO (35,786 km):\n";
        std::cout << "  Total dv: " << result.totalDeltaV << " m/s\n";
        std::cout << "  Time:     " << result.transferTimeHours() << " hours\n\n";
    }

    std::cout << "========================================\n";
}

// =============================================================================
// MAIN FUNCTION - Program Entry Point
// =============================================================================

/**
 * Program entry point - parses arguments and runs transfer calculations.
 *
 * C++ CONCEPT: argc and argv
 * --------------------------
 * argc (argument count): Number of command-line arguments, ALWAYS >= 1
 *                        because the program name counts as argv[0]
 *
 * argv (argument vector): Array of C-strings (char*) containing the arguments
 *                         Note: These are STRINGS, not numbers!
 *                         "400" is the text "400", not the number 400
 *
 * To convert strings to numbers:
 *   std::stod("3.14")  -> double 3.14    (string to double)
 *   std::stoi("42")    -> int 42         (string to int)
 *   std::stol("99999") -> long 99999     (string to long)
 *
 * These throw std::invalid_argument if the string isn't a valid number.
 *
 * C++ CONCEPT: Exception Handling (try/catch)
 * -------------------------------------------
 * Exceptions are C++'s way of handling errors that can't be dealt with locally.
 *
 *   try {
 *       // Code that might fail
 *       risky_function();
 *   }
 *   catch (const std::exception& e) {
 *       // Handle the error
 *       std::cerr << e.what() << "\n";
 *   }
 *
 * WHAT HAPPENS:
 *   1. Code in try block executes normally
 *   2. If an exception is "thrown", execution jumps to matching catch
 *   3. e.what() returns a human-readable error message
 *
 * CATCHING BY CONST REFERENCE:
 *   `const std::exception& e` catches any exception derived from std::exception
 *   By reference: avoids copying the exception object
 *   const: we're only reading, not modifying
 *
 * std::exception is the BASE CLASS for most C++ exceptions:
 *   - std::invalid_argument (bad function argument)
 *   - std::out_of_range (array index out of bounds)
 *   - std::runtime_error (general runtime errors)
 *
 * C++ CONCEPT: std::cerr vs std::cout
 * -----------------------------------
 * std::cout = standard output (normal program output)
 * std::cerr = standard error (error messages)
 *
 * Both usually go to the console, but can be redirected separately:
 *   $ ./hohmann > output.txt 2> errors.txt
 *
 * Use std::cerr for error messages so they can be separated from normal output.
 *
 * Parameters:
 *   argc - Number of command-line arguments
 *   argv - Array of argument strings
 *
 * Returns:
 *   0 on success, 1 on error
 */
int main(int argc, char* argv[]) {
    try {
        // Create Earth as our central body for all calculations
        auto earth = CelestialBody::Earth();

        if (argc == 1) {
            // No arguments provided - show common transfers as a demo
            // This gives users useful information even without arguments
            printCommonTransfers();

            std::cout << "\n";
            // Also show detailed LEO-GEO analysis
            auto leo = Orbit::LEO(earth);
            auto geo = Orbit::GEO(earth);
            HohmannTransfer transfer(leo, geo);
            transfer.printSummary();  // Detailed breakdown
        }
        else if (argc == 2 && std::string(argv[1]) == "--help") {
            // Help requested - show usage instructions
            // Note: std::string(argv[1]) converts C-string to std::string
            // so we can use == for comparison (C-strings can't use == directly)
            printUsage();
        }
        else if (argc == 3) {
            // Custom transfer: user provided two altitudes
            // Convert string arguments to numbers (in km)
            double alt1_km = std::stod(argv[1]);  // May throw if not a number
            double alt2_km = std::stod(argv[2]);

            // Convert km to meters (our internal units)
            // Always be explicit about unit conversions!
            auto orbit1 = Orbit::fromAltitude(earth, alt1_km * 1000.0);  // [km] -> [m]
            auto orbit2 = Orbit::fromAltitude(earth, alt2_km * 1000.0);  // [km] -> [m]

            // Calculate and display the transfer
            HohmannTransfer transfer(orbit1, orbit2);
            transfer.printSummary();
        }
        else {
            // Invalid arguments - show usage and return error
            printUsage();
            return 1;  // Non-zero = error
        }

        return 0;  // Success
    }
    catch (const std::exception& e) {
        // Catch any exception and print a user-friendly error message
        // This handles:
        //   - std::invalid_argument from std::stod() if args aren't numbers
        //   - std::invalid_argument from Orbit() if radius is invalid
        //   - Any other std::exception-derived errors
        std::cerr << "Error: " << e.what() << "\n";
        return 1;  // Return error code to shell
    }
}
