// =============================================================================
// PICOGK NOZZLE SHOWCASE - MAIN ENTRY POINT
// =============================================================================
//
// AEROSPACE CONCEPT: Computational Geometry for Rocket Nozzles
// =============================================================
//
// This project demonstrates using computational geometry to design and
// manufacture rocket nozzle components. The workflow is:
//
//   1. DEFINE parameters (dimensions, expansion ratio, wall thickness)
//   2. GENERATE geometry using voxel-based modeling
//   3. EXPORT to STL format for 3D printing or analysis
//
// WHY VOXEL-BASED GEOMETRY?
// -------------------------
// Traditional CAD uses boundary representations (B-rep): surfaces defined
// by mathematical equations. Voxel-based geometry uses a 3D grid of "on/off"
// cells (like 3D pixels). Advantages:
//
// - Boolean operations (union, subtract) are trivial and robust
// - Complex organic shapes are easy to create
// - Direct path to 3D printing (already discretized)
// - No "degenerate geometry" issues that plague B-rep
//
// Disadvantages:
// - Resolution limited by voxel size
// - File sizes can be large
// - Not parametric (can't easily modify after creation)
//
// PICOGK LIBRARY:
// ---------------
// PicoGK is LEAP 71's open-source computational geometry kernel.
// It provides:
// - Lattice: Collection of geometric primitives (beams, spheres)
// - Voxels: 3D grid representation for solid modeling
// - Mesh: Triangle surface representation for export
// - Boolean operations: Union, subtract, intersect
// - Offset: Expand or shrink geometry by a distance
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. TOP-LEVEL STATEMENTS AND FILE-SCOPED NAMESPACE
//    The `using` statements and namespace declaration use modern C# 10+ syntax.
//
// 2. COMMAND-LINE ARGUMENT PARSING
//    Simple manual parsing of args[] array. For complex CLIs, consider
//    System.CommandLine or similar libraries.
//
// 3. DELEGATE PASSING
//    `Library.Go(voxelSize, BellNozzle.Task)` passes a method reference.
//    PicoGK will call this method after initialization.
//
// 4. TRY-CATCH FOR GRACEFUL ERROR HANDLING
//    Catches exceptions and provides helpful setup instructions.
//
// 5. STRING.TOLOWER() FOR CASE-INSENSITIVE COMPARISON
//    Normalizes input for reliable comparison.
//
// =============================================================================

using NozzleShowcase.Nozzles;
using PicoGK;

/// <summary>
/// PicoGK Nozzle Showcase - Main entry point.
/// </summary>
/// <remarks>
/// <para>
/// This project creates convergent-divergent (de Laval) rocket nozzles
/// using LEAP 71's PicoGK geometry kernel. The output STL files can be
/// used for 3D printing, CFD analysis, or visualization.
/// </para>
/// <para>
/// SETUP REQUIREMENTS:
/// 1. Clone PicoGK from https://github.com/leap71/PicoGK
/// 2. Place the PicoGK folder alongside this project
/// 3. Install the PicoGK runtime (see PicoGK documentation)
/// 4. Run with: dotnet run
/// </para>
/// <para>
/// OUTPUT FILES:
/// - simple_nozzle.stl / bell_nozzle.stl: The hollow nozzle structure
/// - nozzle_flow_path.stl: The inner flow volume (for CFD)
/// Both files are saved to the Output/ folder.
/// </para>
/// </remarks>

namespace NozzleShowcase;

/// <summary>
/// Program entry point.
/// </summary>
/// <remarks>
/// C# CONCEPT: The Program class contains the Main method, which is
/// the entry point for console applications. Modern C# allows top-level
/// statements (no explicit Main), but explicit Main provides clearer
/// structure for larger applications.
/// </remarks>
class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: The Main method signature can be:
    /// - `static void Main()` - no args, no return
    /// - `static void Main(string[] args)` - with args
    /// - `static int Main(...)` - with exit code return
    /// - `static async Task Main(...)` - async entry point
    /// </para>
    /// <para>
    /// PICOGK: Library.Go() must be called to initialize PicoGK before
    /// any geometry operations. It takes a voxel size and a delegate
    /// (method reference) to execute.
    /// </para>
    /// </remarks>
    static void Main(string[] args)
    {
        // =====================================================================
        // BANNER
        // =====================================================================
        // Box-drawing characters create a nice visual header
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║         PicoGK Aerospace Geometry Showcase                ║");
        Console.WriteLine("║         Convergent-Divergent Rocket Nozzle                ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║   Using LEAP 71's computational geometry kernel           ║");
        Console.WriteLine("║   for 3D-printable aerospace components                   ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // =====================================================================
        // ERROR HANDLING
        // =====================================================================
        // C#: try-catch wraps code that might throw exceptions.
        // PicoGK can fail if not properly installed or configured.
        try
        {
            // =================================================================
            // COMMAND-LINE ARGUMENT PARSING
            // =================================================================
            // Default values
            float voxelSize = 0.5f; // Default voxel size in mm
            string nozzleType = "simple"; // Default nozzle type

            // C#: args is a string array of command-line arguments.
            // args[0] is the first argument (not the program name like in C).
            for (int i = 0; i < args.Length; i++)
            {
                // --voxel <size>: Set voxel resolution
                if (args[i] == "--voxel" && i + 1 < args.Length)
                {
                    // C#: float.Parse converts string to float
                    // May throw FormatException if invalid
                    voxelSize = float.Parse(args[i + 1]);
                }
                // --type <type>: Select nozzle type (simple or bell)
                else if (args[i] == "--type" && i + 1 < args.Length)
                {
                    // C#: ToLower() for case-insensitive comparison
                    nozzleType = args[i + 1].ToLower();
                }
                // --help: Show usage information
                else if (args[i] == "--help")
                {
                    PrintUsage();
                    return; // Exit without running nozzle generation
                }
            }

            // Report configuration
            Console.WriteLine($"Voxel Size: {voxelSize} mm");
            Console.WriteLine($"Nozzle Type: {nozzleType}");
            Console.WriteLine();

            // =================================================================
            // PICOGK INITIALIZATION AND TASK EXECUTION
            // =================================================================
            // PICOGK: Library.Go() is the main entry point. It:
            // 1. Initializes the PicoGK runtime with the specified voxel size
            // 2. Calls the provided delegate (method reference)
            // 3. Cleans up when the task completes
            //
            // C# CONCEPT: Passing Methods as Delegates
            // `BellNozzle.Task` is a reference to the static method.
            // This is equivalent to: `new Action(BellNozzle.Task)`
            // The delegate pattern allows PicoGK to call our code after setup.
            if (nozzleType == "bell")
            {
                // Generate bell nozzle with parabolic contour
                Library.Go(voxelSize, BellNozzle.Task);
            }
            else
            {
                // Generate simple conical nozzle (default)
                Library.Go(voxelSize, SimpleNozzle.Task);
            }
        }
        catch (Exception ex)
        {
            // =================================================================
            // ERROR HANDLING AND TROUBLESHOOTING GUIDANCE
            // =================================================================
            // C#: catch (Exception ex) catches all exceptions.
            // In production, you might catch specific exception types.
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Make sure you have:");
            Console.WriteLine("  1. Cloned PicoGK from https://github.com/leap71/PicoGK");
            Console.WriteLine("  2. Placed it in the correct location relative to this project");
            Console.WriteLine("  3. Installed the PicoGK runtime");
            Console.WriteLine();
            Console.WriteLine("See README.md for detailed setup instructions.");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Print command-line usage information.
    /// </summary>
    /// <remarks>
    /// C#: Static method helper for displaying help text.
    /// Called when --help argument is provided.
    /// </remarks>
    static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --voxel <size>   Voxel size in mm (default: 0.5)");
        Console.WriteLine("  --type <type>    Nozzle type: simple or bell (default: simple)");
        Console.WriteLine("  --help           Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run                        # Simple nozzle, 0.5mm voxels");
        Console.WriteLine("  dotnet run --voxel 0.25           # Higher resolution");
        Console.WriteLine("  dotnet run --type bell            # Bell contour nozzle");
        Console.WriteLine("  dotnet run --type bell --voxel 1  # Fast preview");
    }
}
