// =============================================================================
// SIMPLE CONICAL NOZZLE - BASIC DE LAVAL NOZZLE WITH STRAIGHT WALLS
// =============================================================================
//
// AEROSPACE CONCEPT: Conical Nozzle Design
// =========================================
//
// This creates a simple convergent-divergent nozzle with straight conical walls.
// While not as efficient as a bell nozzle, conical nozzles are:
// - Easier to manufacture
// - Good for understanding basic nozzle physics
// - Used in solid rocket motors and some simpler engines
//
// CONSTRUCTION METHOD:
// --------------------
//
// We build the nozzle using voxel-based geometry (like 3D pixels):
//
//   Step 1: Create the INNER FLOW PATH (the volume where gas flows)
//
//           Convergent Cone     Divergent Cone
//                 ╲    ___________    ╱
//                  ╲  /           \  ╱
//                   ╲/             \/
//                   /\    Throat   /\
//                  /  \___________/  \
//                 ╱                   ╱
//
//   Step 2: OFFSET the flow path outward to create wall thickness
//
//           _________________________
//          /  ╲    ___________    ╱  \
//         /    ╲  /           \  ╱    \
//        /      ╲/             \/      \
//        \      /\             /\      /
//         \    /  \___________/  \    /
//          \__╱___________________╱__/
//
//   Step 3: SUBTRACT the inner flow path to create hollow nozzle
//
//           _________________________
//          ╱██╲                  ╱██╲
//         ╱████╲                ╱████╲
//        ╱██████╲______________╱██████╲
//        ╲██████╱              ╲██████╱
//         ╲████╱                ╲████╱
//          ╲__╱__________________╲__╱
//
//        (█ = solid nozzle wall material)
//
// PICOGK CONCEPTS:
// ----------------
// - Lattice: A collection of geometric primitives (beams, spheres)
// - Voxels: 3D grid representation of geometry (like 3D pixels)
// - Offset: Expands or contracts geometry by a distance
// - BoolSubtract: Removes one volume from another
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. READONLY FIELDS
//    `private readonly NozzleParameters _params;`
//    Can only be assigned in constructor. Ensures immutability.
//
// 2. EXPRESSION-BODIED METHODS (C# 6+)
//    Methods with `=>` instead of braces for single-expression bodies.
//
// 3. STATIC METHODS AS ENTRY POINTS
//    `public static void Task()` - Static method used as PicoGK entry point.
//
// 4. PATH.COMBINE AND PATH.GETFULLPATH
//    Safe cross-platform path manipulation.
//
// 5. DIRECTORY.CREATEDIRECTORY
//    Creates directory and all parent directories if they don't exist.
//    Returns without error if directory already exists.
//
// =============================================================================

using System.Numerics;
using PicoGK;

namespace NozzleShowcase.Nozzles;

/// <summary>
/// Creates a simple convergent-divergent (de Laval) rocket nozzle
/// using PicoGK's voxel-based geometry approach.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: This is the most basic nozzle design - two cones joined
/// at the throat. Real rocket engines often use bell (parabolic) contours
/// for better efficiency, but conical nozzles are easier to manufacture
/// and analyze.
/// </para>
/// <para>
/// PICOGK: The geometry is built using the "offset and subtract" pattern:
/// 1. Create inner flow volume from geometric primitives (Lattice -> Voxels)
/// 2. Offset outward to create outer surface
/// 3. Boolean subtract inner from outer to create hollow shell
/// </para>
/// </remarks>
public class SimpleNozzle
{
    // C#: `readonly` fields can only be set in the constructor.
    // This ensures the parameters cannot be accidentally changed after creation.
    // Convention: prefix private fields with underscore.
    private readonly NozzleParameters _params;
    private readonly float _voxelSize;

    /// <summary>
    /// Create a new simple nozzle generator.
    /// </summary>
    /// <param name="parameters">Nozzle dimensions</param>
    /// <param name="voxelSize">Voxel resolution in mm (smaller = more detail)</param>
    public SimpleNozzle(NozzleParameters parameters, float voxelSize = 0.5f)
    {
        _params = parameters;
        _voxelSize = voxelSize;
    }

    // =========================================================================
    // GEOMETRY GENERATION
    // =========================================================================

    /// <summary>
    /// Generate the complete nozzle geometry (hollow shell).
    /// </summary>
    /// <returns>Voxel field representing the hollow nozzle</returns>
    /// <remarks>
    /// PICOGK WORKFLOW:
    /// 1. GenerateFlowPath() creates the inner volume
    /// 2. new Voxels(original) creates a copy
    /// 3. Offset() expands the copy outward
    /// 4. BoolSubtract() removes the inner volume
    ///
    /// This "offset and subtract" pattern is fundamental in
    /// computational geometry for creating hollow shells.
    /// </remarks>
    public Voxels GenerateNozzle()
    {
        Console.WriteLine("Generating nozzle geometry...");

        // =====================================================================
        // STEP 1: Create the inner flow path
        // =====================================================================
        // This is the volume where exhaust gas will flow.
        // In real analysis, this volume would be used for CFD simulation.
        Voxels voxFlowPath = GenerateFlowPath();
        Console.WriteLine("  ✓ Flow path created");

        // =====================================================================
        // STEP 2: Create the outer shell by offsetting
        // =====================================================================
        // PICOGK: new Voxels(existing) creates a copy of the voxel field.
        // This is important - we need to keep the original for subtraction!
        Voxels voxOuter = new(voxFlowPath);

        // PICOGK: Offset() expands the geometry by the specified distance.
        // Positive offset = expansion (grows outward)
        // Negative offset = contraction (shrinks inward)
        voxOuter.Offset(_params.WallThickness);
        Console.WriteLine($"  ✓ Outer shell created (offset by {_params.WallThickness}mm)");

        // =====================================================================
        // STEP 3: Subtract inner to create hollow nozzle
        // =====================================================================
        // PICOGK: BoolSubtract() performs CSG (Constructive Solid Geometry)
        // subtraction. The argument is removed from the object it's called on.
        voxOuter.BoolSubtract(voxFlowPath);
        Console.WriteLine("  ✓ Hollow nozzle created via boolean subtract");

        return voxOuter;
    }

    /// <summary>
    /// Generate just the inner flow path (useful for CFD or visualization).
    /// </summary>
    /// <returns>Voxel field representing the flow volume</returns>
    /// <remarks>
    /// <para>
    /// PICOGK: We use a Lattice to define the geometry, then convert to Voxels.
    /// Lattice = collection of geometric primitives (beams, spheres, etc.)
    /// Voxels = 3D grid representation (like 3D pixels)
    /// </para>
    /// <para>
    /// The flow path consists of two tapered beams (cones):
    /// 1. Convergent: inlet radius -> throat radius
    /// 2. Divergent: throat radius -> exit radius
    /// </para>
    /// </remarks>
    public Voxels GenerateFlowPath()
    {
        // PICOGK: Lattice is a container for geometric primitives.
        // It's an efficient way to define complex shapes before voxelization.
        Lattice latFlowPath = new();

        // =====================================================================
        // CONVERGENT SECTION: from inlet to throat
        // =====================================================================
        // AEROSPACE: The convergent section accelerates subsonic gas.
        // As area decreases, velocity increases (continuity equation).
        // Flow reaches exactly Mach 1.0 at the throat (minimum area).
        //
        // PICOGK: AddBeam creates a tapered cylinder (cone if radii differ).
        // Parameters: start point, end point, start radius, end radius, rounded caps
        latFlowPath.AddBeam(
            new Vector3(0, 0, 0),                           // Inlet position (origin)
            new Vector3(0, 0, _params.ConvergentLength),    // Throat position
            _params.InletRadius,                             // Start radius (larger)
            _params.ThroatRadius,                            // End radius (smaller)
            false                                            // No rounded caps (flat ends)
        );

        // =====================================================================
        // DIVERGENT SECTION: from throat to exit
        // =====================================================================
        // AEROSPACE: The divergent section accelerates supersonic gas!
        // Counterintuitively, once flow is supersonic, INCREASING area
        // causes further acceleration. This is the key insight of de Laval.
        //
        // The supersonic flow expands, pressure drops, and velocity increases.
        // Exit velocity can reach 2000-4500 m/s depending on propellants!
        latFlowPath.AddBeam(
            new Vector3(0, 0, _params.ConvergentLength),                            // Throat position
            new Vector3(0, 0, _params.ConvergentLength + _params.DivergentLength),  // Exit position
            _params.ThroatRadius,                                                    // Start radius (smaller)
            _params.ExitRadius,                                                      // End radius (larger)
            false                                                                    // No rounded caps
        );

        // PICOGK: Convert Lattice to Voxels for solid geometry operations.
        // The voxel grid resolution was set when PicoGK was initialized.
        return new Voxels(latFlowPath);
    }

    // =========================================================================
    // EXPORT FUNCTIONALITY
    // =========================================================================

    /// <summary>
    /// Generate the nozzle and export to STL files.
    /// </summary>
    /// <param name="outputFolder">Folder to save STL files</param>
    /// <remarks>
    /// C#: Directory.CreateDirectory() is safe to call even if directory exists.
    /// It creates all necessary parent directories automatically.
    /// </remarks>
    public void GenerateAndExport(string outputFolder)
    {
        // Ensure output directory exists (creates parents too if needed)
        Directory.CreateDirectory(outputFolder);

        // Print parameters for user reference
        _params.PrintSummary();
        Console.WriteLine();

        // Generate both geometries
        Voxels voxFlowPath = GenerateFlowPath();
        Voxels voxNozzle = GenerateNozzle();

        // Export flow path (useful for CFD analysis)
        string flowPathFile = Path.Combine(outputFolder, "nozzle_flow_path.stl");
        Console.WriteLine($"\nExporting flow path to: {flowPathFile}");
        ExportToStl(voxFlowPath, flowPathFile);

        // Export complete nozzle (for 3D printing or visualization)
        string nozzleFile = Path.Combine(outputFolder, "simple_nozzle.stl");
        Console.WriteLine($"Exporting nozzle to: {nozzleFile}");
        ExportToStl(voxNozzle, nozzleFile);

        Console.WriteLine("\n✓ Export complete!");
    }

    /// <summary>
    /// Export a voxel field to an STL file via mesh conversion.
    /// </summary>
    /// <remarks>
    /// PICOGK EXPORT PIPELINE:
    /// 1. Voxels -> Mesh: Extracts surface triangles using marching cubes
    /// 2. Mesh -> STL: Writes triangles to standard STL format
    ///
    /// STL (Stereolithography) is the standard format for 3D printing.
    /// It represents surfaces as a collection of triangles.
    /// </remarks>
    private void ExportToStl(Voxels voxels, string filePath)
    {
        // PICOGK: new Mesh(voxels) extracts the surface as triangles.
        // This uses the marching cubes algorithm internally.
        Mesh mesh = new(voxels);

        // Save as binary STL file (more compact than ASCII STL)
        mesh.SaveToStlFile(filePath);

        // Report mesh statistics
        // nTriangleCount() returns the number of triangles in the mesh
        Console.WriteLine($"  → Triangles: {mesh.nTriangleCount():N0}");
    }

    // =========================================================================
    // PICOGK TASK ENTRY POINT
    // =========================================================================

    /// <summary>
    /// Create and run the nozzle generation task for PicoGK.
    /// This is the entry point called by PicoGK.Library.Go()
    /// </summary>
    /// <remarks>
    /// <para>
    /// PICOGK: Library.Go(voxelSize, Task) initializes PicoGK and then
    /// calls this static method. All geometry operations must happen
    /// inside this task method (after PicoGK is initialized).
    /// </para>
    /// <para>
    /// C# CONCEPT: Static methods belong to the class, not instances.
    /// They can be passed as delegates (function references) to other methods.
    /// `SimpleNozzle.Task` is a reference to this method.
    /// </para>
    /// </remarks>
    public static void Task()
    {
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  PicoGK Nozzle Showcase");
        Console.WriteLine("  Convergent-Divergent Rocket Nozzle");
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine();

        // Create nozzle with default parameters
        // The voxel size was already set in Library.Go(), but we track it here
        var nozzle = new SimpleNozzle(NozzleParameters.Default, 0.5f);

        // Get output folder (relative to project, not executable)
        // Path.GetFullPath() resolves the ".." references to an absolute path
        string outputFolder = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Output")
        );

        // Generate geometry and export to STL
        nozzle.GenerateAndExport(outputFolder);

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine($"  Output folder: {outputFolder}");
        Console.WriteLine("═══════════════════════════════════════════");
    }
}
