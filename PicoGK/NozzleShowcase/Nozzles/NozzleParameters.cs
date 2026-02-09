// =============================================================================
// NOZZLE PARAMETERS - DATA CLASS FOR DE LAVAL NOZZLE DIMENSIONS
// =============================================================================
//
// AEROSPACE CONCEPT: Convergent-Divergent (de Laval) Nozzle
// ==========================================================
//
// The de Laval nozzle (named after Gustaf de Laval, 1888) is fundamental
// to all rocket propulsion. It converts thermal energy into kinetic energy
// by accelerating gas to supersonic velocities.
//
// HOW IT WORKS:
// -------------
//
//   Combustion        Convergent     Throat      Divergent        Exit
//   Chamber           Section                    Section
//
//   ┌──────────┐     ╲            ║          ╱                 ┌──────┐
//   │ HIGH     │      ╲           ║         ╱                  │ LOW  │
//   │ PRESSURE │       ╲__________║________╱                   │PRESS │
//   │ HOT GAS  │       ╱          ║        ╲                   │ FAST │
//   │          │      ╱           ║         ╲                  │ GAS  │
//   └──────────┘     ╱            ║          ╲                 └──────┘
//
//   Subsonic flow   M < 1       M = 1        M > 1           Supersonic
//   accelerates                (sonic)     (supersonic)
//
// KEY PRINCIPLE:
// In subsonic flow, decreasing area accelerates the gas.
// In supersonic flow, INCREASING area accelerates the gas further!
// The throat (minimum area) is where the flow transitions from sub to supersonic.
//
// DESIGN PARAMETERS:
// ------------------
// 1. THROAT AREA (A*): Determines mass flow rate (choked flow)
//    m_dot = P_chamber * A* * sqrt(gamma / (R * T)) * function(gamma)
//
// 2. EXPANSION RATIO (epsilon = A_exit / A_throat):
//    Determines exit Mach number and exhaust velocity
//    Higher epsilon = higher velocity but needs lower ambient pressure
//
// 3. CONTRACTION RATIO: Affects chamber-to-throat transition losses
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. INIT-ONLY PROPERTIES (C# 9+)
//    `public float InletDiameter { get; init; } = 50f;`
//    Can be set during object initialization, then becomes immutable.
//    Enables object initializer syntax while preventing later modification.
//
// 2. EXPRESSION-BODIED PROPERTIES
//    `public float InletRadius => InletDiameter / 2f;`
//    Computed properties that derive values from other properties.
//    Recalculated on each access (not cached).
//
// 3. STATIC FACTORY PROPERTIES
//    `public static NozzleParameters Default => new();`
//    Creates pre-configured instances with descriptive names.
//    Pattern used instead of multiple constructors.
//
// 4. OBJECT INITIALIZER WITH INIT PROPERTIES
//    `new NozzleParameters { ThroatDiameter = 15f, ... }`
//    Sets init-only properties during construction.
//
// 5. STRING INTERPOLATION
//    `$"║  Throat Diameter: {ThroatDiameter,8:F2} mm"`
//    Embeds expressions in strings. Format: {value,alignment:format}
//
// =============================================================================

namespace NozzleShowcase.Nozzles;

/// <summary>
/// Parameters defining a convergent-divergent (de Laval) nozzle.
/// All dimensions in millimeters.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: The de Laval nozzle is the only way to accelerate gas
/// to supersonic velocities in a continuous flow. Every rocket engine,
/// from the V-2 to the Space Shuttle, uses this fundamental design.
/// </para>
/// <para>
/// C# CONCEPT: This class uses init-only properties for immutable
/// configuration. Once created, the parameters cannot be changed,
/// ensuring geometric consistency throughout nozzle generation.
/// </para>
/// </remarks>
public class NozzleParameters
{
    // =========================================================================
    // PRIMARY DIMENSIONS (init-only for immutability after creation)
    // =========================================================================
    // C#: `{ get; init; }` allows setting only during object initialization.
    // After construction, these properties are effectively readonly.
    // Default values are specified after the `=` sign.

    /// <summary>Inlet diameter at combustion chamber interface [mm]</summary>
    /// <remarks>
    /// AEROSPACE: Should match or exceed combustion chamber diameter.
    /// Smooth transition reduces turbulence and pressure losses.
    /// </remarks>
    public float InletDiameter { get; init; } = 50f;

    /// <summary>Throat diameter (minimum flow area) [mm]</summary>
    /// <remarks>
    /// AEROSPACE: The throat is the most critical dimension!
    /// - Flow velocity = Mach 1.0 exactly at the throat
    /// - Determines maximum mass flow rate (choked flow)
    /// - Must be precisely manufactured for predictable performance
    /// </remarks>
    public float ThroatDiameter { get; init; } = 20f;

    /// <summary>Exit diameter at nozzle outlet [mm]</summary>
    /// <remarks>
    /// AEROSPACE: Larger exit = more expansion = higher exhaust velocity
    /// But: optimal size depends on ambient pressure (altitude).
    /// </remarks>
    public float ExitDiameter { get; init; } = 60f;

    /// <summary>Length of convergent section (inlet to throat) [mm]</summary>
    /// <remarks>
    /// AEROSPACE: Longer convergent section = smoother flow but more weight.
    /// Typical half-angle: 20-45 degrees from centerline.
    /// </remarks>
    public float ConvergentLength { get; init; } = 30f;

    /// <summary>Length of divergent section (throat to exit) [mm]</summary>
    /// <remarks>
    /// AEROSPACE: Length determines the expansion profile.
    /// Too short = flow separation; too long = excessive weight.
    /// Bell nozzles use ~80% of equivalent conical length.
    /// </remarks>
    public float DivergentLength { get; init; } = 50f;

    /// <summary>Wall thickness of the nozzle structure [mm]</summary>
    /// <remarks>
    /// AEROSPACE: Must withstand:
    /// - High temperatures (1000-3500K depending on propellants)
    /// - Internal pressure (1-20 MPa typical)
    /// - Vibration and acoustic loads
    /// Real nozzles often use regenerative cooling channels in the wall.
    /// </remarks>
    public float WallThickness { get; init; } = 3f;

    // =========================================================================
    // COMPUTED PROPERTIES (derived from primary dimensions)
    // =========================================================================
    // C#: Expression-bodied properties (=>) compute values on demand.
    // They're not stored - calculated fresh each time they're accessed.

    /// <summary>Inlet radius [mm]</summary>
    public float InletRadius => InletDiameter / 2f;

    /// <summary>Throat radius [mm]</summary>
    public float ThroatRadius => ThroatDiameter / 2f;

    /// <summary>Exit radius [mm]</summary>
    public float ExitRadius => ExitDiameter / 2f;

    /// <summary>Total nozzle length (convergent + divergent) [mm]</summary>
    public float TotalLength => ConvergentLength + DivergentLength;

    // AEROSPACE: Area calculations use A = pi * r^2
    // These are critical for flow calculations

    /// <summary>Throat cross-sectional area [mm^2]</summary>
    public float ThroatArea => MathF.PI * ThroatRadius * ThroatRadius;

    /// <summary>Exit cross-sectional area [mm^2]</summary>
    public float ExitArea => MathF.PI * ExitRadius * ExitRadius;

    /// <summary>Inlet cross-sectional area [mm^2]</summary>
    public float InletArea => MathF.PI * InletRadius * InletRadius;

    /// <summary>Expansion ratio (exit area / throat area) [dimensionless]</summary>
    /// <remarks>
    /// AEROSPACE: The expansion ratio (epsilon) is THE key design parameter.
    /// It determines the exit Mach number via the isentropic area-Mach relation.
    ///
    /// Typical values:
    /// - Sea level engines: 10-20 (e.g., Merlin 1D: 16)
    /// - Vacuum engines: 40-300 (e.g., RL-10: 84, Merlin Vacuum: 165)
    /// </remarks>
    public float ExpansionRatio => ExitArea / ThroatArea;

    /// <summary>Contraction ratio (inlet area / throat area) [dimensionless]</summary>
    /// <remarks>
    /// AEROSPACE: Affects the subsonic acceleration region.
    /// Typical values: 2-10 for liquid rocket engines.
    /// Higher values give smoother acceleration but longer/heavier nozzles.
    /// </remarks>
    public float ContractionRatio => InletArea / ThroatArea;

    // =========================================================================
    // FACTORY PROPERTIES (pre-configured parameter sets)
    // =========================================================================
    // C#: Static properties that return new instances provide a clean
    // alternative to multiple constructors or factory methods.

    /// <summary>
    /// Create default nozzle parameters for a small rocket engine.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: `new()` is target-typed new (C# 9+).
    /// The compiler infers `new NozzleParameters()` from the return type.
    /// </remarks>
    public static NozzleParameters Default => new();

    /// <summary>
    /// Create parameters for a high expansion ratio nozzle (vacuum optimized).
    /// </summary>
    /// <remarks>
    /// AEROSPACE: Vacuum-optimized nozzles have high expansion ratios
    /// because there's no ambient pressure to cause flow separation.
    /// This configuration: expansion ratio = (80/15)^2 = 28.4
    ///
    /// C# CONCEPT: Object initializer syntax with init-only properties.
    /// Properties can only be set this way, not after construction.
    /// </remarks>
    public static NozzleParameters VacuumOptimized => new()
    {
        InletDiameter = 40f,
        ThroatDiameter = 15f,
        ExitDiameter = 80f,      // Large exit for high expansion
        ConvergentLength = 25f,
        DivergentLength = 70f,   // Longer divergent section
        WallThickness = 2.5f
    };

    /// <summary>
    /// Create parameters for a low expansion ratio nozzle (sea level optimized).
    /// </summary>
    /// <remarks>
    /// AEROSPACE: Sea-level nozzles must avoid over-expansion, which causes
    /// flow separation and thrust loss. Lower expansion ratio ensures the
    /// exhaust pressure matches (or slightly exceeds) atmospheric pressure.
    /// This configuration: expansion ratio = (45/25)^2 = 3.24
    /// </remarks>
    public static NozzleParameters SeaLevelOptimized => new()
    {
        InletDiameter = 50f,
        ThroatDiameter = 25f,    // Larger throat for higher mass flow
        ExitDiameter = 45f,      // Smaller exit to avoid over-expansion
        ConvergentLength = 30f,
        DivergentLength = 35f,   // Shorter divergent section
        WallThickness = 3.5f     // Thicker walls for sea-level pressure loads
    };

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// <summary>
    /// Print nozzle parameters to console in a formatted table.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: String Interpolation with Formatting
    /// `{ThroatDiameter,8:F2}` means:
    /// - ThroatDiameter: the value to insert
    /// - 8: minimum field width (right-aligned by default)
    /// - F2: fixed-point format with 2 decimal places
    /// </remarks>
    public void PrintSummary()
    {
        // Box-drawing characters for nice ASCII table formatting
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       NOZZLE PARAMETERS SUMMARY          ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  Inlet Diameter:      {InletDiameter,8:F2} mm       ║");
        Console.WriteLine($"║  Throat Diameter:     {ThroatDiameter,8:F2} mm       ║");
        Console.WriteLine($"║  Exit Diameter:       {ExitDiameter,8:F2} mm       ║");
        Console.WriteLine($"║  Convergent Length:   {ConvergentLength,8:F2} mm       ║");
        Console.WriteLine($"║  Divergent Length:    {DivergentLength,8:F2} mm       ║");
        Console.WriteLine($"║  Total Length:        {TotalLength,8:F2} mm       ║");
        Console.WriteLine($"║  Wall Thickness:      {WallThickness,8:F2} mm       ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  Throat Area:         {ThroatArea,8:F2} mm^2      ║");
        Console.WriteLine($"║  Exit Area:           {ExitArea,8:F2} mm^2      ║");
        Console.WriteLine($"║  Expansion Ratio:     {ExpansionRatio,8:F2}          ║");
        Console.WriteLine($"║  Contraction Ratio:   {ContractionRatio,8:F2}          ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
    }
}
