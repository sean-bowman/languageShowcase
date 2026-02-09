// =============================================================================
// BELL NOZZLE - PARABOLIC CONTOUR FOR IMPROVED EFFICIENCY
// =============================================================================
//
// AEROSPACE CONCEPT: Bell (Parabolic) Nozzle Contours
// ====================================================
//
// While conical nozzles are simple, they're not optimal. The abrupt angle
// change at the throat causes flow separation and thrust losses. Bell nozzles
// use a smooth parabolic contour that provides:
//
// - Better flow expansion (smoother acceleration)
// - Higher thrust coefficient
// - Shorter length for the same expansion ratio
// - Reduced divergence losses
//
// BELL NOZZLE GEOMETRY:
// ---------------------
//
//   Conical Nozzle:           Bell Nozzle (80% bell):
//
//         ╲     ╱                    ╲___________╱
//          ╲   ╱                      ╲         ╱
//           ╲ ╱                        ╲       ╱
//           ═╪═ Throat                 ═╪═    Throat
//           ╱ ╲                        ╱ ╲
//          ╱   ╲  15 deg              ╱   ╲  Rapid initial
//         ╱     ╲ half-angle         ╱     ╲ expansion
//        ╱       ╲                  (       )
//       ╱         ╲                 │       │  Gradual
//      ╱           ╲                │       │  straightening
//
//   Flow diverges at exit       Flow nearly axial at exit
//   (divergence loss)           (better thrust alignment)
//
// BELL FRACTION:
// --------------
// An "80% bell" (common for rocket engines) means the nozzle length is
// 80% of what a 15-degree half-angle conical nozzle would require for
// the same expansion ratio. The bell achieves this with a parabolic
// contour that:
//
// 1. Starts with rapid expansion just after the throat (high curvature)
// 2. Gradually straightens toward the exit (approaches axial flow)
//
// RADIUS PROFILE:
// ---------------
// This implementation uses: r(t) = r_throat + (r_exit - r_throat) * t^n
//
// Where n < 1 gives faster initial expansion (bell shape).
// n = 0.6 approximates a good bell contour.
//
// =============================================================================
// PICOGK CONCEPT: SPATIAL PAINTING
// =============================================================================
//
// Unlike the SimpleNozzle which uses AddBeam for straight cones, the bell
// nozzle's curved surface requires "spatial painting" - placing many small
// spheres along the desired contour to build up the shape.
//
// This is like pointillism in 3D:
//
//     ○ ○ ○ ○ ○ ○ ○ ○ ○ ○ ○     Each ○ is a sphere
//    ○   ○   ○   ○   ○   ○      Overlapping spheres
//   ○     ○     ○     ○     ○   create a smooth surface
//
// The spheres overlap to create a continuous surface when voxelized.
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. MATH.F CLASS FOR FLOAT PRECISION
//    MathF.PI, MathF.Cos(), MathF.Pow() - float-precision math functions.
//
// 2. NESTED LOOPS FOR PARAMETRIC SURFACES
//    Outer loop: axial position (z coordinate)
//    Inner loop: circumferential position (theta angle)
//
// 3. MATH.MAX FOR BOUNDS CHECKING
//    `Math.Max(steps, 50)` ensures minimum value.
//
// 4. FORMAT SPECIFIERS
//    `{_bellFraction:P0}` - P0 = percentage with 0 decimal places (80%)
//
// =============================================================================

using System.Numerics;
using PicoGK;

namespace NozzleShowcase.Nozzles;

/// <summary>
/// Creates a more realistic bell-contour nozzle using spatial painting.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: Bell nozzles are used in virtually all modern liquid rocket
/// engines because they provide higher efficiency than simple conical nozzles.
/// Famous examples include the RS-25 (Space Shuttle Main Engine), Merlin
/// (Falcon 9), and Raptor (Starship).
/// </para>
/// <para>
/// PICOGK: Unlike the SimpleNozzle which uses tapered beams, this class
/// uses "spatial painting" - placing overlapping spheres along the curved
/// profile to build up the parabolic shape.
/// </para>
/// </remarks>
public class BellNozzle
{
    private readonly NozzleParameters _params;
    private readonly float _voxelSize;

    // AEROSPACE: Bell fraction defines how much the nozzle is shortened
    // compared to an equivalent conical nozzle.
    // 0.8 = 80% bell = common for rocket engines (good efficiency/length tradeoff)
    // Lower values = shorter but less efficient
    // Higher values = longer, approaching conical performance
    private readonly float _bellFraction;

    /// <summary>
    /// Create a new bell nozzle generator.
    /// </summary>
    /// <param name="parameters">Nozzle dimensions</param>
    /// <param name="voxelSize">Voxel resolution in mm</param>
    /// <param name="bellFraction">Bell percentage (0.8 = 80% bell, typical)</param>
    public BellNozzle(NozzleParameters parameters, float voxelSize = 0.5f, float bellFraction = 0.8f)
    {
        _params = parameters;
        _voxelSize = voxelSize;
        _bellFraction = bellFraction;
    }

    // =========================================================================
    // GEOMETRY GENERATION - SPATIAL PAINTING APPROACH
    // =========================================================================

    /// <summary>
    /// Generate the flow path using spatial painting for a smooth bell contour.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PICOGK TECHNIQUE: Spatial Painting
    /// Instead of using geometric primitives (beams), we "paint" the shape
    /// by placing many overlapping spheres. This allows arbitrary curved
    /// surfaces that can't be represented by simple primitives.
    /// </para>
    /// <para>
    /// ALGORITHM:
    /// 1. For each axial position (z), calculate the bell radius using r(t) = r_t + dr * t^0.6
    /// 2. At each z, paint a ring of spheres around the circumference
    /// 3. Spheres overlap to create a continuous surface when voxelized
    /// </para>
    /// </remarks>
    public Voxels GenerateFlowPath()
    {
        Lattice lat = new();

        // =====================================================================
        // CONVERGENT SECTION: Simple cone (same as SimpleNozzle)
        // =====================================================================
        // AEROSPACE: The convergent section isn't critical for bell design -
        // flow is subsonic here and a simple cone works well.
        lat.AddBeam(
            new Vector3(0, 0, 0),
            new Vector3(0, 0, _params.ConvergentLength),
            _params.InletRadius,
            _params.ThroatRadius,
            false
        );

        // =====================================================================
        // DIVERGENT SECTION: Parabolic bell contour via spatial painting
        // =====================================================================
        // Key positions along the nozzle axis (z direction)
        float throatZ = _params.ConvergentLength;
        float exitZ = throatZ + _params.DivergentLength;

        // Determine number of axial steps for smooth curve
        // More steps = smoother surface but slower computation
        int steps = (int)(_params.DivergentLength / (_voxelSize * 2));
        steps = Math.Max(steps, 50); // Minimum 50 steps for acceptable smoothness

        // =====================================================================
        // AXIAL LOOP: Step along the nozzle from throat to exit
        // =====================================================================
        for (int i = 0; i <= steps; i++)
        {
            // t ranges from 0 (throat) to 1 (exit)
            float t = (float)i / steps;

            // Current axial position
            float z = throatZ + t * _params.DivergentLength;

            // =====================================================================
            // PARABOLIC RADIUS PROFILE
            // =====================================================================
            // AEROSPACE: The bell contour uses a power law profile:
            //   r(t) = r_throat + (r_exit - r_throat) * t^n
            //
            // Where n controls the shape:
            //   n = 1.0: Linear (conical nozzle)
            //   n = 0.5: Square root (very rapid initial expansion)
            //   n = 0.6: Good approximation of real bell nozzles
            //
            // The key is n < 1, which gives faster expansion near the throat
            // (where flow needs to turn quickly) and gradual straightening
            // toward the exit (for axial exhaust).
            float radiusExpansion = _params.ExitRadius - _params.ThroatRadius;
            float radius = _params.ThroatRadius + radiusExpansion * MathF.Pow(t, 0.6f);

            // =====================================================================
            // CIRCUMFERENTIAL LOOP: Paint ring of spheres at this cross-section
            // =====================================================================
            // PICOGK: Each sphere radius should be ~2x voxel size for proper overlap.
            // This ensures no gaps between spheres when voxelized.
            float sphereRadius = _voxelSize * 2f;

            // Calculate how many spheres needed around the circumference
            // Circumference = 2 * pi * r, divide by sphere diameter for count
            int circumSteps = (int)(2 * MathF.PI * radius / sphereRadius) + 1;

            for (int j = 0; j < circumSteps; j++)
            {
                // Angle around the circumference (0 to 2*pi)
                float theta = 2 * MathF.PI * j / circumSteps;

                // Convert polar to Cartesian coordinates
                // MATH: x = r*cos(theta), y = r*sin(theta)
                float x = radius * MathF.Cos(theta);
                float y = radius * MathF.Sin(theta);

                // PICOGK: AddSphere places a sphere primitive at the specified location.
                // The overlapping spheres will merge when converted to voxels.
                lat.AddSphere(new Vector3(x, y, z), sphereRadius);
            }
        }

        // Convert the lattice of spheres to a voxel field
        return new Voxels(lat);
    }

    /// <summary>
    /// Generate the complete bell nozzle (hollow shell).
    /// </summary>
    /// <remarks>
    /// Uses the same "offset and subtract" pattern as SimpleNozzle.
    /// The only difference is the flow path has a curved (parabolic) profile.
    /// </remarks>
    public Voxels GenerateNozzle()
    {
        Console.WriteLine("Generating bell nozzle geometry...");

        // Create flow path with parabolic contour
        Voxels voxFlowPath = GenerateFlowPath();
        Console.WriteLine("  ✓ Parabolic flow path created");

        // Offset outward to create wall thickness
        Voxels voxOuter = new(voxFlowPath);
        voxOuter.Offset(_params.WallThickness);
        Console.WriteLine($"  ✓ Outer shell created");

        // Subtract inner to create hollow nozzle
        voxOuter.BoolSubtract(voxFlowPath);
        Console.WriteLine("  ✓ Hollow bell nozzle created");

        return voxOuter;
    }

    // =========================================================================
    // EXPORT FUNCTIONALITY
    // =========================================================================

    /// <summary>
    /// Generate and export bell nozzle to STL files.
    /// </summary>
    public void GenerateAndExport(string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        // Print configuration
        Console.WriteLine("\n--- Bell Nozzle (Parabolic Contour) ---");
        _params.PrintSummary();
        // C#: :P0 format = percentage with 0 decimal places (e.g., "80%")
        Console.WriteLine($"Bell Fraction: {_bellFraction:P0}");
        Console.WriteLine();

        // Generate geometries
        Voxels voxFlowPath = GenerateFlowPath();
        Voxels voxNozzle = GenerateNozzle();

        // Export flow path
        string flowPathFile = Path.Combine(outputFolder, "bell_nozzle_flow_path.stl");
        Console.WriteLine($"\nExporting flow path to: {flowPathFile}");
        ExportToStl(voxFlowPath, flowPathFile);

        // Export complete nozzle
        string nozzleFile = Path.Combine(outputFolder, "bell_nozzle.stl");
        Console.WriteLine($"Exporting nozzle to: {nozzleFile}");
        ExportToStl(voxNozzle, nozzleFile);

        Console.WriteLine("\n✓ Bell nozzle export complete!");
    }

    /// <summary>
    /// Export voxels to STL via mesh conversion.
    /// </summary>
    private void ExportToStl(Voxels voxels, string filePath)
    {
        Mesh mesh = new(voxels);
        mesh.SaveToStlFile(filePath);
        Console.WriteLine($"  → Triangles: {mesh.nTriangleCount():N0}");
    }

    // =========================================================================
    // PICOGK TASK ENTRY POINT
    // =========================================================================

    /// <summary>
    /// PicoGK task entry point for bell nozzle generation.
    /// </summary>
    /// <remarks>
    /// AEROSPACE: This task uses the VacuumOptimized parameters which have
    /// a high expansion ratio - perfect for demonstrating the bell contour's
    /// ability to handle large area changes smoothly.
    /// </remarks>
    public static void Task()
    {
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  PicoGK Bell Nozzle Showcase");
        Console.WriteLine("  Parabolic Contour Rocket Nozzle");
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine();

        // Use vacuum-optimized parameters (high expansion ratio)
        // This demonstrates the bell contour's smooth handling of
        // large area ratios that would cause flow issues in conical nozzles
        var nozzle = new BellNozzle(NozzleParameters.VacuumOptimized, 0.5f);

        string outputFolder = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Output")
        );

        nozzle.GenerateAndExport(outputFolder);

        Console.WriteLine();
        Console.WriteLine($"Output folder: {outputFolder}");
    }
}
