// =============================================================================
// PHYSICAL AND GEOMETRIC CONSTANTS FOR NOZZLE DESIGN
// =============================================================================
//
// AEROSPACE CONCEPT: Design Parameters for Rocket Nozzles
// ========================================================
//
// Rocket nozzle design requires precise dimensional control. Key parameters:
//
// VOXEL SIZE:
// -----------
// PicoGK uses voxel-based geometry (3D pixels). Voxel size determines:
// - Resolution: Smaller voxels = finer detail, larger files, slower computation
// - Typical range: 0.1mm (high detail) to 2mm (fast preview)
// - 0.5mm is a good balance for 3D printing preparation
//
// NOZZLE DIMENSIONS:
// ------------------
//
//     Inlet           Throat           Exit
//       |               |               |
//       v               v               v
//    ╔═════╗         ╔═══╗         ╔═══════╗
//    ║     ║ _______ ║   ║ _______ ║       ║
//    ║     ║/       \║   ║/       \║       ║
//    ║     ║         ║   ║         ║       ║
//    ║     ║\_______/║   ║\_______/║       ║
//    ╚═════╝         ╚═══╝         ╚═══════╝
//      50mm           20mm           60mm    (diameters)
//
//    |<--- 30mm --->|<----- 50mm ----->|
//      Convergent       Divergent
//
// EXPANSION RATIO:
// ----------------
// The ratio of exit area to throat area determines optimal operating altitude:
// - Low ratio (2-4): Sea level operation
// - Medium ratio (10-20): Mid-altitude / vacuum transition
// - High ratio (40-100): Vacuum-optimized (space engines)
//
// For this default nozzle:
// - Expansion ratio = (60/20)^2 = 9.0
// - Contraction ratio = (50/20)^2 = 6.25
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. FILE-SCOPED NAMESPACE (C# 10+)
//    `namespace NozzleShowcase.Utils;`
//    No braces needed - entire file is in this namespace.
//    Reduces indentation and boilerplate.
//
// 2. STATIC CLASS
//    `public static class Constants`
//    Cannot be instantiated - only contains static members.
//    Used for grouping related constants.
//
// 3. NESTED STATIC CLASSES
//    `public static class DefaultNozzle { ... }`
//    Groups related constants together for organization.
//    Access via: Constants.DefaultNozzle.ThroatDiameter
//
// 4. CONST VS STATIC READONLY
//    `public const float VoxelSizeMM = 0.5f;`
//    const: Compile-time constant, inlined everywhere
//    static readonly: Runtime constant, single memory location
//    Use const for truly immutable values known at compile time.
//
// 5. EXPRESSION-BODIED MEMBERS (C# 6+)
//    `public static float ThroatArea => MathF.PI * ...`
//    Concise syntax for computed properties (get-only).
//    Equivalent to: get { return MathF.PI * ...; }
//
// 6. MathF CLASS
//    `MathF.PI`, `MathF.Pow(x, 2)`
//    Float-precision math functions (vs Math for double).
//    Avoids float/double casting in geometry code.
//
// =============================================================================

namespace NozzleShowcase.Utils;

/// <summary>
/// Physical and geometric constants for nozzle design.
/// All dimensions in millimeters unless otherwise noted.
/// </summary>
/// <remarks>
/// <para>
/// C# CONCEPT: Static Classes
/// This class cannot be instantiated. It serves only as a container
/// for static constants and helper methods.
/// </para>
/// <para>
/// DESIGN PATTERN: Nested classes group related constants together,
/// providing clear organization: Constants.DefaultNozzle.ThroatDiameter
/// </para>
/// </remarks>
public static class Constants
{
    // =========================================================================
    // VOXEL RESOLUTION
    // =========================================================================
    // PICOGK: Voxel size controls the resolution of the geometry.
    // Smaller values = more detail but exponentially more computation.
    // Memory usage scales as O(1/voxelSize^3) - halving size = 8x memory!
    //
    // C#: `const` makes this a compile-time constant. The value is
    // literally copied everywhere it's used (no memory reference).
    public const float VoxelSizeMM = 0.5f;

    // =========================================================================
    // DEFAULT NOZZLE DIMENSIONS
    // =========================================================================
    /// <summary>
    /// Default nozzle dimensions for a small rocket engine.
    /// These values produce a nozzle suitable for demonstration and 3D printing.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Nested Static Class
    /// Groups related constants under a descriptive name.
    /// Makes code self-documenting: Constants.DefaultNozzle.ThroatDiameter
    /// </remarks>
    public static class DefaultNozzle
    {
        // AEROSPACE: Inlet connects to combustion chamber
        // Should be sized to match chamber diameter for smooth flow
        public const float InletDiameter = 50f;

        // AEROSPACE: Throat is the minimum area (choke point)
        // Flow reaches exactly Mach 1.0 here (sonic velocity)
        // This is the critical dimension - determines mass flow rate
        public const float ThroatDiameter = 20f;

        // AEROSPACE: Exit diameter determines expansion ratio
        // Larger exit = more expansion = higher vacuum efficiency
        // But over-expansion at sea level causes flow separation
        public const float ExitDiameter = 60f;

        // AEROSPACE: Convergent section accelerates subsonic flow to Mach 1
        // Length affects pressure drop and combustion stability
        public const float ConvergentLength = 30f;

        // AEROSPACE: Divergent section accelerates supersonic flow
        // Longer = smoother expansion but more weight and size
        public const float DivergentLength = 50f;

        // AEROSPACE: Wall thickness for structural integrity
        // Must withstand: high temperature, pressure, vibration
        // 3mm is reasonable for a demonstration/prototype nozzle
        public const float WallThickness = 3f;
    }

    // =========================================================================
    // COMPUTED VALUES
    // =========================================================================
    /// <summary>
    /// Computed values derived from the default nozzle dimensions.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Expression-Bodied Properties
    /// The => syntax creates a get-only property that computes its value.
    /// These are recalculated each time they're accessed (not cached).
    /// </remarks>
    public static class Computed
    {
        // AEROSPACE: Throat area [mm^2]
        // A* in compressible flow equations
        // Mass flow rate: m_dot = rho * A* * a (at choked conditions)
        public static float ThroatArea =>
            MathF.PI * MathF.Pow(DefaultNozzle.ThroatDiameter / 2f, 2);

        // AEROSPACE: Exit area [mm^2]
        // Determines exit Mach number via isentropic relations
        public static float ExitArea =>
            MathF.PI * MathF.Pow(DefaultNozzle.ExitDiameter / 2f, 2);

        // AEROSPACE: Expansion ratio (epsilon) = A_exit / A_throat
        // Critical parameter for nozzle performance
        // Higher ratio = higher exhaust velocity but needs lower back pressure
        // Formula: epsilon = (1/M) * ((2/(gamma+1)) * (1 + (gamma-1)/2 * M^2))^((gamma+1)/(2*(gamma-1)))
        public static float ExpansionRatio => ExitArea / ThroatArea;

        // AEROSPACE: Contraction ratio = A_inlet / A_throat
        // Affects combustion chamber to nozzle transition
        // Typical values: 3 to 10 for liquid rocket engines
        public static float ContractionRatio =>
            (MathF.PI * MathF.Pow(DefaultNozzle.InletDiameter / 2f, 2)) / ThroatArea;
    }

    // =========================================================================
    // OUTPUT PATHS
    // =========================================================================
    /// <summary>
    /// File system paths for output files.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Path.Combine for Cross-Platform Paths
    /// Always use Path.Combine instead of string concatenation with "/"
    /// This handles Windows vs Unix path separators automatically.
    ///
    /// The ".." entries navigate up the directory tree from the
    /// executable location to the project root.
    /// </remarks>
    public static class Paths
    {
        // Navigate from bin/Debug/net9.0/ up to project root, then to Output/
        // AppDomain.CurrentDomain.BaseDirectory = directory containing the exe
        public static string OutputFolder => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Output"
        );
    }
}
